using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Multiplier Card Data", menuName = "ScriptableObjects/Card/MultiplierCard")]
public class MultiplierCard : Card
{
    public override CardType cardType{get{return CardType.Multiplier;}}

    /* 
     * using this card increases the end multiplier for the player by effectValue
     */
    public override void use(Player p) {
        p.multiplier += effectValue;
        p.GameManager.updateWager();
    }
}