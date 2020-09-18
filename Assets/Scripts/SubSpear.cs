using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubSpear : MonoBehaviour
{
    [SerializeField]
    private Spear parentSpear; //Assign in inspector for good measure.

    public Rigidbody rb;
    
    [HideInInspector]
    public bool hitEnemy = false;

    private void OnCollisionEnter(Collision other)
    {
        
        parentSpear.OnSubSpearCollision(this, other);

        Debug.Log(other.gameObject.name + " Collided with this on subspear!");
        //rb.velocity = Vector3.zero;
        //Destroy(rb);
        //Destroy(GetComponent<BoxCollider>());
        GetComponent<BoxCollider>().isTrigger = true;
    }
}
