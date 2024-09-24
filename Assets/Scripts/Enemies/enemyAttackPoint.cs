using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyAttackPoint : MonoBehaviour
{
    private Enemy_Basic enemy; // Reference to the parent enemy script

    void Start()
    {
        // Find the parent object's Enemy component
        enemy = GetComponentInParent<Enemy_Basic>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object is on the "Player" layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Call a method on the enemy object to handle this event
            enemy.OnPlayerInAttackRange(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        Debug.Log("call trigger stay?");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Call a method on the enemy object to handle this event
            enemy.OnPlayerInAttackRange(collision.gameObject);
        }
    }
}
