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
    private bool isGrounded;

    private float xRotation = 0f;

    private Animator animator;

    private float moveX;
    private float moveZ;

    private int currentIdle = 0;
    private const int UPPERBODY = 0;
    private const int LOWERBODY = 1;

    private readonly Animations[] idleAnimations =
    {
        Animations.IDLE1,
        Animations.IDLE2,
        Animations.IDLE3
    };

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        Initialize(animator.layerCount, Animations.IDLE1, animator, DefaultAnimation);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(ChangeIdle());

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
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();

        CheckDeath();
        //CheckJumping();
        //CheckShooting();
        CheckTopAnimation();
        CheckBottomAnimation();
    }

    void CheckDeath()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Play(Animations.DEATH, UPPERBODY, true, true);
            Play(Animations.DEATH, LOWERBODY, true, true);
        }
    }

    void HandleMovement()
    {
        // --- Ground Check ---
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2 - controller.skinWidth + 0.05f);
        isGrounded = Physics.CheckSphere(spherePosition, groundCheckDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Keeps character grounded
        }

        // --- Input Movement ---
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;
        controller.Move(move * movementSpeed * Time.deltaTime);

        // --- Jump ---
        if (Input.GetButtonDown("Jump") && isGrounded)
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
        CheckMovementAnimations(UPPERBODY);
    }

    private void CheckBottomAnimation()
    {
        CheckMovementAnimations(LOWERBODY);
    }

    private void CheckMovementAnimations(int _layer)
    {
        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized;

        if (!isGrounded)
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
}
