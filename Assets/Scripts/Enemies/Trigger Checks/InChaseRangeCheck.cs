using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InChaseRangeCheck : MonoBehaviour
{
    public GameObject Player { get; set; }
    private Enemy _enemy; 

    private void Awake() {
        Player = GameObject.FindGameObjectWithTag("Player");
        _enemy = GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject == Player) {
            _enemy.SetInChaseRange(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject == Player) {
            _enemy.SetInChaseRange(false);
        }
    }
}
