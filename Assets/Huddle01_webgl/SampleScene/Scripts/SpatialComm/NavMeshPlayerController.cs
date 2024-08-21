using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Huddle01;
using Object = UnityEngine.Object;

public class NavMeshPlayerController : MonoBehaviour
{

    private HuddleUserInfo _userInfo;
    public HuddleUserInfo UserInfo => _userInfo;


    [SerializeField]
    private NavMeshAgent _agent;

    [HideInInspector]
    public bool IsLocalPlayer = false;

    [Header("Video Comp")]
    [SerializeField]
    private TMP_Text _nameText;
    [SerializeField]
    private RawImage _videoTexture;

    [Header("Audio Comp")]
    [SerializeField]
    private GameObject _mutedIcon;

    [SerializeField]
    private GameObject _unmutedIcon;

    public Texture2D Texture { get; private set; }

    public bool isVideoPlaying = false;

    private int m_TextureId = 1;

    [HideInInspector]
    public bool IsSpatialComm = false;

    [SerializeField]
    private Texture2D _defaultVideoTexture;


    // Start is called before the first frame update
    void Start()
    {
        GetNewTextureId();
    }

    public void Setup(HuddleUserInfo userInfo)
    {
        _userInfo = userInfo;
    }

    // Update is called once per frame
    void Update()
    {
        if (isVideoPlaying)
        {
            SetupTexture();
        }

        if (IsSpatialComm) 
        {
            if (IsLocalPlayer)
            {
                SetLocalPlayerPositionForSpatialComm(transform.position);
                SetLocalPlayerRotationForSpatialComm(transform.forward);
            }
            else 
            {
                SetPositonForSpatialComm(_userInfo.PeerId, transform.position);
                SetRotationForSpatialComm(_userInfo.PeerId, transform.forward);
            }
        }
    }

    public void MoveToPosition(Vector3 goalPos) 
    {
        _agent.destination = goalPos;
        if (IsLocalPlayer) 
        {
            string posJson = JsonUtility.ToJson(goalPos);
            Huddle01Core.Instance.SendData("*",posJson,"chat");
        }
    }


    #region Video

    public void GetNewTextureId()
    {
        m_TextureId = Huddle01JSNative.NewTexture();
    }

    public void SetupTexture()
    {
        if (Texture != null)
            Object.Destroy(Texture);
        Texture = Texture2D.CreateExternalTexture(1280, 720, TextureFormat.RGBA32, false, true, (IntPtr)m_TextureId);
        _videoTexture.texture = Texture;
    }

    public void AttachVideo()
    {
        Huddle01JSNative.AttachVideo(_userInfo.PeerId, m_TextureId);
    }

    public void StopVideo()
    {
        isVideoPlaying = false;
        //_videoTexture.gameObject.SetActive(false);
        _videoTexture.texture = _defaultVideoTexture;
    }

    public void ResumeVideo()
    {
        _videoTexture.gameObject.SetActive(true);
        Huddle01JSNative.AttachVideo(_userInfo.PeerId, m_TextureId);
        isVideoPlaying = true;
    }

    private void SetPositonForSpatialComm(string peerId,Vector3 pos) 
    {
        Huddle01Core.Instance.SetUpdatedPositionForSpatialComm(peerId, pos);
    }

    private void SetRotationForSpatialComm(string peerId, Vector3 rot) 
    {
        Huddle01Core.Instance.SetUpdatedRotationForSpatialComm(peerId, rot);
    }

    private void SetLocalPlayerPositionForSpatialComm(Vector3 pos) 
    {
        Huddle01Core.Instance.SetLocalPlayerUpdatedPositionForSpatialComm(pos);
    }

    private void SetLocalPlayerRotationForSpatialComm(Vector3 rot)
    {
        Huddle01Core.Instance.SetLocalPlayerUpdatedRotationForSpatialComm(rot);
    }

    #endregion

    #region Audio

    public void MuteMic() 
    {
        SetMuteIcon(true);
    }

    public void ChangeMuteMicStatus(bool muted) 
    {
        SetMuteIcon(muted);
    }

    private void SetMuteIcon(bool muted)
    {
        _mutedIcon.SetActive(muted);
        _unmutedIcon.SetActive(!muted);
    }

    #endregion

    #region Metadata

    public void UpdateMetadata(PeerMetadata peerMetaData) 
    {
        _userInfo.Metadata = peerMetaData;
        _nameText.text = _userInfo.Metadata.Name;
        SetMuteIcon(_userInfo.Metadata.MuteStatus);
    }

    #endregion

    private float ConvertToOneDecimal(float val)
    {
        return Mathf.Round(val * 10f) / 10f;
    }

}
