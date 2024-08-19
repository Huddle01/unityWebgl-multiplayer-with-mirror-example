    var NativeLib = {
    
    huddleClient: null,
    room : null,
    huddleToken:null,
    localStream : null,
    audioContext : null,
    audioListener : null,
    soundObjects : null,
    peersMap : null,
    

    // Video Receive
    NewTexture: function () {
        var tex = GLctx.createTexture();
        if (!tex){
            console.error("Failed to create a new texture for VideoReceiving")
            return LKBridge.NullPtr;
        }

        var id = GL.getNewId(GL.textures);
        tex.name = id;
        GL.textures[id] = tex;
        return id;
    },

    //Start Camera
    StartCamera: function(id)
    {
        
    },

    InitHuddle01WebSdk:function(projectId)
    {
        huddleClient = new HuddleWebCore.HuddleClient({
                        projectId: UTF8ToString(projectId),
                        options: {
                            activeSpeakers: {
                            // Number of active speaker visible in the grid, by default 8
                            size: 10,
                            },
                        },
                    });

        huddleClient.localPeer.on('receive-data', function (data) {
            console.log(data);
            SendMessage("Huddle01Init", "MessageReceived",JSON.stringify(data));
        });

        peersMap = new Map();

        window.addEventListener("beforeunload", function (event) {
           
            if (typeof unityInstance !== 'undefined' && unityInstance) {
                huddleClient.leaveRoom();
                SendMessage("Huddle01Init", "OnLeavingRoom");
            }
        });
    },

    SetUpForSpatialCommForPeer : function(peerId)
    {
        var peerIdString = UTF8ToString(peerId);
        var audioElem = document.getElementById(peerIdString+"_audio");

        if(!audioElem)
        {
            return console.error("audio element not found");
        }

        if(!peersMap[peerIdString].audioStream)
        {
            return console.error("audio stream not found");
        }

        var source = audioContext.createMediaStreamSource(peersMap[peerIdString].audioStream);
        //const track = CreateMediaElementSource(audioContext,audioElem);
        var panner = audioContext.createPanner();
        
        panner.panningModel = 'HRTF';
        panner.distanceModel = 'exponential';
        panner.refDistance = 1;
        panner.maxDistance = 10;
        panner.rolloffFactor = 1;
        panner.coneInnerAngle = 360;
        panner.coneOuterAngle = 360;
        panner.coneOuterGain = 0;

        source.connect(panner);
        panner.connect(audioContext.destination);

        audioElem.srcObject = peersMap[peerIdString].audioStream;
        audioElem.play();

        soundObjects[peerIdString] = { source: source, panner: panner };

        console.log("Spatial comm setup for peer",peerIdString);
    },

    DisconnectPeerPanner: function(peerId)
    {
        if (soundObjects[UTF8ToString(peerId)]) 
        {
            //get panner
            var panner = soundObjects[UTF8ToString(peerId)].panner;
            //disconnect
            panner.disconnect();
            soundObjects.delete(UTF8ToString(peerId));
        }
    },

    SetUpForSpatialComm:function()
    {
        console.log("Init Spatial Comm");
        audioContext = new (window.AudioContext || window.webkitAudioContext)();

        audioListener = audioContext.listener;
        audioListener.positionX.value = 0;
        audioListener.positionY.value = 0;
        audioListener.positionZ.value = 0;
        
        soundObjects = new Map();
        console.log("spatial comm is setup for local peer");
    },

    UpdateListenerPosition:function(posX,posY,posZ)
    {
        audioListener.positionX.value = posX;
        audioListener.positionY.value = posY;
        audioListener.positionZ.value = posZ;
        console.log("Updating local Peer Position");
    },

    UpdateListenerRotation:function(rotX,rotY,rotZ)
    {
        audioListener.forwardX.value = rotX;
        audioListener.forwardY.value = rotY;
        audioListener.forwardZ.value = rotZ;
    },

    UpdatePeerPosition:function(peerId,posX,posY,posZ)
    {   
        if (soundObjects[UTF8ToString(peerId)]) 
        {
            //get panner
            var tempPanner = soundObjects[UTF8ToString(peerId)].panner;
            tempPanner.setPosition(posX, posY, posZ);
            console.log("Updating Peer Position");
        }
    },

    UpdatePeerRotation:function(peerId,rotX,rotY,rotZ)
    {
        if (soundObjects[UTF8ToString(peerId)]) 
        {
            //get panner
            var tempPanner = soundObjects[UTF8ToString(peerId)].panner;
            tempPanner.orientationX.value = rotX;
            tempPanner.orientationY.value = rotY;
            tempPanner.orientationZ.value = rotZ;
            console.log("Updating Peer Rotation");
        }
    },


    JoinRoom : async function(roomId,tokenVal)
    {
        console.log("Join Room");
        //join room
        room = await huddleClient.joinRoom({
                roomId: UTF8ToString(roomId),
                token: UTF8ToString(tokenVal),
                });


        // on join room event
        room.on("room-joined", async function () {

            console.log("Room ID:", room.roomId);
            const remotePeers = huddleClient.room.remotePeers
            SendMessage("Huddle01Init", "OnRoomJoined"); 
            
            //if peer already exists
            for (const entry of Array.from(remotePeers.entries())) {
                (function(entry) {
                    const key = entry[0];
                    const value = entry[1];
                    const tempRemotePeer = value;
                    const peerIdString = tempRemotePeer.peerId;

                    peersMap[peerIdString] = { audioStream: null, videoStream: null };
                    SendMessage("Huddle01Init", "OnPeerAdded", peerIdString);

                    // remote peer on metadata updated event
                    tempRemotePeer.on("metadata-updated", async function () {
                        try {
                            const updatedMetadata = await huddleClient.room.getRemotePeerById(peerIdString).getMetadata();
                            SendMessage("Huddle01Init", "OnPeerMetadataUpdated", JSON.stringify(updatedMetadata));
                        } catch (error) {
                            console.error("Error updating metadata: ", error);
                        }
                    });

            // remote peer on stream available event
            tempRemotePeer.on("stream-playable", async function(data) {
                if (data.label == "audio") {
                    const audioElem = document.createElement("audio");
                    audioElem.id = peerIdString + "_audio";

                    if (!data.consumer.track) {
                        return console.log("track not found");
                    }

                    const stream = new MediaStream([data.consumer.track]);
                    document.body.appendChild(audioElem);

                    audioElem.srcObject = stream;
                    audioElem.play();

                    if (peersMap[peerIdString]) {
                        peersMap[peerIdString].audioStream = stream;
                    }

                    SendMessage("Huddle01Init", "OnPeerUnMute", peerIdString);

                } else if (data.label == "video") {
                    if (!data.consumer.track) {
                        return console.log("track not found");
                    }

                    const videoElem = document.createElement("video");
                    videoElem.id = peerIdString + "_video";

                    document.body.appendChild(videoElem);

                    videoElem.style.display = "none";
                    videoElem.style.opacity = 0;
                    const videoStream = new MediaStream([data.consumer.track]);
                    videoElem.srcObject = videoStream;
                    videoElem.play();
                    SendMessage("Huddle01Init", "ResumeVideo", peerIdString);
                }
            });

            // remote peer on stream closed event
            tempRemotePeer.on("stream-closed", function(data) {
                if (data.label == "audio") {
                    const audioElem = document.getElementById(peerIdString + "_audio");
                    console.log("audio on audio close", audioElem);
                    if (audioElem) {
                        audioElem.srcObject = null;
                        audioElem.remove();
                    }

                    if (peersMap[peerIdString]) {
                        peersMap[peerIdString].audioStream = null;
                    }

                SendMessage("Huddle01Init", "OnPeerMute", peerIdString);

                } else if (data.label == "video") {
                    const videoElem = document.getElementById(peerIdString + "_video");

                    if (videoElem) {
                        videoElem.remove();
                    }

                    SendMessage("Huddle01Init", "StopVideo", peerIdString);
                }
            });

                // metadata already exist
                (async function() {
                    try {
                        const updatedMetadata = await huddleClient.room.getRemotePeerById(peerIdString).getMetadata();
                        SendMessage("Huddle01Init", "OnPeerMetadataUpdated", JSON.stringify(updatedMetadata));
                    } catch (error) {
                        console.error("Error fetching initial metadata: ", error);
                    }
                })();
            })(entry); // Immediately Invoked Function Expression (IIFE) to capture the correct peerId
            }
        });

        
        //room-closed event
        room.on("room-closed", function () {
            console.log("Peer ID:", data.peerId);
            SendMessage("Huddle01Init", "OnRoomClosed");     
        });

        //new-peer-joined event
        huddleClient.room.on("new-peer-joined", function (data) {
        
            console.log("new-peer-joined Peer ID:", data.peer);
            
            peersMap[data.peer.peerId] = { audioStream: null, videoStream:null };
            SendMessage("Huddle01Init", "OnPeerAdded",data.peer.peerId);
       
        
            var remotePeer = data.peer;

            remotePeer.on("metadata-updated", async function () {
                console.log("Successfully updated remote peer metadata of : ", remotePeer.peerId);
                var updatedMetadata = await huddleClient.room.getRemotePeerById(remotePeer.peerId).getMetadata();
                SendMessage("Huddle01Init", "OnPeerMetadataUpdated",JSON.stringify(updatedMetadata));

            });
        
            remotePeer.on("stream-playable", async function(data) 
            {

                if(data.label == "audio")
                {
                    const audioElem = document.createElement("audio");
                    audioElem.id = remotePeer.peerId + "_audio";

                    if(!data.consumer.track)
                    {
                        return console.log("track not found");    
                    }
                    
                    const stream  = new MediaStream([data.consumer.track]);
                    
                    document.body.appendChild(audioElem);
                    audioElem.srcObject = stream;
                    audioElem.play();
                    
                    if(peersMap[remotePeer.peerId])
                    {
                        peersMap[remotePeer.peerId].audioStream = stream;
                    }

                    SendMessage("Huddle01Init", "OnPeerUnMute",remotePeer.peerId);

                }else if(data.label == "video")
                {
                    var videoElem = document.createElement("video");
                    videoElem.id = remotePeer.peerId + "_video";
                    console.log("video created : ",videoElem.id);
                    document.body.appendChild(videoElem);
                    //set properties
                    videoElem.style.display = "none";
                    videoElem.style.opacity = 0;
                    //get stream
                    const videoStream  = new MediaStream([data.consumer.track]);
                    videoElem.srcObject = videoStream;
                    videoElem.play();
                    SendMessage("Huddle01Init", "ResumeVideo",remotePeer.peerId);
                }
            });
        
            remotePeer.on("stream-closed", function (data) {
                console.log("Remote Peer Stream is closed.",data);

                if(data.label == "audio")
                {
                    var audioElem = document.getElementById(remotePeer.peerId+"_audio");
                    if(audioElem)
                    {
                        audioElem.srcObject = null;
                        audioElem.remove();
                    }

                    if(peersMap[remotePeer.peerId])
                    {
                        peersMap[remotePeer.peerId].audioStream = null;
                    }

                    SendMessage("Huddle01Init", "OnPeerMute",remotePeer.peerId);

                }else if(data.label == "video")
                {
                    var videoElem = document.getElementById(remotePeer.peerId + "_video");

                    if(videoElem)
                    {
                        videoElem.remove();
                    }

                    SendMessage("Huddle01Init", "StopVideo",remotePeer.peerId);

                }
            });
        
        });

        //peer-left
        room.on("peer-left", function (peerId) {
            console.log(" peer-left Peer ID:", peerId);

            SendMessage("Huddle01Init", "OnPeerLeft",peerId);

            //remove audio element
            var audioElem = document.getElementById(peerId+"_audio");

            if(audioElem)
            {
                audioElem.srcObject = null;
                audioElem.remove();
            }

            //remove video element
            var videoElem = document.getElementById(peerId + "_video");

            if(videoElem)
            {
                videoElem.remove();
            }

            // delete associated
                 
            peersMap.delete(UTF8ToString(peerId));
        });
    },


    MuteMic : async function(shouldMute,metadataNativ)
    {
        var metadata = JSON.parse(UTF8ToString(metadataNativ));
        console.log("MuteMic metadata name val : ",UTF8ToString(metadataNativ));

        if(shouldMute)
        {
            await huddleClient.localPeer.disableAudio();
            
        }else
        {
            await huddleClient.localPeer.enableAudio();
        }

        huddleClient.localPeer.updateMetadata({ 
            peerId: metadata.peerId,
            muteStatus: metadata.muteStatus,
            videoStatus : metadata.videoStatus,
            name : metadata.name,
            networkId: metadata.networkId
        });

    },

    EnableVideo : async function(enableVideo,metadataNativ)
    {
        var metadata = JSON.parse(UTF8ToString(metadataNativ));
        console.log("EnableVideo metadata name val : ",UTF8ToString(metadataNativ));
        
        if(enableVideo)
        {
            //const producer = await huddleClient.localPeer.produce({ label: "video", stream: mediaStream, appData });
            localStream = await huddleClient.localPeer.enableVideo();

            var videoElem = document.createElement("video");
            videoElem.id = huddleClient.localPeer.peerId + "_video";
            console.log("video created : ",videoElem.id);
            document.body.appendChild(videoElem);

            videoElem.srcObject = localStream;
            videoElem.play();

            SendMessage("Huddle01Init", "ResumeVideo",huddleClient.localPeer.peerId);
        }else
        {
            await huddleClient.localPeer.disableVideo();

            var videoElem = document.getElementById(huddleClient.localPeer.peerId + "_video");

            if(videoElem)
            {
                videoElem.remove();
            }

            SendMessage("Huddle01Init", "StopVideo",huddleClient.localPeer.peerId);
        }

        huddleClient.localPeer.updateMetadata({ 
            peerId: metadata.peerId,
            muteStatus: metadata.muteStatus,
            videoStatus : metadata.videoStatus,
            name : metadata.name,
            networkId: metadata.networkId
        });
    },
    
    LeaveRoom : function()
    {
        for (var entry of Array.from(peersMap.entries()))
       {
            var key = entry[0];
            var value = entry[1];
            var audioElem = document.getElementById(key+"_audio");
            //remove element
            if(audioElem)
            {
                audioElem.srcObject = null;
                audioElem.remove();
            }
            var videoElem = document.getElementById(UTF8ToString(peerId) + "_video");

            if(videoElem)
            {
                videoElem.remove();
            }
       }

       peersMap = new Map();

       huddleClient.leaveRoom();
       SendMessage("Huddle01Init", "OnLeavingRoom");
       //remove all audio associated
    },

    SendTextMessage : function(message)
    {
        var mes = UTF8ToString(message);
        console.log("Sending message",mes);
        huddleClient.localPeer.sendData({ to: "*", payload: mes, label: 'chat' });
    },

    ConsumePeer : function(peerId)
    {
        
    },

    UpdatePeerMeataData : function(metadataVal)
    {
        var metadata = JSON.parse(UTF8ToString(metadataVal));
        huddleClient.localPeer.updateMetadata({ 
            peerId: metadata.peerId,
            muteStatus: metadata.muteStatus,
            name : metadata.name,
            videoStatus: metadata.videoStatus,
            networkId: metadata.networkId
        });
    },

    GetRemotePeerMetaData : function(peerId)
    {
        console.log("GetRemotePeerMetaData of : ", UTF8ToString(peerId));
        var remotePeer = huddleClient.room.getRemotePeerById(UTF8ToString(peerId)).getMetadata();
        console.log("remote Peer Metadata : " ,remotePeer );

        var bufferSize = lengthBytesUTF8(JSON.stringify(remotePeer)) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(JSON.stringify(remotePeer), buffer, bufferSize);
        return buffer;
    },

    GetLocalPeerId : async function()
    {
        var peerId = await huddleClient.localPeer.peerId;
        SendMessage("Huddle01Init", "OnLocalPeerIdReceived",peerId);
    },

    AttachVideo: function (peerId, texId) {
        var tex = GL.textures[texId];
        var lastTime = -1;
        var peerIdString = UTF8ToString(peerId);
        console.log("video id " + UTF8ToString(peerId) + "_video");
        var initialVideo = document.getElementById(UTF8ToString(peerId) + "_video");
        initialVideo.style.opacity = 0;
        initialVideo.style.width = 0;
        initialVideo.style.height = 0;
        initialVideo.style.display = "none";
        setTimeout(function() {
            initialVideo.play();
        }, 0)
        initialVideo.addEventListener("canplay", (event) => {
            initialVideo.play();
        });
 
        //document.body.appendChild(initialVideo);
        var updateVideo = function (peerIdVal,textureId) {
            
            var video = document.getElementById(peerIdVal + "_video");

            if (!video || video === undefined) {
                initialVideo.remove();
                return;
            }
            
            if (!video.paused) {
                
                GLctx.bindTexture(GLctx.TEXTURE_2D, tex);
                
                // Flip Y
                GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true);
                GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, video);
                GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);

                GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MAG_FILTER, GLctx.LINEAR);
                GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
                GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
                GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
            }
            
            requestAnimationFrame(function(){updateVideo(peerIdVal,textureId)});
        };
        
        requestAnimationFrame(function(){updateVideo(peerIdString,tex)});
    },
};

mergeInto(LibraryManager.library, NativeLib);

