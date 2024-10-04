using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    private Enemy _enemy;
    private SpriteRenderer sprite;

    void Start() {
        _enemy = GetComponentInParent<Enemy>();
        sprite = GetComponent<SpriteRenderer>();
    }
    
    public void Punch() {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.StartPunch);
    }

    public void EndPunch() {
        sprite.color = Color.white;
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.EndPunch);
    }

    public void EndShouldBeDamaging() {
        _enemy.AnimationTriggerEvent(Enemy.AnimationTriggerType.EndPunchDamaging);
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
