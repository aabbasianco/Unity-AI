using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("References")]
    public Transform cameraTransform; // Drag your Camera here in Inspector
    public LayerMask groundMask;
    public float groundCheckDistance = 0.2f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private float xRotation = 0f;

    private string currentAnimation = "";
    private Animator animator;

    float moveX;
    float moveZ;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked; // Lock mouse
        //ChangeAnimation("Walk Backwards");
        ChangeAnimation("Rig|Idle_Loop");
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        CheckAnimation();
    }

    void HandleMovement()
    {
        // --- Ground check ---
        isGrounded = Physics.CheckSphere(transform.position + Vector3.down * (controller.height / 2), groundCheckDistance, groundMask);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // --- Keyboard movement ---
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * movementSpeed * Time.deltaTime);

        // --- Jump ---
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        // --- Gravity ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void ChangeAnimation(string _animation, float _crossFade = .2f)
    {
        if (currentAnimation != _animation)
        {
            currentAnimation = _animation;
            animator.CrossFade(_animation, _crossFade);
        }
    }

    private void CheckAnimation()
    {
        if (moveZ > 0.1f) // حرکت به جلو
        {
            ChangeAnimation("Rig|Walk_Loop");
        }
        else if (moveZ < -0.1f) // حرکت به عقب
        {
            ChangeAnimation("Walking Backwards");
        }
        else // ایستادن
        {
            ChangeAnimation("Rig|Idle_Loop");
        }
    }

}
