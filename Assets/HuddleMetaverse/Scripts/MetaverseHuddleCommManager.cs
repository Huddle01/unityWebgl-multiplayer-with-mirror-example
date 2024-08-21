using Huddle01;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Threading.Tasks;

public class MetaverseHuddleCommManager : MonoBehaviour
{
    public static MetaverseHuddleCommManager Instance;

    public Dictionary<string, GameObject> PeersMap = new Dictionary<string, GameObject>();

    [HideInInspector]
    public GameObject LocalPlayer;

    public KeyCode MuteToggle = KeyCode.M;
    public KeyCode VideoToggle = KeyCode.V;

    public GameObject LoadingImage;

    private bool _selfMicMuteStatus = true;
    private bool _selfVideoEnabled = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Huddle01Core.Instance.Init(Constants.HuddldeProjectId);
    }

    private void Update()
    {
        if (Input.GetKeyDown(MuteToggle))
        {
            OnMuteMicClicked();
        }

        if (Input.GetKeyDown(VideoToggle))
        {
            EnableVideo();
        }
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

    private void OnPeerMuteStatusChanged(string peerId, bool isMuted)
    {
        GameObject peerSection = null;
        Debug.Log($"OnPeerMuteStatusChanged : {peerId}");

        if (PeersMap.TryGetValue(peerId, out peerSection))
        {
            Debug.Log($"OnPeerMetaDataUpdated : {peerSection.name}");
            MetaverseThirdPersonController remotePlayer = peerSection.GetComponent<MetaverseThirdPersonController>();
            remotePlayer.ChangeMuteMicStatus(isMuted);
        }
    }

    private void OnMessageReceived(string data)
    {

    }

    private void OnPeerVideoStop(string peerId)
    {
        Debug.Log($"Stop video of : {peerId}");

        MetaverseThirdPersonController localPLayer = LocalPlayer.GetComponent<MetaverseThirdPersonController>();

        if (localPLayer.UserInfo.PeerId == peerId)
        {
            localPLayer.VideoSectionRef.EnableVideo(false, peerId);
            return;
        }

        GameObject peerAssoObj = null;
        if (PeersMap.TryGetValue(peerId, out peerAssoObj))
        {
            Debug.Log($"Got object {peerAssoObj.name}");
            MetaverseThirdPersonController playerController = peerAssoObj.GetComponent<MetaverseThirdPersonController>();
            playerController.VideoSectionRef.EnableVideo(false,peerId);
        }
    }

    private void OnPeerVideoResume(string peerId)
    {
        Debug.Log($"Resume video of : {peerId}");

        MetaverseThirdPersonController localPLayer = LocalPlayer.GetComponent<MetaverseThirdPersonController>();

        if (localPLayer.UserInfo.PeerId == peerId)
        {
            localPLayer.VideoSectionRef.EnableVideo(true, peerId);
            return;
        }

        GameObject peerAssoObj = null;

        foreach (var item in PeersMap)
        {
            Debug.Log(item.Key);
        }

        if (PeersMap.TryGetValue(peerId, out peerAssoObj))
        {
            Debug.Log($"Got object {peerAssoObj.name}");
            MetaverseThirdPersonController playerController = peerAssoObj.GetComponent<MetaverseThirdPersonController>();
            playerController.VideoSectionRef.EnableVideo(true,peerId);
        }
    }

    private void OnPeerMetaDataUpdated(PeerMetadata peerInfo)
    {
        if (LocalPlayer.GetComponent<MetaverseThirdPersonController>().UserInfo.PeerId == peerInfo.PeerId)
        {
            return;
        }

        //check for other peer
        GameObject peerAssoObj = null;
        if (NetworkManager.AllPlayersMap.ContainsKey(peerInfo.NetworkId))
        {
            peerAssoObj = NetworkManager.AllPlayersMap[peerInfo.NetworkId];
            MetaverseThirdPersonController playerController = peerAssoObj.GetComponent<MetaverseThirdPersonController>();
            playerController.UpdateMetadata(peerInfo);
        }
        
    }

    private void OnRoomClosed()
    {
        PeersMap.Clear();
    }

    private void OnPeerLeft(string peerInfo)
    {
        if (PeersMap.ContainsKey(peerInfo))
        {
            PeersMap.Remove(peerInfo);
        }
    }

    private void OnPeerJoined(string peerId)
    {
        StartCoroutine(UpdateNewlyJoinedPeerMetadata(peerId));
        
    }

    private void OnLocalPeerIdReceived(string peerId)
    {
        MetaverseThirdPersonController localPlayerTemp = LocalPlayer.GetComponent<MetaverseThirdPersonController>();
        localPlayerTemp.UserInfo.PeerId = peerId;
        
        localPlayerTemp.UserInfo.Metadata.Name = localPlayerTemp.PlayerName;
        localPlayerTemp.UserInfo.Metadata.MuteStatus = true;
        localPlayerTemp.UserInfo.Metadata.VideoStatus = false;
        localPlayerTemp.UserInfo.Metadata.PeerId = peerId;
        localPlayerTemp.UpdateMetadata(localPlayerTemp.UserInfo.Metadata);
        UpdateLocalPeerMetaData(new PeerMetadata {Name = localPlayerTemp.PlayerName,MuteStatus = true,PeerId = peerId,VideoStatus = false,
                                NetworkId = localPlayerTemp.netIdentity.netId.ToString()});
        
    }

    private void OnJoinRoom()
    {
        Debug.Log($"LocalPLayer val {LocalPlayer==null}");
        MetaverseThirdPersonController playerController = LocalPlayer.GetComponent<MetaverseThirdPersonController>();
        Huddle01Core.Instance.GetLocalPeerId();
        LoadingImage.SetActive(false);
    }


    //Main Methods
    public void UpdateLocalPeerMetaData(PeerMetadata peerMetadata)
    {
        Debug.Log($"UpdateLocalPeerMetaData : {JsonConvert.SerializeObject(peerMetadata)}");
        Huddle01Core.Instance.UpdateLocalPeerMetaData(JsonConvert.SerializeObject(peerMetadata));
    }

    public void JoinRoom()
    {
        Huddle01Core.Instance.JoinRoom(Constants.HuddleRoomId, LocalPlayer.GetComponent<MetaverseThirdPersonController>().HuddleToken);
    }

    public void MuteMic(bool shouldMute)
    {
        _selfMicMuteStatus = shouldMute;
        MetaverseThirdPersonController userSectionRef = LocalPlayer.GetComponent<MetaverseThirdPersonController>();
        userSectionRef.UserInfo.Metadata.MuteStatus = shouldMute;
        userSectionRef.UpdateMetadata(userSectionRef.UserInfo.Metadata);
        Huddle01Core.Instance.MuteMic(shouldMute, userSectionRef.UserInfo.Metadata);
    }


    public void EnableVideoStreaming(bool enableVideo)
    {
        _selfVideoEnabled = enableVideo;
        MetaverseThirdPersonController userSectionRef = LocalPlayer.GetComponent<MetaverseThirdPersonController>();
        userSectionRef.UserInfo.Metadata.VideoStatus = enableVideo;
        userSectionRef.UpdateMetadata(userSectionRef.UserInfo.Metadata);
        Huddle01JSNative.EnableVideo(enableVideo, JsonConvert.SerializeObject(userSectionRef.UserInfo.Metadata));
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

    IEnumerator UpdateNewlyJoinedPeerMetadata(string peerId) 
    {
        yield return new WaitForSeconds(2);

        PeerMetadata peerMetadata = JsonConvert.DeserializeObject<PeerMetadata>(Huddle01JSNative.GetRemotePeerMetaData(peerId));
        Debug.Log($"OnPeerJoined {Huddle01JSNative.GetRemotePeerMetaData(peerId)}");
        GameObject peerAssoObj = null;

        if (NetworkManager.AllPlayersMap.ContainsKey(peerMetadata.NetworkId))
        {
            Debug.Log($"Adding peerid in map {peerId}");
            peerAssoObj = NetworkManager.AllPlayersMap[peerMetadata.NetworkId];
            PeersMap.Add(peerId, peerAssoObj);
            MetaverseThirdPersonController playerController = peerAssoObj.GetComponent<MetaverseThirdPersonController>();
            playerController.UserInfo.Metadata = peerMetadata;
            playerController.UserInfo.PeerId = peerId;
            playerController.UserInfo.IsRemotePeer = true;
            playerController.UserInfo.Role = "guest";

            playerController.UpdateMetadata(peerMetadata);
        }
        else
        {
            Debug.Log($"OnPeerJoined cant find {peerId}");
        }
    }

}
