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

    //Animations
    private int animationSpeed;
    private int animationMotionSpeed;

    public override void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        GetAnimations();
    }

    public override void Update()
    {
        distanceFromPlayer = Vector3.Distance(characterToFollow.position, transform.position);
        MoveTowardsPlayer();
        HealthCheck();
    }

    public void GetAnimations()
    {
        animationSpeed = Animator.StringToHash("Speed");
        animationMotionSpeed = Animator.StringToHash("MotionSpeed");
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
        }
    }

    public void Revive()
    {
        status = "Idle";
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

            if (distanceFromPlayer > 1.5)
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

    public void CalculateDecisions()
    {
        //If ally status!="Downed"
        
        //If ally is too far away from player, status="Following". If ally closer enough, but there's enemies nearby, status="Attacking".
    }
    /*
   
        The max distance that this character will be allowed from the player character will == 2.5

     */


}
