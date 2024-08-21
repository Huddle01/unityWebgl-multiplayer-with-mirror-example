using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Huddle01.Services;

namespace Huddle01 
{
    public class Huddle01Core : Singleton<Huddle01Core>
    {
        public delegate void LocalPeerIdEventHandler(string peerId);
        public delegate void PeerAddedEventHandler(string peerInfo);
        public delegate void PeerLeftEventHandler(string peerInfo);
        public delegate void PeerMutedEventHandler(string peerInfo, bool isMuted);
        public delegate void RoomClosedEventHandler();
        public delegate void PeerMetadataUpdatedEventHandler(PeerMetadata peerMetadata);
        public delegate void JoinRoomEventHandler();
        public delegate void ResumePeerVideoEventHandler(string peerId);
        public delegate void StopPeerVideoEventHandler(string peerId);
        public delegate void MessageReceivedEventHandler(string data);
        public delegate void LeaveRoomEventHandler();
        public delegate void StartConsumingPeerEventHandler(string peerId);
        public delegate void StopConsumingPeerEventHandler(string peerId);

        public static event LocalPeerIdEventHandler LocalPeerId;
        public static event PeerAddedEventHandler PeerAdded;
        public static event PeerLeftEventHandler PeerLeft;
        public static event PeerMutedEventHandler PeerMuted;
        public static event RoomClosedEventHandler RoomClosed;
        public static event PeerMetadataUpdatedEventHandler PeerMetadata;
        public static event JoinRoomEventHandler OnJoinRoom;
        public static event ResumePeerVideoEventHandler OnResumePeerVideo;
        public static event StopPeerVideoEventHandler OnStopPeerVideo;
        public static event MessageReceivedEventHandler OnMessageReceived;
        public static event LeaveRoomEventHandler OnLeaveRoom;
        public static event StartConsumingPeerEventHandler OnStartConsumingPeer;
        public static event StopConsumingPeerEventHandler OnStopConsumingPeer;

        private string _projectId;
        private string _roomId;
        private string _token;

        public string RoomId => _roomId;
        public string Token => _token;

        private List<string> _allPeers = new List<string>();

        public bool AutoConsume =>_autoConsume;
        private bool _autoConsume = true;

        public void Init(string projectId,bool shouldAutoConsume = true)
        {
            _projectId = projectId;
            _autoConsume = shouldAutoConsume;
            Huddle01JSNative.InitHuddle01WebSdk(_projectId, shouldAutoConsume);
        }

        /// <summary>
        /// Join Room
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="token"></param>
        public void JoinRoom(string roomId, string token)
        {
            _roomId = roomId;
            _token = token;
            Huddle01JSNative.JoinRoom(_roomId, _token);
        }

        /// <summary>
        /// Leave current room
        /// </summary>
        public void LeaveRoom()
        {
            Huddle01JSNative.LeaveRoom();
        }

        /// <summary>
        /// Toggle Mute Mic
        /// </summary>
        /// <param name="shouldMute"></param>
        /// <param name="metadata"></param>
        public void MuteMic(bool shouldMute, PeerMetadata metadata)
        {
           // Debug.Log($"Mute mic : {JsonConvert.SerializeObject(metadata)}");
            Huddle01JSNative.MuteMic(shouldMute, JsonConvert.SerializeObject(metadata));
        }

        /// <summary>
        /// Toggle local peer video
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="metadata"></param>
        public void EnableVideo(bool enable, PeerMetadata metadata) 
        {
            //Debug.Log($"EnableVideo : {JsonConvert.SerializeObject(metadata)}");
            Huddle01JSNative.EnableVideo(enable, JsonConvert.SerializeObject(metadata));
        }

        /// <summary>
        /// Send message to room
        /// pass peerId or * to send to all peers 
        /// </summary>
        /// <param name="message"></param>
        public void SendData(string to,string message,string label)
        {
            if (to.Equals("*"))
            {
                Huddle01JSNative.SendTextMessage(message, label);
            }
            else 
            {
                SendData(new List<string> { to},message,label);
            }
        }

        /// <summary>
        /// Send message to peers in the room
        /// Peers which are present in peerIds list will receive the data
        /// </summary>
        /// <param name="message"></param>
        public void SendData(List<string> peerIds,string message, string label)
        {
            string[] peerIdArray = peerIds.ToArray();
            Huddle01JSNative.SendTextMessageToPeers(message, peerIdArray, peerIdArray.Length, label);
        }

        /// <summary>
        /// Start consuming peer
        /// not supported when AutoConsume is true
        /// </summary>
        /// <param name="peerId"></param>
        public void StartConsumerPeer(string peerId)
        {
            if (!_autoConsume)
            {
                Huddle01JSNative.ConsumePeer(peerId);
            }
            else 
            {
                Debug.LogWarning("Consuming peer not supported with AutoConsume mode");
            }
            
        }

