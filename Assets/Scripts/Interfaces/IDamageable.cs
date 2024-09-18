using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void takeKick(int damage, Vector2 force);
    public void Damage(int damage);
}
