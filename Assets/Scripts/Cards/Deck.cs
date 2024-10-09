using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck 
{
    public static int handSize;
    public List<Card> deck = new List<Card>();
    public List<Card> drawPile = new List<Card>();
    public List<Card> hand = new List<Card>(handSize);
    public List<Card> discardPile = new List<Card>();
    //public int comboIndex=0;
    
    public IEnumerator<Card> GetEnumerator()
    {
        return deck.GetEnumerator();
    }

}
