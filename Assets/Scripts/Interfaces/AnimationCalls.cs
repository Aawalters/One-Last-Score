using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCalls : MonoBehaviour
{
    public Enemy_Basic parentEnemy;

    void Start() {
        parentEnemy = GetComponentInParent<Enemy_Basic>();
    }
    public void Punch() {
        parentEnemy.Punch();
    }

    public void EndPunch() {
        parentEnemy.EndPunch();
    }

    public void EndShouldBeDamaging() {
        parentEnemy.EndShouldBeDamaging();
    }
}
