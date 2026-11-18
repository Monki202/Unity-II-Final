using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class PatrolEnemy : MonoBehaviour
{
    NavMeshAgent agent;
    public Transform[] waypoints;
    bool arrived;
    bool patrolling;
    bool seesPlayer;
    int destination;
    public Transform eye;
    public Transform target;
    Vector3 lastPosition;
    public float viewDistance = 10.0f;
    public float viewAngle = 90.0f;
    public LayerMask playerMask;
    public Animator anim;
    [Min(0f)] public float attackRecovery = 0.5f;
    [Range(0f, 180f)] public float attackFacingAngle = 20f;
    [Min(0f)] public float combatTurnSpeed = 360f;
    bool attacking;
    float nextAttackTime;
    int attackLayer;
    bool previousUpdateRotation;
    bool previousIsStopped;
    bool stop;

    public float runSpeed = 20f;
    public float walkSpeed = 4f;

    AudioSource audioSource;
    public AudioSource audioSource2;
    public AudioClip gaspSound;
    bool hasGasped;

    public ParticleSystem sparksLow;
    public ParticleSystem sparksHigh;

    public NavMeshSurface meshSurface;



    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        attackLayer = anim.GetLayerIndex("Attack");
        patrolling = true;
        lastPosition = transform.position;
    }
    
    bool CanSeePlayer()
    {
        if (Vector3.Distance(eye.position, target.position) < viewDistance)
        {
            Vector3 playerDirection = (target.position - eye.position).normalized;
            float angleDifference = Vector3.Angle(eye.forward, playerDirection);
            if (angleDifference < viewAngle /2)
            {
                
                if (!Physics.Linecast(eye.position, target.position, ~playerMask))
                {
                    if(seesPlayer == false)
                    {
                        if(hasGasped == false)
                        {
                            StartCoroutine(HasGasped());
                            hasGasped = true;
                        }
                        StartCoroutine(PlayerSpotted());
                        seesPlayer = true;
                    }
                    //enemy sees player
                    lastPosition = target.position;
                    return true;
                }
                else
                {
                    seesPlayer = false;
                }

            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (stop) return;
        if (attacking) return;
        if (agent.pathPending)
        {
            return;
        }
        //Code for patrolling
        if (patrolling)
        {
            agent.speed = walkSpeed;
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                if (!arrived)
                {
                    arrived = true;
                    //Start Coroutine
                    StartCoroutine(GoToNextPatrolPoint());
                }
            }
        }
        else
        {
            agent.speed = runSpeed;
            arrived = false;
        }
        //Code for when enemy sees target
        if (CanSeePlayer())
        {
            sparksLow.Stop();
            sparksHigh.Play();
            audioSource.volume = 1.0f;
            agent.SetDestination(target.transform.position);
            patrolling = false;
            // Use player distance, not a path that may still be recalculating.
            Vector3 toPlayer = target.position - transform.position;
            Vector3 facingDirection = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
            if (!agent.isOnOffMeshLink && toPlayer.magnitude <= agent.stoppingDistance + 0.1f)
            {
                if (facingDirection.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation,
                        Quaternion.LookRotation(facingDirection), combatTurnSpeed * Time.deltaTime);
                }

                if (Time.time >= nextAttackTime && attackLayer >= 0 &&
                    Vector3.Angle(transform.forward, facingDirection) <= attackFacingAngle)
                {
                    StartCoroutine(AttackOnce());
                }
            }
            else
            {
                anim.SetBool("Attack", false);
            }
        }
        else
        {
            sparksLow.Play();
            sparksHigh.Stop();
            audioSource.volume = 0f;
            if (!patrolling)
            {
                anim.SetBool("Attack", false);
                agent.SetDestination(lastPosition);
                if(agent.remainingDistance < agent.stoppingDistance)
                {
                    patrolling = true;
                    StartCoroutine(GoToNextPatrolPoint());
                }
            }
        }
        //play move animation
        anim.SetFloat("Moving", agent.velocity.sqrMagnitude);
    }

    IEnumerator AttackOnce()
    {
        attacking = true;
        previousIsStopped = agent.isStopped;
        previousUpdateRotation = agent.updateRotation;
        agent.isStopped = true;
        agent.updateRotation = false;
        anim.SetFloat("Moving", 0f);
        anim.SetBool("Attack", true);

        // Wait for the existing animation state, then let one swing complete.
        float entryDeadline = Time.time + 2f;
        while (!anim.GetCurrentAnimatorStateInfo(attackLayer).IsName("Standing Melee Attack Horizontal")
               && Time.time < entryDeadline)
            yield return null;

        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(attackLayer);
        if (!state.IsName("Standing Melee Attack Horizontal"))
            Debug.LogWarning("Attack animation did not start. Check the Attack layer and transitions.", this);

        while (state.IsName("Standing Melee Attack Horizontal") && state.normalizedTime < 1f)
        {
            yield return null;
            state = anim.GetCurrentAnimatorStateInfo(attackLayer);
        }

        anim.SetBool("Attack", false);
        float exitDeadline = Time.time + 2f;
        while ((anim.IsInTransition(attackLayer) ||
                anim.GetCurrentAnimatorStateInfo(attackLayer).IsName("Standing Melee Attack Horizontal"))
               && Time.time < exitDeadline)
            yield return null;

        agent.isStopped = previousIsStopped;
        agent.updateRotation = previousUpdateRotation;
        nextAttackTime = Time.time + attackRecovery;
        attacking = false;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (attacking)
        {
            if (anim != null) anim.SetBool("Attack", false);
            if (agent != null)
            {
                agent.updateRotation = previousUpdateRotation;
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                    agent.isStopped = previousIsStopped;
            }
        }
        attacking = false;
        arrived = false;
    }

    IEnumerator GoToNextPatrolPoint()
    {
        if(waypoints.Length == 0)
        {
            yield break;
        }

        patrolling = true;
        yield return new WaitForSeconds(2.0f);
        // A patrol wait must not change the destination during combat.
        if (!patrolling || attacking) yield break;
        arrived = false;
        agent.destination = waypoints[destination].position;
        destination = (destination + 1) % waypoints.Length;
    }

    IEnumerator PlayerSpotted()
    {
        meshSurface.enabled = false;
        //agent.enabled = false;
        anim.SetBool("Spotted", true);
        yield return new WaitForSeconds(0.708f);
        anim.SetBool("Spotted", false);
        meshSurface.enabled = true;
        //agent.enabled = true;
    }

    IEnumerator HasGasped()
    {
        audioSource2.PlayOneShot(gaspSound, 1.0f);
        yield return new WaitForSeconds(1.0f);
        hasGasped = false;
    }

} 
