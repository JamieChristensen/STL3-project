using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpearNetworked : NetworkBehaviour
{
    public Rigidbody rb;
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    public bool hitEnemy;

    [SerializeField]
    private float dropOffDistance = 50f;
    private float distanceTravelled;

    public PlayerNetworked playerController;

    public GameObject telegraphingSpear;

    private Vector3 previousVelocity;

    [SerializeField]
    private SubSpear[] subSpears; //Assign in inspector.
    private int activatedSubSpears;

    [SerializeField]
    private GameObject impactParticles, impactBloodParticles; //Assign in inspector.

    private List<Transform> impaledEnemies = new List<Transform>();

    [Header("Sounds:")]
    [SerializeField]
    private SoundPlayer spearHitWallSoundPlayer;
    [SerializeField]
    private SoundPlayer spearHitEnemySoundPlayer;

    public Transform[] childrenOnStart; //Used by "raiseEventOnCollision" for 

    public override void OnStartServer()
    {
        base.OnStartServer();

        //Do things necessary for when it starts on server.
    }

    void Start()
    {
        previousPosition = transform.position;
        previousRotation = transform.rotation;

        childrenOnStart = transform.GetComponentsInChildren<Transform>();
    }

    [Server]
    void Update()
    {
        if (startCounter)
        {
            counter -= Time.deltaTime;
        }
        if (counter < 0)
        {
            NetworkServer.Destroy(gameObject);
            Destroy(gameObject);
        }

        distanceTravelled += Vector3.Distance(transform.position, previousPosition);

        if (rb == null)
        {
            return;
        }
        if (distanceTravelled > dropOffDistance && !rb.useGravity)
        {
            rb.useGravity = true;
        }
    }

    [Server]
    void LateUpdate()
    {

        previousPosition = transform.position;
        previousRotation = transform.rotation;
        if (rb != null)
        {
            previousVelocity = rb.velocity;
        }
    }

    [Client]
    private void PlaySound(SoundPlayer soundPlayer)
    {
        soundPlayer.PlaySound();
    }

    [ClientRpc]
    void SpawnBloodParticles(Vector3 point, Transform parent)
    {
        GameObject go = Instantiate(impactBloodParticles, point, Quaternion.identity, parent);
        go.transform.position = point;
        //NetworkServer.Spawn(go);
    }


    public bool hitWall = false;
    bool startCounter = false;
    float counter = 2f;
    private void OnCollisionEnter(Collision other)
    {
        if (telegraphingSpear != null)
        {
            telegraphingSpear.SetActive(false);
        }
        if (other.transform.CompareTag("Player"))
        {
            if (!hitEnemy)
            {
                if (other.transform.GetComponent<PlayerNetworked>().isAlive)
                {
                    /*
                    if (!other.transform.GetComponent<PlayerNetworked>().isLocalPlayer && !isServer)
                    {

                    }
                    */
                    other.transform.GetComponent<PlayerNetworked>().Die(transform, previousVelocity);
                    SpawnBloodParticles(other.GetContact(0).point, other.transform);
                }



                //rb.angularVelocity = Vector3.zero;
                //rb.velocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.None;


                //transform.position = previousPosition;
                //transform.rotation = previousRotation;

                PlaySound(spearHitEnemySoundPlayer);
            }
            hitEnemy = true;

            GetComponent<Collider>().isTrigger = true;

            startCounter = true;
        }

        if (other.transform.CompareTag("Shield"))
        {
            if (hitEnemy)
            {
                return;
            }

            foreach (var trail in GetComponentsInChildren<TrailRenderer>())
            {
                trail.gameObject.SetActive(false);
            }

            GameObject impactParticle = Instantiate(impactParticles, other.GetContact(0).point, Quaternion.identity);
            impactParticle.transform.position = other.GetContact(0).point;
            NetworkServer.Spawn(impactParticle);


            rb.constraints = RigidbodyConstraints.None;
            rb.mass = 1;
            if (other.transform.GetComponent<Rigidbody>() == null)
            {
                var otherRb = other.gameObject.AddComponent<Rigidbody>();
                //otherRb.isKinematic = true;
            }
            FixedJoint joint = this.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = other.transform.GetComponent<Rigidbody>();

            spearHitWallSoundPlayer.PlaySound();
            hitEnemy = true;
            GetComponent<Collider>().enabled = false;

            startCounter = true;

        }

        if (other.transform.CompareTag("Wall"))
        {
            if (hitWall)
            {
                return;
            }
            if (hitEnemy)
            {
                return;
            }
            GameObject impactParticle = Instantiate(impactParticles, other.GetContact(0).point, Quaternion.identity);
            impactParticle.transform.position = other.GetContact(0).point;
            NetworkServer.Spawn(impactParticle);

            rb.constraints = RigidbodyConstraints.FreezeAll;

            rb.velocity = Vector3.zero;
            //transform.position = previousPosition;
            //transform.rotation = previousRotation;

            // Destroy(rb);

            foreach (Transform trans in impaledEnemies)
            {
                foreach (Transform child in trans.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
                    child.gameObject.tag = "DeadEnemy";
                }
                trans.gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
                trans.gameObject.tag = "DeadEnemy";

            }
            gameObject.layer = LayerMask.NameToLayer("Spear");

            /*
            if (playerController.isLocalPlayer && playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
            */

            spearHitWallSoundPlayer.PlaySound();
            hitWall = true;

            startCounter = true;
        }

        /*
        if (other.transform.CompareTag("Enemy"))
        {
            if (hitEnemy == false)
            {
                Destroy(Instantiate(impactBloodParticles, other.GetContact(0).point, Quaternion.identity, other.transform), 5);
                EnemyController controller = other.transform.GetComponent<EnemyController>();

                controller.OnSpearHit();
                controller.TurnOnRagdoll(previousVelocity, other.GetContact(0).point, rb);
                Debug.DrawRay(transform.position, rb.velocity, Color.red, 5f);
                Debug.DrawRay(transform.position, previousVelocity, Color.green, 5f);

                other.transform.tag = "ImpaledEnemy";
                other.transform.gameObject.layer = LayerMask.NameToLayer("ImpaledEnemy");
                other.transform.parent = transform;
                impaledEnemies.Add(other.transform);

                rb.velocity = previousVelocity;
                if (subSpears[activatedSubSpears] != null)
                {
                    subSpears[activatedSubSpears].gameObject.SetActive(true);
                    activatedSubSpears++;
                }
                GetComponent<Collider>().isTrigger = true;
                spearHitEnemySoundPlayer.PlaySound();
            }

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
        }
        */

    }

    public void OnSubSpearCollision(SubSpear subSpear, Collision other)
    {
        if (telegraphingSpear != null)
        {
            telegraphingSpear.SetActive(false);
        }

        if (other.transform.CompareTag("DeadEnemy"))
        {
            if (!subSpear.hitEnemy)
            {
                Destroy(Instantiate(impactBloodParticles, other.GetContact(0).point, Quaternion.identity, other.transform), 5);
                rb.angularVelocity = Vector3.zero;
                rb.velocity = Vector3.zero;

                rb.constraints = RigidbodyConstraints.FreezeAll;
                transform.position = previousPosition;
                transform.rotation = previousRotation;

                if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
                {
                    playerController.mostRecentlyThrownSpear = null;
                }
            }
            subSpear.hitEnemy = true;
            spearHitEnemySoundPlayer.PlaySound();
        }
        if (other.transform.CompareTag("Wall"))
        {
            Destroy(Instantiate(impactParticles, other.GetContact(0).point, Quaternion.identity), 5);


            rb.velocity = Vector3.zero;

            rb.constraints = RigidbodyConstraints.FreezeAll;
            transform.position = previousPosition;
            transform.rotation = previousRotation;

            subSpear.GetComponent<Collider>().enabled = false;

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }

            foreach (SubSpear sSpear in subSpears)
            {
                sSpear.gameObject.SetActive(false);
            }
            foreach (Transform trans in impaledEnemies)
            {
                foreach (Transform child in trans.GetComponentsInChildren<Transform>())
                {
                    if (child.gameObject.layer == LayerMask.NameToLayer("ImpaledEnemy"))
                    {
                        child.gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
                        child.gameObject.tag = "DeadEnemy";
                    }
                }
                trans.gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
                trans.gameObject.tag = "DeadEnemy";

            }

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
            subSpear.enabled = false;
            subSpear.GetComponent<Collider>().isTrigger = true;
            transform.DetachChildren();
            spearHitWallSoundPlayer.PlaySound();
        }

        if (other.transform.CompareTag("Enemy"))
        {
            if (subSpear.hitEnemy == false)
            {
                Destroy(Instantiate(impactBloodParticles, other.GetContact(0).point, Quaternion.identity, other.transform), 5);
                EnemyController controller = other.transform.GetComponent<EnemyController>();

                controller.OnSpearHit();
                controller.TurnOnRagdoll(previousVelocity, other.GetContact(0).point, subSpear.rb);
                Debug.DrawRay(transform.position, rb.velocity, Color.red, 5f);
                Debug.DrawRay(transform.position, previousVelocity, Color.green, 5f);

                other.transform.tag = "ImpaledEnemy";
                other.transform.gameObject.layer = LayerMask.NameToLayer("ImpaledEnemy");
                other.transform.parent = transform;
                impaledEnemies.Add(other.transform);
                hitEnemy = true;
                //rb.useGravity = true;
                rb.velocity = previousVelocity;


                if (activatedSubSpears < subSpears.Length)
                {
                    subSpears[activatedSubSpears].gameObject.SetActive(true);
                    activatedSubSpears++;
                }
                subSpear.GetComponent<Collider>().isTrigger = true;
                spearHitEnemySoundPlayer.PlaySound();
            }

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
        }
    }
}
