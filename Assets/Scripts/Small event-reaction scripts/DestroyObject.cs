using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [SerializeField]
    private float delay;

    [SerializeField]
    GameObject objectToDestroy;
    
    public void DestroyReferencedObject()
    {
        if (objectToDestroy == null)
        {
            Debug.Log("No assigned object to destroy on: " + gameObject.name + " please assign an object.");
            return;
        }
        Destroy(objectToDestroy, delay);
    }
}
