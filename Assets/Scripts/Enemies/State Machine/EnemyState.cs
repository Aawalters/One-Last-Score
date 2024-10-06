using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState
{
    protected Enemy e;
    protected EnemyStateMachine enemyStateMachine;   
    public EnemyStateMachine.EnemyStates id;

    public EnemyState(Enemy enemy, EnemyStateMachine enemyStateMachine) {
        this.e = enemy;
        this.enemyStateMachine = enemyStateMachine;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void FrameUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType) { }
}
