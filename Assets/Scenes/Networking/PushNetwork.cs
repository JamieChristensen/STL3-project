using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PushNetwork : MonoBehaviour
{
    [SerializeField]
    private float radius;
    [SerializeField]
    private float applyExplosionForce;
    [SerializeField]
    private float force, upwardsModifier;

    [SerializeField]
    private LayerMask layerMask;

    [Header("Gizmo-settings:")]
    [SerializeField]
    private bool drawImpactAreaGizmo;

    public float timer = 5;

    public void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (drawImpactAreaGizmo)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawSphere(transform.position, radius);
        }
    }

    [Server]
    public void Start()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask);

        foreach (Collider other in colliders)
        {
            if (other.transform.root.tag == "Player")
            {
                return;
            }
            if (other.GetComponent<Rigidbody>() != null)
            {
                var rb = other.GetComponent<Rigidbody>();
                rb.AddExplosionForce(force, transform.position, radius, upwardsModifier, ForceMode.VelocityChange);
            }
            Debug.Log("PUSHING!");
        }
    }
}
