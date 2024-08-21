using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using Huddle01;

namespace Huddle01.Sample 
{
    public class AudioMeetingExample : MonoBehaviour
    {
        public Dictionary<string, GameObject> PeersMap = new Dictionary<string, GameObject>();

        private GameObject _selfUserSection;

        [SerializeField]
        private string _projectId;
        [SerializeField]
        private string _apiKey;

        [SerializeField]
        private GameObject _userSectionPrefab;

        [SerializeField]
        private Transform _userSectionContentHolder;

        [SerializeField]
        private string _roomId;
        public string RoomId => _roomId;

        private string _token;
        public string Token => _token;

        [Header("Sections")]
        [SerializeField]
        private GameObject _userSectionSection;
        [SerializeField]
        private GameObject _joinRomSection;
        [SerializeField]
        private GameObject _userOptions;
        [SerializeField]
        private TMP_Text _headerText;


        [Header("Input fields")]
        [SerializeField]
        private TMP_InputField _nameInputFeild;
        [SerializeField]
        private TMP_InputField _tokenInputFeild;
        [SerializeField]
        private TMP_InputField _roomInputFeild;

        private bool _selfMicMuteStatus = true;
        private bool _selfVideoEnabled = false;


        void Start()
        {
            _headerText.text = $"RoomID : {_roomId}";
            Huddle01Core.Instance.Init(_projectId);
        }

        private void OnEnable()
        {
            Huddle01Core.OnJoinRoom += OnJoinRoom;
            Huddle01Core.LocalPeerId += OnLocalPeerIdReceived;
            Huddle01Core.PeerAdded += OnPeerJoined;
            Huddle01Core.PeerLeft += OnPeerLeft;
            Huddle01Core.PeerMuted += OnPeerMuted;
            Huddle01Core.RoomClosed += OnRoomClosed;
            Huddle01Core.PeerMetadata += OnPeerMetaDataUpdated;
            Huddle01Core.OnResumePeerVideo += OnPeerVideoResume;
            Huddle01Core.OnStopPeerVideo += OnPeerVideoStop;
        }


        private void OnDisable()
        {
            Huddle01Core.OnJoinRoom -= OnJoinRoom;
            Huddle01Core.LocalPeerId -= OnLocalPeerIdReceived;
            Huddle01Core.PeerAdded -= OnPeerJoined;
            Huddle01Core.PeerLeft -= OnPeerLeft;
            Huddle01Core.PeerMuted -= OnPeerMuted;
            Huddle01Core.RoomClosed -= OnRoomClosed;
            Huddle01Core.PeerMetadata -= OnPeerMetaDataUpdated;
            Huddle01Core.OnResumePeerVideo -= OnPeerVideoResume;
            Huddle01Core.OnStopPeerVideo -= OnPeerVideoStop;
        }

        #region Callbacks
        private void OnRoomClosed()
        {
            DestroyAllChildren(_userSectionContentHolder);

        }

        private void OnPeerMuted(string peerId,bool isMuted)
        {
            UserSectionBase userSectionRef = _selfUserSection.GetComponent<UserSectionBase>();

            if (userSectionRef.UserInfo.PeerId == peerId)
            {
                return;
            }

            //check for other peer
            GameObject peerSection = null;

            if (PeersMap.TryGetValue(peerId, out peerSection))
            {
                Debug.Log($"OnPeerMetaDataUpdated : {peerSection.name}");
                peerSection.GetComponent<UserSectionBase>().MuteUser(isMuted);
            }
            else
            {
                Debug.LogError("Peer not found");
            }

        }

        private void OnPeerLeft(string peerInfo)
        {
            Debug.Log($"OnPeerLeft : {peerInfo}");
            GameObject temp = null;
            if (PeersMap.TryGetValue(peerInfo, out temp))
            {
                Debug.Log($"OnPeerLeft : {temp.name}");
                PeersMap.Remove(peerInfo);
                Destroy(temp);
            }
        }

        private void OnPeerJoined(string peerId)
        {
            Debug.Log($"Peer Joined : {peerId}");
            GameObject peerSection = Instantiate(_userSectionPrefab, _userSectionContentHolder);
            UserSectionBase userSectionRef = peerSection.GetComponent<UserSectionBase>();
            Debug.Log($"Adding peer to map : {peerId}");
            PeersMap.Add(peerId, peerSection);
            HuddleUserInfo userInfo = new HuddleUserInfo();
            userInfo.PeerId = peerId;
            userSectionRef.Setup(userInfo);
            //userSectionRef.UpdateMetadata(JsonConvert.DeserializeObject<PeerMetadata>(JSNative.GetRemotePeerMetaData(peerId)));
        }

        private void OnJoinRoom()
        {
            _headerText.text = $"Room Joined";

            _selfUserSection = Instantiate(_userSectionPrefab, _userSectionContentHolder);
            UserSectionBase userSectionRef = _selfUserSection.GetComponent<UserSectionBase>();

            HuddleUserInfo selfUserInfo = new HuddleUserInfo();
            selfUserInfo.IsRemotePeer = false;
            selfUserInfo.Role = "guest";
            userSectionRef.Setup(selfUserInfo);

            _userOptions.SetActive(true);
            _joinRomSection.SetActive(false);
            _userSectionSection.SetActive(true);

            Huddle01Core.Instance.GetLocalPeerId();
        }

