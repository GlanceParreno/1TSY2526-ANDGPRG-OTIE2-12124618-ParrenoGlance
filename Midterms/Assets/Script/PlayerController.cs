using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 6.5f;
    [Range(0f, 1f)] public float airControlMultiplier = 0.6f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;

    // --- Double Jump System ---
    private bool canDoubleJump = false;
    private bool hasDoubleJumped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        // --- Ground Check ---
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // --- Input ---
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // --- Air Control ---
        float control = isGrounded ? 1f : airControlMultiplier;
        Vector3 velocity = rb.linearVelocity;
        Vector3 targetVelocity = move * moveSpeed * control;
        targetVelocity.y = velocity.y;

        rb.linearVelocity = targetVelocity;
    }

    private void HandleJump()
    {
        // Update ground status
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Reset double jump when grounded
        if (isGrounded && hasDoubleJumped)
        {
            hasDoubleJumped = false;
        }

        // --- Jump Input ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // --- Normal Jump ---
            if (isGrounded)
            {
                Jump();
                hasDoubleJumped = false;
            }
            // --- Double Jump (also works when falling) ---
            else if (canDoubleJump && !hasDoubleJumped)
            {
                // Reset Y velocity for consistent feel
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

                // Slightly stronger if falling, slightly weaker if rising
                float forceMultiplier = rb.linearVelocity.y < 0 ? 1.1f : 0.9f;
                Jump(forceMultiplier);

                hasDoubleJumped = true;
                Debug.Log("💨 Double Jump!");
            }
        }

        // Optional: add stronger gravity for snappy jumps/falls
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * 15f);
        }
    }

    private void Jump(float multiplier = 1f)
    {
        rb.AddForce(Vector3.up * jumpForce * multiplier, ForceMode.Impulse);
    }

    // --- Called by the Blue Coin Power-Up ---
    public void EnableDoubleJump()
    {
        canDoubleJump = true;
        Debug.Log("💙 Double Jump Power-Up Enabled!");
    }

    // --- Debug visualization in Scene View ---
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
