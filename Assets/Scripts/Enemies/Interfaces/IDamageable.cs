using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IDamageable
{
    int MaxHealth { get; set; }
    int CurrentHealth { get; set; }
    Animator Anim { get; set; }
    object DeathLock { get; set; }
    bool IsDead { get; set; }

    void Damage(int damage);
    void Die();
    void TakeKick(int damage, Vector2 force);
    void StopAttack();
}