        /// <summary>
        /// Stop consuming peer
        /// not supported when AutoConsume is true 
        /// </summary>
        /// <param name="peerId"></param>
        public void StopConsumeingPeer(string peerId)
        {
            if (!_autoConsume)
            {
                Huddle01JSNative.StopConsumingPeer(peerId);
            }
            else
            {
                Debug.LogWarning("StopConsumeingPeer peer not supported with AutoConsume mode");
            }

        }

        public void GetLocalPeerId()
        {
            Huddle01JSNative.GetLocalPeerId();
        }

        public void UpdateLocalPeerMetaData(string metadata)
        {
            Huddle01JSNative.UpdatePeerMeataData(metadata);
        }

        public void SetUpdatedPositionForSpatialComm(string peerId, Vector3 pos)
        {
            Huddle01JSNative.UpdatePeerPosition(peerId, ConvertToOneDecimal(pos.x), ConvertToOneDecimal(pos.y), ConvertToOneDecimal(pos.z));
        }

        public void SetUpdatedRotationForSpatialComm(string peerId, Vector3 rot)
        {
            Huddle01JSNative.UpdatePeerRotation(peerId, rot.x, rot.y, rot.z);
        }

        public void SetLocalPlayerUpdatedPositionForSpatialComm(Vector3 pos)
        {
            Huddle01JSNative.UpdateListenerPosition(ConvertToOneDecimal(pos.x), ConvertToOneDecimal(pos.y), ConvertToOneDecimal(pos.z));
        }

        public void SetLocalPlayerUpdatedRotationForSpatialComm(Vector3 rot)
        {
            Huddle01JSNative.UpdateListenerRotation(rot.x, rot.y, rot.z);
        }

        public void SetupSpatialCommForRemotePeer(string peerId) 
        {
            Huddle01JSNative.SetUpForSpatialCommForPeer(peerId);
        }

        public void SetupSpatialCommForLocalPeer()
        {
            Huddle01JSNative.SetUpForSpatialComm();
        }

        public void DisableSpatialAudioForPeer(string peerId) 
        {
            Huddle01JSNative.DisconnectPeerPanner(peerId);
        }

        #region Callbacks

        public void OnRoomJoined()
        {
           // Debug.Log("Room Joined");
            OnJoinRoom?.Invoke();
        }

        public void OnLocalPeerIdReceived(string peerId)
        {
           // Debug.Log($"OnLocalPeerIdReceived {peerId}");
            LocalPeerId?.Invoke(peerId);
        }

        public void OnPeerAdded(string peerInfo)
        {
           // Debug.Log($"OnPeerAdded {peerInfo}");
            if (!_allPeers.Contains(peerInfo)) _allPeers.Add(peerInfo);
            PeerAdded?.Invoke(peerInfo);
        }

        public void OnPeerLeft(string peerInfo)
        {
           // Debug.Log($"OnPeerLeft {peerInfo}");
            if (!_allPeers.Contains(peerInfo)) _allPeers.Remove(peerInfo);
            PeerLeft?.Invoke(peerInfo);
        }

        public void OnPeerMute(string peerId)
        {
            //Debug.Log($"OnPeerMute {peerId}");
            PeerMuted?.Invoke(peerId,true);
        }

        public void OnPeerUnMute(string peerId)
        {
            //Debug.Log($"OnPeerMute {peerId}");
            PeerMuted?.Invoke(peerId,false);
        }

        public void OnRoomClosed()
        {
            //Debug.Log($"OnRoomClosed");
            RoomClosed?.Invoke();
        }

        public void OnPeerMetadataUpdated(string peerInfo)
        {
           //Debug.Log($"peerInfo {peerInfo}");
            PeerMetadata response = JsonConvert.DeserializeObject<PeerMetadata>(peerInfo);
            PeerMetadata?.Invoke(response);
        }

        public void ResumeVideo(string peerId) 
        {
            OnResumePeerVideo?.Invoke(peerId);
        }

        public void StopVideo(string peerId) 
        {
            OnStopPeerVideo?.Invoke(peerId);
        }

        public void MessageReceived(string data)
        {
           // Debug.Log($"Message received : {data}");
            OnMessageReceived?.Invoke(data);
        }

        public void OnLeavingRoom() 
        {
            //Debug.Log($"Message received");
            OnLeaveRoom.Invoke();
        }

        public void OnStartingConsumePeerSuccessfully(string peerId) 
        {
            OnStartConsumingPeer?.Invoke(peerId);
        }

        public void OnStopConsumePeerSuccessfully(string peerId)
        {
            OnStopConsumingPeer?.Invoke(peerId);
        }

        #endregion

        private float ConvertToOneDecimal(float val) 
        {
            return Mathf.Round(val * 10f) / 10f;
        }
    }
}

