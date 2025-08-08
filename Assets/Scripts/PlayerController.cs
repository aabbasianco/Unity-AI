using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : AnimatorBrain
{
    [Header("Movement Settings")]
    public float movementSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

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

        Initialize(animator.layerCount, Animations.IDLE, animator, DefaultAnimation);

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
        HandleMovement();
        HandleMouseLook();

        CheckJumping();
        CheckShooting();
        CheckDeath();

        CheckTopAnimation();
        CheckBottomAnimation();
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
        }
        else if (movement.magnitude > 0.1f)
        {
            // Determine target animation based on dominant direction
            if (Mathf.Abs(moveZ) > Mathf.Abs(moveX))
            {
                targetAnimation = moveZ > 0 ? Animations.WALKFORWARD : Animations.WALKBACKWARD;
            }
            else
            {
                targetAnimation = moveX > 0 ? Animations.WALKRIGHT : Animations.WALKLEFT;
            }
            
            transitionTime = GetTransitionTime(currentAnim, targetAnimation);
        }
        else
        {
            targetAnimation = idleAnimations[currentIdle];
            transitionTime = GetTransitionTime(currentAnim, targetAnimation);
        }

        if (targetAnimation != Animations.NONE)
        {
            Play(targetAnimation, _layer, false, false, transitionTime);
        }
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
}
