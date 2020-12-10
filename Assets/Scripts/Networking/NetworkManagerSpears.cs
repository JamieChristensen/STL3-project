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
        if (spawnPositions[0] == null)
        {
            spawnPositions[0] = GameObject.Find("Pos 1").transform;
        }
        if (spawnPositions[1] == null)
        {
            spawnPositions[1] = GameObject.Find("Pos 2").transform;
        }


        // add player at correct spawn position
        GameObject player = Instantiate(playerPrefab, spawnPositions[nextIndex].position, Quaternion.identity);
        nextIndex = (nextIndex + 1) % spawnPositions.Length;

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        
        foreach (PlayerNetworked player in FindObjectsOfType<PlayerNetworked>())
        {
            player.ReadyPlayers();
        }
        
    }

}
