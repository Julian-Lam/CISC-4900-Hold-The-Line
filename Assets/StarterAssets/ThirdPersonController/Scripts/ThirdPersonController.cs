using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -9.81f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            c = GetComponent<Character>();
            playerInventory = GetComponent<Inventory>();

            interactTextbox.SetActive(false);
            hitMarker.SetActive(false);
            dialogueTextBox.SetActive(false);

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else


            Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();

            if (!Pause.isAnInterfaceActive && Dialogue.activeDialogue == null && !Dialogue.lockInput)
            {
                if (!isDowned)
                {
                    OnInteract();
                    if (!isHoldingInteract)
                    {
                        GroundedCheck();
                        Move();
                        if (currentWeapon != null)
                        {
                            OnUseWeapon();
                        }
                        OnCallAirStrike();
                        SetLookAnimaton();
                    }
                }
            }

            HealthCheck();

            if (isDowned|| Pause.isAnInterfaceActive || Dialogue.activeDialogue!= null || Dialogue.lockInput)
            {
                _input.fire = false;
                _input.aim = false;
                _input.reload = false;
                _input.sprint = false;
                _input.selectFireMode = false;
                _input.strafe = 0;
                _input.jump = false;
                _input.move = Vector2.zero;
                _input.walkBackwards = false;
                _input.callAirStrike = false;
                _input.interact = false;
                _animator.SetFloat(_animIDSpeed, 0f);
                _animator.SetFloat(_animIDMotionSpeed, 0f);
                _animator.SetFloat(strafeState, 0);
            }
        }

        private void LateUpdate()
        {
            if (!Pause.isAnInterfaceActive && !isHoldingInteract)
            {
                CameraRotation();
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

            //CUSTOM ANIMATIONS BELOW
            aimAnimation = Animator.StringToHash("Aim");
            fireWeaponAnimation = Animator.StringToHash("Fire");
            strafeState = Animator.StringToHash("StrafeDirection");
            lookUpDownAnimaton = Animator.StringToHash("LookUpDown");
            aimOnlyAnimation = Animator.StringToHash("AimOnly");
            knockOutAnimation = Animator.StringToHash("IsDowned");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private Character c;

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = c.hasSpeedBuff ? 7f : (_input.sprint ? (c.stamina>0? (_input.aim? MoveSpeed : SprintSpeed) : MoveSpeed) : MoveSpeed);

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero&&!_input.aim)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            //Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            //BEGIN CUSTOM ADDITION FOR MOVE FUNCTION

            if(_input.sprint && !_input.aim && _input.move!=Vector2.zero)
            {
                c.decreaseStamina(0.25f);
            }

            //While aiming, character rotation == camera rotation
            if (_input.aim)
            {
                _targetRotation = _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection;
            if (_input.aim)
            {
                targetDirection = _mainCamera.transform.forward * _input.move.y +
                                  _mainCamera.transform.right * _input.move.x;
                targetDirection.y = 0f;
            }
            else
            {
                targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            }

            if (_hasAnimator && _input.aim && _input.move != Vector2.zero)
            {
                if (_input.walkBackwards)
                {
                    _animator.SetFloat(strafeState, 1, 0.1f, Time.deltaTime);
                }
                else if (_input.strafe != 0)
                {
                    if (_input.strafe < 0)
                    {
                        _animator.SetFloat(strafeState, 2, 0.1f, Time.deltaTime);
                    }
                    else if (_input.strafe > 0)
                    {
                        _animator.SetFloat(strafeState, 3, 0.1f, Time.deltaTime);
                    }
                }
                else
                {
                    _animator.SetFloat(strafeState, 0, 0.1f, Time.deltaTime);
                }
            }
            else
            {
                _animator.SetFloat(strafeState, 0);
            }

            //END SECTION OF CUSTOM ADDITION FOR MOVE FUNCTION

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }

            


        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f && !isDowned && !_input.interact && !Pause.isAnInterfaceActive)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        //START CUSTOM ADDITION

        //Other animations
        private int aimAnimation;
        private int fireWeaponAnimation;
        private int strafeState;
        private int lookUpDownAnimaton;
        private int aimOnlyAnimation;
        private int knockOutAnimation;

        public Weapon currentWeapon;

        public bool isHoldingInteract;

        public bool isDowned;

        private Transform playerCameraRootTransform;
        public float rotationX;

        public GameObject interactTextbox;
        public TextMeshProUGUI description;

        public TextMeshProUGUI weaponNameText;

        public TextMeshProUGUI weaponAmmoText;

        public Transform playerModel;

        public void ResetRotation()
        {
            if (!Pause.isAnInterfaceActive)
            {
                playerModel.localRotation = Quaternion.identity;
            }
            isHoldingInteract = false;
        }

        private void OnInteract()
        {
            float range = 8.0f;

            Ray inspector = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            RaycastHit hit;

            Debug.DrawRay(_mainCamera.transform.position, _mainCamera.transform.forward*8, Color.green);

            if (Physics.Raycast(inspector,out hit, range) && !Pause.isAnInterfaceActive && Dialogue.activeDialogue==null)
            {
                //If aiming at interactable
                if (hit.collider.TryGetComponent<Interactable>(out Interactable i))
                {
                    //Helper function for when you finish or stop interacting
                    void releaseActions()
                    {
                        i.ReleaseAction();
                        ResetRotation();
                        _input.interact = false;
                        isHoldingInteract = false;
                    }

                    if (i is MonoBehaviour mb && mb.enabled)
                    {
                        //Message for interactable
                        description.text = i.Description();
                        interactTextbox.SetActive(true);
                        
                        if (_input.interact)
                        {
                            //Interact with interactable
                            i.Interact(gameObject);

                            //Debug.Log("Interacted with interactable: " + i);

                            //If on ground and you have to hold to interact
                            if (i.CanHoldInteract() && Grounded)
                            {
                                //Look at object
                                Quaternion q = Quaternion.LookRotation((mb.transform.position - playerModel.position).normalized);

                                playerModel.rotation = Quaternion.RotateTowards(playerModel.rotation, q, 5f);
                                isHoldingInteract = true;

                                //If hold-interacting, do not move or do anything else
                                _input.move = Vector2.zero;
                                _input.jump = false;
                                _animator.SetFloat(_animIDSpeed, 0f);
                                _animator.SetFloat(_animIDMotionSpeed, 0f);
                            }
                            
                            //If done interacting
                            if (i.Release())
                            {
                                releaseActions();
                            }
                        }
                        else
                        {
                            //When you stop interacting
                            if (i.CanHoldInteract()&&!i.Release())
                            {
                                releaseActions();
                            }
                        }
                    }
                    else
                    {
                        interactTextbox.SetActive(false);
                        _input.interact = false;
                        ResetRotation();
                        isHoldingInteract = false;
                    }
                }
                else
                {
                    //Safety check if aiming away from interactable
                    _input.interact = false;
                    isHoldingInteract = false;
                    ResetRotation();
                    interactTextbox.SetActive(false);
                }
            }
            else
            {
                _input.interact = false;
                isHoldingInteract = false;
                ResetRotation();
                interactTextbox.SetActive(false);
            }
        }

        private void OnUseWeapon()
        {
            if (currentWeapon != null)
            {
                currentWeapon.isTriggerHeld = _input.fire;
                currentWeapon.isUsingADS = _input.aim;
                OnFire();
                OnAim();
                OnReload();
                OnSwitchFireMode();
                WeaponImageHandler();
            }
        }

        private void OnFire()
        {
            if(currentWeapon != null)
            {
                if ((_input.fire && currentWeapon.ammoLeft>0 && currentWeapon.reserveAmmoLeft>0)|| _input.aim)
                {
                    _animator.SetBool(aimAnimation, true);
                }

                if (_input.fire && !currentWeapon.isReloading)
                {
                    currentWeapon.Fire();
                    _animator.SetFloat(fireWeaponAnimation, currentWeapon.fireAnimation);
                }
                else if (!_input.aim && currentWeapon.isReloading)
                {
                    currentWeapon.LeaveAim();
                    _animator.SetFloat(fireWeaponAnimation, currentWeapon.fireAnimation);
                }
                else
                {
                    _animator.SetFloat(fireWeaponAnimation, currentWeapon.fireAnimation);
                }
            }
        }

        private void OnAim()
        { 
            if (currentWeapon != null)
            {
                if (currentWeapon != null && _input.aim && !currentWeapon.isReloading)
                {
                    currentWeapon.Aim();
                    currentWeapon.SetAimFromCamera();
                    _animator.SetBool(aimAnimation, true);
                    _animator.SetBool(aimOnlyAnimation, true);
                }
                else if (currentWeapon != null && (!_input.aim || currentWeapon.isReloading))
                {
                    currentWeapon.LeaveAim();
                    currentWeapon.SetAimFromBarrel();
                    _animator.SetBool(aimOnlyAnimation, false);
                }

                if ((!_input.aim && !_input.fire && !currentWeapon.aimAfterFire) ||currentWeapon.isReloading)
                {
                    _animator.SetBool(aimAnimation, false);
                }
            }
        }

        private void OnReload()
        {
            if (currentWeapon != null && _input.reload)
            {
                _input.aim = false;
                currentWeapon.Reload();
                _input.reload = false;
            }
        }
        private void OnSwitchFireMode()
        {
            if (currentWeapon != null && _input.selectFireMode)
            {
                currentWeapon.SwitchFireMode();
                _input.selectFireMode = false;
            }
        }

        public void SetPlayerCameraRootTransform()
        {
            playerCameraRootTransform = transform.Find("PlayerCameraRoot");
        }

        public float GetRotationX(Transform t)
        {
            float angle = t.localEulerAngles.x;
            return angle > 180 ? angle - 360 : angle;
        }
        public void SetLookAnimaton()
        {
            SetPlayerCameraRootTransform();
            rotationX = GetRotationX(playerCameraRootTransform);
            if (_hasAnimator&&_input.aim&&currentWeapon!=null)
            {
                _animator.SetFloat(lookUpDownAnimaton, rotationX);
            }
        }

        public void HealthCheck()
        {
            if (c.health <= 0)
            {
                //Prevent player from doing anything
                isDowned = true;
                _input.jump = false;
                _input.sprint=false;

                _animator.SetBool(aimAnimation, false);
                _animator.SetBool(aimOnlyAnimation, false);
                if (currentWeapon != null)
                {
                    currentWeapon.gameObject.SetActive(false);
                }
                _animator.SetBool(knockOutAnimation, true);
                _controller.height = 0.6f;
                _controller.center = new Vector3(0, 0.2f, 0);
                c.decreaseStamina(c.maxStamina);
            }
        }

        public Image currentWeaponImage;
        public Image currentFireModeImage;

        public Sprite noSelectFireImage;
        public Sprite autoFireImage;
        public Sprite semiFireImage;
        public GameObject hitMarker;

        public void WeaponImageHandler()
        {
            if (currentWeapon != null)
            {
                currentWeaponImage.sprite = currentWeapon.gunSprite;
                weaponNameText.text = currentWeapon.weaponName;

                if (!currentWeapon.isReloading)
                {
                    weaponAmmoText.text = "Ammo: "+currentWeapon.ammoLeft.ToString()+" | "+currentWeapon.reserveAmmoLeft.ToString();
                }
                else if (currentWeapon.isReloading)
                {
                    weaponAmmoText.text = "Reloading... | " + currentWeapon.reserveAmmoLeft.ToString();
                }

                if (!currentWeapon.canBeAutomatic)
                {
                    currentFireModeImage.sprite = noSelectFireImage;
                }
                else
                {
                    if (currentWeapon.isAutomatic)
                    {
                        currentFireModeImage.sprite = autoFireImage;
                    }
                    else if(!currentWeapon.isAutomatic)
                    {
                        currentFireModeImage.sprite = semiFireImage;
                    }
                }

                if (currentWeapon.hitTarget)
                {
                    hitMarker.SetActive(true);
                }
                else
                {
                    hitMarker.SetActive(false);
                }
            }
        }

        public GameObject buyCanvas;

        private Inventory playerInventory;

        public GameObject playerHUDs;

        public GameObject dialogueTextBox;

        public GameObject explosionParticle;

        public GameObject airSupport;

        private void OnCallAirStrike()
        {
            float range = 30.0f;

            Ray inspector = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            RaycastHit hit;

            Debug.DrawRay(_mainCamera.transform.position, _mainCamera.transform.forward * 8, Color.green);

            if (Physics.Raycast(inspector, out hit, range) && !Pause.isAnInterfaceActive)
            {
                if(_input.callAirStrike)
                {

                    AirStrikeMarker marker = playerInventory.FindItem<AirStrikeMarker>();

                    if (marker==null)
                    {
                        playerInventory.OnUseFail("You do not have an Airstrike Flare on you.");
                        _input.callAirStrike = false;
                    }
                    else
                    {
                        if (Pause.airStrikeCooldown > 0)
                        {
                            playerInventory.OnUseFail("There is too much air traffic over New Isselville at the moment.");
                            _input.callAirStrike = false;
                        }
                        else
                        {
                            //"Ride into the Danger Zone" - https://www.youtube.com/watch?v=siwpn14IE7E (Kenny Loggins - Danger Zone (Official Video - Top Gun))
                            Collider[] charactersInDangerZone = Physics.OverlapSphere(hit.point, 5.5f);

                            int numOfConfirmedHits = 0;
                            int numOfConfirmedKills = 0;

                            //Every character in the danger zome will decrease health and armor
                            foreach(Collider col in charactersInDangerZone)
                            {
                                if(col.TryGetComponent<Character>(out Character target))
                                {
                                    if(target.health>0 && (target.faction != c.faction || (Pause.allowFriendlyFire && target.faction == c.faction)))
                                    {
                                        numOfConfirmedHits++;

                                        if (target.health - marker.splashDamage <= 0)
                                        {
                                            numOfConfirmedKills++;
                                        }

                                        target.decreaseHealthAndArmor(target.armor);
                                        target.decreaseTrueHealth(marker.splashDamage);
                                    }
                                }
                            }

                            //Random coordinates
                            float Xpos = Random.Range(-4, 4);
                            float Zpos = Random.Range(-4, 4);

                            float YPos = hit.point.y;

                            //BOOM F--KING BOOM BABY
                            Instantiate(explosionParticle, hit.point, Quaternion.identity);
                            AudioSource.PlayClipAtPoint(marker.explosionSound, transform.TransformPoint(_controller.center), 1);
                            for (int explodeCount = 0; explodeCount < 7; explodeCount++)
                            {
                                Instantiate(explosionParticle,hit.point+new Vector3(Xpos,YPos,Zpos),Quaternion.identity);
                            }

                            if (numOfConfirmedKills >= 5)
                            {
                                playerInventory.OnUseFail("Strike successful: " + numOfConfirmedHits + " target(s) hit, " + numOfConfirmedKills + " EKIA. Hell f-*static* yea! Crush them dirtbags!");
                            }
                            else if(numOfConfirmedHits >=1)
                            {
                                playerInventory.OnUseFail("Strike successful: " + numOfConfirmedHits + " target(s) hit, " + numOfConfirmedKills + " EKIA. Good work!");
                            }
                            else if(numOfConfirmedHits==0) {
                                playerInventory.OnUseFail("Strike unsuccessful: " + numOfConfirmedHits + " target(s) hit, " + numOfConfirmedKills + " EKIA. Mission failed! We'll get 'em next time!");
                            }

                            //Summon airplane | Not too close to the hitorigin

                            /*
                             
                            //The original plan was to get them to spawn in donuts, but the plane is often harder to see that way 
                            
                            float innerRadius = 45;
                            float outerRadius = 50;

                            float randomAngle = Random.Range(0,2*Mathf.PI);
                            float ratio = innerRadius / outerRadius;


                            float donut = Mathf.Sqrt(Random.Range(Mathf.Pow(ratio, 2), 1f)) * outerRadius;

                            float ringX = donut * Mathf.Cos(randomAngle);
                            float ringZ = donut * Mathf.Sin(randomAngle);
                            */
                            float skies = YPos + 13;

                            //Vector3 aircraftSpawnPoint = new Vector3(ringX+hit.point.x,skies,ringZ+hit.point.z);

                            Vector3 aircraftSpawnPoint = new Vector3(hit.point.x , skies, hit.point.z) + (-transform.forward*5);

                            Vector3 origin = new Vector3(hit.point.x, skies, hit.point.z);

                            Instantiate(airSupport, aircraftSpawnPoint, Quaternion.LookRotation(origin-aircraftSpawnPoint));

                            //Reset
                            Pause.airStrikeCooldown = 10;
                            playerInventory.RemoveItem(marker);
                            _input.callAirStrike = false;
                        }
                    }
                }
            }
            else
            {
                _input.callAirStrike = false;
            }
        }
    }
}