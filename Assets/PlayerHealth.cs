using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;    // Maximum health of the player
    public int currentHealth;      // Current health of the player
    public Slider healthBar;       // UI Slider for health bar
    public GameObject dieScreen;   // Reference to the die screen canvas
    public GameObject player;      // Reference to the player game object (for disabling movement)

    void Start()
    {
        currentHealth = maxHealth; // Set health to max at start
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collided object has the layer "Enemy"
        Debug.Log("Player took damage from enemy collision aaa"); 
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            TakeDamage(5); // Call TakeDamage from PlayerHealth
        }
    }

    // Function to take damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage; // Subtract damage from health
        healthBar.value = currentHealth; // Update health bar UI

        if (currentHealth <= 0)
        {
            Die(); // Call Die function if health reaches 0
        }
    }

    // Player dies when health is 0
    void Die()
    {
        Debug.Log("Player has died.");
        // Add further actions like disabling player controls, showing Game Over screen, etc.

        // Show the death screen
        dieScreen.SetActive(true);

        // Disable player controls by disabling the player game object or its movement script
        player.SetActive(false);

    }
}
