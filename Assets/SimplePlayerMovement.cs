using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed, jumpForce, moveDirection;
    public bool isJumping = false, isGrounded;
    public Transform groundCheck;
    public LayerMask groundObjects;
    public Vector2 checkGroundSize; // width height of ground check box
    public Rigidbody2D rb;
    public GameObject currentOneWayPlatform;
    [SerializeField] public BoxCollider2D playerCollider;
    public float waitTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    private void FixedUpdate() {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, checkGroundSize, 0f, groundObjects);
        Move();
    }

    public void ProcessInput() {
        // Normal Movement Input
        // scale of -1 -> 1
        moveDirection = Input.GetAxis("Horizontal");
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }

        }
    }

    private void Move()
    {
        
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        if (isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(new Vector2(0f, jumpForce));
        }
        isJumping = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = other.gameObject;
        }
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(playerCollider, platformCollider);
        yield return new WaitForSeconds(waitTime);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(groundCheck.position, checkGroundSize);
    }
}
