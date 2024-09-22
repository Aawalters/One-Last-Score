using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{

    public Player p;
    public DeckController deckController;
    public HashSet<IDamageable> iDamageableSet = new HashSet<IDamageable>();
    public bool shouldBeDamaging { get; private set;} = false;
    // art audio
    [Header("Art/Audio")]
    public AudioSource audioSource;
    public AudioClip KickAudio;
    public AudioClip MissAudio;

    private void Awake()
    {
        // instance = transform.Find("Player Controller").GetComponent<PlayerController>();
        // DontDestroyOnLoad(this);
        //p = new Player();
    }

    // Start is called before the first frame update
    void Start()
    {
        // setting defaults
        p.facingRight = false;
        p.healthCurrent = p.healthMax; // Set health to max at start
        p.healthBar.maxValue = p.healthMax;
        p.healthBar.value = p.healthCurrent;

        // accessing components
        p.rb = GetComponent<Rigidbody2D>();
        p.anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
        Animate();
    }

    private void FixedUpdate()
    {
        if (p.rb.velocity.y == 0) {
            p.midJump = false;
        }

        // if is Grounded and just ended midJump, then play landing animation
        
        p.isGrounded = Physics2D.OverlapBox(p.groundCheck.position, p.checkGroundSize, 0f, p.groundObjects) && !p.midJump;
        Move();
    }

    private void Move()
    {
        // if !isGrounded and !isGrappling
        float adjustAirControl = 1;
        if (!p.isGrounded && !p.isGrappling) {
            adjustAirControl = p.airControl;
        } else if (!p.isGrounded && p.isGrappling) {
            adjustAirControl = p.grappleAirControl;
        }
        // if in air, maintain prev x for momentum, add additoinal movement with restriction (adjustAirControl)
        float xVelocity = (p.rb.velocity.x * (!p.isGrounded ? 1 : p.friction)) + (p.moveDirection * p.moveSpeed * adjustAirControl);
        p.rb.velocity = new Vector2( Mathf.Clamp(xVelocity, -p.XMaxSpeed, p.XMaxSpeed),
            Mathf.Clamp(p.rb.velocity.y, -p.YMaxSpeed, p.YMaxSpeed));

        p.anim.SetBool("isWalking", Mathf.Abs(p.rb.velocity.x) > 0.1f);

        if (p.isJumping)
        {
            p.rb.velocity = new Vector2(p.rb.velocity.x, 0f);
            p.rb.AddForce(new Vector2(0f, p.jumpForce));
            p.midJump = true;
        }
        p.isJumping = false;
    }

    private void Animate()
    {
        if (p.moveDirection > 0 && !p.facingRight)
        {
            FlipCharacter();
        }
        else if (p.moveDirection < 0 && p.facingRight)
        {
            FlipCharacter();
        }
    }

    private void ProcessInput()
    {
        // Normal Movement Input
        // scale of -1 -> 1
        p.moveDirection = Input.GetAxis("Horizontal");
        if (Input.GetButtonDown("Jump") && p.isGrounded)
        {
            p.isJumping = true;
            p.midJump = true;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (p.currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }

        }
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.K))
        {
            p.anim.SetBool("isKicking", true);
        }
        // Grappling hook Input
        //grapplingGun.SetSpring(isGrounded);
        if (Input.GetKeyDown(KeyCode.Mouse1)  || Input.GetKeyDown(KeyCode.J))
        {
            //isGrappling = true;
            p.grapplingGun.SetGrapplePoint();
        }
        if (Input.GetKey(KeyCode.Mouse1)  || Input.GetKey(KeyCode.J))
        {
            p.grapplingGun.pull();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.J))
        {
            //isGrappling = false;
            p.grapplingGun.stopGrappling();
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
            p.grapplingGun.PullEnemy();
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            p.grapplingGun.StopPullingEnemy();
        }
    }

    public IEnumerator Kick()
    {
        shouldBeDamaging = true;
        Debug.Log("on ground kick? " + p.isGrounded);

        while (shouldBeDamaging) {
            Collider2D[] enemyList = Physics2D.OverlapCircleAll(p.attackPoint.transform.position, p.attackRadius, p.enemyLayer);
            Vector2 force;

            foreach (Collider2D enemyObject in enemyList)
            {
                // calculate direction of force and factor in player and enemy velocity to strength
                Vector2 dir = enemyObject.transform.position - transform.position;
                dir.Normalize();
                float weightedForce = p.kickForce + (p.rb.velocity.magnitude + enemyObject.GetComponent<Rigidbody2D>().velocity.magnitude) * p.movementForceMultiplier;
                float weightedUpForce = p.kickUpForce + (p.rb.velocity.magnitude + enemyObject.GetComponent<Rigidbody2D>().velocity.magnitude) * p.movementUpForceMultiplier;

                // if isGrounded, add slight upward force, but don't multiply it by force
                // for both, clamp (maybe log max) force
                if (p.isGrounded) {
                    force = new Vector2(Mathf.Sign(dir.x) * weightedForce, weightedUpForce);
                    if (math.abs(force.x) > p.maxKickForce) { // clamp x
                        force.x = force.x > 0 ? p.maxKickForce : p.maxKickForce * -1;
                    }
                } else {
                    dir = dir * weightedForce;
                    force = Vector2.ClampMagnitude(dir * p.kickForce, p.maxKickForce); // clamp total force
                }

                // apply damage + force to enemy 
                IDamageable iDamageable = enemyObject.GetComponent<IDamageable>();
                if (iDamageable != null && !iDamageableSet.Contains(iDamageable)) {
                    Debug.Log("Force of player kick: " + force);

                    iDamageable.takeKick(p.kickDamage, force);
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
        p.anim.SetBool("isKicking", false);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("OneWayPlatform"))
        {
            p.currentOneWayPlatform = other.gameObject;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            TakeDamage(5); 
        }
    }

    // private void OnCollisionExit2D(Collision2D other)
    // {
    //     if (other.gameObject.CompareTag("OneWayPlatform"))
    //     {
    //         p.currentOneWayPlatform = null;
    //     }
    // }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = p.currentOneWayPlatform.GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(p.playerCollider, platformCollider);
        yield return new WaitForSeconds(p.waitTime);
        Physics2D.IgnoreCollision(p.playerCollider, platformCollider, false);
    }

    public void FlipCharacter()
    {
        p.facingRight = !p.facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(p.attackPoint.transform.position, p.attackRadius);
        // Gizmos.DrawWireSphere(groundCheck.transform.position, checkRadius);
        // isGrounded = Physics2D.OverlapBox(groundCheck.position, new Vector2(2, 2), 0f);
        Gizmos.DrawCube(p.groundCheck.position, p.checkGroundSize);
    }

    // Function to take damage
    public void TakeDamage(int damage)
    {
        p.healthCurrent -= damage;
        p.healthBar.value = p.healthCurrent; 
        if (p.healthCurrent <= 0)
        {
            Die(); 
        }
    }

    void Die()
    {
        Debug.Log("DEATH");
        p.dieScreen.SetActive(true);
        gameObject.SetActive(false);
    }
}
