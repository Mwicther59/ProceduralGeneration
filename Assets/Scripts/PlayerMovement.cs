using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;            // Movement speed
    public float gravity = -9.81f;      // Gravity affecting the player
    public float jumpHeight = 1.5f;     // Jump height

    public Transform groundCheck;       // Reference to an empty GameObject at player's feet
    public float groundDistance = 0.4f; // Radius for the ground check
    public LayerMask groundMask;        // Which layers count as ground

    private CharacterController controller;
    private Vector3 velocity;           // Player's velocity for applying gravity
    private bool isGrounded;            // Whether the player is touching the ground

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Reset vertical velocity if the player is grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        // Jumping logic
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
