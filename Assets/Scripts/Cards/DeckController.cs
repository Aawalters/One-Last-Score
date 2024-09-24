using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckController : MonoBehaviour
{
    public static DeckController instance;

    public List<Card> allCards;
    //public List<Card> clanCards;
    public Deck currentDeck;

    private void Awake()
    {
        instance = this;
    }

    //fills deck with card data set
    public Deck GetNewDeck()
    //public Deck GetNewDeck(Player _player)
    {
        Deck temp = new Deck();
        for(var i = 0; i < allCards.Count; i++) {
            DeckAdd(allCards[i], temp);
        }

        // //adds player's clan card
        // if(_player.currentClan.clanName.Equals("Huma")){
        //     DeckAdd(clanCards[0], temp);
        // } else if (_player.currentClan.clanName.Equals("Mani")){
        //     DeckAdd(clanCards[1], temp);
        // } else if (_player.currentClan.clanName.Equals("Nih-Tee")){
        //     DeckAdd(clanCards[2], temp);
        // }

        //PopulateDraw(temp);
        PopulateHand(temp);
        return temp;
    }

    // //adds the beaten clan's card to player deck
    // public void addClanCard(int index, Deck deck){
    //     DeckAdd(clanCards[index], deck);
    // }

    // public Deck GetEnemyDeck(Enemy e){
    //     Deck temp = new Deck();
    //     for(var i = 0; i < e.enemyMoves.Count; i++){
    //         DeckAdd(e.enemyMoves[i], temp);
    //     }
    //     PopulateDraw(temp);
    //     return temp;
    // }

    //shuffles cards
    public void Shuffle(bool includeHand, Deck deck){
        List<Card> shuffledCards = new List<Card>();
        //adds in the cards in the hand if necessary
        if (includeHand) {
            shuffledCards.AddRange( deck.deck );
        }
        else {
            shuffledCards.AddRange(deck.discardPile);
            shuffledCards.AddRange(deck.drawPile);
        }

        //iterates through and shuffles the cards
        for(var i = 0; i < shuffledCards.Count; i++){
            int rand = Random.Range(i, shuffledCards.Count);
            var temp = shuffledCards[i];
            shuffledCards[i] = shuffledCards[rand];
            shuffledCards[rand] = temp;
            
        }

        deck.drawPile = shuffledCards;
        deck.discardPile.Clear();

        if(includeHand){
            deck.hand.Clear();
            DrawUntilFull(deck);
        }
    }

    //takes card from top of pile, removes and puts in the hand
    public Card DrawCard(Deck deck){
        if(deck.drawPile.Count <= 0){
            Shuffle(false, deck);
        }
        Card c = deck.drawPile[0];
        deck.hand.Add(c);
        deck.drawPile.RemoveAt(0);

        // //if this is buggy just comment out, its for the root system
        // if(deck.hand.Count == 5){
        //     //Debug.Log("updating combo index");
        //     updateComboIndex(deck);
        // }
        return c;
    }

    //takes in index of which card was played, removes from hand and puts into discard
    public void DiscardCard(int i, Deck deck){
        deck.discardPile.Add(deck.hand[i]);
        deck.hand.RemoveAt(i);
    }

    //adds cards to deck
    public void DeckAdd(Card c, Deck deck){
        deck.deck.Add(c);
    }

    //run when encounter starting, it makes the draw pile = to deck
    public void PopulateDraw(Deck deck){
        deck.drawPile = deck.deck;
    }

    //put all the cards in the hand
    public void PopulateHand(Deck deck){
        deck.hand = deck.deck;
    }

    public Card infinDrawCard(Deck deck) {
        return deck.hand[Random.Range(0, deck.hand.Count)];
    }

    //good to call after a turn, will draw until hand has 5 cards in it
    public void DrawUntilFull(Deck deck) {
        //if this doesn't work then switch to a while loop
        var num = 5 - deck.hand.Count;
        for (var i = 0; i < num; i++) {
            DrawCard(deck);
        }
    }

/*
    //calculates the range of the root combo (returns the furthest index)
    public void updateComboIndex(Deck deck){
        CardType c = deck.hand[0].cardType;
        bool isCombo = true;
        var index = 0;
        var comboIndex = 0;
        //Debug.Log("Root card type is: " + c);
        while((index<5) && (isCombo) && (!c.Equals(CardType.Utility))){
            //Debug.Log("In while loop");
            //Debug.Log("card type checking is: " + deck.hand[index].cardType);
            if(deck.hand[index].cardType.Equals(c)){
                //Debug.Log("is a combo");
                comboIndex = index;
                //Debug.Log("New combo index: " + comboIndex);
                index++;
            } else {
                //Debug.Log("Not a combo");
                isCombo = false;
            }
        }
        //Debug.Log("Combo index is: " + comboIndex);
        deck.comboIndex = comboIndex;
    }
    

    public bool isInCombo(int index, Deck deck){
        if(index <= deck.comboIndex){
            return true;
        }
        return false;
    }

    public List<Card> getComboCards(Deck deck){
        List<Card> comboList = new List<Card>();
        for(int i=0; i<=deck.comboIndex; i++){
            comboList.Add(deck.hand[i]);
        }
        return comboList;
    }

    public Card GetEnemyMove(Deck enemyDeck){
        int rand = Random.Range(0, 5);
        Card c = enemyDeck.hand[rand];
        DiscardCard(rand, enemyDeck);
        DrawUntilFull(enemyDeck);
        return c;
    }
    */
}