using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyMoveable
{
    // components
    Animator Anim { get; set; }
    Rigidbody2D RB { get; set; }

    // state
    bool IsGrounded { get; set; }
    bool ShouldJump { get; set; }
    bool MidJump { get; set; }
    bool PlayerAbove { get; set; }
    bool PlayerBelow { get; set; }
    bool IsPaused { get; set; }
    bool FacingRight { get; set; }
    Vector3 TopEnemyTransform { get; set; }
    Vector3 BottomEnemyTransform { get; set; }
    float BodyGravity { get; set; }
    float MaxJumpHeight { get; set; } // threshold for y jump check for platforms
    float MaxJumpDistance { get; set; } // threshold for x jump check for platforms
    Vector2 PlatformDetectionOrigin { get; set; } // should be at top of character head
    Vector2 LandingTarget { get; set; } // target for jump landing
    GameObject CurrentOneWayPlatform { get; set; }
    BoxCollider2D Collider { get; set; } // read only

    // editor properties
    float ChaseSpeed { get; set; }
    float MaxYJumpForce { get; set; }
    float MaxXJumpForce { get; set; }
    LayerMask PlatformDetectionMask { get; set; }
    float FallthroughTime { get; set; } // time needed to fall through a platform before reenabling collisions
    float LandingOffset { get; set; } // y offset from landing target, since calculation can't be pixel perfect (don't want it to be anyway)
    float JumpDelay { get; set; }
    LayerMask GroundLayer { get; set; }

    void FlipCharacter(bool right);
     // Helper function to encapsulate the pause logic
    IEnumerator PauseAction(float delay);
    // walk in x direction
    void WalkToTarget(Vector2 target);
    // find x and y jump force needed to reach platformPosition
    Vector2 CalculateJumpForce(Vector2 platformPosition);
    // look for platforms within detection box (max jump dist/height) and find furthest landing target within max jump
    void DetectTargetFromPlatform();
    // for falling through platforms
    IEnumerator DisableCollision();
}
