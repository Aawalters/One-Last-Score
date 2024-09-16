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
        }
        if (health < currentHealth) {
            currentHealth = health;
            anim.SetTrigger("Impact");
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
            Debug.Log(impactForce);

            if (impactForce > collisionForceThreshold) {
                int collisionDamage = Mathf.RoundToInt(impactForce);
                health -= collisionDamage;
                Debug.Log("Enemy took " + collisionDamage + " damage due to impact.");
            }
        }
    }

    public void takeKick(int damage, Vector2 force) {
        health -= damage;
        inImpact = true;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}
