using System.Collections;
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

    private int currentIdle = 0;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked; // Lock mouse
        //ChangeAnimation("Walk Backwards");
        ChangeAnimation("Idle 1");

        StartCoroutine(ChangeIdle());
    }

    IEnumerator ChangeIdle()
    {
        while (true)
        {
            // Wait until the current idle animation finishes
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
                && animator.GetCurrentAnimatorStateInfo(0).IsName(currentAnimation));

            // Switch to next idle
            ++currentIdle;
            if (currentIdle >= 3)
                currentIdle = 0;

            // Change idle animation
            CheckIdle();

            // Wait a little (optional delay between idles)
            yield return new WaitForSeconds(0.2f);
        }
    }


    void Update()
    {
        HandleMovement();
        HandleMouseLook();

        if (isGrounded && Input.GetKeyDown(KeyCode.Mouse0))
        {
            ChangeAnimation("Rig|Punch_Cross");
        }

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
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            ChangeAnimation("Rig|Jump_Start");
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

    public void ChangeAnimation(string _animation, float _crossFade = .1f, float _time = 0f)
    {
        if (_time > 0) StartCoroutine(Wait());
        else Validate();

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(_time - _crossFade);
            Validate();
        }

        void Validate()
        {
            if (currentAnimation != _animation)
            {
                currentAnimation = _animation;

                if (currentAnimation == "")
                    CheckAnimation();
                else
                    animator.CrossFade(_animation, _crossFade);
            }
        }
    }

    private void CheckAnimation()
    {
        if (currentAnimation == "Rig|Jump_Start" || currentAnimation == "Rig|Punch_Cross" || currentAnimation == "Rig|Jump_Land")
            return;
        if (currentAnimation == "Rig|Jump_Loop")
        {
            if (isGrounded)
                ChangeAnimation("Rig|Jump_Land");

            return;
        }

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
            CheckIdle();
        }
    }

    void CheckIdle()
    {
        switch (currentIdle)
        {
            case 0:
                ChangeAnimation("Idle 1");
                break;
            case 1:
                ChangeAnimation("Idle 2");
                break;
            case 2:
                ChangeAnimation("Idle 3");
                break;
            default:
                break;
        }
    }

}
