using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    public EnemyAttackState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) {
        id = EnemyStateMachine.EnemyStates.Attack;
    }

    public override void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType) {
        switch (triggerType) {
            case Enemy.AnimationTriggerType.StartPunch:
                e.StartCoroutine(e.Punch());
                break;
            case Enemy.AnimationTriggerType.EndPunch:
                e.EndPunch();
                break;
            case Enemy.AnimationTriggerType.EndPunchDamaging:
                e.EndShouldBeDamaging();
                break;
        }
    }
    
    public override void EnterState()
    {
        e.Anim.SetBool("isPunching", true);
    }

    public override void FrameUpdate()
    {
        // must finish punch animation before considering next action
        if (!e.IsPaused && !e.InImpact) {
            if (!e.Anim.GetBool("isPunching") && !e.InAttackRange) {
                if (!e.InChaseRange) {
                    enemyStateMachine.changeState(e.IdleState);
                } else {
                    enemyStateMachine.changeState(e.ChaseState);
                }
            } else { // repeatedely punch if in range
                EnterState();
            }
        }
    }
}
