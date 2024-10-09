using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSurface : MonoBehaviour
{
    public Player p;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            player.GM.Death();
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            //Enemy_Basic enemyScript = other.gameObject.GetComponent<Enemy_Basic>();
            //enemyScript.Damage(1000);
        }
    }
}
