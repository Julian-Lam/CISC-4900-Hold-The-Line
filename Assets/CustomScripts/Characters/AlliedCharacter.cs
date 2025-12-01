using UniGLTF;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AlliedCharacter : CharacterAI
{

    [Header("Allied-Specific Stats")]
    public Transform characterToFollow;
    private Character friendlyChar;

    //Statuses: Idle, Following, Downed, Attacking

    public string status;
    public float distanceFromPlayer;
    private Vector3 followingCharacterCoords;

    private ReviveAlly reviveScript;

    public float gracePeriod;

    public bool isFollowing;

    //Animations
    private int animationKnockedOut;

    public enum AIAllyState
    {
        Idle,
        Following,
        Attacking,
        Downed,
        Reloading
    }

    public AIAllyState currentState;

    public override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        reviveScript = GetComponent<ReviveAlly>();

        friendlyChar = characterToFollow.GetComponent<Character>();

        potentialEnemyColliders = new Collider[15];

        animationKnockedOut = Animator.StringToHash("IsDowned");
    }

    public override void Update()
    {
        base.Update();
        followingCharacterCoords = characterToFollow.position;
        distanceFromPlayer = Vector3.Distance(followingCharacterCoords, transform.position);

        if (currentTarget != null)
        {
            currentTargetCoords = currentTarget.transform.position;
            distanceFromCurrentTarget = Vector3.Distance(currentTargetCoords, transform.position);
        }

        if (gracePeriod > 0)
        {
            gracePeriod-=Time.deltaTime;
        }

        if (health < maxHealth)
        {
            reviveScript.enabled = true;
        }
        else
        {
            reviveScript.enabled = false;
        }

        if (currentState != AIAllyState.Downed)
        {
            CalculateDecisions();
            OnUseWeapon();
        }
    }

    public void KnockOut()
    {
        //Make sure the character cannot do anything in this state.
        agent.isStopped = true;
        controller.height = 0.6f;
        controller.center = new Vector3(0, 0.2f, 0);
        aim = false;
        fire = false;
        reload = false;
        animator.SetBool(animationAim, false);
        animator.SetBool(animationAimOnly, false);
        animator.SetFloat(animationFire, 0);
        currentWeapon.gameObject.SetActive(false);
        animator.SetBool(animationKnockedOut, true);
    }

    public void Revive()
    {
        //Heal and follow player
        health = maxHealth * 0.67f;
        currentState = AIAllyState.Following;
        agent.isStopped = false;

        //Reset target after reviving
        currentTarget = null;
        distanceFromCurrentTarget = Mathf.Infinity;

        animator.SetBool(animationKnockedOut, false);
        animator.SetBool(animationAim, false);
        animator.SetBool(animationAimOnly, false);
        animator.SetFloat(animationFire, 0);
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.ChangeParent(currentWeapon.GetWeaponStorage());

        fire = false;
        aim = false;
        reload = false;
        controller.height = 1.5f;
        controller.center = new Vector3(0, 0.75f, 0);
        gracePeriod = 2;
    }

    private Collider[] potentialEnemyColliders;

    public void CalculateClosestEnemy()
    {
        //If character is being attacked, prioritize attacking its attacker
        if (attacker != null)
        {
            PrioritizeAttacker(distanceFromCurrentTarget);
        }
        else if (currentTarget == null && EnemyCharacter.enemyList.Count > 0)
        {
            //Using a "OverlapSphere", find how many possible enemies there are from a certain radius
            int numOfPotentialEnemies = Physics.OverlapSphereNonAlloc(transform.position, 6.5f, potentialEnemyColliders, LayerMask.GetMask("Enemy"));
            float closest = Mathf.Infinity;

            //Debug.Log("Number of potential enemies: "+numOfPotentialEnemies);

            //For every possible enemies that the sphere detects, find the closest
            for (int i = 0; i < numOfPotentialEnemies; i++)
            {

                //Debug.Log("Enemy #" + (i + 1) + potentialEnemyColliders[i]);

                if (potentialEnemyColliders[i].TryGetComponent<EnemyCharacter>(out EnemyCharacter c))
                {
                    if (c == null || c.health <= 0)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(c.transform.position, transform.position);
                    if (c is EnemyCharacter && distance < closest && distance < 6.5 && c.health > 0)
                    {
                        currentTarget = c;
                        closest = distance;
                    }
                }
            }
            distanceFromCurrentTarget = closest;
        }

        CancelTargeting();
    }


    public void MoveTowardsPlayer()
    {
        fire = false;
        aim = false;

        //If close enough to player, look at player
        if (distanceFromPlayer < 3.5 && !Pause.isAnInterfaceActive)
        {
            Quaternion q = Quaternion.LookRotation((followingCharacterCoords - transform.position).normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 5f);
        }

        if (distanceFromPlayer > 5.5f)
        {
            agent.speed = 5;
        }
        else
        {
            agent.speed = 3;
        }

        //animator.SetFloat(animationSpeed, agent.velocity.magnitude);

        agent.stoppingDistance = 2;

        //If close enough to player, stop
        if (distanceFromPlayer > 2)
        {
            agent.isStopped = false;
            agent.destination = followingCharacterCoords;
        }
        else
        {
            agent.isStopped = true;
            //animator.SetFloat(animationSpeed, agent.velocity.magnitude);
        }

        //Teleport to player if stuck somewhere
        if (distanceFromPlayer > 15 && currentState!=AIAllyState.Downed)
        {
            float RandomX = Random.Range(-2, 2);
            float RandomZ = Random.Range(-2, 2);

            transform.position = characterToFollow.position + new Vector3(RandomX,0,RandomZ);
            distanceFromPlayer = Vector3.Distance(followingCharacterCoords, transform.position);
        }
    }

    //To be dealt with in a future date
    public void Idle()
    {

    }

    public void Attack()
    {
        agent.stoppingDistance = 2;
        Quaternion q = Quaternion.LookRotation((currentTargetCoords - transform.position).normalized);

        //If pointing weapon at player, try to find another away around player
        if (IsMuzzleSweeping() && currentTarget!=null)
        {
            agent.destination = currentTarget.transform.position;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 5f);
        }
        else // if(gracePeriod<0)
        {
            ShootTarget();
        }
    }

    //Check if pointing weapon at player
    public bool IsMuzzleSweeping()
    {
        Vector3 chest = transform.position + transform.up * 1.2f + transform.forward * 0.2f;

        Ray r1 = new Ray(currentWeapon.GetShootFromWhere().position,currentWeapon.GetShootFromWhere().forward);
        Ray r2 = new Ray(chest, transform.forward);

        return CheckMuzzleSweeping(r1) || CheckMuzzleSweeping(r2);
    }

    public bool CheckMuzzleSweeping(Ray r)
    {
        if (Physics.Raycast(r, out RaycastHit hit, 8))
        {
            if (hit.collider != null && hit.collider.TryGetComponent<Character>(out Character c))
            {
                //If what character is aiming at is a friend
                if (faction == c.faction)
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
        else
        {
            return false;
        }
    }

    public void CalculateDecisions()
    {
        CalculateClosestEnemy();
        if (health <= 0)
        {
            currentState = AIAllyState.Downed;
        }
        else if(gracePeriod>0 && isFollowing)
        {
            currentState = AIAllyState.Following;
        }
        else
        {
            /*
            if (!isFollowing)
            {
                currentState = AIAllyState.Idle;
            }
            else */ if (currentWeapon.ammoLeft / currentWeapon.maxAmmo <= 0.67f && currentWeapon.isReloading)
            {
                currentState = AIAllyState.Reloading;
            }
            else if (isFollowing && (distanceFromPlayer > 6.6f || currentWeapon.isReloading || currentTarget == null))
            {
                currentState = AIAllyState.Following;
            } 
            else if (currentTarget != null && distanceFromCurrentTarget < 6.5f && IsTargetAlive() && gracePeriod <= 0)
            {
                currentState = AIAllyState.Attacking;
            }
            else
            {
                currentState = AIAllyState.Idle;
            }
        }

        ChangeState();

        animator.SetFloat(animationSpeed, agent.velocity.magnitude);
        animator.SetFloat(animationMotionSpeed, agent.velocity.magnitude * 0.5f);
    }

    public void ChangeState()
    {
        switch (currentState)
        {
            case AIAllyState.Following:
                MoveTowardsPlayer();
                break;
            case AIAllyState.Attacking:
                Attack();
                break;
            case AIAllyState.Downed:
                KnockOut();
                break;
            case AIAllyState.Reloading:
                MoveTowardsPlayer();
                reload = true;
                break;
            default:
                Idle();
                break;
        }
    }
}
