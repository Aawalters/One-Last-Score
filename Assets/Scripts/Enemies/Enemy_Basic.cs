using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy_Basic : MonoBehaviour, IDamageable
{
    [field: SerializeField] public int MaxHealth { get; set; }
    [field: SerializeField] public int CurrentHealth { get; set; }
    public Animator Anim { get; set; }
    private Rigidbody2D rb;
    public GameEnemyManager GameEnemyManager;
    public bool facingRight;
    public object DeathLock { get; set; } = new object();
    public bool IsDead { get; set; } = false;

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

    public float baseMass;
    [Range(0, 1)]
    [Tooltip("Affects enemy floatiness during Impact State (for easier juggles)")]
    public float postImpactMassScale;

    [Header("AI")] // rn the variables are horrendous, need to move everything into a sep class (AI Brain?) later for reusability/decoupling
    // set properties
    public Transform player;
    public float chaseSpeed = 2f;
    public LayerMask groundLayer;
    public bool aiEnabled = false;
    public float stoppingDistance = 1f; // threshold for how player can be before moving
    public float maxJumpYForce;
    private float bodyGravity;
    private BoxCollider2D collider; // read only

    // state
    public bool isGrounded;
    public bool shouldJump;
    public bool midJump;
    public Vector2 direction;
    public bool isPaused = false;

    // transforms for detection
    private Vector3 topEnemyTransform;
    private Vector3 bottomEnemyTransform;

    // jumping and platforms
    public float maxYJumpForce;
    public float maxXJumpForce;
    private float maxJumpHeight; // threshold for y jump check for platforms
    private float maxJumpDistance; // threshold for x jump check for platforms
    private Vector2 platformDetectionOrigin; // should be at top of character head
    public LayerMask platformDetectionMask;
    public float jumpDelay = .3f;
    public bool playerAbove;
    public bool playerBelow;
    public Vector2 landingTarget; // target for jump landing
    public float landingOffset = 1.5f; // y offset from landing target, since calculation can't be pixel perfect (don't want it to be anyway)
    public GameObject currentOneWayPlatform;
    public float fallthroughTime = 0.5f; // time needed to fall through a platform before reenabling collisions

    // punching!
    public GameObject detectAttack;
    public float attackRadius;
    public int punchDamage;
    public bool shouldBeDamaging { get; private set;} = false;

    public LayerMask playerLayer; // **bars**
    public float attackWait = 1f;

    [Header("Knockback Path Tracer")]
    public float pointSpacing = 0.5f;  // Minimum distance between recorded points
    private LineRenderer lineRenderer;
    private Vector3 lastRecordedPosition;  // Last recorded position to avoid redundant points

    // Start is called before the first frame update
    void Start()
    {
        // accessing components
        Anim = transform.Find("Sprite").GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        // GameEnemyManager = FindObjectOfType<GameEnemyManager>(); // Reference the GameManager in the scene
        collider = gameObject.GetComponent<BoxCollider2D>();

        // setting properties
        facingRight = false;
        CurrentHealth = MaxHealth;
        bodyGravity = Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale;
        topEnemyTransform = collider.bounds.center + (Vector3.up * collider.bounds.extents.y); // height based on collider
        bottomEnemyTransform = collider.bounds.center + (Vector3.down * collider.bounds.extents.y); // feet based on collider
        maxJumpHeight = (Mathf.Pow(maxYJumpForce, 2) / (2 * bodyGravity)) - Math.Abs(topEnemyTransform.y - bottomEnemyTransform.y); // Calculate max height AI can jump
        float timeToApex = maxYJumpForce / bodyGravity;
        maxJumpDistance = maxXJumpForce * timeToApex; // Calculate the max horizontal distance AI can jump
        landingTarget = Vector2.zero;

        // Initialize the last recorded position
        lastRecordedPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() {
        // updating transforms
        topEnemyTransform = transform.position + (Vector3.up * collider.bounds.extents.y);
        bottomEnemyTransform = transform.position + (Vector3.down * collider.bounds.extents.y);
        platformDetectionOrigin = topEnemyTransform + (Vector3.up * (maxJumpHeight / 2)) + ((facingRight ? Vector3.right : Vector3.left) * (maxJumpDistance / 2));
        // record path during impact
        if (inImpact) {
            rb.mass = baseMass * postImpactMassScale;
            if (Vector3.Distance(transform.position, lastRecordedPosition) > pointSpacing) {
                AddPointToPath(transform.position);
            }
        } else {
            rb.mass = baseMass;
        }

        // if low velocity, then no longer inImpact
        if (rb.velocity.magnitude < collisionForceThreshold) {
            inImpact = false;
            ClearPath();
            Anim.SetBool("ImpactBool", false);
        }

        StartCoroutine(enemyAI(aiEnabled && !isPaused));

        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
    }

    // basic AI script
    private IEnumerator enemyAI(bool enabled) {
        if (enabled) {
            if (rb.velocity.y == 0) { // exit jump state when landing
                midJump = false;
            }

            // checks
            playerAbove = (player.transform.position.y > topEnemyTransform.y) ? true : false; // check for need to jump to player level
            playerBelow = (player.transform.position.y < bottomEnemyTransform.y) ? true : false;
            isGrounded = Physics2D.Raycast(bottomEnemyTransform, Vector2.down * .3f, groundLayer) && !midJump; // check if standing on ground

            // direction to player
            direction = player.position - transform.position;  
            int xDirection = direction.x == 0 ? 0 : (direction.x > 0 ? 1 : -1);

            // grounded and in control abilities
            if (isGrounded && !inImpact && !Anim.GetBool("isPunching")) { // && !isPaused
                WalkToPlayer(xDirection); // chase after player

                if (playerAbove) { // look for landing target if player above
                    DetectTargetFromPlatform();
                    shouldJump = landingTarget != Vector2.zero;
                }
                // looking to jump and valid landing target exists
                if (shouldJump) {
                    Vector2 val = CalculateJumpForce(landingTarget + (Vector2.up * landingOffset));

                    yield return PauseAction(jumpDelay);

                    rb.velocity = new Vector2(val.x, 0f);
                    rb.AddForce(new Vector2(0f, val.y), ForceMode2D.Impulse);
                    Debug.Log("landing target: " + landingTarget + " -> jump force: " + val);
                    shouldJump = false;
                    midJump = true;
                }

                // looking to drop down to player
                if (playerBelow && currentOneWayPlatform != null) {
                    yield return PauseAction(jumpDelay);
                    StartCoroutine(DisableCollision());
                }

                
            }

            if (midJump && rb.velocity.y < 0) { // if falling from a jump, regain control
                WalkToPlayer(xDirection);
            }
        }
    }

    // Helper function to encapsulate the pause logic
    private IEnumerator PauseAction(float delay) {
        isPaused = true;
        Anim.SetBool("isWalking", false);
        yield return new WaitForSeconds(delay);
        isPaused = false;
    }
    // if out of reach, walk towards player, otherwise idle
    private void WalkToPlayer(int xDirection) {
        bool isOutOfReach = Math.Abs(direction.x) > stoppingDistance;
        rb.velocity = new Vector2(isOutOfReach ? xDirection * chaseSpeed : 0, rb.velocity.y);
        FlipCharacter(isOutOfReach ? xDirection > 0 : facingRight); // Maintain direction if idle
        Anim.SetBool("isWalking", isOutOfReach);
    }

    // find x and y jump force needed to reach platformPosition
    public Vector2 CalculateJumpForce(Vector2 platformPosition) {
        // Horizontal and vertical distances
        float deltaX = Math.Abs(platformPosition.x - transform.position.x);
        float deltaY = Math.Abs(platformPosition.y - bottomEnemyTransform.y);

        // Calculate the vertical velocity needed to reach the platform height
        float verticalVelocity = Math.Min(Mathf.Sqrt(2 * bodyGravity * deltaY), maxYJumpForce);
        // Time to reach the apex (top of the jump) at platform height
        float timeToApex = verticalVelocity / bodyGravity;
        // Calculate the horizontal velocity needed to reach the platform during that time
        float horizontalVelocity = (facingRight ? 1 : -1) * Math.Min(maxXJumpForce, deltaX / timeToApex);

        // Return the calculated initial velocity as a 2D vector (x, y)
        return new Vector2(horizontalVelocity, verticalVelocity);
    }

    // look for platforms within detection box (max jump dist/height) and find furthest landing target within max jump
    public void DetectTargetFromPlatform() {

        Vector2 boxSize = new Vector2(maxJumpDistance, maxJumpHeight);
        Collider2D[] hits = Physics2D.OverlapBoxAll(platformDetectionOrigin, boxSize, 0f, platformDetectionMask);
        if (hits.Length == 0) { // No platforms detected, exit early
            // Debug.Log("no colliders detected");
            landingTarget = Vector2.zero; // special val
        } else { // look for platform furthest from enemy
            float maxDistance = float.MinValue;
            Collider2D furthestPlatform = null;

            foreach (var hit in hits) {
                Debug.Log("hit: " + hit.name);
                // Check if the hit object has the "OneWayPlatform" tag
                if (hit.CompareTag("OneWayPlatform")) {
                    // Calculate the distance between the enemy and the hit platform
                    float distance = Vector2.Distance(bottomEnemyTransform, hit.transform.position);
                    if (distance > maxDistance) {
                        maxDistance = distance;
                        furthestPlatform = hit;
                    }
                }
            }

            // Get the collider bounds of the furthest platform
            if (furthestPlatform != null) {
                Bounds platformBounds = furthestPlatform.GetComponent<Collider2D>().bounds;
                float platformXDist = Math.Abs(rb.transform.position.x - (facingRight ? platformBounds.max.x : platformBounds.min.x));
                float jumpX = (facingRight ? 1 : -1) * Math.Min(platformXDist, maxJumpDistance);
                landingTarget = new Vector2(
                    jumpX,
                    platformBounds.max.y
                );
            } else {
                // Debug.Log("no platforms detected");
                landingTarget = Vector2.zero; // special val
            }
        }
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(collider, platformCollider);
        yield return new WaitForSeconds(fallthroughTime);
        Physics2D.IgnoreCollision(collider, platformCollider, false);
    }

    // receiving impact reaction
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform")) {
            currentOneWayPlatform = collision.gameObject;
        }

        if (!collision.gameObject.CompareTag("OneWayPlatform") && (inImpact || collision.gameObject.CompareTag("enemy"))) {
            
            float impactForce = collision.relativeVelocity.magnitude;
            int collisionDamage = 0;

            if (impactForce > collisionForceThreshold) { // if force > threshold, then deal dmg, otherwise no longer in inImpact state
                collisionDamage = Mathf.RoundToInt(impactForce * collisionDamageMultiplier); // note: consider log max for extreme cases
                Damage(collisionDamage);

                // if collide wtih enemy, treat as if you were inImpact
                if (collision.gameObject.CompareTag("enemy")) { 
                    inImpact = true;
                    Anim.SetBool("ImpactBool", true);
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

    public void TakeKick(int damage, Vector2 force) {
        Damage(damage);
        inImpact = true;
        Anim.SetBool("ImpactBool", true);

        if (force.x < 0) {
            FlipCharacter(true);
        } else if (force.x > 0) {
            FlipCharacter(false);
        }

        rb.velocity = Vector2.zero; // so previous velocity doesn't interfere
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void Die()
    {
        lock (DeathLock) // Ensure only one thread executes this block at a time
        {
            if (!IsDead)
            {
                IsDead = true;
                // Perform death logic, animations, destruction, etc.
                // Destroy(gameObject);
               GameEnemyManager.Death(gameObject); // call this one cause list with enemies needs to be updated, this one calls Destroy too
            }
        }
    }

    public void Damage(int damage)
    {
        CurrentHealth -= damage;
        Anim.SetTrigger("ImpactTrigger");

        if (CurrentHealth <= 0) {
            if (!gameObject.IsDestroyed())
            {
                Die();
                Debug.Log("dead as hell");
            }
        }
    }

    public void OnPlayerInAttackRange(GameObject player) {
        Anim.SetBool("isPunching", true);
    }

    // punch active frames
    public IEnumerator Punch() {
        shouldBeDamaging = true;
        while (shouldBeDamaging) {
            Collider2D player = Physics2D.OverlapCircle(detectAttack.transform.position, attackRadius, playerLayer);
            if (player != null) {
                // StartCoroutine(player.GetComponent<PlayerController>().TakeDamage(punchDamage));
            }
            yield return null; // wait a frame
        }
    }

    // end punch active frames
    public void EndShouldBeDamaging() {
        shouldBeDamaging = false;
    }

    // set end of animation
    public void EndPunch() {
        Anim.SetBool("isPunching", false);
        StartCoroutine(PauseAction(attackWait));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(bottomEnemyTransform, Vector2.down * .3f); // isGrounded

        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawCube(platformDetectionOrigin, new Vector2(maxJumpDistance, maxJumpHeight)); // jump detection box
        Gizmos.DrawLine(bottomEnemyTransform, landingTarget); // visual for where jumping landing target is

        Gizmos.DrawWireSphere(detectAttack.transform.position, attackRadius);

        Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
        Gizmos.DrawRay(bottomEnemyTransform, (facingRight ? Vector2.right : Vector2.left) * stoppingDistance); // stopping distance
    }

    public void StopAttack()
    {
        //ignore
    }
}
