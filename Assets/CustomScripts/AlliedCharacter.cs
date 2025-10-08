using UnityEngine;
using UnityEngine.AI;

public class AlliedCharacter : Character
{

    [Header("Allied-Specific Stats")]
    public Transform characterToFollow;

    //Statuses: Idle, Following, Downed, Attacking

    public string status;
    public float distanceFromPlayer;
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterController controller;

    private Character currentTarget;
    public float distanceFromCurrentTarget = Mathf.Infinity;

    private ReviveAlly reviveScript;

    public float gracePeriodMax = 120;
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

        GetAnimations();
    }

    public override void Update()
    {
        base.Update();
        distanceFromPlayer = Vector3.Distance(characterToFollow.position, transform.position);

        if (currentTarget != null)
        {
            distanceFromCurrentTarget = Vector3.Distance(currentTarget.transform.position, transform.position);
        }

        if (gracePeriod > 0)
        {
            gracePeriod--;
        }

        if (health < maxHealth)
        {
            reviveScript.enabled = true;
        }
        else
        {
            reviveScript.enabled = false;
        }

        if (currentTarget != null && currentTarget.health <= 0)
        {
            currentTarget = null;
            distanceFromCurrentTarget = Mathf.Infinity;
        }

        if (currentState != AIAllyState.Downed)
        {
            CalculateClosestEnemy();
            CalculateDecisions();
            OnUseWeapon();
        }
        ChangeState();

        Debug.Log(IsMuzzleSweeping());

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

    public void KnockOut()
    {
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
        health = maxHealth * 0.67f;
        currentState = AIAllyState.Following;
        agent.isStopped = false;

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
        gracePeriod = gracePeriodMax;
    }

    public void CalculateClosestEnemy()
    {
        if (currentTarget == null && EnemyCharacter.enemyList.Count > 0)
        {
            float closest = Mathf.Infinity;
            foreach (EnemyCharacter c in EnemyCharacter.enemyList)
            {
                if (c == null || c.health <= 0)
                {
                    continue;
                }

                float distance = Vector3.Distance(c.transform.position, transform.position);
                if (c is EnemyCharacter && distance < closest && distance < 4.5 && c.health > 0)
                {
                    currentTarget = c;
                    closest = Vector3.Distance(c.transform.position, transform.position);
                    break;
                }
            }
            distanceFromCurrentTarget = closest;
        }

        if (distanceFromCurrentTarget > 4.5 || currentTarget.health <= 0)
        {
            distanceFromCurrentTarget = Mathf.Infinity;
            currentTarget = null;
        }
        //Debug.Log(currentTarget);
    }

    public void MoveTowardsPlayer()
    {
        fire = false;

        if (distanceFromPlayer < 3.5 && !Pause.isGamePaused)
        {
            //Vector3 lookAtThis = new Vector3(characterToFollow.position.x, transform.position.y, characterToFollow.position.z);
            //transform.LookAt(lookAtThis);
            Quaternion q = Quaternion.LookRotation((characterToFollow.transform.position - transform.position).normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 5f);
        }

        //animator.SetFloat(animationSpeed, agent.velocity.magnitude);

        agent.stoppingDistance = 2;

        if (distanceFromPlayer > 2)
        {
            agent.isStopped = false;
            agent.destination = characterToFollow.position;
        }
        else
        {
            agent.isStopped = true;
            //animator.SetFloat(animationSpeed, agent.velocity.magnitude);
        }
    }

    public void Idle()
    {

    }

    public void Attack()
    {
        if (IsMuzzleSweeping() && currentTarget!=null)
        {
            agent.stoppingDistance = 2;
            agent.destination = currentTarget.transform.position;
            Quaternion q = Quaternion.LookRotation((currentTarget.transform.position - transform.position).normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 5f);
        }
        else
        {
            if (currentTarget == null || distanceFromCurrentTarget >= 4.5 || currentTarget.health <= 0 || gracePeriod > 0)
            {
                fire = false;
                //Debug.Log("Canceled Attack");
                return;
            }
            else if (currentTarget != null)
            {
                agent.stoppingDistance = 2;
                Quaternion q = Quaternion.LookRotation((currentTarget.transform.position - transform.position).normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 5f);

                fire = true;
            }
        }
    }

    public bool IsMuzzleSweeping()
    {
        Vector3 chest = transform.position + transform.up * 1.2f + transform.forward * 0.2f;

        Ray r1 = new Ray(currentWeapon.GetShootFromWhere().position,currentWeapon.GetShootFromWhere().forward);
        Ray r2 = new Ray(chest, transform.forward);

        Debug.DrawRay(currentWeapon.GetShootFromWhere().position, currentWeapon.GetShootFromWhere().forward*8);
        Debug.DrawRay(chest, transform.forward*8);

        return CheckMuzzleSweeping(r1) || CheckMuzzleSweeping(r2);
    }

    public bool CheckMuzzleSweeping(Ray r)
    {
        if (Physics.Raycast(r, out RaycastHit hit, 8))
        {
            if (hit.collider != null && hit.collider.TryGetComponent<Character>(out Character c))
            {
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
            else if (currentTarget != null && distanceFromCurrentTarget < 4.5f && currentTarget.health > 0 && gracePeriod <= 0)
            {
                currentState = AIAllyState.Attacking;
            }
            else
            {
                currentState = AIAllyState.Idle;
            }
        }
        

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
