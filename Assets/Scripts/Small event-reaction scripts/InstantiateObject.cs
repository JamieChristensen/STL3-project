using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InstantiateObject : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToSpawn;

    [SerializeField]
    private bool destroyAfterDuration;

    [SerializeField]
    private float destroyTime;


    [Server]
    public void InstantiateObjAtThisPosition()
    {
        GameObject go = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
        NetworkServer.Spawn(go);
        if (destroyAfterDuration)
        {
            Destroy(go, destroyTime);
        }
    }
}
