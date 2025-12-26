using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.1f;

    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Ensure Rigidbody is set up correctly for a character
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Programmatic Physics Fix: Create Zero Friction material to prevent wall sticking
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            PhysicsMaterial zeroFrictionMat = new PhysicsMaterial("ZeroFriction");
            zeroFrictionMat.dynamicFriction = 0f;
            zeroFrictionMat.staticFriction = 0f;
            zeroFrictionMat.frictionCombine = PhysicsMaterialCombine.Minimum;
            zeroFrictionMat.bounceCombine = PhysicsMaterialCombine.Minimum;
            col.material = zeroFrictionMat;
        }
    }

    void Update()
    {
        HandleJump();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (rb == null) return;

        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        // Get camera forward/right vectors but flatten Y to avoid flying/digging into ground
        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            
            camForward.y = 0;
            camRight.y = 0;
            
            camForward.Normalize();
            camRight.Normalize();

            // Calculate movement direction relative to camera
            // Fix: If S is pressed (moveZ is negative), we want to move backwards relative to camera look
            Vector3 movement = (camForward * moveZ + camRight * moveX).normalized;

            if (movement.magnitude >= 0.1f)
            {
                // Move the character
                Vector3 targetVelocity = movement * moveSpeed;
                targetVelocity.y = rb.linearVelocity.y; // Preserve vertical velocity (gravity/jumping)
                rb.linearVelocity = targetVelocity;

                // Rotate character to face movement direction
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            }
            else
            {
                // Stop horizontal movement immediately when no input, but keep falling
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }
        }
    }

    void HandleJump()
    {
        // Simple ground check raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            // Calculate speed for blend tree or simple walking state
            float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
}
