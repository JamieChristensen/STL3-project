using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateObject : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToSpawn;

    [SerializeField]
    private bool destroyAfterDuration;

    [SerializeField]
    private float destroyTime;



    public void InstantiateObjAtThisPosition()
    {
        GameObject go = Instantiate(objectToSpawn, transform.position, Quaternion.identity);

        if (destroyAfterDuration)
        {
            Destroy(go, destroyTime);
        }
    }
}
