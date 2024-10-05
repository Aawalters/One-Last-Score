using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Knockback Card Data", menuName = "ScriptableObjects/Card/KnockbackCard")]
public class KnockbackCard : Card
{
    public override CardType cardType{get{return CardType.PlayerBuff;}}

    /* 
     * using this card increases the knockback (kick force) for the player by effectValue
     */
    public override void use(GameManager GM) {
        GM.p.kickForce += effectValue;
    }
}