using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck 
{
    public List<Card> deck = new List<Card>();
    public List<Card> drawPile = new List<Card>();
    public List<Card> hand = new List<Card>(5);
    public List<Card> discardPile = new List<Card>();
    //public int comboIndex=0;
    
}
