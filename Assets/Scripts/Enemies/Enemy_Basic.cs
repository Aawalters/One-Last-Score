using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : MonoBehaviour, IDamageable
{
    // game state
    public int maxHealth = 1000;
    public int currentHealth;
    private Animator anim;
    private Rigidbody2D rb;
    private GameManager gameManager;
    private bool facingRight;
    //

    // collision tuning
    public float collisionForceThreshold;
    private bool inImpact = false;
    [Range(0, 1)]
    public float collisionDamageMultiplier; // determines how much collision force is factored into damage
    public float bounceForce; // strength of bouncing against other things
    [Range(0, 1)]
    public float collisionForceMultiplier; // determines how much weight impact force factors into bounce
    //

    // AI
    public Transform player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;
    public bool isGrounded;
    private bool shouldJump;
    private float direction;
    public float enemyHeight; // used for 'isXabove' detection
    public bool aiEnabled = false;
    //

    // tracing knockback path (debugging + fx later??)
    private LineRenderer lineRenderer;
    public float pointSpacing = 0.5f;  // Minimum distance between recorded points
    private Vector3 lastRecordedPosition;  // Last recorded position to avoid redundant points
    //

    // Start is called before the first frame update
    void Start()
    {
        anim = transform.Find("Sprite").GetComponent<Animator>();
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>(); // Reference the GameManager in the scene
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        facingRight = false;

        // Initialize the last recorded position
        lastRecordedPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() {
        // Only record a new point if the enemy has moved a significant distance
        if (inImpact && Vector3.Distance(transform.position, lastRecordedPosition) > pointSpacing) {
            AddPointToPath(transform.position);
        }

        // if low velocity, then no longer inImpact
        if (rb.velocity.magnitude < collisionForceThreshold) {
            inImpact = false;
            ClearPath();
            anim.SetBool("ImpactBool", false);
        }

        if(aiEnabled) {
            // very basic AI script
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, .3f, groundLayer);
            direction = Mathf.Sign(player.position.x - transform.position.x); 
            // bool isPlayerBelow = Physics2D.Raycast(transform.position, Vector2.up, 4f, 1 << player.gameObject.layer);

            if (isGrounded && !inImpact) {
                rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
                if (direction > 0) {
                    FlipCharacter(true);
                } else if (direction < 0) {
                    FlipCharacter(false);
                }

                RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);
                RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 1.5f, groundLayer);

                bool isPlayerAbove = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + enemyHeight), Vector2.up, 3f, 1 << player.gameObject.layer);
                RaycastHit2D platformAbove = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + enemyHeight), Vector2.up, 3f, groundLayer);
                
                // if no ground and gap ahead, or player above and platform available, then jump
                if ((!groundInFront.collider && !gapAhead.collider) || (isPlayerAbove && platformAbove.collider)) {
                    // Debug.Log("jump params at time of jump: " + groundInFront.collider + gapAhead.collider + isPlayerAbove + platformAbove.collider);
                    shouldJump = true;
                }
            }

            // basic guessing/heuristic jump logic
            if (isGrounded && shouldJump) {
                Debug.Log(gameObject.name + "jumped");
                shouldJump = false;
                Vector2 direction = (player.position - transform.position).normalized;
                Vector2 jumpDirection = direction * jumpForce;
                rb.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
            }
        }
    }

     // This method is called when the enemy collides with another object
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (inImpact || collision.gameObject.CompareTag("enemy")) {
            
            float impactForce = collision.relativeVelocity.magnitude;
            int collisionDamage = 0;

            if (impactForce > collisionForceThreshold) { // if force > threshold, then deal dmg, otherwise no longer in inImpact state
                collisionDamage = Mathf.RoundToInt(impactForce * collisionDamageMultiplier); // consider log max for extreme cases
                Damage(collisionDamage);

                // if collide wtih enemy, treat as if you were inImpact
                if (collision.gameObject.CompareTag("enemy")) { 
                    inImpact = true;
                    anim.SetBool("ImpactBool", true);
                } else { // bounce off surfaces, not enemies
                    // direction opposite of collision
                    Vector2 bounceDirection = collision.contacts[0].normal;
                    // flip direction to face impact
                    if (bounceDirection.x < 0) {
                        FlipCharacter(true);
                    } else if (bounceDirection.x > 0) {
                        FlipCharacter(false);
                    }
                    rb.AddForce(bounceDirection * (impactForce * collisionForceMultiplier), ForceMode2D.Impulse);
                }
            }

            Debug.Log(gameObject.name + " <- " + impactForce + " impact force, " + collisionDamage + " impact damage <- " + collision.gameObject.name);
        }
    }

    private void FlipCharacter(bool right) {
        // Debug.Log("flip character called with: " + right);
        // storing whether object is already facingRight to avoid double flipping
        if (right && !facingRight) {
            facingRight = true;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        } else if (!right && facingRight) {
            facingRight = false;
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

    public void ClearPath()
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
        Gizmos.DrawRay(transform.position, Vector2.down * .3f); // isGrounded

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, new Vector2(direction, 0) * 2f); // groundInFront

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + new Vector3(direction, 0, 0), Vector2.down * 1.5f); // gapAhead

        Gizmos.color = Color.green;
        Gizmos.DrawRay(new Vector2(transform.position.x, transform.position.y + enemyHeight), Vector2.up * 3f); // isPlatformAbove/isPlayerAbove (they can be diff, but for now we're saying they're the same)
    }


}
