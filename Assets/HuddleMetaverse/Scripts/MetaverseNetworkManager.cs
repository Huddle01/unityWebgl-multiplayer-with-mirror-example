using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MetaverseNetworkManager : NetworkManager
{
    //unity.huddle01.media
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log("Instantiating new player");
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        //changing name of gameobject for easy debug
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
