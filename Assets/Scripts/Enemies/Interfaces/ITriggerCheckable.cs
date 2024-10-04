using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface ITriggerCheckable
{
    bool InChaseRange { get; set; }
    bool InAttackRange { get; set; }

    void SetInChaseRange(bool InChaseRange);
    void SetInAttackRange(bool inAttackRange);
}
