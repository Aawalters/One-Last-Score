using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IPuncher
{
    GameObject DetectAttack { get; set; }
    float AttackRadius { get; set; }
    int PunchDamage { get; set; }
    bool ShouldBeDamaging { get; set; }
    float AttackWait { get; set; }

    // punch active frames
    IEnumerator Punch();
    // end punch active frames
    void EndShouldBeDamaging();
    // set end of animation
    void EndPunch();
}
