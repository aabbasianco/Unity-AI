using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : AnimatorBrain
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f; // This will be calculated dynamically
    public float mouseSensitivity = 2f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    
    [Header("Speed Presets")]
    public float slowWalkSpeed = 2f;
    public float normalWalkSpeed = 3f;
    public float fastWalkSpeed = 5f;
    public float normalRunSpeed = 7f;
    public float fastRunSpeed = 8.5f;
    public float normalSprintSpeed = 9.5f;
    public float fastSprintSpeed = 10.5f;
    
    [Header("Scene Movement Configuration")]
    [SerializeField] private SpeedStyle walkSpeedStyle = SpeedStyle.Normal;
    [SerializeField] private SpeedStyle runSpeedStyle = SpeedStyle.Normal;
    [SerializeField] private SpeedStyle sprintSpeedStyle = SpeedStyle.Normal;
    
    [Header("Movement State")]
    [SerializeField] private MovementMode currentMovementMode = MovementMode.Walk;
    [SerializeField] private float movementSpeed = 5f; // Current calculated speed
    
    [Header("Modifiers")]
    [SerializeField] private bool isInjured = false;
    [SerializeField] private float injuredSpeedReduction = 0.3f; // 30% speed reduction when injured
    
    [Header("Input Settings")]
    [SerializeField] private bool holdShiftToRun = true; // If false, Left Shift toggles between modes
    
    [Header("Movement Speed Thresholds")]
    [SerializeField] private float walkThreshold = 3f;     // Below this = walk
    [SerializeField] private float runThreshold = 8f;      // Between walk and run = run  
    [SerializeField] private float sprintThreshold = 9.5f;  // Above run = sprint
    
    [Header("8-Directional Settings")]
    [SerializeField] private float diagonalThreshold = 0.5f; // Threshold for diagonal movement detection
    
    [Header("Layer Weight Transition")]
    [SerializeField] private float layerTransitionSpeed = 5f; // How fast layers transition

    [Header("Animation Transition Settings")]
    [SerializeField] private float defaultTransitionTime = 0.2f;
    [SerializeField] private float sameAxisTransitionTime = 0.1f;  // Forward/Back or Left/Right
    [SerializeField] private float crossAxisTransitionTime = 0.3f; // Forward/Back to Left/Right or vice versa
    [SerializeField] private float idleToMovementTime = 0.15f;
    [SerializeField] private float movementToIdleTime = 0.25f;
    [Header("References")]
    public Transform cameraTransform;
    public LayerMask groundMask;

    public float groundCheckDistance = 0.3f;

    private CharacterController controller;
    private Vector3 velocity;

    private float xRotation = 0f;

    private Animator animator;

    private float moveX;
    private float moveZ;

    private int currentIdle = 0;
    private const int UPPERBODY = 0;
    private const int LOWERBODY = 1;
    private const int WALKLAYER = 1;    // Assuming you'll create these layers
    private const int RUNLAYER = 2;     // in your animator controller
    private const int SPRINTLAYER = 3;

    public bool shooting = false;
    public bool jumping = false;

    private readonly Animations[] idleAnimations =
    {
        Animations.IDLE
        //Animations.IDLE2,
        //Animations.IDLE3
    };

    public static PlayerController instance;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Initialize with the actual number of layers in the animator
        int layerCount = Mathf.Max(animator.layerCount, 4); // Ensure we have at least 4 layers
        Initialize(layerCount, Animations.IDLE, animator, DefaultAnimation);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(ChangeIdle());
    }

    IEnumerator ChangeIdle()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            ++currentIdle;
            if (currentIdle >= idleAnimations.Length)
            {
                currentIdle = 0;
            }
        }
    }

    void Update()
    {
        HandleMovementInput();
        HandleMovement();
        HandleMouseLook();

        CheckJumping();
        CheckShooting();
        CheckDeath();

        CheckTopAnimation();
        CheckBottomAnimation();
    }
    
    void HandleMovementInput()
    {
        // Handle Left Shift input for run/sprint
        if (holdShiftToRun)
        {
            // Hold to run/sprint mode
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // Cycle between run and sprint on additional presses
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    if (currentMovementMode == MovementMode.Walk)
                        currentMovementMode = MovementMode.Run;
                    else if (currentMovementMode == MovementMode.Run)
                        currentMovementMode = MovementMode.Sprint;
                }
            }
            else
            {
                currentMovementMode = MovementMode.Walk;
            }
        }
        else
        {
            // Toggle mode on shift press
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                switch (currentMovementMode)
                {
                    case MovementMode.Walk:
                        currentMovementMode = MovementMode.Run;
                        break;
                    case MovementMode.Run:
                        currentMovementMode = MovementMode.Sprint;
                        break;
                    case MovementMode.Sprint:
                        currentMovementMode = MovementMode.Walk;
                        break;
                }
            }
        }
        
        // Calculate current movement speed
        CalculateMovementSpeed();
    }
    
    void CalculateMovementSpeed()
    {
        // Get base speed based on current movement mode and scene configuration
        float baseSpeed = GetBaseSpeedForMode(currentMovementMode);
        
        // Apply modifiers
        float finalSpeed = ApplySpeedModifiers(baseSpeed);
        
        // Update movement speeds
        movementSpeed = finalSpeed;
        walkSpeed = finalSpeed; // Keep walkSpeed in sync for compatibility
    }
    
    float GetBaseSpeedForMode(MovementMode mode)
    {
        switch (mode)
        {
            case MovementMode.Walk:
                return GetSpeedByStyle(walkSpeedStyle, slowWalkSpeed, normalWalkSpeed, fastWalkSpeed);
            case MovementMode.Run:
                return GetSpeedByStyle(runSpeedStyle, normalRunSpeed, normalRunSpeed, fastRunSpeed);
            case MovementMode.Sprint:
                return GetSpeedByStyle(sprintSpeedStyle, normalSprintSpeed, normalSprintSpeed, fastSprintSpeed);
            default:
                return normalWalkSpeed;
        }
    }
    
    float GetSpeedByStyle(SpeedStyle style, float slowSpeed, float normalSpeed, float fastSpeed)
    {
        switch (style)
        {
            case SpeedStyle.Slow:
                return slowSpeed;
            case SpeedStyle.Normal:
                return normalSpeed;
            case SpeedStyle.Fast:
                return fastSpeed;
            default:
                return normalSpeed;
        }
    }
    
    float ApplySpeedModifiers(float baseSpeed)
    {
        float modifiedSpeed = baseSpeed;
        
        // Apply injured state modifier
        if (isInjured)
        {
            modifiedSpeed *= (1f - injuredSpeedReduction);
        }
        
        // Add more modifiers here as needed
        // Example: if (isEncumbered) modifiedSpeed *= 0.8f;
        
        return modifiedSpeed;
    }

    void CheckDeath()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Play(Animations.DEATH, UPPERBODY, true, true);
            Play(Animations.DEATH, LOWERBODY, true, true);
            shooting = false;
        }
    }

    void CheckJumping()
    {
        if (!jumping && grounded && Input.GetKeyDown(KeyCode.Space))
        {
            Play(Animations.JUMPUP, LOWERBODY, true, false);
            //rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
            jumping = true;
            //jump sound
        }
    }

    void CheckShooting()
    {
        if (Input.GetKeyDown(KeyCode.R)) Play(Animations.PISTOLRELOAD, UPPERBODY, true, false);
        shooting = GetCurrentAnimations(UPPERBODY) != Animations.PISTOLRELOAD && Input.GetKey(KeyCode.Mouse0);
    }

    void HandleMovement()
    {
        // --- Ground Check ---
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2 - controller.skinWidth + 0.05f);
        grounded = Physics.CheckSphere(spherePosition, groundCheckDistance, groundMask);

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f; // Keeps character grounded
        }

        // --- Input Movement ---
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;
        controller.Move(move * movementSpeed * Time.deltaTime);

        // --- Jump ---
        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // --- Apply Gravity ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- Update Animations ---
        //CheckMovementAnimations(LOWERBODY);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Horizontal rotation
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Update top-layer animations if needed
        //CheckTopAnimation();
    }

    private void CheckTopAnimation()
    {
        if (shooting)
        {
            Play(Animations.PISTOLSHOOT, UPPERBODY, false, false);
            return;
        }
        CheckMovementAnimations(UPPERBODY);
    }

    private void CheckBottomAnimation()
    {
        CheckMovementAnimations(LOWERBODY);
    }

    private void CheckMovementAnimations(int _layer)
    {
        Vector3 movement = new Vector3(moveX, 0f, moveZ);
        Animations currentAnim = GetCurrentAnimations(_layer);
        Animations targetAnimation = Animations.NONE;
        float transitionTime = defaultTransitionTime;

        if (!grounded)
        {
            targetAnimation = Animations.JUMPAIR;
            transitionTime = GetTransitionTime(currentAnim, targetAnimation);
            Play(targetAnimation, _layer, false, false, transitionTime);
            
            // Reset all movement layer weights when jumping
            if (_layer == LOWERBODY)
            {
                SetLayerWeight(WALKLAYER, 0f);
                SetLayerWeight(RUNLAYER, 0f);
                SetLayerWeight(SPRINTLAYER, 0f);
            }
        }
        else if (movement.magnitude > 0.1f)
        {
            // Determine movement direction
            Animations walkAnim, runAnim, sprintAnim;
            GetMovementAnimations(out walkAnim, out runAnim, out sprintAnim);
            
            if (_layer == UPPERBODY)
            {
                // Upper body selects animation based on current movement speed
                Animations upperBodyAnimation = GetUpperBodyAnimation(walkAnim, runAnim, sprintAnim);
                targetAnimation = upperBodyAnimation;
                transitionTime = GetTransitionTime(currentAnim, targetAnimation);
                Play(targetAnimation, _layer, false, false, transitionTime);
            }
            else if (_layer == LOWERBODY)
            {
                // Lower body - play animations on all movement layers and update weights
                PlayMovementAnimationsOnAllLayers(walkAnim, runAnim, sprintAnim);
                UpdateMovementLayerWeights();
            }
        }
        else
        {
            targetAnimation = idleAnimations[currentIdle];
            transitionTime = GetTransitionTime(currentAnim, targetAnimation);
            Play(targetAnimation, _layer, false, false, transitionTime);
            
            // Reset all movement layer weights when idle
            if (_layer == LOWERBODY)
            {
                SetLayerWeight(WALKLAYER, 0f);
                SetLayerWeight(RUNLAYER, 0f);
                SetLayerWeight(SPRINTLAYER, 0f);
            }
        }
    }
    
    private void GetMovementAnimations(out Animations walkAnim, out Animations runAnim, out Animations sprintAnim)
    {
        // Normalize input values
        float normalizedX = Mathf.Abs(moveX);
        float normalizedZ = Mathf.Abs(moveZ);
        
        // Determine if this is diagonal movement
        bool isDiagonal = normalizedX > diagonalThreshold && normalizedZ > diagonalThreshold;
        
        if (isDiagonal)
        {
            // 8-Directional: Handle diagonal movement
            if (moveZ > 0) // Forward diagonals
            {
                if (moveX > 0) // Forward-Right
                {
                    walkAnim = Animations.WALKFORWARDRIGHT;
                    runAnim = Animations.RUNFORWARDRIGHT;
                    sprintAnim = Animations.SPRINTFORWARDRIGHT;
                }
                else // Forward-Left
                {
                    walkAnim = Animations.WALKFORWARDLEFT;
                    runAnim = Animations.RUNFORWARDLEFT;
                    sprintAnim = Animations.SPRINTFORWARDLEFT;
                }
            }
            else // Backward diagonals
            {
                if (moveX > 0) // Backward-Right
                {
                    walkAnim = Animations.WALKBACKWARDRIGHT;
                    runAnim = Animations.RUNBACKWARDRIGHT;
                    sprintAnim = Animations.SPRINTBACKWARDRIGHT;
                }
                else // Backward-Left
                {
                    walkAnim = Animations.WALKBACKWARDLEFT;
                    runAnim = Animations.RUNBACKWARDLEFT;
                    sprintAnim = Animations.SPRINTBACKWARDLEFT;
                }
            }
        }
        else
        {
            // 4-Directional: Handle cardinal directions
            if (normalizedZ > normalizedX)
            {
                if (moveZ > 0) // Forward
                {
                    walkAnim = Animations.WALKFORWARD;
                    runAnim = Animations.RUNFORWARD;
                    sprintAnim = Animations.SPRINTFORWARD;
                }
                else // Backward
                {
                    walkAnim = Animations.WALKBACKWARD;
                    runAnim = Animations.RUNBACKWARD;
                    sprintAnim = Animations.SPRINTBACKWARD;
                }
            }
            else
            {
                if (moveX > 0) // Right
                {
                    walkAnim = Animations.WALKRIGHT;
                    runAnim = Animations.RUNRIGHT;
                    sprintAnim = Animations.SPRINTRIGHT;
                }
                else // Left
                {
                    walkAnim = Animations.WALKLEFT;
                    runAnim = Animations.RUNLEFT;
                    sprintAnim = Animations.SPRINTLEFT;
                }
            }
        }
    }
    
    private Animations GetUpperBodyAnimation(Animations walkAnim, Animations runAnim, Animations sprintAnim)
    {
        // Upper body uses discrete animation selection based on speed thresholds
        MovementType currentMovement = GetCurrentMovementType();
        
        switch (currentMovement)
        {
            case MovementType.Walk:
                return walkAnim;
            case MovementType.Run:
                return runAnim;
            case MovementType.Sprint:
                return sprintAnim;
            default:
                return walkAnim;
        }
    }
    
    private void PlayMovementAnimationsOnAllLayers(Animations walkAnim, Animations runAnim, Animations sprintAnim)
    {
        // Play corresponding animations on each movement layer
        Animations currentWalkAnim = GetCurrentAnimations(WALKLAYER);
        Animations currentRunAnim = GetCurrentAnimations(RUNLAYER);
        Animations currentSprintAnim = GetCurrentAnimations(SPRINTLAYER);
        
        float transitionTime = defaultTransitionTime;
        
        // Only play if animation is different to avoid unnecessary crossfades
        if (currentWalkAnim != walkAnim)
        {
            Play(walkAnim, WALKLAYER, false, false, transitionTime);
        }
        
        if (currentRunAnim != runAnim)
        {
            Play(runAnim, RUNLAYER, false, false, transitionTime);
        }
        
        if (currentSprintAnim != sprintAnim)
        {
            Play(sprintAnim, SPRINTLAYER, false, false, transitionTime);
        }
    }
    
    private void UpdateMovementLayerWeights()
    {
        float walkWeight = 0f;
        float runWeight = 0f;
        float sprintWeight = 0f;
        
        if (movementSpeed <= walkThreshold)
        {
            // Pure walk
            walkWeight = 1f;
        }
        else if (movementSpeed <= runThreshold)
        {
            // Blend from walk to run
            float blendFactor = (movementSpeed - walkThreshold) / (runThreshold - walkThreshold);
            walkWeight = 1f - blendFactor;
            runWeight = blendFactor;
        }
        else if (movementSpeed <= sprintThreshold)
        {
            // Blend from run to sprint
            float blendFactor = (movementSpeed - runThreshold) / (sprintThreshold - runThreshold);
            runWeight = 1f - blendFactor;
            sprintWeight = blendFactor;
        }
        else
        {
            // Pure sprint
            sprintWeight = 1f;
        }
        
        // Smoothly transition layer weights
        float currentWalkWeight = GetLayerWeight(WALKLAYER);
        float currentRunWeight = GetLayerWeight(RUNLAYER);
        float currentSprintWeight = GetLayerWeight(SPRINTLAYER);
        
        SetLayerWeight(WALKLAYER, Mathf.Lerp(currentWalkWeight, walkWeight, layerTransitionSpeed * Time.deltaTime));
        SetLayerWeight(RUNLAYER, Mathf.Lerp(currentRunWeight, runWeight, layerTransitionSpeed * Time.deltaTime));
        SetLayerWeight(SPRINTLAYER, Mathf.Lerp(currentSprintWeight, sprintWeight, layerTransitionSpeed * Time.deltaTime));
        
        // Debug information (remove this later if not needed)
        if (Input.GetKey(KeyCode.LeftControl)) // Hold Left Ctrl to see debug info (changed from Shift since Shift is used for movement)
        {
            MovementType currentType = GetCurrentMovementType();
            string direction = GetCurrentDirection();
            string modifiers = isInjured ? " [INJURED]" : "";
            Debug.Log($"Mode: {currentMovementMode} | Speed: {movementSpeed:F1}{modifiers} | Dir: {direction} | Type: {currentType} | Walk: {GetLayerWeight(WALKLAYER):F2} | Run: {GetLayerWeight(RUNLAYER):F2} | Sprint: {GetLayerWeight(SPRINTLAYER):F2}");
        }
    }
    
    private string GetCurrentDirection()
    {
        float normalizedX = Mathf.Abs(moveX);
        float normalizedZ = Mathf.Abs(moveZ);
        
        if (normalizedX < 0.1f && normalizedZ < 0.1f) return "Idle";
        
        bool isDiagonal = normalizedX > diagonalThreshold && normalizedZ > diagonalThreshold;
        
        if (isDiagonal)
        {
            if (moveZ > 0)
                return moveX > 0 ? "Forward-Right" : "Forward-Left";
            else
                return moveX > 0 ? "Backward-Right" : "Backward-Left";
        }
        else
        {
            if (normalizedZ > normalizedX)
                return moveZ > 0 ? "Forward" : "Backward";
            else
                return moveX > 0 ? "Right" : "Left";
        }
    }
    
    private MovementType GetCurrentMovementType()
    {
        if (movementSpeed <= walkThreshold) 
            return MovementType.Walk;
        else if (movementSpeed <= runThreshold) 
            return MovementType.Run;
        else 
            return MovementType.Sprint;
    }
    
    private enum MovementType
    {
        Walk,
        Run,
        Sprint
    }
    
    public enum MovementMode
    {
        Walk,
        Run,
        Sprint
    }
    
    public enum SpeedStyle
    {
        Slow,
        Normal,
        Fast
    }

    private float GetTransitionTime(Animations fromAnimation, Animations toAnimation)
    {
        // If transitioning to/from idle
        if (IsIdleAnimation(fromAnimation) && !IsIdleAnimation(toAnimation))
            return idleToMovementTime;
        
        if (!IsIdleAnimation(fromAnimation) && IsIdleAnimation(toAnimation))
            return movementToIdleTime;

        // If both are movement animations
        if (IsMovementAnimation(fromAnimation) && IsMovementAnimation(toAnimation))
        {
            // Same axis transition (forward/back or left/right)
            if (IsSameAxisTransition(fromAnimation, toAnimation))
                return sameAxisTransitionTime;
            
            // Cross axis transition (forward/back to left/right or vice versa)
            if (IsCrossAxisTransition(fromAnimation, toAnimation))
                return crossAxisTransitionTime;
        }

        return defaultTransitionTime;
    }

    private bool IsIdleAnimation(Animations animation)
    {
        return animation == Animations.IDLE /*|| animation == Animations.IDLE2 || animation == Animations.IDLE3*/;
    }

    private bool IsMovementAnimation(Animations animation)
    {
        return animation == Animations.WALKFORWARD || animation == Animations.WALKBACKWARD || 
               animation == Animations.WALKLEFT || animation == Animations.WALKRIGHT;
    }

    private bool IsSameAxisTransition(Animations from, Animations to)
    {
        // Forward/Backward transitions
        bool isForwardBackTransition = (from == Animations.WALKFORWARD || from == Animations.WALKBACKWARD) &&
                                      (to == Animations.WALKFORWARD || to == Animations.WALKBACKWARD);
        
        // Left/Right transitions  
        bool isLeftRightTransition = (from == Animations.WALKLEFT || from == Animations.WALKRIGHT) &&
                                    (to == Animations.WALKLEFT || to == Animations.WALKRIGHT);
        
        return isForwardBackTransition || isLeftRightTransition;
    }

    private bool IsCrossAxisTransition(Animations from, Animations to)
    {
        // Forward/Backward to Left/Right
        bool isForwardBackToLeftRight = (from == Animations.WALKFORWARD || from == Animations.WALKBACKWARD) &&
                                       (to == Animations.WALKLEFT || to == Animations.WALKRIGHT);
        
        // Left/Right to Forward/Backward
        bool isLeftRightToForwardBack = (from == Animations.WALKLEFT || from == Animations.WALKRIGHT) &&
                                       (to == Animations.WALKFORWARD || to == Animations.WALKBACKWARD);
        
        return isForwardBackToLeftRight || isLeftRightToForwardBack;
    }

    void DefaultAnimation(int _layer)
    {
        if (_layer == UPPERBODY) CheckTopAnimation();
        else CheckBottomAnimation();
    }
    
    // Public methods for external control
    public void SetMovementMode(MovementMode mode)
    {
        currentMovementMode = mode;
        CalculateMovementSpeed();
    }
    
    public MovementMode GetMovementMode()
    {
        return currentMovementMode;
    }
    
    public void SetInjuredState(bool injured)
    {
        isInjured = injured;
        CalculateMovementSpeed();
    }
    
    public bool GetInjuredState()
    {
        return isInjured;
    }
    
    public void SetSceneSpeedStyles(SpeedStyle walkStyle, SpeedStyle runStyle, SpeedStyle sprintStyle)
    {
        walkSpeedStyle = walkStyle;
        runSpeedStyle = runStyle;
        sprintSpeedStyle = sprintStyle;
        CalculateMovementSpeed();
    }
    
    public float GetCurrentEffectiveSpeed()
    {
        return movementSpeed;
    }
    
    public string GetMovementInfo()
    {
        string modifiers = isInjured ? " [INJURED]" : "";
        return $"Mode: {currentMovementMode} | Speed: {movementSpeed:F1}{modifiers} | Walk Style: {walkSpeedStyle} | Run Style: {runSpeedStyle} | Sprint Style: {sprintSpeedStyle}";
    }
}
