using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    public Player p;
    public HashSet<IDamageable> iDamageableSet = new HashSet<IDamageable>();
    public bool shouldBeDamaging { get; private set; } = false;

    private GameManager GM;

    private void Awake()
    {
        GM = p.GameManager;
        p.playerChargeMeter = GameObject.Find("Player Charge Meter");
        p.playerExtendedChargeMeter = GameObject.Find("Player Extended Charge Meter");
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

        // charge kick values
        p.playerChargeMeter.GetComponent<Slider>().value = 0;
        p.playerChargeMeter.SetActive(false); // hide
        p.playerChargeMeter.GetComponent<Slider>().maxValue = 1;

        p.playerExtendedChargeMeter.GetComponent<Slider>().value = 0; // reset value
        p.playerExtendedChargeMeter.SetActive(false); // hide
        p.forceIncrease = p.maxKickForce - p.baseKickForce;
        // below is setting seondary bar to be proportionally larger than original bar based on extended max force
        float extendedPercent = (p.extendedMaxKickForce - p.baseKickForce) / p.forceIncrease; // 1 + percent increase
        p.playerExtendedChargeMeter.GetComponent<Slider>().maxValue = extendedPercent;
        p.playerExtendedChargeMeter.GetComponent<RectTransform>().sizeDelta = new Vector2(
            p.playerChargeMeter.GetComponent<RectTransform>().sizeDelta.x * extendedPercent, 
            p.playerExtendedChargeMeter.GetComponent<RectTransform>().sizeDelta.y);

        p.kickChargeRate = 1f / p.maxChargeTime; // calc charge rate needed to reach desired time
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

        p.playerChargeMeter.GetComponent<Slider>().value = p.kickCharge; // just in case
        p.playerExtendedChargeMeter.GetComponent<Slider>().value = p.kickCharge;
    }

    private void FixedUpdate()
    {
        if (p.rb.velocity.y == 0) {
            p.midJump = false;
        }

        // if is Grounded and just ended midJump, then play landing animation
        p.isGrounded = Physics2D.OverlapBox(p.groundCheck.position, p.checkGroundSize, 0f, p.groundObjects) && !p.midJump;
        Move();

        if (p.charging) {
            p.kickCharge += (p.kickChargeRate * Time.deltaTime) + (p.rb.velocity.magnitude * p.movementChargeRateMultiplier);
            // if grappling, use extended max, else regular max
            float max = p.grapplingGun.isGrappling ? p.playerExtendedChargeMeter.GetComponent<Slider>().maxValue : 1f;
            p.kickCharge = Mathf.Clamp(p.kickCharge, 0, max);   
        }
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
            Mathf.Min(p.rb.velocity.y, p.YMaxSpeed));

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
        // can buffer kick
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.K) && !p.anim.GetBool("isKicking"))
        {
            p.anim.SetBool("isKicking", true);
        }
        // while not holding down button, set back to normal
        if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.K)) && p.charging && !p.isHit && p.anim.GetBool("isKicking")) {
            p.playerChargeMeter.SetActive(true);
            p.anim.speed = 0; // pause anim
        } else {
            p.charging = false;
            p.anim.speed = 1; // unpause anim
            p.playerChargeMeter.SetActive(false);
        }

        // Grappling hook Input
        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.J)) 
        { 
            p.grapplingGun.SetGrapplePoint(); 
            //p.anim.SetBool("midJump", true); 
        }
        if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.J)) {
            p.grapplingGun.pull();
            p.playerExtendedChargeMeter.SetActive(true);
        } else if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.J)) { 
            p.grapplingGun.stopGrappling(); 
            //p.anim.SetBool("midJump", false); 
        } else {
            p.playerExtendedChargeMeter.SetActive(false);
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

    public void StartCharging() {
        p.charging = true;
    }

    // kick active frames
    public IEnumerator Kick()
    {
        shouldBeDamaging = true;
        // calculate kick force
        int dir = p.facingRight ? 1 : -1;
        float chargeIncrease = p.kickCharge * p.forceIncrease;
        float weightedXForce = dir * (p.baseKickForce + chargeIncrease);
        Vector2 force = new Vector2(weightedXForce, 0);
        float weightedYForce = p.kickUpForce + (chargeIncrease * p.chargeUpForceMultiplier);

        // if grounded or moving up, kick upward, else downward
        force.y = ((p.isGrounded || p.rb.velocity.y > 0) ? 1 : -1) * weightedYForce;

        float kickRadius = p.kickRadius;
        if (p.kickCharge > 1f) { // during grapple
            float t = Mathf.Clamp((p.kickCharge - 1f) / (p.playerExtendedChargeMeter.GetComponent<Slider>().maxValue - 1f), 0f, 1f);
            kickRadius = Mathf.Lerp(p.kickRadius, p.extendedChargeRadius, t);
        }

        while (shouldBeDamaging) {
            Collider2D[] enemyList = Physics2D.OverlapCircleAll(p.kickPoint.transform.position, kickRadius, p.enemyLayer);

            foreach (Collider2D enemyObject in enemyList)
            {
                // calculate direction of force and factor in player velocity to overall power
                // Vector2 dir = enemyObject.transform.position - transform.position;
                // dir.Normalize();
                // float weightedForce = p.kickForce + (p.rb.velocity.magnitude + enemyObject.GetComponent<Rigidbody2D>().velocity.magnitude) * p.movementForceMultiplier;
                // float weightedUpForce = p.kickUpForce + (p.rb.velocity.magnitude + enemyObject.GetComponent<Rigidbody2D>().velocity.magnitude) * p.movementUpForceMultiplier;

                // apply damage + force to enemy 
                IDamageable iDamageable = enemyObject.GetComponent<IDamageable>();
                if (iDamageable != null && !iDamageableSet.Contains(iDamageable)) {
                    iDamageable.TakeKick(p.kickDamage, force);
                    iDamageable.StopAttack(); // cancel enemy attack
                    iDamageableSet.Add(iDamageable);
                }
            }
            yield return null; // wait a frame
        }

        // post active-frame processing
        if (iDamageableSet.Count == 0) {
            GM.audioSource.clip = GM.MissAudio;
            GM.audioSource.Play();
        } else {
            GM.audioSource.clip = GM.KickAudio;
            GM.audioSource.Play();

            if (force.magnitude > p.hitStopForceThreshold) {
                StartCoroutine(GM.HitStop(force.magnitude * p.hitStopScaling));
                StartCoroutine(GM.ScreenShake(force.magnitude * p.hitStopScaling, force.magnitude * p.screenShakeScaling));
            }
        }
        iDamageableSet.Clear();
    }

    // set end of kick active frames
    public void EndShouldBeDamaging() {
        shouldBeDamaging = false;
        p.kickCharge = 0; // reset charge
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
        float kickRadius = p.kickRadius;
        if (p.kickCharge > 1f) { // during grapple
            float t = Mathf.Clamp((p.kickCharge - 1f) / (p.playerExtendedChargeMeter.GetComponent<Slider>().maxValue - 1f), 0f, 1f);
            kickRadius = Mathf.Lerp(p.kickRadius, p.extendedChargeRadius, t);
        }
        Gizmos.DrawWireSphere(p.kickPoint.transform.position, kickRadius);
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

            // if you get hurt, cancel kick
            p.anim.SetBool("isKicking", false); 
            EndShouldBeDamaging();
            p.playerChargeMeter.SetActive(false);

            p.rb.velocity = Vector2.zero; // so previous velocity doesn't interfere (would super stop player momentum tho? maybe change in future)
            p.rb.AddForce(force, ForceMode2D.Impulse);

            if (GM.healthCurrent <= 0) {
                p.GameManager.Death();
            } else {
                yield return new WaitForSeconds(p.iFrames);
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
