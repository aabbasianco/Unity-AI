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
        Animations.IDLE1,
        Animations.IDLE2,
        Animations.IDLE3
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

        Initialize(animator.layerCount, Animations.IDLE1, animator, DefaultAnimation);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(ChangeIdle());
    }

    // --- New ChangeIdle: waits until the current idle animation completes (one full cycle) ---
    IEnumerator ChangeIdle()
    {
        while (true)
        {
            // Play the current idle (ensures animator is in that state)
            Play(idleAnimations[currentIdle], LOWERBODY, false, false, .1f);

            // obtain the target state's shortNameHash
            int targetHash = Animator.StringToHash(GetAnimName(idleAnimations[currentIdle]));

            // --- wait until the animator actually transitions into that state (current or next) ---
            bool targetEntered = false;
            while (!targetEntered)
            {
                if (animator == null) yield break;

                var current = animator.GetCurrentAnimatorStateInfo(LOWERBODY);
                var next = animator.GetNextAnimatorStateInfo(LOWERBODY);

                if (current.shortNameHash == targetHash || next.shortNameHash == targetHash)
                {
                    targetEntered = true;
                    break;
                }

                yield return null;
            }

            // --- now wait until that state's normalizedTime >= 1 (one full cycle) ---
            bool finished = false;
            while (!finished)
            {
                if (animator == null) yield break;

                var current = animator.GetCurrentAnimatorStateInfo(LOWERBODY);
                var next = animator.GetNextAnimatorStateInfo(LOWERBODY);

                // if target is in 'next' (during transition / about to be current)
                if (next.shortNameHash == targetHash)
                {
                    // wait until next.normalizedTime >= 1 (one full loop)
                    if (next.normalizedTime >= 1f)
                    {
                        finished = true;
                        break;
                    }
                }
                // if target is already the current state
                else if (current.shortNameHash == targetHash)
                {
                    if (current.normalizedTime >= 1f)
                    {
                        finished = true;
                        break;
                    }
                }
                else
                {
                    // target no longer active (got interrupted by another animation) -> restart waiting
                    break;
                }

                yield return null;
            }

            if (finished)
            {
                // advance to next idle only if we actually finished the clip
                ++currentIdle;
                if (currentIdle >= idleAnimations.Length) currentIdle = 0;
            }
            else
            {
                // interrupted: don't advance currentIdle, just loop and wait again
            }

            // small yield to avoid tight-looping before next Play call
            yield return null;
        }
    }

    // helper to map Animations enum to animator state names (must match your Animator states)
    private string GetAnimName(Animations a)
    {
        switch (a)
        {
            case Animations.IDLE1: return "Idle 1";
            case Animations.IDLE2: return "Idle 2";
            case Animations.IDLE3: return "Idle 3";
            case Animations.WALKFWD: return "Walk Forward";
            case Animations.WALKBWD: return "Walk Backward";
            case Animations.WALKLEFT: return "Walk Left";
            case Animations.WALKRIGHT: return "Walk Right";
            case Animations.RUNFWD: return "Run Forward";
            case Animations.RUNBWD: return "Run Backward";
            case Animations.JUMPSTART: return "Jump Start";
            case Animations.JUMPAIR: return "Jump Air";
            case Animations.JUMPLAND: return "Jump Land";
            case Animations.PISTOLSHOOT: return "Pistol Shoot";
            case Animations.PISTOLRELOAD: return "Pistol Reload";
            case Animations.DEATH: return "Death";
            // add other mappings as needed
            default: return a.ToString();
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
            Play(Animations.JUMPSTART, LOWERBODY, true, false);
            jumping = true;
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
        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized;

        if (!grounded)
        {
            Play(Animations.JUMPAIR, _layer, false, false);
        }
        else if (movement.magnitude > 0f)
        {
            if (moveZ > 0)
                Play(Animations.WALKFWD, _layer, false, false);
            else if (moveZ < 0)
                Play(Animations.WALKBWD, _layer, false, false);
            else if (moveX > 0)
                Play(Animations.WALKRIGHT, _layer, false, false);
            else if (moveX < 0)
                Play(Animations.WALKLEFT, _layer, false, false);
        }
        else
        {
            Play(idleAnimations[currentIdle], _layer, false, false);
        }
    }

    void DefaultAnimation(int _layer)
    {
        if (_layer == UPPERBODY) CheckTopAnimation();
        else CheckBottomAnimation();
    }

    // optional: draw ground-check sphere in scene view for debugging
    private void OnDrawGizmosSelected()
    {
        if (controller == null) return;
        Gizmos.color = grounded ? Color.green : Color.red;
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2 - controller.skinWidth + 0.05f);
        Gizmos.DrawWireSphere(spherePosition, groundCheckDistance);
    }
}
