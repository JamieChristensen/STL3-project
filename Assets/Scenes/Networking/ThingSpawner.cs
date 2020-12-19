using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using STL2.Events;

public class ThingSpawner : NetworkBehaviour
{
    public Rigidbody thingPrefab;

    public GameObject instantiatedThing;



    public Vector3 spawnVel = new Vector3(0, 9, 0);
    public float velocityMultiplier;



    [Server]
    void Update()
    {
        if (isServer)
        {
            if (instantiatedThing == null)
            {
                SpawnThing();
            }
        }
    }

    [Server]
    void SpawnThing()
    {
        GameObject instance = Instantiate(thingPrefab.gameObject, this.transform.position, this.transform.rotation);

        var rb = instance.GetComponent<Rigidbody>();

        var direction = Vector3.up;

        var rando = Random.insideUnitSphere;
        rando = new Vector3(Mathf.Abs(rando.x), Mathf.Abs(rando.y), Mathf.Abs(rando.z));
        rb.AddForce((spawnVel + rando) * velocityMultiplier, ForceMode.VelocityChange);

        NetworkServer.Spawn(instance);
        instantiatedThing = instance;
    }
}