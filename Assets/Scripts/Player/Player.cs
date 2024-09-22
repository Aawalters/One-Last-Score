using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Player
{

    public int healthCurrent;       // Current health of the player
    public int healthMax = 100;     // Maximum health of the player
    public Slider healthBar;       // UI Slider for health bar

    public int attackCharge;
    public int healCharge;
    public int bleedValue;

    public GameObject dieScreen;   // Reference to the die screen canvas

    public DeckController deckController;
    public Deck deck;

    public Animator anim;
    
    // grappling
    [Header("Grappling")]
    public GrapplingRope grappleRope;
    public GrapplingGun grapplingGun;
    ///////////////////////
    public SpringJoint2D m_springJoint2D;
    public float XMaxSpeed = 20f, YMaxSpeed = 30f;
    public bool isGrappling = false;
    ///////////////////////

    // player movement
    [Header("Player Movement")]
    public float moveSpeed, jumpForce, moveDirection;
    public bool isJumping = false, facingRight, isGrounded, midJump;
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