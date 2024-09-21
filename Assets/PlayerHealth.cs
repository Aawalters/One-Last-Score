using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;   
    public int currentHealth;   
    public Slider healthBar;     
    public GameObject dieScreen; 
    public GameObject player;     

    void Start()
    {
        currentHealth = maxHealth; 
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player took damage from enemy collision aaa"); 
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            TakeDamage(5); 
        }
    }

    // Function to take damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth; // Update health bar UI

        if (currentHealth <= 0)
        {
            Die(); 
        }
    }

    void Die()
    {
        Debug.Log("DEAD");
        dieScreen.SetActive(true);
        player.SetActive(false);

    }
}
