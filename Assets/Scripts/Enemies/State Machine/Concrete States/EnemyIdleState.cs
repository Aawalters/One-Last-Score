using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private Vector2 _targetPos;
    private float _currentTime = 0f;

    public EnemyIdleState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine) {
        id = EnemyStateMachine.EnemyStates.Idle;
    }

    public override void EnterState() {
        _targetPos = e.transform.position;
    }

    public override void FrameUpdate()
    {
        // check state
        if (!e.IsPaused && !e.InImpact) {
            if (e.InAttackRange) {
                enemyStateMachine.changeState(e.AttackState);
            } else if (e.InChaseRange) {
                enemyStateMachine.changeState(e.ChaseState);
            }
        }
    
        // regular logic
        if (_currentTime > e.IdleTimeBetweenMove) {
            _currentTime = 0f;
            SetTarget();
        }
        _currentTime += Time.deltaTime;
    }

    public override void PhysicsUpdate()
    {
        e.WalkToTarget(_targetPos);
    }

    private void SetTarget() {
        float randomX = Random.Range(-e.IdleRange, e.IdleRange);
        _targetPos = new Vector2(e.transform.position.x + randomX, e.transform.position.y);
    }
}
