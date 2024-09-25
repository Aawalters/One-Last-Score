using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wager Card Data", menuName = "ScriptableObjects/Card/WagerCard")]
public class WagerCard : Card
{
    public override CardType cardType{get{return CardType.Additor;}}

    /* 
     * using this card increases/decreases the wager val. for the player by effectValue
     */
    public override void use(Player p) {
        p.wager += effectValue;
        p.GameManager.updateWager();
    }
}