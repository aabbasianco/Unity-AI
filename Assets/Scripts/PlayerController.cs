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
    public float groundCheckDistance = 0.2f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private float xRotation = 0f;

    private string currentAnimation = "";
    private Animator animator;

    float moveX;
    float moveZ;

    private int currentIdle = 0;

    private const int UPPERBODY = 0;
    private const int LOWERBODY = 1;

    void Start()
    {
        Initialize(GetComponent<Animator>().layerCount, Animations.IDLE1, GetComponent<Animator>(), DefaultAnimation);

        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
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
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

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

    private void CheckTopAnimation()
    {

    }

    private void CheckBottomAnimation()
    {

    }

    void DefaultAnimation(int _layer)
    {

    }
}
