using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : MonoBehaviour
{
    public float health = 5f;
    public float currentHealth;
    private Animator anim;
    private Rigidbody2D rb;
    private GameManager gameManager;
    public float collisionForceThreshold;
    private bool inImpact = false;
    [Range(0, 1)]
    public float collisionDamageMultiplier; // determines how much collision force is factored into damage
    public float bounceForce; // strength of bouncing against other things
    [Range(0, 1)]
    public float collisionForceMultiplier; // determines how much weight impact force factors into bounce
    public Transform player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;

    private bool isGrounded;
    private bool shouldJump;
    private float direction;
    // Start is called before the first frame update
    void Start()
    {
        anim = transform.Find("Sprite").GetComponent<Animator>();
        currentHealth = health;
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>(); // Reference the GameManager in the scene
    }

    // Update is called once per frame
    void Update()
    {
        // if low velocity, then no longer inImpact
        if (rb.velocity.magnitude < collisionForceThreshold) {
            inImpact = false;
            anim.SetBool("ImpactBool", false);
        }
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

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);
        direction = Mathf.Sign(player.position.x - transform.position.x); 
        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 4f, 1 << player.gameObject.layer);
        // bool isPlayerBelow = Physics2D.Raycast(transform.position, Vector2.up, 4f, 1 << player.gameObject.layer);

        if (isGrounded) {
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);

            RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);
            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 2f, groundLayer);
            RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 4f, groundLayer);
            
            // if no ground and gap ahead, or player above and platform available, then jump
            if ((!groundInFront.collider && !gapAhead.collider) || (isPlayerAbove && platformAbove.collider)) {
                shouldJump = true;
            }
        }
    }

    private void FixedUpdate() {
        if (isGrounded && shouldJump) {
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
            Debug.Log("impact force: " + impactForce);

            if (impactForce > collisionForceThreshold) { // if force > threshold, then deal dmg, otherwise no longer in inImpact state
                int collisionDamage = Mathf.RoundToInt(impactForce * collisionDamageMultiplier); // consider log max for extreme cases
                health -= collisionDamage;
                Debug.Log("Enemy took " + collisionDamage + " damage due to impact.");

                if (collision.gameObject.CompareTag("enemy")) { // if collide wtih enemy, treat as if you were inImpact
                    inImpact = true;
                    anim.SetBool("ImpactBool", true);
                } else { // bounce off surfaces, not enemies
                    // direction opposite of collision
                    Vector2 bounceDirection = collision.contacts[0].normal;
                    // Debug.Log("bounce direction: " + -bounceDirection);
                    rb.AddForce(bounceDirection * (impactForce * collisionForceMultiplier), ForceMode2D.Impulse);
                }
                
            }
        }

        // collision tag = enemy
    }

    public void takeKick(int damage, Vector2 force) {
        health -= damage;
        inImpact = true;
        anim.SetBool("ImpactBool", true);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector2.down * 1f); // isGrounded

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, new Vector2(direction, 0) * 2f); // groundInFront

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + new Vector3(direction, 0, 0), Vector2.down * 2f); // gapAhead

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector2.up * 4f); // platformAbove
    }
}
