using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : MonoBehaviour
{
    // game state
    public float health = 5f;
    public float currentHealth;
    private Animator anim;
    private Rigidbody2D rb;
    private GameManager gameManager;
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
        currentHealth = health;
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>(); // Reference the GameManager in the scene
        lineRenderer = gameObject.GetComponent<LineRenderer>();

        // Initialize the last recorded position
        lastRecordedPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // taking dmg
        if (health < currentHealth) {
            currentHealth = health;
            anim.SetTrigger("ImpactTrigger");
        }
        // death
        if (health < 0) {
            Destroy(gameObject);
            Debug.Log("dead as hell");
            // gameManager.OnEnemyKilled();
        }
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

        // very basic AI script
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, .3f, groundLayer);
        direction = Mathf.Sign(player.position.x - transform.position.x); 
        // bool isPlayerBelow = Physics2D.Raycast(transform.position, Vector2.up, 4f, 1 << player.gameObject.layer);

        if (isGrounded && !inImpact) {
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);

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

     // This method is called when the enemy collides with another object
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (inImpact || collision.gameObject.CompareTag("enemy")) {
            
            float impactForce = collision.relativeVelocity.magnitude;
            Debug.Log(gameObject.name + " took " + impactForce + " impact force from " + collision.gameObject.name);

            if (impactForce > collisionForceThreshold) { // if force > threshold, then deal dmg, otherwise no longer in inImpact state
                int collisionDamage = Mathf.RoundToInt(impactForce * collisionDamageMultiplier); // consider log max for extreme cases
                health -= collisionDamage;
                Debug.Log(gameObject.name + " took " + collisionDamage + " impact damage from " + collision.gameObject.name);

                if (collision.gameObject.CompareTag("enemy")) { // if collide wtih enemy, treat as if you were inImpact
                    inImpact = true;
                    anim.SetBool("ImpactBool", true);
                } else { // bounce off surfaces, not enemies
                    // direction opposite of collision
                    Vector2 bounceDirection = collision.contacts[0].normal;
                    rb.AddForce(bounceDirection * (impactForce * collisionForceMultiplier), ForceMode2D.Impulse);
                }
            }
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
        health -= damage;
        inImpact = true;
        anim.SetBool("ImpactBool", true);
        rb.velocity = Vector2.zero; // so previous velocity doesn't interfere
        rb.AddForce(force, ForceMode2D.Impulse);
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
