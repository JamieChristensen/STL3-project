using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private GameObject particleExplosionPrefab;


    private void OnCollisionEnter(Collision other)
    {
        Transform particleTransform = Instantiate(particleExplosionPrefab, transform.position, Quaternion.identity).GetComponent<Transform>();
        particleTransform.rotation = transform.rotation;


        if (other.transform.CompareTag("Player"))
        {
            PlayerController pc = other.transform.GetComponent<PlayerController>();
            pc.Die();
            pc.rb.AddExplosionForce(10f, transform.position, 3f, 0.5f, ForceMode.Impulse);
        }

        if (other.transform.CompareTag("Enemy"))
        {
            EnemyController controller = other.transform.GetComponent<EnemyController>();
            controller.isDead = true;
            controller.navMeshAgent.isStopped = true;
            controller.TurnOnRagdoll();

            other.transform.tag = "DeadEnemy";
        }

        Destroy(particleTransform.gameObject, 5f); //Let particles fully dissolve before removing the obj.
        Destroy(this.gameObject);
    }
}
