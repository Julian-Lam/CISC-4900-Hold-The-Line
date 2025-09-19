using UnityEngine;
using UnityEngine.AI;

public class AlliedCharacter : Character
{

    [Header("Allied-Specific Stats")]
    public Transform characterToFollow;
    
    //Statuses: Idle, Following, Downed, Attacking
    
    public string status;
    //public bool isDown;
    //public bool isFollowing;
    public float distanceFromPlayer;
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterController controller;

    private ReviveAlly reviveScript;

    public Weapon currentWeapon;

    //Animations
    private int animationSpeed;
    private int animationMotionSpeed;
    private int animationKnockedOut;
    private int animationAim;
    private int animationAimOnly;
    private int animationFire;

    public override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        reviveScript = GetComponent<ReviveAlly>();

        reviveScript.enabled = false;

        GetAnimations();
    }

    public override void Update()
    {
        base.Update();
        distanceFromPlayer = Vector3.Distance(characterToFollow.position, transform.position);
        CalculateDecisions();
        MoveTowardsPlayer();
        HealthCheck();
        OnUseWeapon();
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

    public void HealthCheck()
    {
        if (health <= 0)
        {
            status = "Downed";
            agent.isStopped = true;
            currentWeapon.gameObject.SetActive(false);
            animator.SetBool(animationKnockedOut, true);
            controller.height = 0.6f;
            controller.center = new Vector3(0, 0.2f, 0);
            aim = false;
            fire = false;
            reload = false;
            animator.SetBool(animationAim, false);
            animator.SetBool(animationAimOnly, false);
            reviveScript.enabled = true;
        }
    }

    public void Revive()
    {
        status = "Following";
        agent.isStopped = false;
        animator.SetBool(animationKnockedOut, false);
        controller.height = 1.5f;
        controller.center = new Vector3(0, 0.75f, 0);
    }

    public void MoveTowardsPlayer()
    {
        if (status=="Following")
        {

            if (distanceFromPlayer < 3.5)
            {
                Vector3 lookAtThis = new Vector3(characterToFollow.position.x, transform.position.y, characterToFollow.position.z);
                transform.LookAt(lookAtThis);
            }

            animator.SetFloat(animationSpeed, agent.velocity.magnitude);

            if (agent.velocity.magnitude != 0)
            {
                animator.SetFloat(animationMotionSpeed, agent.velocity.magnitude*0.5f);
            }

            if (distanceFromPlayer > 2)
            {
                agent.isStopped = false;
                agent.destination = characterToFollow.position;
            }
            else
            {
                agent.isStopped = true;
                animator.SetFloat(animationSpeed, agent.velocity.magnitude);
            }
        }
    }

    public void CalculateAttacks()
    {
        if (status == "Attacking")
        {
        }
        else
        {
            if (currentWeapon.ammoLeft / currentWeapon.maxAmmo <= 0.67f)
            {
                reload = true;
            }
        }
    }

    public void CalculateDecisions()
    {
        if (status != "Downed")
        {
            if (distanceFromPlayer > 5)
            {
                status = "Following";
            }

            
            
        }

        //If ally status!="Downed"
        
        //If ally is too far away from player, status="Following". If ally closer enough, but there's enemies nearby, status="Attacking".
    }
    /*
   
        The max distance that this character will be allowed from the player character will == 2.5

     */

    public bool fire;
    public bool aim;
    public bool reload;

    private void OnUseWeapon()
    {
        if (currentWeapon != null)
        {
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
                currentWeapon.Fire();
                animator.SetFloat(animationFire, currentWeapon.fireAnimation);
            }
            else if (!aim && currentWeapon.isReloading)
            {
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
                currentWeapon.Aim();
                currentWeapon.SetAimFromChest();
                animator.SetBool(animationAim, true);
                animator.SetBool(animationAimOnly, true);
            }
            else if (currentWeapon != null && (!aim || currentWeapon.isReloading))
            {
                currentWeapon.LeaveAim();
                currentWeapon.SetAimFromBarrel();
                animator.SetBool(animationAimOnly, false);
            }

            if ((!aim && !fire && !currentWeapon.aimAfterFire) || currentWeapon.isReloading)
            {
                animator.SetBool(animationAim, false);
            }
        }
    }

    private void OnReload()
    {
        if (currentWeapon != null && reload)
        {
            aim = false;
            currentWeapon.Reload();
            reload = false;
        }
    }


}
