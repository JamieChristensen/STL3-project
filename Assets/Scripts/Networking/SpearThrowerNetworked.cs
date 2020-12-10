using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class SpearThrowerNetworked : NetworkBehaviour
{

    public GameObject spearPrefab;

    public GameObject visualSpear;

    public float spearSpeed = 60;

    public float countInterval = 1f;
    private float counter = 0;

    [Server]
    void Update()
    {
        if (isServer)
        {
            counter += Time.deltaTime;
            if (counter > countInterval)
            {
                counter = 0;
                ThrowSpear(spearSpeed);
                visualSpear.SetActive(false);
            }

            if (counter > countInterval / 2)
            {
                visualSpear.SetActive(true);
            }
        }
    }

    [Server]
    private void ThrowSpear(float spearSpeed)
    {
        //spearThrowSoundPlayer.PlaySound();
        CmdThrowSpear(spearSpeed);
    }

    [Server]
    private void CmdThrowSpear(float spearSpeed)
    {
        GameObject spearObj = Instantiate(spearPrefab, visualSpear.transform.position, visualSpear.transform.rotation);

        SpearNetworked spear = spearObj.GetComponentInChildren<SpearNetworked>();

        var direction = transform.forward;

        spear.rb.AddForce((direction).normalized * spearSpeed, ForceMode.VelocityChange);

        NetworkServer.Spawn(spearObj);
    }
}
