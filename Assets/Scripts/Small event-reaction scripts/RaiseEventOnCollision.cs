using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STL2.Events;

public class RaiseEventOnCollision : MonoBehaviour
{
    [SerializeField]
    private string tagToCheckFor;
    [SerializeField]
    private VoidEvent eventToRaise;

    [SerializeField]
    private bool destroyObjectThatCollidesWithThis;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(tagToCheckFor))
        {
            eventToRaise.Raise();
            if (destroyObjectThatCollidesWithThis)
            {
                if (other.transform.GetComponent<Spear>() != null)
                {
                    foreach (Transform transform in other.transform.GetComponent<Spear>().childrenOnStart)
                    {
                        Destroy(transform.gameObject);
                    }
                }
            }

        }
    }
}
