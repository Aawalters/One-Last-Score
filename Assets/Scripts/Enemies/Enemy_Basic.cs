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
        if (health < currentHealth) {
            currentHealth = health;
            anim.SetTrigger("Impact");
        }

        if (health < 0) {
            Destroy(gameObject);
            Debug.Log("dead as hell");
            gameManager.OnEnemyKilled();
        }
    }

    public void takeKick(int damage, Vector2 direction, float kickForce) {
        Debug.Log("Hit");
        health -= damage;
        Debug.Log(direction);
        rb.AddForce(direction * kickForce, ForceMode2D.Impulse);
    }
}
