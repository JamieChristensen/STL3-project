using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using STL2.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle, Patrolling, Seeking /*when alarmed/aware of player presence*/, FightingPlayer, Dead
    }
    public NavMeshAgent navMeshAgent;

    [SerializeField]
    private Transform playerTransform; //Assign in inspector.

    [SerializeField]
    private GameStats gameStats;

    [SerializeField]
    private IntVariable enemiesInScene;

    [SerializeField]
    private VoidEvent eventOnDeath;

    [Header("Patrol-settings:")]
    [Tooltip("If true, the path will loop around to the start. If false, the agent will reverse direction at end of path.")]
    [SerializeField]
    private bool isPathClosed;
    [Tooltip("Minimum two patrol-points, or the agent won't enter patrol-state. Agent starts going to 0th index.")]
    [SerializeField]
    private Vector3[] patrolPoints = new Vector3[2]; //Assign in inspector 
    int currentPatrolPointIndex;
    bool pathDirectionIsForward = true;

    private EnemyState state;

    private Color defaultGizmoColor = new Color(1, 1, 0, 0.55f);
    private Color currentPointGizmoColor = new Color(0, 1, 0, 0.55f);

    //private bool playerDetected = false; //Might use for communicating info to other agents or tracking state - not necessary for now.

    private Vector3 lastKnownPlayerPosition;

    [HideInInspector]
    public bool isDead = false; //Is set to true by spear on collision, used for death FOV-fadeout. 

    [Header("FOV related:")]
    [SerializeField]
    private float deathFadeFOVTime;

    [SerializeField]
    private FieldOfView fieldOfView; //Assign in inspector
    [SerializeField]
    private MeshRenderer viewMeshRenderer;  //The meshrenderer of the FOV.
    [SerializeField]
    private Color deathFOVColor, detectedPlayerFOVColor, seekingFOVColor;
    private Color baseFOVColor;
    [SerializeField]
    private float viewRange;


    [Header("Shooting-related:")]
    [SerializeField]
    private float desiredFightingDistance;
    [SerializeField]
    private float projectileSpeed;
    [SerializeField]
    private GameObject bulletPrefab; //Assign in inspector
    [SerializeField]
    private Transform bulletSpawn;
    [SerializeField]
    private float reloadTime = 0.8f;
    private float reloadTimer;
    [SerializeField]
    private GameObject muzzleFlashParticles;

    Vector3 targetPosition;

    [Header("Movement-tuning:")]
    [SerializeField]
    private float turnSmoothTime = 0.12f; //Used for turning towards player when player is closer than desired shooting distance. Might be used if strafing enemies are implemented
    private float turnsSmoothVelocity;

    [SerializeField]
    private float baseMoveSpeed, fightingMoveSpeed, baseTurnSpeed, fightingTurnSpeed; //Navmesh agent rotation-speed.

    [SerializeField]
    private PhysicMaterial physMaterialOnDeath;

    [Tooltip("Distance at which the enemy stops running towards last known player position")]
    [SerializeField]
    private float stopChaseDistance;

    [Header("Ragdoll and animations")]
    [SerializeField]
    public List<Collider> ragdollParts = new List<Collider>();

    [SerializeField]
    private Animator animator; //model animator.

    [SerializeField]
    private EnemyBubble enemyBubble;
    [SerializeField]
    private float bubbleDuration = 1f;

    [Header("Sounds:")]
    [SerializeField]
    private SoundPlayer gunshotSoundPlayer;
    [SerializeField]
    private SoundPlayer deathSoundPlayer, alertSoundPlayer;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = defaultGizmoColor;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Gizmos.color = i == currentPatrolPointIndex ? currentPointGizmoColor : defaultGizmoColor;

            if (i == 0 || i == patrolPoints.Length - 1)
            {
                Gizmos.DrawSphere(patrolPoints[i], 0.8f);
            }
            else
            {
                Gizmos.DrawSphere(patrolPoints[i], 0.5f);
            }
        }
        Gizmos.DrawCube(targetPosition, new Vector3(0.8f, 0.8f, 0.8f));
    }

    void Awake()
    {
        baseFOVColor = viewMeshRenderer.material.color;
        //Guarantee references: 
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        if (playerTransform == null)
        {
            playerTransform = FindObjectOfType<PlayerController>().transform;
        }

        if (patrolPoints.Length <= 1)
        {
            state = EnemyState.Idle;
        }
        else
        {
            state = EnemyState.Patrolling;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SetRagdollParts();

        if (state == EnemyState.Patrolling)
        {
            currentPatrolPointIndex = 0;
            navMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex]);
        }
        fieldOfView.viewRadius = viewRange;

        enemiesInScene.amount++;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead && state != EnemyState.Dead)
        {
            StartCoroutine(DeathSequence());
            state = EnemyState.Dead;
        }

        if (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime;
        }

        switch (state)
        {
            case EnemyState.Idle:
                //Just doing nothing.
                Debug.Log("Enemy entered idle: check that it has enough path-points.");
                break;
            case EnemyState.Patrolling:
                if (viewMeshRenderer.material.color != baseFOVColor)
                {
                    viewMeshRenderer.material.SetColor("_Color", baseFOVColor);
                }
                navMeshAgent.speed = baseMoveSpeed;
                navMeshAgent.angularSpeed = baseTurnSpeed;

                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentPatrolPointIndex = NextPointOnPathIndex(currentPatrolPointIndex);
                    navMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex]);
                }

                if (IsTransformInFOV(playerTransform, fieldOfView))
                {
                    state = EnemyState.FightingPlayer;
                    alertSoundPlayer.PlaySound(0.2f, 0);
                    enemyBubble.ActivateBubble(EnemyState.FightingPlayer, bubbleDuration);
                    reloadTimer = 0; //So the enemy doesn't shoot immediately when seeing the player.
                    lastKnownPlayerPosition = playerTransform.position;
                }

                break;

            case EnemyState.Seeking:
                if (viewMeshRenderer.material.color != seekingFOVColor)
                {
                    viewMeshRenderer.material.SetColor("_Color", seekingFOVColor);
                }
                navMeshAgent.SetDestination(lastKnownPlayerPosition);
                float distance = Vector3.Distance(transform.position, lastKnownPlayerPosition);
                Debug.Log("Distance to player is: " + distance);

                if (distance <= stopChaseDistance) //Navmeshdistance had issues updating fast enough.
                {
                    Debug.Log("Changed state to partrolling as distance is: " + navMeshAgent.remainingDistance +
                    "\n New navmesh destination: " + navMeshAgent.destination +
                    "\n Last known player position: " + lastKnownPlayerPosition +
                    "\n my position: " + transform.position);

                    state = EnemyState.Patrolling;
                    enemyBubble.ActivateBubble(EnemyState.Patrolling, bubbleDuration);

                }

                if (IsTransformInFOV(playerTransform, fieldOfView))
                {
                    state = EnemyState.FightingPlayer;
                    enemyBubble.ActivateBubble(EnemyState.FightingPlayer, bubbleDuration);
                    reloadTimer = 0; //So the enemy doesn't shoot immediately when seeing the player. 
                }

                break;

            case EnemyState.FightingPlayer:
                if (viewMeshRenderer.material.color != detectedPlayerFOVColor)
                {
                    viewMeshRenderer.material.SetColor("_Color", detectedPlayerFOVColor);
                }
                lastKnownPlayerPosition = playerTransform.position;

                //The following modifies movement and lookdirection for combat - the enemies try to get to a fighting distance on the player, while shooting at the player whenever their gun is loaded. 
                navMeshAgent.speed = fightingMoveSpeed;
                navMeshAgent.angularSpeed = fightingTurnSpeed;

                if (!IsTransformInFOV(playerTransform, fieldOfView))
                {
                    state = EnemyState.Seeking;
                    enemyBubble.ActivateBubble(EnemyState.Seeking, bubbleDuration);
                }
                else
                {
                    if (Vector3.Distance(playerTransform.position, transform.position) < desiredFightingDistance)
                    {
                        targetPosition = transform.position;
                        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
                        float targetAngle = Mathf.Atan2(dirToPlayer.x, dirToPlayer.z) * Mathf.Rad2Deg;
                        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnsSmoothVelocity, turnSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, angle, 0f);
                    }
                    else
                    {
                        targetPosition = (playerTransform.position - transform.position).normalized * desiredFightingDistance + transform.position;
                    }
                    navMeshAgent.SetDestination(targetPosition);

                    //Shooting at player:
                    if (reloadTimer > reloadTime)
                    {
                        FireBullet(bulletSpawn, playerTransform.position, projectileSpeed);
                        reloadTimer = 0;
                    }
                }
                break;
        }
    }

    private bool IsTransformInFOV(Transform transform, FieldOfView fieldOfView)
    {
        if (fieldOfView.visibleTargets.Contains(transform))
        {
            return true;
        }
        return false;
    }

    public void OnSpearHit()
    {
        isDead = true;
        navMeshAgent.enabled = false;
    }

    IEnumerator DeathSequence()
    {
        deathSoundPlayer.PlaySound();
        if (eventOnDeath != null)
        {
            eventOnDeath.Raise();
        }
        gameStats.kills++;
        enemiesInScene.amount--;

        GetComponent<CapsuleCollider>().material = physMaterialOnDeath;

        viewMeshRenderer.material.SetColor("_Color", deathFOVColor);

        bool hasFadedFOV = false;
        float fovFadeTimer = 0f;

        float maxAngle = fieldOfView.viewAngle + (fieldOfView.viewAngle * 0.1f);
        float maxDist = fieldOfView.viewRadius + (fieldOfView.viewRadius * 0.1f);

        while (!hasFadedFOV)
        {
            if (fovFadeTimer >= deathFadeFOVTime)
            {
                hasFadedFOV = true;
            }
            float t = fovFadeTimer / deathFadeFOVTime;

            fieldOfView.viewAngle = Mathf.Lerp(maxAngle, 0f, t);
            fieldOfView.viewRadius = Mathf.Lerp(maxDist, 0, t);
            fovFadeTimer += Time.deltaTime;
            yield return new WaitForSeconds(0f);
        }
    }


    private void FireBullet(Transform spawnPosition, Vector3 targetPosition, float bulletSpeed)
    {
        gunshotSoundPlayer.PlaySound();

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPosition.position, Quaternion.identity);
        bulletObj.transform.LookAt(targetPosition, Vector3.up);
        Rigidbody rb = bulletObj.GetComponent<Rigidbody>();

        Vector3 direction = (targetPosition - spawnPosition.position).normalized;

        rb.AddForce(direction * bulletSpeed, ForceMode.VelocityChange);
        rb.AddRelativeTorque(transform.forward * 45, ForceMode.VelocityChange);

        //"muzzle-flash"-Particles:
        GameObject particles = Instantiate(muzzleFlashParticles, spawnPosition.position, spawnPosition.rotation);
    }

    private int NextPointOnPathIndex(int currentIndex)
    {
        if (isPathClosed)
        {
            currentIndex++;
            currentIndex %= patrolPoints.Length;
            return currentIndex;
        }
        else
        {
            if (currentIndex >= patrolPoints.Length - 1)
            {
                pathDirectionIsForward = false;
                return currentIndex - 1;
            }
            if (currentIndex == 0)
            {
                pathDirectionIsForward = true;
                return currentIndex + 1;
            }

            int pathDir = pathDirectionIsForward ? 1 : -1;
            return currentIndex + pathDir;
        }
    }


    public void TurnOnRagdoll(Vector3 force, Vector3 impactPoint, Rigidbody spearRb)
    {
        Collider closestCollider = null; //Ensures something is assigned to closestCollider.
        float smallestDistance = Mathf.Infinity;

        foreach (Collider coll in ragdollParts)
        {
            Rigidbody rigidbody = coll.GetComponent<Rigidbody>();
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            coll.isTrigger = false;
            float distance = Vector3.Distance(coll.transform.position, impactPoint);
            if (distance < smallestDistance)
            {
                closestCollider = coll;
                smallestDistance = distance;
            }
        }

        if (closestCollider == null)
        {
            Debug.LogError("No collider found in TurnOnRagdoll().");
            return;
        }
        Debug.DrawRay(closestCollider.transform.position, closestCollider.attachedRigidbody.velocity, Color.magenta, 5f);
        FixedJoint joint = closestCollider.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = spearRb;

        animator.enabled = false;
    }
    public void TurnOnRagdoll()
    {
        foreach (Collider coll in ragdollParts)
        {
            Rigidbody rigidbody = coll.GetComponent<Rigidbody>();
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            coll.isTrigger = false;
        }
        animator.enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    private void SetRagdollParts()
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        Debug.Log("Amount of colliders in children: " + colliders.Length);

        foreach (Collider coll in colliders)
        {
            if (coll.gameObject != this.gameObject)
            {
                coll.isTrigger = true;
                Rigidbody collRb = coll.GetComponent<Rigidbody>();
                collRb.useGravity = false;
                collRb.isKinematic = true;
                ragdollParts.Add(coll);
            }
        }
    }
}
