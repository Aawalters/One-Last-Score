using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Player
{
    [Header("Health")]
    public int healthCurrent;       // Current health of the player
    public int healthMax = 100;     // Maximum health of the player
    public Slider healthBar;       // UI Slider for health bar
    public bool isHit = false;

    public int attackCharge;
    public int healCharge;
    public int bleedValue;

    [Header("Screens")]
    public DeathScreen DeathScreen;   // Reference to the die screen script

    [Header("Cards")]
    public DeckController deckController;
    public Deck deck;
    public Image UICard;
    public Image CooldownImg;
    public GameEnemyManager GameEnemyManager;
    public StatusEffectManager StatusEffectManager;
    public float cardCDTime = 5.0f, cardCDTimer = 0;
    public bool cardIsOnCD = false;

    [Header("Status")]
    public float wager = 500;
    public float multiplier = 1f; // baseline need 5x multiplier? so 2500, nerf to like 2300

    public Animator anim;
    
    // grappling
    [Header("Grappling")]
    public GrapplingRope grappleRope;
    public GrapplingGun grapplingGun;
    public SpringJoint2D m_springJoint2D;
    public float XMaxSpeed = 20f, YMaxSpeed = 30f;

    // player movement
    [Header("Player Movement")]
    public float moveSpeed, jumpForce, moveDirection;
    public bool isJumping = false, facingRight, isGrounded, midJump;
    public float defaultGravity = 2.5f; // base gravity when jumping
    public float fallingGravity = 3.5f; // set higher gravity when falling for less floatiness
    public Transform groundCheck;
    public LayerMask groundObjects;
    public Vector2 checkGroundSize; // width height of ground check box
    public Rigidbody2D rb;
    public GameObject currentOneWayPlatform;
    [SerializeField] public BoxCollider2D playerCollider;
    public float waitTime = 0f;
    [Range(0, 1)]
    public float airControl, grappleAirControl, friction; 
    // degree of player air control, air grappling control, and friction on surfaces
    
    // attack tuning
    [Header("Attack Tuning")]
    public GameObject attackPoint;
    public float kickForce = 0.5f, kickUpForce, maxKickForce, attackRadius; //max is clamping kick force
    public LayerMask enemyLayer;
    public int kickDamage = 1;
    [Range(0, 1)]
    public float movementForceMultiplier; // determines how much player & enemy velocity should affect kick force 
    [Range(0, 1)]
    public float movementUpForceMultiplier; // determines how much player & enemy velocity affect grounded upward kick force
}