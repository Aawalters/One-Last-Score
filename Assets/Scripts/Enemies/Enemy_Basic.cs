using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : MonoBehaviour
{
    public float health = 5f;
    public float currentHealth;
    private Animator anim;
    public Rigidbody2D rb;
    private GameManager gameManager;
    public float collisionForceThreshold;
    private bool inImpact = false;
    [Range(0, 1)]
    public float collisionDamageMultiplier; // determines how much collision force is factored into damage
    public float bounceForce; // strength of bouncing against other things
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
        if (rb.velocity.magnitude < collisionForceThreshold) {
            inImpact = false;
            anim.SetBool("ImpactBool", false);
        }
        if (health < currentHealth) {
            currentHealth = health;
            anim.SetTrigger("ImpactTrigger");
        }

        if (health < 0) {
            Destroy(gameObject);
            Debug.Log("dead as hell");
            // gameManager.OnEnemyKilled();
        }
    }

     // This method is called when the enemy collides with another object
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (inImpact) {
            
            float impactForce = collision.relativeVelocity.magnitude;
            Debug.Log("impact force: " + impactForce);

            if (impactForce > collisionForceThreshold) { // if force > threshold, then deal dmg, otherwise no longer in inImpact state
                int collisionDamage = Mathf.RoundToInt(impactForce * collisionDamageMultiplier); // consider log max for extreme cases
                health -= collisionDamage;
                Debug.Log("Enemy took " + collisionDamage + " damage due to impact.");

                // direction opposite of collision
                Vector2 bounceDirection = collision.contacts[0].normal;
                // Debug.Log("bounce direction: " + -bounceDirection);
                rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
            }
        }
    }

    public void takeKick(int damage, Vector2 force) {
        health -= damage;
        inImpact = true;
        anim.SetBool("ImpactBool", true);
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}
