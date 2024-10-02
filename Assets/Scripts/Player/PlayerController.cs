using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;

public class PlayerController : MonoBehaviour
{

    public Player p;
    public DeckController deckController;
    public GameObject iOSPanel;
    public HashSet<IDamageable> iDamageableSet = new HashSet<IDamageable>();
    public bool shouldBeDamaging { get; private set; } = false;
    // art audio
    [Header("Art/Audio")]
    public AudioSource audioSource;
    public AudioClip KickAudio;
    public AudioClip MissAudio;
    public AudioClip CardPullAudio;
    public AudioClip GoodPullAudio;
    public AudioClip BadPullAudio;
    public AudioClip DeckShuffle;
    private MovementJoystick MovementJoystickScript;
    private ButtonsAndClick ButtonsAndClickScript;

    private void Awake()
    {
        // instance = transform.Find("Player Controller").GetComponent<PlayerController>();
        // DontDestroyOnLoad(this);
        //p = new Player();
    }

    // Start is called before the first frame update
    void Start()
    {
        p.GameManager.Wager();

        // setting defaults
        p.facingRight = false;
        p.healthCurrent = p.healthMax; // Set health to max at start
        p.healthBar.maxValue = p.healthMax;
        p.healthBar.value = p.healthCurrent;

        // accessing components
        p.rb = GetComponent<Rigidbody2D>();
        p.anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        //p.deckController = GetComponent<DeckController>();
        p.deck = p.deckController.GetNewDeck();

        if (iOSPanel.activeSelf)
        {
            MovementJoystickScript = iOSPanel.GetComponent<MovementJoystick>();
            ButtonsAndClickScript = iOSPanel.GetComponent<ButtonsAndClick>();
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
                directionPlayerFaces();
            }
        }
        if (p.cardIsOnCD) {
            ApplyCooldown();
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

        p.anim.SetBool("isWalking", Mathf.Abs(p.rb.velocity.x) > 0.1f);

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
            if ((aimDirection.x > 0 && !p.facingRight) || (aimDirection.x < 0 && p.facingRight)) {
                FlipCharacter();
            }
        }
        // Handle character flipping only based on movement when moving
        else if (p.moveDirection != 0)
        {
            if ((p.moveDirection > 0 && !p.facingRight) || (p.moveDirection < 0 && p.facingRight))
            {
                FlipCharacter();
            }
        }
        // If not moving, flip character based on aim direction
        else if ((aimDirection.x > 0 && !p.facingRight) || (aimDirection.x < 0 && p.facingRight))
        {
            FlipCharacter();
        }

    }

    private void directionPlayerFacesMobile()
    {
        Vector2 mousePos = Input.mousePosition;

        // Handle character flipping only based on movement when moving
        if (p.moveDirection != 0)
        {
            if ((p.moveDirection > 0 && !p.facingRight) || (p.moveDirection < 0 && p.facingRight))
            {
                FlipCharacter();
            }
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
        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.J)) p.grapplingGun.SetGrapplePoint();
        if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.J)) p.grapplingGun.pull();
        else if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.J)) p.grapplingGun.stopGrappling();
        // Pull enemies
        if (Input.GetKeyDown(KeyCode.E)) p.grapplingGun.PullEnemy();
        if (Input.GetKeyUp(KeyCode.E)) p.grapplingGun.StopPullingEnemy();
        //card drawing - TODO: ADD COOLDOWN (in battle manager maybe?)
        if (Input.GetKeyDown(KeyCode.F)) {
            useCard();
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
        p.moveDirection = MovementJoystickScript.joystickVec.x;
        
        if (ButtonsAndClickScript.isJumping && p.isGrounded)
        {
            p.isJumping = true;
            ButtonsAndClickScript.isJumping = false; // so that jumping is not spammed
        }
        if (MovementJoystickScript.joystickVec.normalized.y < -0.90)
        {
            if (p.currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }

        }

        // attacks
        if (ButtonsAndClickScript.isKicking)
        {
            p.anim.SetBool("isKicking", true);
            ButtonsAndClickScript.isKicking = false;
        }
        // Grappling hook Input
        //if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.J)) p.grapplingGun.SetGrapplePoint();
        if (ButtonsAndClickScript.pulling) p.grapplingGun.pull();
        //else if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.J)) p.grapplingGun.stopGrappling();

        if (ButtonsAndClickScript.drawCard)
        {
            useCard();
            ButtonsAndClickScript.drawCard = false;
        }
        if (ButtonsAndClickScript.pause)
        {
            p.GameManager.Pause();
            ButtonsAndClickScript.pause = false;
        }
    }

    private void useCard()
    {
        if (p.cardIsOnCD) { //don't do anything if the card is on CD
            return;
        } else {
            p.cardIsOnCD = true;
            p.cardCDTimer = p.cardCDTime;
            Card card = p.deckController.infinDrawCard(p.deck);
            StartCoroutine(playCardSound(card));
            card.use(p);
            p.CooldownImg.sprite = card.cardImage;
            p.StatusEffectManager.AddStatusEffect(card.effectImage);
        }
    }

    IEnumerator playCardSound(Card card)
    {
        audioSource.clip = CardPullAudio;
        audioSource.Play();
        yield return new WaitForSeconds(CardPullAudio.length);
        if (card.cardType == CardType.Multiplier || card.cardType == CardType.PlayerBuff) {
            audioSource.clip = GoodPullAudio;
        } else {
            audioSource.clip = BadPullAudio;
        }
        audioSource.Play();
    }

    void ApplyCooldown()
    {
        p.cardCDTimer -= Time.deltaTime;

        if (p.cardCDTimer < 0) {
            p.cardIsOnCD = false;
            p.cardCDTimer = 0;
            p.UICard.GetComponentInChildren<TextMeshProUGUI>().text = " ";
            audioSource.clip = DeckShuffle;
            audioSource.Play();
        } else {
            p.UICard.GetComponentInChildren<TextMeshProUGUI>().text = Mathf.RoundToInt(p.cardCDTimer).ToString();
            p.CooldownImg.fillAmount = p.cardCDTimer / p.cardCDTime;
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
        //if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        //{
        //    StartCoroutine(TakeDamage(5)); 
        //}
    }

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
    public IEnumerator TakeDamage(int damage)
    {
        if (!p.isHit) {
            p.isHit = true;
            p.healthCurrent -= damage;
            p.healthBar.value = p.healthCurrent;
            p.anim.SetBool("isHurt", true);
            p.anim.SetBool("isKicking", false); // if you get hurt, cancel kick (rewards precision maybe?)
            shouldBeDamaging = false;
            if (p.healthCurrent <= 0)
            {
                p.GameManager.Death();
            }
            yield return new WaitForSeconds(1f);
            p.isHit = false;
            p.anim.SetBool("isHurt", false);
        }
    }

    public void SetControls(bool status)
    {
        p.ControlsEnabled = status;
    }
}
