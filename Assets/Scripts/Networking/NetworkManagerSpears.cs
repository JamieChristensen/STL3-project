using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


[AddComponentMenu("")]
public class NetworkManagerSpears : NetworkManager
{
    public Transform[] spawnPositions = new Transform[2];
    private int nextIndex = 0;

    

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // add player at correct spawn position
        GameObject player = Instantiate(playerPrefab, spawnPositions[nextIndex].position, Quaternion.identity);
        nextIndex = (nextIndex + 1) % spawnPositions.Length;

        NetworkServer.AddPlayerForConnection(conn, player);

    }

}
