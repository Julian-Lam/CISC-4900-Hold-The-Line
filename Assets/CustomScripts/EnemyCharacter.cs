using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCharacter : Character
{
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterController controller;

    private Vector3 spawnCoords;

    private Character currentTarget;
    public float distanceFromCurrentTarget = Mathf.Infinity;
    private Vector3 currentTargetCoords;

    public Weapon currentWeapon;

    public float distanceToStopAndAttack;

    public float distanceFromSpawn;

    public bool fire;
    public bool aim;
    public bool reload;

    public static List<EnemyCharacter> enemyList = new List<EnemyCharacter>();
    public static List<EnemyCharacter> enemyCorpseList = new List<EnemyCharacter>();

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

    //Animations
    private int animationSpeed;
    private int animationMotionSpeed;
    private int animationDeath;
    private int animationPunch;

    //Animations to be used in future enemy types
    private int animationAim;
    private int animationAimOnly;
    private int animationFire;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
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
        } else if (enemyType == EnemyType.Sniper)
        {
            distanceToStopAndAttack = Mathf.Infinity;
        }

        spawnCoords = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        GetAnimations();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (!isDead)
        {
            if (currentTarget != null)
            {
                currentTargetCoords = currentTarget.transform.position;
                distanceFromCurrentTarget = Vector3.Distance(currentTargetCoords, transform.position);
            }

            if (meleeCooldown > 0)
            {
                meleeCooldown -= Time.deltaTime;
            }

            if (moveCooldown > 0)
            {
                moveCooldown -= Time.deltaTime;
            }

            distanceFromSpawn = Vector3.Distance(transform.position, spawnCoords);

            CalculateDecisions();
        }
    }

    public void GetAnimations()
    {
        animationSpeed = Animator.StringToHash("Speed");
        animationMotionSpeed = Animator.StringToHash("MotionSpeed");
        animationAim = Animator.StringToHash("Aim");
        animationAimOnly = Animator.StringToHash("AimOnly");
        animationFire = Animator.StringToHash("Fire");
        animationDeath = Animator.StringToHash("IsDead");
        animationPunch = Animator.StringToHash("Punching");
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

    public static void DeleteCorpses()
    {
        if (enemyCorpseList.Count > 3)
        {
            Destroy(enemyCorpseList[0].gameObject);
            enemyCorpseList.RemoveAt(0);
        }
    }

    public bool isDead;

    public bool IsTargetAlive()
    {
        if (currentTarget != null)
        {
            if (currentTarget.health > 0)
            {
                return true;
            }
            else
            {

                return false;
            }
        }
        else
        {
            return false;
        }
    }
    public void OnDeath()
    {
        if (isDead)
        {
            return;
        }
        isDead = true;
        animator.SetBool(animationDeath, true);
        agent.isStopped = true;
        controller.height = 0.6f;
        controller.center = new Vector3(0, 0.2f, 0);

        enemyList.Remove(this);
        charList.Remove(this);

        enemyCorpseList.Add(this);
    }

    public void HandleSpeed()
    {
        if (currentState == AIEnemyState.Following)
        {
            agent.speed = 3;
        }
        else
        {
            agent.speed = 2;
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
                closest = distance;
            }
        }
        distanceFromCurrentTarget = closest;

        if (distanceFromCurrentTarget > 6 || !IsTargetAlive())
        {
            distanceFromCurrentTarget = Mathf.Infinity;
            currentTarget = null;
        }
    }

    public void MoveTowardsTarget()
    {
        if (currentTarget != null)
        {
            if (distanceFromCurrentTarget < 5 && !Pause.isGamePaused)
            {
                Quaternion q = Quaternion.LookRotation((currentTargetCoords - transform.position).normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 5f);
            }

            agent.stoppingDistance = distanceToStopAndAttack;

            if (distanceFromCurrentTarget > distanceToStopAndAttack)
            {
                agent.isStopped = false;
                agent.destination = currentTargetCoords;
            }
            else
            {
                agent.isStopped = true;
            }
        }
    }

    public bool returningToBase;

    public void ReturnToSpawnIfNeeded()
    {
        if (distanceFromSpawn > 12)
        {
            //Debug.Log("Returning to base");
            returningToBase = true;
            agent.isStopped = false;
            agent.destination = spawnCoords;
            agent.stoppingDistance=1;
        }
        else if (distanceFromSpawn < 2)
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
                //Debug.Log("Set new destination at: "+ newDestination);
                destinationSet = true;
            }
            else if(destinationSet)
            {
                agent.destination = newDestination;
                agent.stoppingDistance = 1;
                if (Vector3.Distance(newDestination, transform.position) < 2)
                {
                    //Debug.Log("Reached destination.");
                    destinationSet = false;
                    moveCooldown = 5;
                }
            }
        }
    }

    public float meleeCooldown;

    public void Attack()
    {
        if (enemyType == EnemyType.Melee)
        {
            if (meleeCooldown <= 0)
            {
                //Debug.Log("Punch Start");
                animator.SetTrigger(animationPunch);
                meleeCooldown = 1f;
            }
        }
    }

    private void OnPunch()
    {
        if (distanceFromCurrentTarget<=1f)
        {
            currentTarget.decreaseHealthAndArmor(10f, 15f);
            CharacterController targetController = currentTarget.gameObject.GetComponent<CharacterController>();
            Instantiate(currentTarget.hitParticle, targetController.transform.TransformPoint(targetController.center), Quaternion.identity);
        }
    }

    public void CalculateDecisions()
    {
        ChooseTarget();
        
        if (health <= 0)
        {
            currentState = AIEnemyState.Dead;
        }
        else
        {
            if (distanceFromCurrentTarget <= distanceToStopAndAttack && IsTargetAlive())
            {
                currentState = AIEnemyState.Attacking;
            }
            /*else if (currentWeapon.ammoLeft / currentWeapon.maxAmmo <= 0.67f && currentWeapon.isReloading)
            {
                currentState = AIEnemyState.Reloading;
            }*/
            else if (currentTarget != null && (!returningToBase || distanceFromCurrentTarget<3) && distanceFromSpawn < 10 && distanceFromCurrentTarget > distanceToStopAndAttack)
            {
                currentState = AIEnemyState.Following;
            }
            else
            {
                currentState = AIEnemyState.Patroling;
            }
        }

        ChangeState();
        HandleSpeed();

        animator.SetFloat(animationSpeed, agent.velocity.magnitude);
        animator.SetFloat(animationMotionSpeed, agent.velocity.magnitude * 0.5f);
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
                OnDeath();
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
