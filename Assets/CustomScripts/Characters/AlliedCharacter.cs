using UniGLTF;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AlliedCharacter : Character
{

    [Header("Allied-Specific Stats")]
    public Transform characterToFollow;
    private Character friendlyChar;

    //Statuses: Idle, Following, Downed, Attacking

    public string status;
    public float distanceFromPlayer;
    private Vector3 followingCharacterCoords;
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterController controller;

    public Character currentTarget;
    public float distanceFromCurrentTarget = Mathf.Infinity;
    private Vector3 currentTargetCoords;

    private ReviveAlly reviveScript;

    public float gracePeriod;

    public Weapon currentWeapon;

    public bool isFollowing;

    //Animations
    private int animationSpeed;
    private int animationMotionSpeed;
    private int animationKnockedOut;
    private int animationAim;
    private int animationAimOnly;
    private int animationFire;

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

        GetAnimations();
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

        //If target is dead, reset targeting
        if (currentTarget != null && !IsTargetAlive())
        {
            currentTarget = null;
            distanceFromCurrentTarget = Mathf.Infinity;
        }

        //Allied bot cannot use money, has to give it to human player for use
        if (currency > 0)
        {
            Pay(currency,friendlyChar);
        }

        if (currentState != AIAllyState.Downed)
        {
            CalculateDecisions();
            OnUseWeapon();
        }
    }

    public void GetAnimations()
    {
        animationSpeed = Animator.StringToHash("Speed");
        animationMotionSpeed = Animator.StringToHash("MotionSpeed");
        animationKnockedOut = Animator.StringToHash("IsDowned");
        animationAim = Animator.StringToHash("Aim");
        animationAimOnly = Animator.StringToHash("AimOnly");
        animationFire = Animator.StringToHash("Fire");
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

    //Return true if the charcter that it is hunting down is alive
    public bool IsTargetAlive()
    {
        if(currentTarget != null)
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
            int numOfPotentialEnemies = Physics.OverlapSphereNonAlloc(transform.position, 4.5f, potentialEnemyColliders, LayerMask.GetMask("Enemy"));
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
                    if (c is EnemyCharacter && distance < closest && distance < 4.5 && c.health > 0)
                    {
                        currentTarget = c;
                        closest = distance;
                    }
                }
            }
            distanceFromCurrentTarget = closest;
        }

        //If target is dead or too far, forget about attacking it
        if (distanceFromCurrentTarget > 4.5 || !IsTargetAlive())
        {
            distanceFromCurrentTarget = Mathf.Infinity;
            attackerDistance = Mathf.Infinity;
            attacker = null;
            currentTarget = null;
        }
    }

    //Set current target to attacker
    public void PrioritizeAttacker(float distance)
    {
        currentTarget = attacker;
        distanceFromCurrentTarget = attackerDistance;
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
    }

    //To be dealt with in a future date
    public void Idle()
    {

    }

    public void Attack()
    {
        //Look at enemy
        Quaternion q = Quaternion.LookRotation((currentTargetCoords - transform.position).normalized);
        agent.stoppingDistance = 2; 

        //If pointing weapon at player, try to find another away around player
        if (IsMuzzleSweeping() && currentTarget!=null)
        {
            agent.destination = currentTarget.transform.position;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 5f);
        }
        else
        {
            //If target is invalid, too far, or dead, return
            if (currentTarget == null || distanceFromCurrentTarget >= 4.5 || !IsTargetAlive() || gracePeriod > 0)
            {
                fire = false;
                aim = false;
                //Debug.Log("Canceled Attack");
                return;
            }
            else if (currentTarget != null)
            {
                //Look at target and use weapon on target
                transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 5f);
                aim = true;
                fire = true;
            }
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
            else if (isFollowing && (distanceFromPlayer > 5.5f || currentWeapon.isReloading || currentTarget == null))
            {
                currentState = AIAllyState.Following;
            } 
            else if (currentTarget != null && distanceFromCurrentTarget < 4.5f && IsTargetAlive() && gracePeriod <= 0)
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

    public bool fire;
    public bool aim;
    public bool reload;

    private void OnUseWeapon()
    {
        if (currentWeapon != null)
        {
            //AI must use automatic weapons
            currentWeapon.isAutomatic = true;

            currentWeapon.isTriggerHeld = fire;
            currentWeapon.isUsingADS = aim;
            OnFire();
            OnAim();
            OnReload();
        }
    }

    private void OnFire()
    {
        if (currentWeapon != null)
        {
            if (fire||aim)
            {
                animator.SetBool(animationAim, true);
            }

            if (fire && !currentWeapon.isReloading)
            {
                //Debug.Log("Firing");
                currentWeapon.Fire();
                animator.SetFloat(animationFire, currentWeapon.fireAnimation);
            }
            else if (!aim && currentWeapon.isReloading)
            {
                //Debug.Log("Firing");
                currentWeapon.LeaveAim();
                animator.SetFloat(animationFire, currentWeapon.fireAnimation);
            }
            else
            {
                animator.SetFloat(animationFire, currentWeapon.fireAnimation);
            }
        }
    }

    private void OnAim()
    {
        if (currentWeapon != null)
        {
            if (currentWeapon != null && aim && !currentWeapon.isReloading)
            {
                //Debug.Log("Aiming");
                currentWeapon.Aim();
                currentWeapon.SetAimFromChest();
                animator.SetBool(animationAim, true);
                animator.SetBool(animationAimOnly, true);
            }
            else if (currentWeapon != null && (!aim || currentWeapon.isReloading))
            {
                //Debug.Log("Leaving Aim");
                currentWeapon.LeaveAim();
                currentWeapon.SetAimFromBarrel();
                animator.SetBool(animationAimOnly, false);
            }

            if ((!aim && !fire && !currentWeapon.aimAfterFire) || currentWeapon.isReloading)
            {
                //Debug.Log("Leaving Aim");
                animator.SetBool(animationAim, false);
            }
        }
    }

    private void OnReload()
    {
        if (currentWeapon != null && reload)
        {
            //Debug.Log("Reloading");
            aim = false;
            currentWeapon.Reload();
            reload = false;
        }
    }
}
