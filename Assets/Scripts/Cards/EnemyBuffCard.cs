using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Buff Card Data", menuName = "ScriptableObjects/Card/EnemyBuffCard")]
public class EnemyBuffCard : Card
{
    //public GameEnemyManager GameEnemyManager;
    // public int ExtraHealth = 50;
    public int ExtraDamage = 15;
    public float ExtraSpeed = 0.2f;
    public override CardType cardType{get{return CardType.EnemyBuff;}}

    /* 
     * using this card increases the damage the enemies deal by effectValue
     */
    public override void use(Player p) {
        Debug.Log("IT BUFFS ENEMIES ");
        p.GameEnemyManager.BuffEnemies(effectValue, ExtraDamage, ExtraSpeed);
    }
}