using UnityEngine;
using UnityEngine.AI;

public class CharacterAI : Character
{
    public Weapon currentWeapon;

    public bool fire;
    public bool aim;
    public bool reload;

    private bool oldWeaponAllowSelectFire;

    protected NavMeshAgent agent;
    protected Animator animator;
    protected CharacterController controller;

    protected Character currentTarget;
    public float distanceFromCurrentTarget = Mathf.Infinity;
    protected Vector3 currentTargetCoords;

    protected int animationSpeed;
    protected int animationMotionSpeed;
    protected int animationAim;
    protected int animationAimOnly;
    protected int animationFire;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        GetAnimations();

        if (currentWeapon != null)
        {
            oldWeaponAllowSelectFire = currentWeapon.canBeAutomatic;
            currentWeapon.canBeAutomatic = true;
            currentWeapon.reserveAmmoLeft = Mathf.Infinity;
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public void GetAnimations()
    {
        animationSpeed = Animator.StringToHash("Speed");
        animationMotionSpeed = Animator.StringToHash("MotionSpeed");
        animationAim = Animator.StringToHash("Aim");
        animationAimOnly = Animator.StringToHash("AimOnly");
        animationFire = Animator.StringToHash("Fire");
    }

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

    //Return true if the charcter that it is hunting down is alive
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

    public void PrioritizeAttacker(float distance)
    {
        currentTarget = attacker;
        distanceFromCurrentTarget = attackerDistance;
    }

    public void CancelTargeting()
    {
        if (distanceFromCurrentTarget > 4.5 || !IsTargetAlive())
        {
            distanceFromCurrentTarget = Mathf.Infinity;
            attackerDistance = Mathf.Infinity;
            attacker = null;
            currentTarget = null;
            fire = false;
            aim = false;
        }
    }

    public void ShootTarget()
    {
        //Look at enemy
        Quaternion q = Quaternion.LookRotation((currentTargetCoords - transform.position).normalized);

        //If target is invalid, too far, or dead, return
        if (currentTarget == null || distanceFromCurrentTarget >= 4.5 || !IsTargetAlive())
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

    protected void OnUseWeapon()
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

    protected void OnFire()
    {
        if (currentWeapon != null)
        {
            if (fire || aim)
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

    protected void OnAim()
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

    protected void OnReload()
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
