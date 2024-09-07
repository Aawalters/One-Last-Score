using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{

    public float moveSpeed;
    public float jumpForce;
    public Transform groundCheck;
    public LayerMask groundObjects;
    public float checkRadius;
    private Rigidbody2D rb;
    private bool facingRight = true;
    private float moveDirection;
    private bool isJumping = false;
    private bool isGrounded;

    private GameObject currentOneWayPlatform;
    [SerializeField] private BoxCollider2D playerCollider;
    public float waitTime = 0f;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
        Animate();
        // Debug.Log(isGrounded);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundObjects);
        Move();
    }

    private void Move()
    {
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
        if (isJumping) {
            rb.AddForce(new Vector2(0f, jumpForce));
        }
        isJumping = false;
    }

    private void Animate()
    {
        if (moveDirection > 0 && !facingRight)
        {
            FlipCharacter();
        }
        else if (moveDirection < 0 && facingRight)
        {
            FlipCharacter();
        }
    }

    private void ProcessInput()  
    {
        // scale of -1 -> 1
        moveDirection = Input.GetAxis("Horizontal");
        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
        } 
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            if (currentOneWayPlatform != null) 
            {
                StartCoroutine(DisableCollision());
            }

        }
    }

    private void  OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("OneWayPlatform")) {
            currentOneWayPlatform = other.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("OneWayPlatform")) {
            currentOneWayPlatform = null;
        }
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(playerCollider, platformCollider);
        yield return new WaitForSeconds(waitTime);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

    private void FlipCharacter()
    {
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }
}
