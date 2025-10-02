using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyCharacter : Character
{
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterController controller;

    private Vector3 spawnCoords;

    private Character currentTarget;
    public float distanceFromCurrentTarget = Mathf.Infinity;

    public Weapon currentWeapon;

    public float distanceToStopAndAttack;

    public float distanceFromSpawn;

    public bool fire;
    public bool aim;
    public bool reload;

    public float meleeCooldown = 1f;

    public static List<EnemyCharacter> enemyList = new List<EnemyCharacter>();
    static float numberOfEnemies;

    public enum EnemyType
    {
        Melee,
        Ranged,
        Sniper
    }

    public EnemyType enemyType;

    public enum AIEnemyState
    {
        Patroling,
        Following,
        Attacking,
        Dead,
        Reloading
    }

    public AIEnemyState currentState;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
        numberOfEnemies++;
        enemyList.Add(this);
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        if (enemyType == EnemyType.Ranged)
        {
            distanceToStopAndAttack = 4f;
        }
        else if (enemyType == EnemyType.Melee)
        {
            distanceToStopAndAttack = 1f;
        }else if(enemyType == EnemyType.Sniper)
        {
            distanceToStopAndAttack = Mathf.Infinity;
        }

        spawnCoords = new Vector3(transform.position.x,transform.position.y,transform.position.z);

        GetAnimations();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (currentTarget != null)
        {
            distanceFromCurrentTarget = Vector3.Distance(currentTarget.transform.position, transform.position);
        }

        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }

        moveCooldown -= Time.deltaTime;

        distanceFromSpawn = Vector3.Distance(transform.position, spawnCoords);

        CalculateDecisions();
        ChangeState();
        ChooseTarget();
    }

    public void GetAnimations()
    {

    }

    //TAKEN FROM ThirdPersonController.cs

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(controller.center), FootstepAudioVolume);
        }
    }

    //BELOW IS CUSTOM

    public float timeUntilDisappearance = 3f;


    public void IsDead()
    {
        agent.isStopped = true;
        controller.height = 0.6f;
        controller.center = new Vector3(0, 0.2f, 0);
        timeUntilDisappearance -= Time.deltaTime;

        enemyList.Remove(this);

        if (timeUntilDisappearance <= 0)
        {
            Destroy(gameObject);
        }


    }

    public void ChooseTarget()
    {

        float closest = Mathf.Infinity;
        foreach (Character c in charList)
        {
            float distance = Vector3.Distance(c.transform.position, transform.position);
            if (c.faction == "BluFor" && distance < closest && c.health > 0)
            {
                currentTarget = c;
                closest = Vector3.Distance(c.transform.position, transform.position);
            }
        }
        distanceFromCurrentTarget = closest;

        if (distanceFromCurrentTarget > 4.5 || currentTarget.health <= 0)
        {
            distanceFromCurrentTarget = Mathf.Infinity;
            currentTarget = null;
        }
    }

    public void MoveTowardsTarget()
    {
        if (currentTarget != null)
        {
            if (distanceFromCurrentTarget < 3.5)
            {
                Quaternion q = Quaternion.LookRotation((currentTarget.transform.position - transform.position).normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 5f);
            }

            agent.stoppingDistance = distanceToStopAndAttack;

            if (distanceFromCurrentTarget > distanceToStopAndAttack)
            {
                agent.isStopped = false;
                agent.destination = currentTarget.transform.position;
            }
            else
            {
                agent.isStopped = true;
                //animator.SetFloat(animationSpeed, agent.velocity.magnitude);
            }
        }
    }

    public bool returningToBase;

    public void ReturnToSpawnIfNeeded()
    {
        if (distanceFromSpawn > 10)
        {
            returningToBase = true;
            agent.isStopped = false;
            agent.destination = spawnCoords;
        }else if (distanceFromSpawn < 2)
        {
            returningToBase = false;
        }
    }

    public bool destinationSet;
    private Vector3 newDestination;
    public float moveCooldown = 0f;

    public void Patrol()
    {
        if (!returningToBase)
        {
            if (!destinationSet && moveCooldown <=0)
            {
                float destinationX = Random.Range(-8, 8);
                float destinationZ = Random.Range(-8, 8);

                newDestination = new Vector3(transform.position.x + destinationX, transform.position.y, transform.position.z + destinationZ);
                destinationSet = true;
            }
            else if(destinationSet)
            {
                agent.destination = newDestination;
                if (Vector3.Distance(newDestination, transform.position) < 2)
                {
                    destinationSet = false;
                    moveCooldown = 5;
                }
            }
        }
    }

    public float cooldown = 0f;

    public void Attack()
    {
        if (enemyType == EnemyType.Melee)
        {
            if (cooldown <= 0)
            {
                currentTarget.decreaseHealthAndArmor(10f, 15f);
                cooldown = meleeCooldown;
            }
        }
    }

    public void CalculateDecisions()
    {
        if (health <= 0)
        {
            currentState = AIEnemyState.Dead;
        }
        else
        {
            if (distanceFromCurrentTarget <= distanceToStopAndAttack && currentTarget.health > 0)
            {
                currentState = AIEnemyState.Attacking;
            }
            /*else if (currentWeapon.ammoLeft / currentWeapon.maxAmmo <= 0.67f && currentWeapon.isReloading)
            {
                currentState = AIEnemyState.Reloading;
            }*/
            else if (currentTarget != null && !returningToBase && distanceFromSpawn < 10 && distanceFromCurrentTarget > distanceToStopAndAttack)
            {
                currentState = AIEnemyState.Following;
            }
            else
            {
                currentState = AIEnemyState.Patroling;
            }
        }

    }

    public void ChangeState()
    {
        switch (currentState)
        {
            case AIEnemyState.Patroling:
                ReturnToSpawnIfNeeded();
                Patrol();
                break;
            case AIEnemyState.Following:
                MoveTowardsTarget();
                break;
            case AIEnemyState.Attacking:
                Attack();
                break;
            case AIEnemyState.Dead:
                IsDead();
                break;
            case AIEnemyState.Reloading:
                MoveTowardsTarget();
                reload = true;
                break;
            default:
                break;
        }
    }
}
