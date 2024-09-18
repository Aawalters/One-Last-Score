using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerInput : MonoBehaviour
{
    // grappling
    [Header("Scripts Ref:")]
    public GrapplingRope grappleRope;
    public GrapplingGun grapplingGun;
    //

    // player movement
    public float moveSpeed;
    public float jumpForce;
    public Transform groundCheck;
    public LayerMask groundObjects;
    public Vector2 checkGroundSize; // width height of ground check box
    private Rigidbody2D rb;
    private bool facingRight;
    private float moveDirection;
    private bool isJumping = false;
    private bool isGrounded;
    private bool midJump;
    private GameObject currentOneWayPlatform;
    [SerializeField] private BoxCollider2D playerCollider;
    public float waitTime = 0f;
    //

    // attack tuning
    public GameObject attackPoint;
    public float attackRadius;
    public LayerMask enemyLayer;
    public int kickDamage = 1;
    public float kickForce = 0.5f;
    public float kickUpForce;
    public float maxKickForce; // clamping kick force
    [Range(0, 1)]
    public float movementForceMultiplier; // determines how much player & enemy velocity should affect kick force 
    public float movementUpForceMultiplier; // determines how much player & enemy velocity affect grounded upward kick force
    //

    // art audio
    public AudioSource audioSource;
    public AudioClip KickAudio;
    public AudioClip MissAudio;
    public Animator anim;
    //


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        facingRight = false;
        audioSource = GetComponent<AudioSource>();
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
        if (rb.velocity.y == 0) {
            midJump = false;
        }

        // if is Grounded and just ended midJump, then play landing animation
        
        // isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundObjects);
        isGrounded = Physics2D.OverlapBox(groundCheck.position, checkGroundSize, 0f, groundObjects) && !midJump;
        Move();
    }

    private void Move()
    {
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
        if (rb.velocity.x > .1f || rb.velocity.x < -.1f)
        {
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }

        if (isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(new Vector2(0f, jumpForce));
            midJump = true;
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
        // Normal Movement Input
        // scale of -1 -> 1
        moveDirection = Input.GetAxis("Horizontal");
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            midJump = true;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }

        }
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.K))
        {
            anim.SetBool("isKicking", true);
        }
        // Grappling hook Input
        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.J))
        {
            // grab
            grapplingGun.SetGrapplePoint();
        }
        if (grapplingGun.isGrappled)
        {
            // Player is connected but hasn't moved yet
            if (Input.GetKeyDown(KeyCode.Q))
            {
                grapplingGun.PullPlayer();  // Pull the player towards the grapple point
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("PULL ENEMY");
                grapplingGun.PullEnemy();  // Pull the enemy towards the player
            }
        }
        if (Input.GetKey(KeyCode.Mouse1) && grappleRope.enabled)
        {
            // rotation grappling
            grapplingGun.RotateGun(grapplingGun.grapplePoint);
        }
        if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.J))
        {
            // release
            grappleRope.enabled = false;
            grapplingGun.isGrappled = false;
            grapplingGun.m_springJoint2D.enabled = false;
            grapplingGun.ReleaseEnemy();
        }
        else
        {
            // rotation not grappling
            Vector2 mousePos = grapplingGun.m_camera.ScreenToWorldPoint(Input.mousePosition);
            grapplingGun.RotateGun(mousePos);
        }
    }

    public void Kick()
    {
        Collider2D[] enemyList = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, enemyLayer);
        Vector2 force;

        foreach (Collider2D enemyObject in enemyList)
        {
            Vector2 dir = enemyObject.transform.position - transform.position;
            dir.Normalize();
            float weightedForce = kickForce + (rb.velocity.magnitude + enemyObject.GetComponent<Rigidbody2D>().velocity.magnitude) * movementForceMultiplier;
            float weightedUpForce = kickUpForce * movementUpForceMultiplier;
            Debug.Log("on ground kick? " + isGrounded);
            // if isGrounded, add slight upward force, but don't multiply it by force
            if (isGrounded) {
                force = new Vector2(Mathf.Sign(dir.x) * weightedForce, weightedUpForce);
                if (math.abs(force.x) > maxKickForce) { // clamp x
                    force.x = force.x > 0 ? maxKickForce : maxKickForce * -1;
                }
            } else {
                dir = dir * weightedForce;
                force = Vector2.ClampMagnitude(dir * kickForce, maxKickForce); // clamp total force
            }
            Debug.Log("Force of player kick: " + force);
            // Debug.Log("Hit direction: " + dir);
            enemyObject.GetComponent<Enemy_Basic>().takeKick(kickDamage, force);
        }
        if (enemyList.Length == 0)
        {
            audioSource.clip = MissAudio;
            audioSource.Play();
        } else
        {
            audioSource.clip = KickAudio;
            audioSource.Play();
        }
    }

    public void EndKick()
    {
        anim.SetBool("isKicking", false);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = other.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("OneWayPlatform"))
        {
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.transform.position, attackRadius);
        // Gizmos.DrawWireSphere(groundCheck.transform.position, checkRadius);
        // isGrounded = Physics2D.OverlapBox(groundCheck.position, new Vector2(2, 2), 0f);
        Gizmos.DrawCube(groundCheck.position, checkGroundSize);
    }
}
