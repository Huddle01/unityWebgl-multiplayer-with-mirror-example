using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Huddle01 
{
    public class Huddle01JSNative
    {
        public delegate void JSDelegate(string message);

        [DllImport("__Internal")]
        internal static extern void StartCamera(string videoId);

        [DllImport("__Internal")]
        internal static extern int NewTexture();

        [DllImport("__Internal")]
        internal static extern void AttachVideo(string peerId, int m_TextureId);

        [DllImport("__Internal")]
        internal static extern void InitHuddle01WebSdk(string appIdjson);

        [DllImport("__Internal")]
        internal static extern void JoinRoom(string roomId, string token);

        [DllImport("__Internal")]
        internal static extern void LeaveRoom();

        [DllImport("__Internal")]
        internal static extern void MuteMic(bool shouldMute, string metaData);

        [DllImport("__Internal")]
        internal static extern void EnableVideo(bool enable, string metaData);

        [DllImport("__Internal")]
        internal static extern void SendTextMessage(string message);

        [DllImport("__Internal")]
        internal static extern void ConsumePeer(string peerId);

        [DllImport("__Internal")]
        internal static extern void UpdatePeerMeataData(string metadataJson);

        [DllImport("__Internal")]
        internal static extern void GetLocalPeerId();

        [DllImport("__Internal")]
        internal static extern string GetRemotePeerMetaData(string peerId);

        [DllImport("__Internal")]
        internal static extern string SetUpForSpatialCommForPeer(string peerId);

        [DllImport("__Internal")]
        internal static extern string SetUpForSpatialComm();

        [DllImport("__Internal")]
        internal static extern string UpdateListenerPosition(float PosX, float PosY, float PosZ);

        [DllImport("__Internal")]
        internal static extern string UpdateListenerRotation(float RotX, float RotY, float RotZ);

        [DllImport("__Internal")]
        internal static extern string UpdatePeerPosition(string peerId, float PosX, float PosY, float PosZ);

        [DllImport("__Internal")]
        internal static extern string UpdatePeerRotation(string peerId, float RotX, float RotY, float RotZ);

        [DllImport("__Internal")]
        internal static extern string DisconnectPeerPanner(string peerId);

    }
}