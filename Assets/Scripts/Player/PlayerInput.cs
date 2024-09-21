using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class PlayerInput : MonoBehaviour
{
    // grappling
    [Header("Grappling")]
    public GrapplingRope grappleRope;
    public GrapplingGun grapplingGun;
    ///////////////////////
    public SpringJoint2D m_springJoint2D;
    public float XMaxSpeed = 20f;
    public float YMaxSpeed = 30f;
    private bool isGrappling = false;
    ///////////////////////

    // player movement
    [Header("Player Movement")]
    public float moveSpeed;
    public float jumpForce;
    public Transform groundCheck;
    public LayerMask groundObjects;
    public Vector2 checkGroundSize; // width height of ground check box
    private Rigidbody2D rb;
    public bool facingRight;
    private float moveDirection;
    private bool isJumping = false;
    private bool isGrounded;
    private bool midJump;
    private GameObject currentOneWayPlatform;
    [SerializeField] private BoxCollider2D playerCollider;
    public float waitTime = 0f;
    [Range(0, 1)]
    public float airControl; // degree of player air control
    [Range(0, 1)]
    public float grappleAirControl; // degree of player air grappling control
    [Range(0, 1)]
    public float friction; // degree of friction on surfaces (to counter natural momentum)
    //

    // attack tuning
    [Header("Attack Tuning")]
    public GameObject attackPoint;
    public float attackRadius;
    public LayerMask enemyLayer;
    public int kickDamage = 1;
    public float kickForce = 0.5f;
    public float kickUpForce;
    public float maxKickForce; // clamping kick force
    [Range(0, 1)]
    public float movementForceMultiplier; // determines how much player & enemy velocity should affect kick force 
    [Range(0, 1)]
    public float movementUpForceMultiplier; // determines how much player & enemy velocity affect grounded upward kick force
    public bool shouldBeDamaging { get; private set;} = false;
    private HashSet<IDamageable> iDamageableSet = new HashSet<IDamageable>();
    //

    // art audio
    [Header("Art/Audio")]
    public AudioSource audioSource;
    public AudioClip KickAudio;
    public AudioClip MissAudio;
    public Animator anim;
    //

    public int maxHealth = 100;    // Maximum health of the player
    public int currentHealth;      // Current health of the player
    public Slider healthBar;       // UI Slider for health bar
    public GameObject dieScreen;   // Reference to the die screen canvas


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
        currentHealth = maxHealth; // Set health to max at start
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
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
        // if !isGrounded and !isGrappling
        float adjustAirControl = 1;
        if (!isGrounded && !isGrappling) {
            adjustAirControl = airControl;
        } else if (isGrappling) {
            adjustAirControl = grappleAirControl;
        }
        float xVelocity = (rb.velocity.x * (!isGrounded ? 1 : friction)) + (moveDirection * moveSpeed * adjustAirControl);
        rb.velocity = new Vector2( Mathf.Clamp(xVelocity, -XMaxSpeed, XMaxSpeed),
            isGrappling ? Mathf.Clamp(rb.velocity.y, -YMaxSpeed, YMaxSpeed) : rb.velocity.y);

        anim.SetBool("isWalking", Mathf.Abs(rb.velocity.x) > 0.1f);

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
        //grapplingGun.SetSpring(isGrounded);
        if (Input.GetKeyDown(KeyCode.Mouse1)  || Input.GetKeyDown(KeyCode.J))
        {
            //isGrappling = true;
            grapplingGun.SetGrapplePoint();
        }
        if (Input.GetKey(KeyCode.Mouse1)  || Input.GetKey(KeyCode.J))
        {
            grapplingGun.pull();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.J))
        {
            //isGrappling = false;
            grapplingGun.stopGrappling();
        }
        //Pull Player
        //else if (Input.GetKey(KeyCode.Q))
        //{
        //    grapplingGun.pull();
        //}
        //else if (Input.GetKeyUp(KeyCode.Q))
        //{
        //    grapplingGun.stopPulling();
        //}
        //Pull Enemies
        if (Input.GetKeyDown(KeyCode.E))
        {
            grapplingGun.PullEnemy();
        }
        if (Input.GetKeyUp(KeyCode.E))

        {
            grapplingGun.StopPullingEnemy();
        }
    }

    public IEnumerator Kick()
    {
        shouldBeDamaging = true;
        Debug.Log("on ground kick? " + isGrounded);


        while (shouldBeDamaging) {
            Collider2D[] enemyList = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, enemyLayer);
            Vector2 force;

            foreach (Collider2D enemyObject in enemyList)
            {
                // calculate direction of force and factor in player and enemy velocity to strength
                Vector2 dir = enemyObject.transform.position - transform.position;
                dir.Normalize();
                float weightedForce = kickForce + (rb.velocity.magnitude + enemyObject.GetComponent<Rigidbody2D>().velocity.magnitude) * movementForceMultiplier;
                float weightedUpForce = kickUpForce + (rb.velocity.magnitude + enemyObject.GetComponent<Rigidbody2D>().velocity.magnitude) * movementUpForceMultiplier;

                // if isGrounded, add slight upward force, but don't multiply it by force
                // for both, clamp (maybe log max) force
                if (isGrounded) {
                    force = new Vector2(Mathf.Sign(dir.x) * weightedForce, weightedUpForce);
                    if (math.abs(force.x) > maxKickForce) { // clamp x
                        force.x = force.x > 0 ? maxKickForce : maxKickForce * -1;
                    }
                } else {
                    dir = dir * weightedForce;
                    force = Vector2.ClampMagnitude(dir * kickForce, maxKickForce); // clamp total force
                }

                // apply damage + force to enemy 
                IDamageable iDamageable = enemyObject.GetComponent<IDamageable>();
                if (iDamageable != null && !iDamageableSet.Contains(iDamageable)) {
                    Debug.Log("Force of player kick: " + force);

                    iDamageable.takeKick(kickDamage, force);
                    iDamageableSet.Add(iDamageable);
                }
            }
            yield return null; // wait a frame
        }

        // post active frame processing
        if (iDamageableSet.Count == 0) {
            audioSource.clip = MissAudio;
            audioSource.Play();
        } else {
            audioSource.clip = KickAudio;
            audioSource.Play();
        }
        iDamageableSet.Clear();
    }

    // set end of kick active frames
    public void EndShouldBeDamaging() {
        shouldBeDamaging = false;
    } 

    // set end of animation
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
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            TakeDamage(5); 
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

    public void FlipCharacter()
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

    // Function to take damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth; 
        if (currentHealth <= 0)
        {
            Die(); 
        }
    }

    void Die()
    {
        Debug.Log("DEATH");
        dieScreen.SetActive(true);
        this.gameObject.SetActive(false);

    }
}
