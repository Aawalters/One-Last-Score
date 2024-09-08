using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : MonoBehaviour
{
    public float health = 5f;
    public float currentHealth;
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        currentHealth = health;
    }

    // Update is called once per frame
    void Update()
    {
        if (health < currentHealth) {
            currentHealth = health;
            anim.SetTrigger("Impact");
        }

        if (health < 0) {
            Debug.Log("dead as hell");
        }
    }
}
