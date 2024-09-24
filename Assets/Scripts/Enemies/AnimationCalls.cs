using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCalls : MonoBehaviour
{
    private Enemy_Basic parentEnemy;
    private SpriteRenderer sprite;

    void Start() {
        parentEnemy = GetComponentInParent<Enemy_Basic>();
        sprite = GetComponent<SpriteRenderer>();
    }
    public void Punch() {
        StartCoroutine(parentEnemy.Punch());
    }

    public void EndPunch() {
        sprite.color = Color.white;
        parentEnemy.EndPunch();
    }

    public void EndShouldBeDamaging() {
        parentEnemy.EndShouldBeDamaging();
    }

    // temp until we get polish animations
    // indicating beginning to windup
    public void StartPunch() {
        sprite.color = new Color(0, 1, 0, 0.5f);
    }

    // indicating punch is about to come out and in danger zone
    public void AboutToPunch() {
        sprite.color = new Color(1, 0, 0, 0.5f);
    }
}
