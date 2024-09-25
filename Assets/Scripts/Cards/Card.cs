using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum CardType {Attack, Huma, Mani, Nihtee, Heal, Utility };
public enum CardType {Multiplier, Additor, EnemyBuff, PlayerBuff};

public abstract class Card : ScriptableObject 
{
   
    public string cardName;
    public string cardDescription;
    public Sprite cardImage;
    public Sprite effectImage;
    public abstract CardType cardType{get;}
    public int effectValue;
    public int id;
    public string cardUserAnim;

    public abstract void use(Player p);

    //public abstract void use(Player p, Enemy e);
    //public abstract void use(Player p);

}
