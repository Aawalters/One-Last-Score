using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy_Basic : MonoBehaviour, IDamageable
{
    [Header("State")]
    public int maxHealth = 1000;
    public int currentHealth;
    private Animator anim;
    private Rigidbody2D rb;
    private GameManager gameManager;
    private bool facingRight;

    [Header("Collision Tuning")]
    public bool inImpact = false;
    public float maxVelocity; // don't want enemies to break game speed

    [Tooltip("Force threshold needed to cause damage and impact state, if not met then return to normal control")]
    public float collisionForceThreshold;

    [Range(0, 1)]
    [Tooltip("Determines how much collision force is factored into impact damage")]
    public float collisionDamageMultiplier;

    [Range(0, 1)]
    [Tooltip("Determines how much impact force is factored into bounce")]
    public float collisionForceMultiplier;

    public float mass;
    [Range(0, 1)]
    [Tooltip("Affects enemy floatiness during Impact State (for easier juggles)")]
    public float postImpactMassScale;

    [Header("AI")]
    // set properties
    public Transform player;
    public float chaseSpeed = 2f;
    public LayerMask groundLayer;
    public bool aiEnabled = false;
    public float stoppingDistance = 2f; // threshold for how player can be before moving
    public float jumpXForce;
    public float maxJumpYForce; 
    private float bodyGravity; 

    // state
    public bool isGrounded;
    public bool shouldJump;
    public bool midJump;
    public Vector2 direction;

    // detection checks
    public float topOfEnemy; // used for 'isXabove' detection
    private Vector3 topEnemyTransform;
    public float maxYJumpForce;
    public float maxXJumpForce;
    private float maxJumpHeight; // threshold for y jump check for platforms
    private float maxJumpDistance; // threshold for x jump check for platforms
    /*
    walk towards player
    if player above and platform within radius
    */
    public float bottomOfEnemy; // used for 'isXbelow' detection
    private Vector3 bottomEnemyTransform;

    [Header("Knockback Path Tracer")]
    public float pointSpacing = 0.5f;  // Minimum distance between recorded points
    private LineRenderer lineRenderer;
    private Vector3 lastRecordedPosition;  // Last recorded position to avoid redundant points

    // Start is called before the first frame update
    void Start()
    {
        // accessing components
        anim = transform.Find("Sprite").GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        gameManager = FindObjectOfType<GameManager>(); // Reference the GameManager in the scene

        // setting properties
        facingRight = false;
        currentHealth = maxHealth;
        topEnemyTransform = transform.position + (Vector3.up * topOfEnemy);
        bottomEnemyTransform = transform.position + (Vector3.down * bottomOfEnemy);
        bodyGravity = Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale;
        maxJumpHeight = Mathf.Pow(maxYJumpForce, 2) / (2 * bodyGravity); // Calculate max height AI can jump
        float timeToApex = maxYJumpForce / bodyGravity;
        maxJumpDistance = maxXJumpForce * timeToApex; // Calculate the max horizontal distance AI can jump

        // Initialize the last recorded position
        lastRecordedPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() {
        // record path during impact
        if (inImpact) {
            rb.mass = mass * postImpactMassScale;
            if (Vector3.Distance(transform.position, lastRecordedPosition) > pointSpacing) {
                AddPointToPath(transform.position);
            }
        } else {
            rb.mass = mass;
        }

        // if low velocity, then no longer inImpact
        if (rb.velocity.magnitude < collisionForceThreshold) {
            inImpact = false;
            ClearPath();
            anim.SetBool("ImpactBool", false);
        }

        enemyAI(aiEnabled);

        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
    }

    // basic AI script
    private void enemyAI(bool enabled) {
        if (enabled) {
            if (rb.velocity.y == 0) {
                midJump = false;
            }

            isGrounded = Physics2D.Raycast(bottomEnemyTransform, Vector2.down * .3f, groundLayer) && !midJump;
            direction = player.position - transform.position;  
            int xDirection = direction.x == 0 ? 0 : (direction.x > 0 ? 1 : -1);

            if (isGrounded && !inImpact) { // grounded and in control abilities
                // if out of reach, walk towards player, otherwise idle
                bool isOutOfReach = Math.Abs(direction.x) > stoppingDistance;
                rb.velocity = new Vector2(isOutOfReach ? xDirection * chaseSpeed : 0, rb.velocity.y);
                FlipCharacter(isOutOfReach ? xDirection > 0 : facingRight); // Maintain direction if idle
                anim.SetBool("isWalking", isOutOfReach);

                if (shouldJump) {
                    rb.velocity = new Vector2(xDirection * jumpXForce, 0f);
                    rb.AddForce(CalculateJumpForce(new Vector2(-1.5f,-2f)), ForceMode2D.Impulse);
                    shouldJump = false;
                    midJump = true;
                }
                // Player above and platform above checks
                // Vector2 upwardVector = Vector2.up * 3f;
                // bool isPlayerAbove = Physics2D.Raycast(topEnemyTransform, upwardVector, 1 << player.gameObject.layer);
                // RaycastHit2D platformAbove = Physics2D.Raycast(topEnemyTransform, upwardVector, groundLayer);
            }

            if (midJump && rb.velocity.y < 0) { // if falling from a jump, regain control
                Debug.Log("regain control from jump");
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
            // isGrounded = Physics2D.Raycast(bottomEnemyTransform, Vector2.down * .3f, groundLayer);
            // direction = Mathf.Sign(player.position.x - transform.position.x); 
            // // bool isPlayerBelow = Physics2D.Raycast(transform.position, Vector2.up, 4f, 1 << player.gameObject.layer);

            // if (isGrounded && !inImpact) {
            //     rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
            //     FlipCharacter(direction > 0);
            //     Vector2 directionVector = new Vector2(direction, 0);

            //     // Ground in front and gap ahead checks
            //     RaycastHit2D groundInFront = Physics2D.Raycast(bottomEnemyTransform, directionVector * 2f, groundLayer);
            //     RaycastHit2D gapAhead = Physics2D.Raycast(bottomEnemyTransform + (Vector3)directionVector, Vector2.down * 1.5f, groundLayer);

            //     // Player above and platform above checks
            //     Vector2 upwardVector = Vector2.up * 3f;
            //     bool isPlayerAbove = Physics2D.Raycast(topEnemyTransform, upwardVector, 1 << player.gameObject.layer);
            //     RaycastHit2D platformAbove = Physics2D.Raycast(topEnemyTransform, upwardVector, groundLayer);
                
            //     // if no ground and gap ahead, or player above and platform available, then jump
            //     if ((!groundInFront.collider && !gapAhead.collider) || (isPlayerAbove && platformAbove.collider)) {
            //         Debug.Log("jump params at time of jump: " + groundInFront.collider + gapAhead.collider + isPlayerAbove + platformAbove.collider);
            //         shouldJump = true;
            //     }
            // }

            // // basic guessing/heuristic jump logic
            // if (isGrounded && shouldJump) {
            //     Debug.Log(gameObject.name + "jumped");
            //     shouldJump = false;
            //     Vector2 direction = (player.position - transform.position).normalized;
            //     Vector2 jumpDirection = direction * jumpForce;
            //     rb.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
            // }
        }
    }

    public Vector2 CalculateJumpForce(Vector2 platformPosition) {
        // Horizontal and vertical distances
        float deltaX = platformPosition.x - transform.position.x;
        float deltaY = platformPosition.y - bottomEnemyTransform.y;

        // Calculate the vertical velocity needed to reach the platform height
        float verticalVelocity = Mathf.Sqrt(2 * bodyGravity * deltaY);
        // Time to reach the apex (top of the jump) at platform height
        float timeToApex = verticalVelocity / bodyGravity;
        // Calculate the horizontal velocity needed to reach the platform during that time
        float horizontalVelocity = deltaX / (timeToApex * 2);

        // Return the calculated initial velocity as a 2D vector (x, y)
        return new Vector2(horizontalVelocity, verticalVelocity);
    }

    // receiving impact reaction
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("OneWayPlatform") && (inImpact || collision.gameObject.CompareTag("enemy"))) {
            
            float impactForce = collision.relativeVelocity.magnitude;
            int collisionDamage = 0;

            if (impactForce > collisionForceThreshold) { // if force > threshold, then deal dmg, otherwise no longer in inImpact state
                collisionDamage = Mathf.RoundToInt(impactForce * collisionDamageMultiplier); // note: consider log max for extreme cases
                Damage(collisionDamage);

                // if collide wtih enemy, treat as if you were inImpact
                if (collision.gameObject.CompareTag("enemy")) { 
                    inImpact = true;
                    anim.SetBool("ImpactBool", true);
                } else { // bounce off surfaces, not enemies
                    Vector2 bounceDirection = collision.contacts[0].normal;
                    if (Math.Abs(bounceDirection.x) > 0) {
                        FlipCharacter(bounceDirection.x < 0);
                    }
                    rb.AddForce(bounceDirection * (impactForce * collisionForceMultiplier), ForceMode2D.Impulse);
                }
            }

            Debug.Log(gameObject.name + " <- " + impactForce + " impact force, " + collisionDamage + " impact damage <- " + collision.gameObject.name);
        }
    }

    private void FlipCharacter(bool right) {
        // storing whether object is already facingRight to avoid double flipping
        if (right != facingRight) {
            facingRight = !facingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    // Method to add a point to the LineRenderer
    private void AddPointToPath(Vector3 newPoint)
    {
        // Update the number of points in the LineRenderer
        lineRenderer.positionCount += 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newPoint);

        // Update the last recorded position
        lastRecordedPosition = newPoint;
    }

    private void ClearPath()
    {
        lineRenderer.positionCount = 0;
    }

    public void takeKick(int damage, Vector2 force) {
        Damage(damage);
        inImpact = true;
        anim.SetBool("ImpactBool", true);

        if (force.x < 0) {
            FlipCharacter(true);
        } else if (force.x > 0) {
            FlipCharacter(false);
        }

        rb.velocity = Vector2.zero; // so previous velocity doesn't interfere
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void Damage(int damage)
    {
        currentHealth -= damage;
        anim.SetTrigger("ImpactTrigger");

        if (currentHealth <= 0) {
            Destroy(gameObject);
            Debug.Log("dead as hell");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + (Vector3.down * bottomOfEnemy), Vector2.down * .3f); // isGrounded

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(topEnemyTransform, Vector2.up * maxJumpHeight); // max height detection
        Gizmos.color = Color.red;
        Gizmos.DrawRay(topEnemyTransform, Vector2.left * maxJumpDistance); // max jump x distance detection
        Gizmos.DrawRay(topEnemyTransform, Vector2.right * maxJumpDistance); // max jump x distance detection

        // Gizmos.color = Color.blue;
        // Gizmos.DrawRay(transform.position + (Vector3.down * bottomOfEnemy), new Vector2(direction, 0) * 2f); // groundInFront

        // Gizmos.color = Color.red;
        // Gizmos.DrawRay(transform.position + (Vector3.down * bottomOfEnemy) + new Vector3(direction, 0, 0), Vector2.down * 1.5f); // gapAhead

        // Gizmos.color = Color.green;
        // Gizmos.DrawRay(transform.position + (Vector3.up * topOfEnemy), Vector2.up * 3f); // isPlatformAbove/isPlayerAbove (they can be diff, but for now we're saying they're the same)
    }


}
