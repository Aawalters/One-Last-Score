using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Player p;
    public HashSet<IDamageable> iDamageableSet = new HashSet<IDamageable>();
    public bool shouldBeDamaging { get; private set; } = false;

    private GameManager GM;

    private void Awake()
    {
        GM = p.GameManager;
    }

    // Start is called before the first frame update
    void Start()
    {
        // GM.Wager();

        // setting defaults
        p.facingRight = false;

        GM.healthCurrent = GM.healthMax; // Set health to max at start
        GM.healthBar.maxValue = GM.healthMax;
        GM.healthBar.value = GM.healthCurrent;

        // accessing components
        p.rb = GetComponent<Rigidbody2D>();
        p.anim = GetComponent<Animator>();

        //p.deckController = GetComponent<DeckController>();
        GM.deckController.currentDeck = GM.deckController.GetNewDeck();

        if (GM.iOSPanel.activeSelf)
        {
            p.MovementJoystickScript = GM.iOSPanel.GetComponent<MovementJoystick>();
            p.ButtonsAndClickScript = GM.iOSPanel.GetComponent<ButtonsAndClick>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (p.ControlsEnabled)
        {
            if (p.GameManager.mobile)
            {
                ProcessInputMobile();
                p.anim.SetBool("midJump", p.midJump);
                directionPlayerFacesMobile();
            } else
            {
                ProcessInput();
                p.anim.SetBool("midJump", p.midJump);
                if (p.grapplingGun.isGrappling)
                {
                    p.anim.SetBool("midJump", true);
                }
                directionPlayerFaces();
            }
        }
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
        // if grapping, lower grav
        // otherwise, if falling set higher grav for snappiness
        p.rb.gravityScale = (p.rb.velocity.y < 0) ? p.fallingGravity : p.defaultGravity;
        if (p.grapplingGun.isGrappling) p.rb.gravityScale = 1f;

        // if !isGrounded and !isGrappling
        float adjustAirControl = 1;
        if (!p.isGrounded && !p.grapplingGun.isGrappling) {
            adjustAirControl = p.airControl;
        } else if (!p.isGrounded && p.grapplingGun.isGrappling) {
            adjustAirControl = p.grappleAirControl;
        }
        // if in air/grappling, maintain prev x for momentum (else add ground friction), add directed movement with air control restrictions
        float xVelocity = (p.rb.velocity.x * (!p.isGrounded || p.grapplingGun.isGrappling ? 1 : p.friction))
            + (p.moveDirection * p.moveSpeed * adjustAirControl);
        p.rb.velocity = new Vector2(Mathf.Clamp(xVelocity, -p.XMaxSpeed, p.XMaxSpeed),
            Mathf.Clamp(p.rb.velocity.y, -p.YMaxSpeed, p.YMaxSpeed));

        if (p.isGrounded)
        {
            p.anim.SetBool("isWalking", Mathf.Abs(p.rb.velocity.x) > 0.1f);
        }

        if (p.isJumping)
        {
            p.rb.velocity = new Vector2(p.rb.velocity.x, 0f);
            p.rb.AddForce(new Vector2(0f, p.jumpForce));
            p.midJump = true;
        }
        p.isJumping = false;
    }

    private void directionPlayerFaces()
    {
        Vector2 mousePos = p.grapplingGun.m_camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (mousePos - (Vector2)p.grapplingGun.firePoint.position).normalized;

        // when kicking, face player towards cursor to make attack easier
        if (p.anim.GetBool("isKicking")) {
            FlipCharacter(aimDirection.x > 0);
        }
        // Handle character flipping only based on movement when moving
        else if (p.moveDirection != 0) {
            FlipCharacter(p.moveDirection > 0);
        }
        // If not moving, flip character based on aim direction
        else {
            FlipCharacter(aimDirection.x > 0);
        }

    }

    private void directionPlayerFacesMobile()
    {
        Vector2 mousePos = Input.mousePosition;

        // Handle character flipping only based on movement when moving
        if (p.moveDirection != 0)
        {
            FlipCharacter(p.moveDirection > 0);
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
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (p.currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }

        }

        // attacks
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.K))
        {
            p.anim.SetBool("isKicking", true);
        }
        // Grappling hook Input
        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.J)) 
        { 
            p.grapplingGun.SetGrapplePoint(); 
            //p.anim.SetBool("midJump", true); 
        }
        if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.J)) p.grapplingGun.pull();
        else if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.J)) 
        { 
            p.grapplingGun.stopGrappling(); 
            //p.anim.SetBool("midJump", false); 
        }
        // Pull enemies
        if (Input.GetKeyDown(KeyCode.E)) p.grapplingGun.PullEnemy();
        if (Input.GetKeyUp(KeyCode.E)) p.grapplingGun.StopPullingEnemy();
        //card drawing - TODO: ADD COOLDOWN (in battle manager maybe?)
        if (Input.GetKeyDown(KeyCode.F)) {
            GM.useCard();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            p.GameManager.Pause();
        }
    }

    private void ProcessInputMobile()
    {
        // Normal Movement Input
        // scale of -1 -> 1
        p.moveDirection = p.MovementJoystickScript.joystickVec.x;
        
        if (p.ButtonsAndClickScript.isJumping && p.isGrounded)
        {
            p.isJumping = true;
            p.ButtonsAndClickScript.isJumping = false; // so that jumping is not spammed
        }
        if (p.MovementJoystickScript.joystickVec.normalized.y < -0.90)
        {
            if (p.currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }

        }

        // attacks
        if (p.ButtonsAndClickScript.isKicking)
        {
            p.anim.SetBool("isKicking", true);
            p.ButtonsAndClickScript.isKicking = false;
        }
        // Grappling hook Input
        //if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.J)) p.grapplingGun.SetGrapplePoint();
        if (p.ButtonsAndClickScript.pulling) p.grapplingGun.pull();
        //else if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.J)) p.grapplingGun.stopGrappling();

        if (p.ButtonsAndClickScript.drawCard)
        {
            GM.useCard();
            p.ButtonsAndClickScript.drawCard = false;
        }
        if (p.ButtonsAndClickScript.pause)
        {
            p.GameManager.Pause();
            p.ButtonsAndClickScript.pause = false;
        }
    }

    // kick active frames
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

                // if isGrounded, add slight upward force but don't multiply upward force
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

                    iDamageable.TakeKick(p.kickDamage, force);
                    iDamageable.StopAttack(); // cancel enemy attack
                    iDamageableSet.Add(iDamageable);
                }
            }
            yield return null; // wait a frame
        }

        // post active frame processing
        if (iDamageableSet.Count == 0) {
            GM.audioSource.clip = GM.MissAudio;
            GM.audioSource.Play();
        } else {
            GM.audioSource.clip = GM.KickAudio;
            GM.audioSource.Play();
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
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = p.currentOneWayPlatform.GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(p.playerCollider, platformCollider);
        yield return new WaitForSeconds(p.waitTime);
        Physics2D.IgnoreCollision(p.playerCollider, platformCollider, false);
    }

    public void FlipCharacter(bool right)
    {
        // storing whether object is already facingRight to avoid double flipping
        if (right != p.facingRight) {
            p.facingRight = !p.facingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(p.attackPoint.transform.position, p.attackRadius);
        // Gizmos.DrawWireSphere(groundCheck.transform.position, checkRadius);
        // isGrounded = Physics2D.OverlapBox(groundCheck.position, new Vector2(2, 2), 0f);
        Gizmos.DrawCube(p.groundCheck.position, p.checkGroundSize);
    }

    // Function to take damage + iframes + knockback
    public IEnumerator TakeDamage(int damage, Vector2 force)
    {
        if (!p.isHit) {
            p.isHit = true;
            GM.healthCurrent -= damage;
            GM.healthBar.value = GM.healthCurrent;
            p.anim.SetBool("isHurt", true);
            p.anim.SetBool("isKicking", false); // if you get hurt, cancel kick (rewards precision maybe?)
            shouldBeDamaging = false;

            p.rb.velocity = Vector2.zero; // so previous velocity doesn't interfere (would super stop player momentum tho? maybe change in future)
            p.rb.AddForce(force, ForceMode2D.Impulse);

            if (GM.healthCurrent <= 0) {
                p.GameManager.Death();
            } else {
                yield return new WaitForSeconds(p.iFrames); // hitstun (i-frames?), should make sep later
                p.isHit = false;
                p.anim.SetBool("isHurt", false);
            }
        }
    }

    public void SetControls(bool status)
    {
        p.ControlsEnabled = status;
    }
}
