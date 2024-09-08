using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement1 : MonoBehaviour
{

    public float moveSpeed;
    public float jumpForce;
    public Transform groundCheck;
    public LayerMask groundObjects;
    public float checkRadius;
    private Rigidbody2D rb;
    private bool facingRight;
    private float moveDirection;
    private bool isJumping = false;
    private bool isGrounded;

    private GameObject currentOneWayPlatform;
    [SerializeField] private BoxCollider2D playerCollider;
    public float waitTime = 0f;
    public Animator anim;
    public GameObject attackPoint;
    public float attackRadius;
    public LayerMask enemyLayer;
    public int kickDamage = 1;
    public float kickForce = 0.5f;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        facingRight = false;
        
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
        if (rb.velocity.x > .1f || rb.velocity.x < -.1f) {
            anim.SetBool("isWalking", true);
        } else {
            anim.SetBool("isWalking", false);
        }

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

        if (Input.GetMouseButtonDown(0)) {
            anim.SetBool("isKicking", true);
        }
    }

    public void Kick() {
        Collider2D[] enemyList = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, enemyLayer);

        foreach (Collider2D enemyObject in enemyList) {
            Vector2 dir = enemyObject.transform.position - transform.position;
            dir.Normalize();
            enemyObject.GetComponent<Enemy_Basic>().takeKick(kickDamage, dir, kickForce);
        }
    }

    public void EndKick() {
        anim.SetBool("isKicking", false);
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

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(attackPoint.transform.position, attackRadius);
    }
}
