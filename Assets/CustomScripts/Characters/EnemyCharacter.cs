using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCharacter : CharacterAI
{
    private Vector3 spawnCoords;

    public float distanceToStopAndAttack;

    public float distanceFromSpawn;

    private float defaultSpeed;

    public Item[] lootTable;

    public static List<EnemyCharacter> enemyList = new List<EnemyCharacter>();
    public static List<EnemyCharacter> enemyCorpseList = new List<EnemyCharacter>();

    private CorpseDisposal disposalScript;

    private float oldWeaponDamage;

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
    private int animationDeath;
    private int animationPunch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
        enemyList.Add(this);
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        disposalScript = GetComponent<CorpseDisposal>();

        disposalScript.enabled = false;
        
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

        if (currentWeapon != null)
        {
            oldWeaponDamage = currentWeapon.damagePerBullet;
            currentWeapon.damagePerBullet *= 0.67f;
        }

        spawnCoords = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        defaultSpeed = agent.speed;

        animationDeath = Animator.StringToHash("IsDead");
        animationPunch = Animator.StringToHash("Punching");
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (!isDead && !Pause.isAnInterfaceActive)
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
            OnUseWeapon();
        }
    }

    public static void DeleteCorpses()
    {
        if (enemyCorpseList.Count > 3)
        {
            Destroy(enemyCorpseList[0].gameObject);
            enemyCorpseList.RemoveAt(0);
        }
    }

    public bool isDead;
    public GameObject enemyHealthBar;

    public void OnDeath()
    {
        if (isDead)
        {
            return;
        }
        isDead = true;
        disposalScript.enabled = true;
        agent.isStopped = true;
        controller.height = 0.6f;
        controller.center = new Vector3(0, 0.2f, 0);
        aim = false;
        fire = false;
        reload = false;
        animator.SetBool(animationAim, false);
        animator.SetBool(animationAimOnly, false);
        animator.SetFloat(animationFire, 0);
        if (currentWeapon != null)
        {
            currentWeapon.damagePerBullet = oldWeaponDamage;
            currentWeapon.reserveAmmoLeft = Mathf.Round(Random.Range(0, currentWeapon.maxReserveAmmo));
            currentWeapon.Drop();
            currentWeapon = null;
        }

        animator.SetBool(animationDeath, true);

        enemyList.Remove(this);
        charList.Remove(this);

        enemyCorpseList.Add(this);

        Destroy(enemyHealthBar);
    }

    public void DropLoot()
    {
        Transform dropLocation = transform.Find("LootDropLocation");
        float totalWeight = 0;
        foreach (Item iw in lootTable)
        {
            totalWeight += iw.weight;
        }

        for(int i = 0; i < 3; i++)
        {
            float randomNumber = Random.Range(0, totalWeight);
            float currentWeight = 0;
            foreach (Item item in lootTable)
            {
                currentWeight += item.weight;
                if (randomNumber < currentWeight)
                {
                    Vector3 offset = new Vector3(Random.Range(-0.5f,0.5f),0, Random.Range(-0.5f, 0.5f));
                    
                    GameObject newItem = Instantiate(item.gameObject, dropLocation.position+offset, Quaternion.Euler(-90,0,0));
                    Physics.IgnoreCollision(newItem.GetComponent<Collider>(),GetComponent<Collider>());
                    break;
                }
            }
        }
    }

    public void HandleSpeed()
    {
        if (currentState == AIEnemyState.Following)
        {
            agent.speed = defaultSpeed;
        }
        else
        {
            agent.speed = 2;
        }
    }

    public void ChooseTarget()
    {
        //If character is being attacked, prioritize attacking the attacker
        if (attacker != null)
        {
            PrioritizeAttacker(distanceFromCurrentTarget);
        }
        else if(currentTarget == null)
        {
            //Choose between the player or the player's single ally.
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
        }

        CancelTargeting();
    }

    public void MoveTowardsTarget()
    {
        if (currentTarget != null)
        {
            agent.stoppingDistance = 2f;

            //If close enough to target, look at target
            if (distanceFromCurrentTarget < 5)
            {
                Quaternion q = Quaternion.LookRotation((currentTargetCoords - transform.position).normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 5f);
            }

            agent.stoppingDistance = distanceToStopAndAttack;

            //If close enough to target, stop
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
        //If too far from spawn, attempt returning to spawn
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
            //When reaching spawn, stop attempting to retrun to spawn
            returningToBase = false;
        }
    }

    public bool destinationSet;
    private Vector3 newDestination;
    public float moveCooldown = 0f;

    public void Patrol()
    {
        agent.isStopped = false;
        if (!returningToBase)
        {
            if (!destinationSet && moveCooldown <=0)
            {
                //Random coordinates
                float destinationX = Random.Range(-8, 8);
                float destinationZ = Random.Range(-8, 8);

                //Set new destination
                newDestination = new Vector3(transform.position.x + destinationX, transform.position.y, transform.position.z + destinationZ);
                //Debug.Log("Set new destination at: "+ newDestination);
                destinationSet = true;
            }
            else if(destinationSet)
            {
                //Go to new destination
                agent.destination = newDestination;
                agent.stoppingDistance = 1;
                
                //When at destination, stop
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
        else if (enemyType == EnemyType.Ranged && currentWeapon != null)
        {
            ShootTarget();
        }
    }

    private void OnPunch()
    {
        if (distanceFromCurrentTarget<=1f)
        {
            currentTarget.decreaseHealthAndArmor(10f, 15f);
            CharacterController targetController = currentTarget.gameObject.GetComponent<CharacterController>();
            currentTarget.RegisterAttacker(this);
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
