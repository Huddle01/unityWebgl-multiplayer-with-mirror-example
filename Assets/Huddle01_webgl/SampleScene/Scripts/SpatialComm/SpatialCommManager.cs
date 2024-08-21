using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Huddle01;
using TMPro;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

public class SpatialCommManager : MonoBehaviour
{
    public Dictionary<string, GameObject> PeersMap = new Dictionary<string, GameObject>();

    public ClickMoveNavAgent ClickMoveNavAgentRef;

    [HideInInspector]
    public GameObject LocalPlayer;

    [SerializeField]
    private string _projectId;
    [SerializeField]
    private string _apiKey;

    private string _roomId;
    private string _token;
    private string _name;

    [Header("Player prrfab")]
    [SerializeField]
    private GameObject _playerPrefab;

    [Header("Menu Section")]
    [SerializeField]
    private GameObject _menuPanel;
    [SerializeField]
    private GameObject _inGameOptionsPanel;
    [SerializeField]
    private TMP_InputField _roomIdInputField;
    [SerializeField]
    private TMP_InputField _tokenInputField;
    [SerializeField]
    private TMP_InputField _nameInputFeild;

    [Header("Header")]
    [SerializeField]
    private TMP_Text _headerText;

    private bool _selfMicMuteStatus = true;
    private bool _selfVideoEnabled = false;

