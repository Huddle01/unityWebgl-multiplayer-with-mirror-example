using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Huddle01;
using Object = UnityEngine.Object;
using System;

public class VideoSection : MonoBehaviour
{

    private int m_TextureId = -1;

    public Texture2D Texture { get; private set; }

    public int CurrentSelectedMaterial = 0;

    [Header("Materials")]
    [SerializeField]
    private Material _normalMaterial;
    [SerializeField]
    private Material _blackAndWhiteMaterial;
    [SerializeField]
    private Material _hologramMaterial;


    [Header("Video Screen")]
    [SerializeField]
    private GameObject _videoScreenHolder;
    [SerializeField]
    private GameObject _videoScreen01;
    [SerializeField]
    private GameObject _videoScreen02;

    public bool isVideoPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL
        GetNewTextureId();
        
#endif

    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_WEBGL
        if (isVideoPlaying)
        {
            SetupTexture();
        }
#endif
    }

    public void SetMaterial(int matId) 
    {
        CurrentSelectedMaterial = matId;
        switch (matId)
        {
            case 0:

                _videoScreen01.GetComponent<MeshRenderer>().material = _normalMaterial;
                _videoScreen02.GetComponent<MeshRenderer>().material = _normalMaterial;

                break;

            case 1:
                _videoScreen01.GetComponent<MeshRenderer>().material = _blackAndWhiteMaterial;
                _videoScreen02.GetComponent<MeshRenderer>().material = _blackAndWhiteMaterial;
                break;

            case 2:
                _videoScreen01.GetComponent<MeshRenderer>().material = _hologramMaterial;
                _videoScreen02.GetComponent<MeshRenderer>().material = _hologramMaterial;
                break;
        }
    }

    public void GetNewTextureId()
    {
#if UNITY_WEBGL
        m_TextureId = Huddle01JSNative.NewTexture();
#endif
    }

    public void SetupTexture()
    {
        if (Texture != null)
            Object.Destroy(Texture);
        Texture = Texture2D.CreateExternalTexture(1280, 720, TextureFormat.RGBA32, false, true, (IntPtr)m_TextureId);

        _videoScreen01.GetComponent<MeshRenderer>().material.SetTexture("_MainTex",Texture);
        _videoScreen02.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", Texture);
    }

    public void EnableVideo(bool enable,string peerId) 
    {
        #if UNITY_WEBGL
        isVideoPlaying = enable;
        _videoScreenHolder.SetActive(enable);
        if (enable)
        {
            Huddle01JSNative.AttachVideo(peerId, m_TextureId);
        }

        #endif
    }

    private void CloneMaterial() 
    {
        
    }
}
