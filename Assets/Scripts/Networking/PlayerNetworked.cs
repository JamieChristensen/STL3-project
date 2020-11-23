using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using STL2.Events;
using Mirror;


public class PlayerNetworked : NetworkBehaviour
{
    [SerializeField]
    private CharacterController controller;     //Assign in inspector

    public Rigidbody rb;    //Assign in inspector

    [SerializeField]
    private Camera cam;     //Assign in inspector

    private float horizontalInput, verticalInput, shootInput;

    [SerializeField]
    private KeyCode bulletTimeKey, redirectSpearKey;

    [Header("Other")]
    [Tooltip("The layers that spear-targetting is impacted by, best as ground only, or ground+enemies")]
    [SerializeField]
    private LayerMask spearCollisionLayers;

    [SerializeField]
    private float moveSpeed;

    [Range(0, 20)]
    [SerializeField]
    private float controllerGravity;
    private float rayMaxDist = 500f;
    private bool isDead;

    [SerializeField]
    private Collider[] collidersToActivateOnDeath;



    [Header("Spear")]
    [SerializeField]
    private float spearSpeed;
    [SerializeField]
    private GameObject spearPrefab;
    [SerializeField]
    private GameObject spearInHand;
    private float reloadTimer;
    [SerializeField]
    private float reloadTime;
    private bool readyToShoot;

    [HideInInspector]
    public SpearNetworked mostRecentlyThrownSpear;   //Names are hard. - public so spear can un-recent itself when colliding with walls/enemies.
    private bool isRedirectingSpear;
    private bool doRedirectSpear;
    [SerializeField]
    private LineRenderer telegraphingLineRenderer;
    [SerializeField]
    private float maxDistanceTelegraph;
    [SerializeField]
    private LayerMask telegraphLayerMask;

    [HideInInspector]
    public Vector3 mousePos;    //Used by camera

    [Header("Sound effects")]
    [SerializeField]
    private SoundPlayer deathSoundPlayer;
    [SerializeField]
    private SoundPlayer spearThrowSoundPlayer;
    [SerializeField]
    private SoundPlayer spearRedirectSoundPlayer;

    [Header("Networking")]
    private NetworkManager networkManager;

