using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Buff Card Data", menuName = "ScriptableObjects/Card/EnemyBuffCard")]
public class EnemyBuffCard : Card
{
    public override CardType cardType{get{return CardType.EnemyBuff;}}

    /* 
     * using this card increases the damage the enemies deal by effectValue
     */
    public override void use(Player p) {
        //p.kickDamage += effectValue;
        //TODO: maybe change use to also take in a list of enemies? see how game manager works
    }
}