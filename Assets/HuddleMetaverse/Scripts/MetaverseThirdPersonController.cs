using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;
using TMPro;
using Mirror;
using UnityEngine.Networking;
using Huddle01;

public class MetaverseThirdPersonController : vThirdPersonController
{

    private HuddleUserInfo _userInfo = new HuddleUserInfo();
    public HuddleUserInfo UserInfo => _userInfo;

    [Header("- Name Section")]
    public TMP_Text NameText;

    [Header("- Video Section")]
    public VideoSection VideoSectionRef;

    [Space(10)]
    [SerializeField]
    private MeshRenderer audioStatusHolder;
    [SerializeField]
    private Material _muteMaterial;
    [SerializeField]
    private Material _unmuteMaterial;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string PlayerName;

    [SyncVar(hook = nameof(OnVideoMaterialChanged))]
    public int VideoMateiralId;

    [Header("- Player Data container")]
    public LocalPlayerData LocalPlayerDataContainer;

    [SyncVar(hook = nameof(OnHuddleTokenChanged))]
    public string HuddleToken = null;

    private bool _isHuddleInit = false;

    public KeyCode VideoMaterial01 = KeyCode.Alpha1;
    public KeyCode VideoMaterial02 = KeyCode.Alpha2;
    public KeyCode VideoMaterial03 = KeyCode.Alpha3;


    private void Start()
    {
        // call for all players to regsiter them in a dic
        NetworkManager.AllPlayersMap.Add(netId.ToString(),this.gameObject);
        Debug.Log($"Adding player, count now {NetworkManager.AllPlayersMap.Count}");
    }
    
    [Client]
    private void Update()
    {
        if (isLocalPlayer) 
        {
            if (Input.GetKeyDown(VideoMaterial01))
            {
                CmdSetVideoMaterial(0);
            }

            if (Input.GetKeyDown(VideoMaterial02))
            {
                CmdSetVideoMaterial(1);
            }

            if (Input.GetKeyDown(VideoMaterial03))
            {
                CmdSetVideoMaterial(2);
            }
        }
    }

    [Client]
    private void OnDestroy()
    {
        NetworkManager.AllPlayersMap.Remove(netId.ToString());
    }

    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer) 
        {
            MetaverseHuddleCommManager.Instance.LocalPlayer = this.gameObject;
            LocalPlayerDataContainer.PlayerName = "Player" + Random.Range(100, 999);
            CmdSetupPlayer(LocalPlayerDataContainer.PlayerName);
            CmdSetHuddleToken(Constants.HuddleRoomId, Constants.HuddleApiKey);
        }
        
    }

    void OnNameChanged(string oldName, string newName)
    {
        NameText.text = newName;
    }

    void OnVideoMaterialChanged(int oldMaterial, int newMaterial)
    {
        VideoSectionRef.SetMaterial(newMaterial);
    }

    void OnHuddleTokenChanged(string oldToken, string newToken)
    {
        Debug.Log($"Huddle token : {newToken}");
        HuddleToken = newToken;
        if (isLocalPlayer) 
        {
            Huddle01Init.Instance.JoinRoom(Constants.HuddleRoomId, HuddleToken);
        }
        
    }

    [Command]
    public void CmdSetupPlayer(string name)
    {
        // player info sent to server, then server updates sync vars which handles it on all clients
        PlayerName = name;
    }

    [Command]
    public void CmdSetVideoMaterial(int videoMat)
    {
        // player info sent to server, then server updates sync vars which handles it on all clients
        VideoMateiralId = videoMat;
    }

    [Command]
    public void CmdSetHuddleToken(string roomId,string apiKey)
    {
        StartCoroutine(GetAndSetHuddleToken(roomId,apiKey));
    }

    IEnumerator GetAndSetHuddleToken(string roomId, string apiKey) 
    {
        string apiUrl = Constants.HuddleGetTokenUrl + "apiKey=" + apiKey + "&role=guest&roomId=" + roomId;
        Debug.Log(apiUrl);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                Debug.Log(webRequest.downloadHandler.text);
                HuddleToken = webRequest.downloadHandler.text;
            }
        }
    }

    public void UpdateMetadata(PeerMetadata peerMetaData)
    {
        _userInfo.Metadata = peerMetaData;
        _userInfo.Metadata.NetworkId = netId.ToString();
        SetMuteIcon(_userInfo.Metadata.MuteStatus);
    }

    private void SetMuteIcon(bool muted)
    {
        if (muted)
        {
            audioStatusHolder.material = _muteMaterial;
        }
        else 
        {
            audioStatusHolder.material = _unmuteMaterial;
        }
    }

    public void ChangeMuteMicStatus(bool muted)
    {
        SetMuteIcon(muted);
    }

}
