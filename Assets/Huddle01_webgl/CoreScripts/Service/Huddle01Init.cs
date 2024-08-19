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
    public class Huddle01Init : Singleton<Huddle01Init>
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

        private string _projectId;
        private string _roomId;
        private string _token;

        public string RoomId => _roomId;
        public string Token => _token;

        public List<string> _allPeers = new List<string>();

        public void Init(string projectId)
        {
            _projectId = projectId;
            Huddle01JSNative.InitHuddle01WebSdk(_projectId);
        }

        public void JoinRoom(string roomId, string token)
        {
            _roomId = roomId;
            _token = token;
            Huddle01JSNative.JoinRoom(_roomId, _token);
        }

        public void LeaveRoom()
        {
            Huddle01JSNative.LeaveRoom();
        }

        public void MuteMic(bool shouldMute, PeerMetadata metadata)
        {
            Debug.Log($"Mute mic : {JsonConvert.SerializeObject(metadata)}");
            Huddle01JSNative.MuteMic(shouldMute, JsonConvert.SerializeObject(metadata));
        }

        public void EnableVideo(bool enable, PeerMetadata metadata) 
        {
            Debug.Log($"EnableVideo : {JsonConvert.SerializeObject(metadata)}");
            Huddle01JSNative.EnableVideo(enable, JsonConvert.SerializeObject(metadata));
        }

        public void SendTextMessage(string message)
        {
            Huddle01JSNative.SendTextMessage(message);
        }

        public void ConsumerPeer(string peerId)
        {
            Huddle01JSNative.ConsumePeer(peerId);
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
            Debug.Log("Room Joined");
            OnJoinRoom?.Invoke();
        }

        public void OnLocalPeerIdReceived(string peerId)
        {
            Debug.Log($"OnLocalPeerIdReceived {peerId}");
            LocalPeerId?.Invoke(peerId);
        }

        public void OnPeerAdded(string peerInfo)
        {
            Debug.Log($"OnPeerAdded {peerInfo}");
            if (!_allPeers.Contains(peerInfo)) _allPeers.Add(peerInfo);
            PeerAdded?.Invoke(peerInfo);
        }

        public void OnPeerLeft(string peerInfo)
        {
            Debug.Log($"OnPeerLeft {peerInfo}");
            if (!_allPeers.Contains(peerInfo)) _allPeers.Remove(peerInfo);
            PeerLeft?.Invoke(peerInfo);
        }

        public void OnPeerMute(string peerId)
        {
            Debug.Log($"OnPeerMute {peerId}");
            PeerMuted?.Invoke(peerId,true);
        }

        public void OnPeerUnMute(string peerId)
        {
            Debug.Log($"OnPeerMute {peerId}");
            PeerMuted?.Invoke(peerId,false);
        }

        public void OnRoomClosed()
        {
            Debug.Log($"OnRoomClosed");
            RoomClosed?.Invoke();
        }

        public void OnPeerMetadataUpdated(string peerInfo)
        {
            Debug.Log($"peerInfo {peerInfo}");
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
            Debug.Log($"Message received : {data}");
            OnMessageReceived?.Invoke(data);
        }

        public void OnLeavingRoom() 
        {
            Debug.Log($"Message received");
            OnLeaveRoom.Invoke();
        }

        #endregion

        private float ConvertToOneDecimal(float val) 
        {
            return Mathf.Round(val * 10f) / 10f;
        }
    }
}

