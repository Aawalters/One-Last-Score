using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Power Card Data", menuName = "ScriptableObjects/Card/AttackpowerCard")]
public class AttackpowerCard : Card
{
    public override CardType cardType{get{return CardType.PlayerBuff;}}

    /* 
     * using this card increases the damage the player's kick deals by effectValue
     */
    public override void use(GameManager GM) {
        GM.p.kickDamage += effectValue;
    }
}