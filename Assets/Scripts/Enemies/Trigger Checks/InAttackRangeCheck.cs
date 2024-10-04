using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAttackRangeCheck : MonoBehaviour {
    public GameObject Player { get; set; }
    private Enemy _enemy; 

    private void Awake() {
        Player = GameObject.FindGameObjectWithTag("Player");
        _enemy = GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject == Player) {
            _enemy.SetInAttackRange(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        // Check if the collided xobject is on the "Player" layer
        if (collision.gameObject == Player) {
            _enemy.SetInAttackRange(false);
        }
    }
}