        private void OnLocalPeerIdReceived(string peerId)
        {
            UserSectionBase userSectionRef = _selfUserSection.GetComponent<UserSectionBase>();
            userSectionRef.UserInfo.PeerId = peerId;
            if (string.IsNullOrEmpty(_nameInputFeild.text))
            {
                _nameInputFeild.text = "Guest";
            }
            userSectionRef.UserInfo.Metadata.Name = _nameInputFeild.text;
            userSectionRef.UserInfo.Metadata.MuteStatus = false;
            userSectionRef.UserInfo.Metadata.VideoStatus = false;
            userSectionRef.UserInfo.Metadata.PeerId = peerId;
            UpdateLocalPeerMetaData(userSectionRef.UserInfo.Metadata);
            userSectionRef.UpdateMetadata(userSectionRef.UserInfo.Metadata);
            MuteMic(true);
        }

        private void OnPeerMetaDataUpdated(PeerMetadata peerInfo)
        {
            //check for self
            UserSectionBase userSectionRef = _selfUserSection.GetComponent<UserSectionBase>();

            if (userSectionRef.UserInfo.PeerId == peerInfo.PeerId)
            {
                return;
            }

            //check for other peer
            GameObject peerSection = null;
            Debug.Log($"OnPeerMetaDataUpdated : {peerInfo.PeerId}");

            if (PeersMap.TryGetValue(peerInfo.PeerId, out peerSection))
            {
                Debug.Log($"OnPeerMetaDataUpdated : {peerSection.name}");
                peerSection.GetComponent<UserSectionBase>().UpdateMetadata(peerInfo);
            }
            else
            {
                Debug.LogError("Peer not found");
            }
        }


        private void OnPeerVideoStop(string peerId)
        {
            UserSectionBase userSectionRef = _selfUserSection.GetComponent<UserSectionBase>();

            if (userSectionRef.UserInfo.PeerId == peerId)
            {
                userSectionRef.StopVideo();
                return;
            }

            //check for other peer
            GameObject peerSection = null;
            Debug.Log($"OnPeerVideoStop : {peerId}");

            if (PeersMap.TryGetValue(peerId, out peerSection))
            {
                Debug.Log($"OnPeerMetaDataUpdated : {peerSection.name}");
                peerSection.GetComponent<UserSectionBase>().StopVideo();
            }
            else
            {
                Debug.LogError("Peer not found");
            }

        }

        private void OnPeerVideoResume(string peerId)
        {
            UserSectionBase userSectionRef = _selfUserSection.GetComponent<UserSectionBase>();

            if (userSectionRef.UserInfo.PeerId == peerId)
            {
                userSectionRef.ResumeVideo();
                return;
            }

            //check for other peer
            GameObject peerSection = null;
            Debug.Log($"OnPeerVideoStop : {peerId}");

            if (PeersMap.TryGetValue(peerId, out peerSection))
            {
                Debug.Log($"OnPeerMetaDataUpdated : {peerSection.name}");
                peerSection.GetComponent<UserSectionBase>().ResumeVideo();
            }
            else
            {
                Debug.LogError("Peer not found");
            }
        }

        #endregion

        #region Main Functions

        public void JoinRoom()
        {
            Debug.Log("Join Room Clicked");
            Huddle01Core.Instance.JoinRoom(_roomInputFeild.text, _tokenInputFeild.text);
        }

        public void UpdateLocalPeerMetaData(PeerMetadata peerMetadata)
        {
            Debug.Log($"UpdateLocalPeerMetaData : {JsonConvert.SerializeObject(peerMetadata)}");
            Huddle01Core.Instance.UpdateLocalPeerMetaData(JsonConvert.SerializeObject(peerMetadata));
        }

        public void MuteMic(bool shouldMute)
        {
            _selfMicMuteStatus = shouldMute;
            UserSectionBase userSectionRef = _selfUserSection.GetComponent<UserSectionBase>();
            Debug.Log($"Mute mic metadata : {JsonConvert.SerializeObject(userSectionRef.UserInfo.Metadata)}");
            userSectionRef.UserInfo.Metadata.MuteStatus = shouldMute;
            userSectionRef.UpdateMetadata(userSectionRef.UserInfo.Metadata);
            Huddle01Core.Instance.MuteMic(shouldMute, userSectionRef.UserInfo.Metadata);
        }


        public void EnableVideoStreaming(bool enableVideo) 
        {
            _selfVideoEnabled = enableVideo;
            UserSectionBase userSectionRef = _selfUserSection.GetComponent<UserSectionBase>();
            Debug.Log($"Mute mic metadata : {JsonConvert.SerializeObject(userSectionRef.UserInfo.Metadata)}");
            userSectionRef.UserInfo.Metadata.VideoStatus = enableVideo;
            userSectionRef.UpdateMetadata(userSectionRef.UserInfo.Metadata);
            Huddle01Core.Instance.EnableVideo(enableVideo, userSectionRef.UserInfo.Metadata);
        }

        public void OnMuteMicClicked()
        {
            MuteMic(!_selfMicMuteStatus);
        }

        public void EnableVideo() 
        {
            EnableVideoStreaming(!_selfVideoEnabled);
        }

        public void LeaveRoom()
        {
            Huddle01Core.Instance.LeaveRoom();
            DestroyAllChildren(_userSectionContentHolder);
        }

        public void SendMessageToRoom() 
        {
            Huddle01Core.Instance.SendData("*","Hello guyzz","chat");
        }

        #endregion

        #region Generel helpers

        private void DestroyAllChildren(Transform parent)
        {
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }
        }

        #endregion

    }
}