    private void Awake()
    {
        if (cam == null)
        {
            cam = GameObject.FindObjectOfType<Camera>();
        }


    }
    private void Start()
    {
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }
    }

    private void Update()
    {
        if (!hasAuthority)
        {
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        shootInput = Input.GetAxisRaw("Fire1");
        bool bulletTimeOn = Input.GetKey(bulletTimeKey);
        bool pressedRedirectSpear = Input.GetKeyDown(redirectSpearKey);

        if (shootInput > 0.2f && readyToShoot)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayMaxDist, spearCollisionLayers))
            {
                Vector3 target;
                if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("DeadEnemy"))
                {
                    target = hit.collider.transform.position;
                }
                else
                {
                    target = new Vector3(hit.point.x, spearInHand.transform.position.y, hit.point.z);
                }
                ThrowSpear(target, spearSpeed);
                readyToShoot = false;
                reloadTimer = 0;

            }
        }

        if (pressedRedirectSpear || isRedirectingSpear)
        {
            isRedirectingSpear = true;
            bulletTimeOn = true;
            if (mostRecentlyThrownSpear != null)
            {
                mostRecentlyThrownSpear.telegraphingSpear.SetActive(true);
            }
            else
            {
                isRedirectingSpear = false;
            }
        }

        if (isRedirectingSpear)
        {
            if (Input.GetKeyUp(redirectSpearKey))
            {
                //Calculate new velocity in fixedUpdate:
                doRedirectSpear = true;
            }
        }

        if (bulletTimeOn)
        {
            Time.timeScale = 0.33f;          //Half speed.
            //Fixed delta-time is taken care of in CameraController.
        }
        else
        {
            Time.timeScale = 1f;
        }

        //Timers:
        if (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime;
        }
        else
        {
            readyToShoot = true;
            spearInHand.SetActive(true);
        }
    }

    [Client]
    void PlayerMovement()
    {
        if (!hasAuthority)
        {
            return;
        }
        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput).normalized;


        CmdMove(direction);
    }

    [Command]
    void CmdMove(Vector3 direction)
    {

        RpcMove(direction);
    }

    [ClientRpc]
    void RpcMove(Vector3 direction)
    {
        controller.Move(direction * moveSpeed * Time.fixedDeltaTime);
        controller.Move(Vector3.down * controllerGravity * Time.fixedDeltaTime);
    }

    [Client]
    void PlayerLookRotationAndRedirectSpear()
    {
        if (!hasAuthority)
        {
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayMaxDist, spearCollisionLayers))
        {
            //Debug.DrawLine(ray.origin, hit.point, Color.white);
            Vector3 lookPos = hit.point;
            mousePos = lookPos;
            transform.LookAt(lookPos);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z); //Prevents looking down.

            Vector3 pos0 = telegraphingLineRenderer.transform.position;
            RaycastHit hit2;
            Vector3 pos1 = telegraphingLineRenderer.transform.position;
            if (Physics.Raycast(telegraphingLineRenderer.transform.position, telegraphingLineRenderer.transform.up, out hit2, maxDistanceTelegraph, telegraphLayerMask))
            {
                pos1 = hit2.point;
            }
            else
            {
                pos1 = telegraphingLineRenderer.transform.position + (telegraphingLineRenderer.transform.up * maxDistanceTelegraph);
            }
            telegraphingLineRenderer.SetPositions(new Vector3[2] { pos0, pos1 });

            if (isRedirectingSpear && mostRecentlyThrownSpear != null)
            {
                Transform spearTransform = mostRecentlyThrownSpear.telegraphingSpear.transform;

                spearTransform.LookAt(lookPos);
                spearTransform.rotation = Quaternion.Euler(0, spearTransform.rotation.eulerAngles.y, spearTransform.rotation.eulerAngles.z);
            }
            if (doRedirectSpear)
            {
                Transform spearTransform = mostRecentlyThrownSpear.transform;
                mostRecentlyThrownSpear.rb.velocity = Vector3.zero;
                spearTransform.rotation = mostRecentlyThrownSpear.telegraphingSpear.transform.rotation;
                spearTransform.Rotate(new Vector3(90, 0, 0), Space.Self); //Spears are rotated proportionally to eachother..

                Vector3 target = new Vector3(hit.point.x, spearInHand.transform.position.y, hit.point.z);
                mostRecentlyThrownSpear.rb.AddForce((target - spearTransform.position).normalized * spearSpeed, ForceMode.VelocityChange);
                doRedirectSpear = false;
                isRedirectingSpear = false;
                mostRecentlyThrownSpear.telegraphingSpear.SetActive(false);

                spearRedirectSoundPlayer.PlaySound();
            }
        }


    }

    private void FixedUpdate()
    {
        if (!hasAuthority)
        {
            return;
        }

        PlayerMovement();

        PlayerLookRotationAndRedirectSpear();
    }

    [Client]
    private void ThrowSpear(Vector3 target, float spearSpeed)
    {
        spearThrowSoundPlayer.PlaySound();
        spearInHand.SetActive(false);


        CmdThrowSpear(target, spearSpeed);
    }

    [Command]
    private void CmdThrowSpear(Vector3 target, float spearSpeed)
    {
        GameObject spearObj = Instantiate(spearPrefab, spearInHand.transform.position, spearInHand.transform.rotation);


        SpearNetworked spear = spearObj.GetComponentInChildren<SpearNetworked>();
        target = new Vector3(target.x, spearInHand.transform.position.y, target.z);

        spear.rb.AddForce((target - spearInHand.transform.position).normalized * spearSpeed, ForceMode.VelocityChange);

        mostRecentlyThrownSpear = spear;
        spear.playerController = this;

        NetworkServer.Spawn(spearObj, this.gameObject);
    }


    //TODO: MAKE THIS A CLIENTRPC FROM SERVER AND MAKE SERVER CALL IT IN SPEARNETWORKED.CS
    public void Die(Rigidbody rbToParent)
    {
        //Play some sound-effects
        //unparent all transforms/rigidbodies, free up all restrictions on them, apply a minor explosion-force centered a bit below the player.

        foreach (Collider coll in collidersToActivateOnDeath)
        {
            coll.isTrigger = false;
            coll.GetComponent<Rigidbody>().isKinematic = false;
            telegraphingLineRenderer.gameObject.SetActive(false);
        }
        transform.DetachChildren();

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;

        StartCoroutine(OnDeath());
        //rb.AddExplosionForce(10f, transform.position, 4f, 1f, ForceMode.Impulse);
        //rbToParent.transform.SetParent(transform);

        this.enabled = false;
    }

public void Die()
    {
        //Play some sound-effects
        //unparent all transforms/rigidbodies, free up all restrictions on them, apply a minor explosion-force centered a bit below the player.

        foreach (Collider coll in collidersToActivateOnDeath)
        {
            coll.isTrigger = false;
            coll.GetComponent<Rigidbody>().isKinematic = false;
            telegraphingLineRenderer.gameObject.SetActive(false);
        }
        transform.DetachChildren();

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;

        StartCoroutine(OnDeath());
        //rb.AddExplosionForce(10f, transform.position, 4f, 1f, ForceMode.Impulse);

        this.enabled = false;
    }

    public IEnumerator OnDeath()
    {
        if (isDead)
        {
            yield break;
        }
        else
        {
            isDead = true;
        }
        deathSoundPlayer.PlaySound();
        yield return new WaitForSecondsRealtime(deathSoundPlayer.audioSource.clip.length - 1f);
        //SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
}
