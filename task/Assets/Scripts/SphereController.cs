using UnityEngine;

public class SphereController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float acceleration = 0.5f; // Adjust as needed for how fast you want the sphere to accelerate
    private Rigidbody rb;

    // For checking if the sphere is grounded
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Constant forward movement
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, moveSpeed);

        // Check for jump
        bool jump = Input.GetKeyDown(KeyCode.UpArrow) && isGrounded;
        if (jump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Left/Right acceleration
        float moveDirection = Input.GetAxis("Horizontal");
        rb.AddForce(Vector3.right * moveDirection * acceleration, ForceMode.Acceleration);
    }

    // Check if the sphere is grounded using physics
    private void OnCollisionStay(Collision collision)
    {
        // This checks if the collision happened below the sphere (i.e., it's on the ground)
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) // mostly upward
            {
                isGrounded = true;
                return; 
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