    // Start is called before the first frame update
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
        Huddle01Core.RoomClosed += OnRoomClosed;
        Huddle01Core.PeerMetadata += OnPeerMetaDataUpdated;
        Huddle01Core.OnResumePeerVideo += OnPeerVideoResume;
        Huddle01Core.OnStopPeerVideo += OnPeerVideoStop;
        Huddle01Core.OnMessageReceived += OnMessageReceived;
        Huddle01Core.PeerMuted += OnPeerMuteStatusChanged;
    }    

    private void OnDisable()
    {
        Huddle01Core.OnJoinRoom -= OnJoinRoom;
        Huddle01Core.LocalPeerId -= OnLocalPeerIdReceived;
        Huddle01Core.PeerAdded -= OnPeerJoined;
        Huddle01Core.PeerLeft -= OnPeerLeft;
        Huddle01Core.RoomClosed -= OnRoomClosed;
        Huddle01Core.PeerMetadata -= OnPeerMetaDataUpdated;
        Huddle01Core.OnResumePeerVideo -= OnPeerVideoResume;
        Huddle01Core.OnStopPeerVideo -= OnPeerVideoStop;
        Huddle01Core.OnMessageReceived -= OnMessageReceived;
        Huddle01Core.PeerMuted -= OnPeerMuteStatusChanged;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Callbacks

    private void OnRoomClosed()
    {
        foreach (var item in PeersMap)
        {
            Destroy(item.Value);
        }

        PeersMap.Clear();

        Destroy(LocalPlayer);
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
        GameObject peerSection = Instantiate(_playerPrefab);
        peerSection.transform.position = Vector3.zero;
        NavMeshPlayerController userSectionRef = peerSection.GetComponent<NavMeshPlayerController>();
        Debug.Log($"Adding peer to map : {peerId}");
        PeersMap.Add(peerId, peerSection);
        HuddleUserInfo userInfo = new HuddleUserInfo();
        userInfo.PeerId = peerId;
        userSectionRef.Setup(userInfo);
        try
        {
            Debug.Log($"Get metadata {Huddle01JSNative.GetRemotePeerMetaData(peerId)}");
            userSectionRef.UpdateMetadata(JsonConvert.DeserializeObject<PeerMetadata>(Huddle01JSNative.GetRemotePeerMetaData(peerId)));
        }
        catch 
        {
            Debug.Log("Metadata is empty");
        }
    }

    private void OnJoinRoom()
    {
        _headerText.text = $"Room Joined";

        LocalPlayer = Instantiate(_playerPrefab);
        LocalPlayer.transform.position = Vector3.zero;
        NavMeshPlayerController playerController = LocalPlayer.GetComponent<NavMeshPlayerController>();
        playerController.IsLocalPlayer = true;
        HuddleUserInfo selfUserInfo = new HuddleUserInfo();
        selfUserInfo.IsRemotePeer = false;
        selfUserInfo.Role = "guest";
        playerController.Setup(selfUserInfo);
        
        _menuPanel.SetActive(false);
        _inGameOptionsPanel.SetActive(true);

        ClickMoveNavAgentRef.LocalPlayer = playerController;
        Huddle01Core.Instance.SetupSpatialCommForLocalPeer();
        playerController.IsSpatialComm = true;
        Huddle01Core.Instance.GetLocalPeerId();
    }

    private void OnLocalPeerIdReceived(string peerId)
    {
        NavMeshPlayerController localPlayerTemp = LocalPlayer.GetComponent<NavMeshPlayerController>();
        localPlayerTemp.UserInfo.PeerId = peerId;
        if (string.IsNullOrEmpty(_nameInputFeild.text))
        {
            _nameInputFeild.text = "Guest";
        }
        localPlayerTemp.UserInfo.Metadata.Name = _nameInputFeild.text;
        localPlayerTemp.UserInfo.Metadata.MuteStatus = false;
        localPlayerTemp.UserInfo.Metadata.VideoStatus = false;
        localPlayerTemp.UserInfo.Metadata.PeerId = peerId;
        UpdateLocalPeerMetaData(localPlayerTemp.UserInfo.Metadata);
        localPlayerTemp.UpdateMetadata(localPlayerTemp.UserInfo.Metadata);
        localPlayerTemp.MuteMic();
    }

    private void OnPeerMuteStatusChanged(string peerId, bool isMuted)
    {
        StartCoroutine(PostOnPeerMuteStatsChanged(peerId, isMuted));
    }

    IEnumerator PostOnPeerMuteStatsChanged(string peerId, bool isMuted) 
    {
        GameObject peerSection = null;
        Debug.Log($"OnPeerMuteStatusChanged : {peerId}");

        if (PeersMap.TryGetValue(peerId, out peerSection))
        {
            Debug.Log($"OnPeerMetaDataUpdated : {peerSection.name}");
            NavMeshPlayerController remotePlayer = peerSection.GetComponent<NavMeshPlayerController>();
            remotePlayer.ChangeMuteMicStatus(isMuted);

            yield return new WaitForSeconds(1);

            if (isMuted)
            {
                remotePlayer.IsSpatialComm = false;
                //disable spatial comm
                Huddle01Core.Instance.DisableSpatialAudioForPeer(peerId);
            }
            else
            {
                //setup spatial comm
                remotePlayer.IsSpatialComm = true;
                Debug.Log($"Setting up spatial comm for peer {peerId}");
                Huddle01Core.Instance.SetupSpatialCommForRemotePeer(peerId);
            }
        }
        else
        {
            Debug.LogError("Peer not found");
        }
    }

    private void OnPeerMetaDataUpdated(PeerMetadata peerInfo)
    {
        if (LocalPlayer.GetComponent<NavMeshPlayerController>().UserInfo.PeerId == peerInfo.PeerId)
        {
            return;
        }

        //check for other peer
        GameObject peerSection = null;
        Debug.Log($"OnPeerMetaDataUpdated : {peerInfo.PeerId}");

        if (PeersMap.TryGetValue(peerInfo.PeerId, out peerSection))
        {
            Debug.Log($"OnPeerMetaDataUpdated : {peerSection.name}");
            peerSection.GetComponent<NavMeshPlayerController>().UpdateMetadata(peerInfo);
        }
        else
        {
            Debug.LogError("Peer not found");
        }
    }

    private void OnPeerVideoStop(string peerId)
    {
        NavMeshPlayerController localPlayerTemp = LocalPlayer.GetComponent<NavMeshPlayerController>();
        if (localPlayerTemp.UserInfo.PeerId == peerId)
        {
            localPlayerTemp.StopVideo();
            return;
        }

        GameObject peerSection = null;
        Debug.Log($"OnPeerVideoStop : {peerId}");

        if (PeersMap.TryGetValue(peerId, out peerSection))
        {
            Debug.Log($"OnPeerMetaDataUpdated : {peerSection.name}");
            peerSection.GetComponent<NavMeshPlayerController>().StopVideo();
        }
        else
        {
            Debug.LogError("Peer not found");
        }
    }

    private void OnPeerVideoResume(string peerId)
    {
        NavMeshPlayerController localPlayerTemp = LocalPlayer.GetComponent<NavMeshPlayerController>();
        if (localPlayerTemp.UserInfo.PeerId == peerId)
        {
            localPlayerTemp.ResumeVideo();
            return;
        }

        GameObject peerSection = null;
        Debug.Log($"OnPeerVideoStop : {peerId}");

        if (PeersMap.TryGetValue(peerId, out peerSection))
        {
            Debug.Log($"OnPeerMetaDataUpdated : {peerSection.name}");
            peerSection.GetComponent<NavMeshPlayerController>().ResumeVideo();
        }
        else
        {
            Debug.LogError("Peer not found");
        }
    }


    private void OnMessageReceived(string data)
    {
        Debug.Log($"received data : {data}");

        MessageReceivedResponse response = JsonConvert.DeserializeObject<MessageReceivedResponse>(data);

        string fromPeerId = response.From;
        if (LocalPlayer.GetComponent<NavMeshPlayerController>().UserInfo.PeerId == fromPeerId) return;

        Vector3 goalPos = JsonConvert.DeserializeObject<Vector3>(response.Payload);

        //check for other peer
        GameObject peerSection = null;
        Debug.Log($"OnMessageReceived : {fromPeerId}");

        if (PeersMap.TryGetValue(fromPeerId, out peerSection))
        {
            Debug.Log($"OnPeerMetaDataUpdated : {peerSection.name}");
            peerSection.GetComponent<NavMeshPlayerController>().MoveToPosition(goalPos);
        }
        else
        {
            Debug.LogError("Peer not found");
        }


        //Move to position
    }

    #endregion

    #region Public function

    public void JoinRoom()
    {
        Debug.Log("Join Room Clicked");
        _roomId = _roomIdInputField.text;
        _token = _tokenInputField.text;
        _name = _nameInputFeild.text;
        Huddle01Core.Instance.JoinRoom(_roomId, _token);
    }

    public void UpdateLocalPeerMetaData(PeerMetadata peerMetadata)
    {
        Debug.Log($"UpdateLocalPeerMetaData : {JsonConvert.SerializeObject(peerMetadata)}");
        Huddle01Core.Instance.UpdateLocalPeerMetaData(JsonConvert.SerializeObject(peerMetadata));
    }

    public void MuteMic(bool shouldMute)
    {
        _selfMicMuteStatus = shouldMute;
        NavMeshPlayerController userSectionRef = LocalPlayer.GetComponent<NavMeshPlayerController>();
        Debug.Log($"Mute mic metadata : {JsonConvert.SerializeObject(userSectionRef.UserInfo.Metadata)}");
        userSectionRef.UserInfo.Metadata.MuteStatus = shouldMute;
        userSectionRef.UpdateMetadata(userSectionRef.UserInfo.Metadata);
        Huddle01Core.Instance.MuteMic(shouldMute, userSectionRef.UserInfo.Metadata);
    }


    public void EnableVideoStreaming(bool enableVideo)
    {
        _selfVideoEnabled = enableVideo;
        NavMeshPlayerController userSectionRef = LocalPlayer.GetComponent<NavMeshPlayerController>();
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
        OnRoomClosed();
    }

    #endregion

}
