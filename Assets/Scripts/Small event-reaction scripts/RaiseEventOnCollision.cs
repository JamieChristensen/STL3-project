using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STL2.Events;
using Mirror;

public class RaiseEventOnCollision : MonoBehaviour
{
    [SerializeField]
    private string tagToCheckFor;
    [SerializeField]
    private VoidEvent eventToRaise;

    [SerializeField]
    private bool destroyObjectThatCollidesWithThis;

    [Server]
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(tagToCheckFor))
        {
            if (other.transform.GetComponent<SpearNetworked>().hitEnemy)
            {
                return;
            }
            eventToRaise.Raise();
            if (destroyObjectThatCollidesWithThis)
            {
                if (other.transform.GetComponent<SpearNetworked>() != null)
                {

                    foreach (Transform transform in other.transform.GetComponent<SpearNetworked>().childrenOnStart)
                    {
                        NetworkManager.Destroy(transform.gameObject);
                    }
                }
            }

        }
    }
}
