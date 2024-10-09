using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckSelect : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Card> safeDeck;
    public List<Card> neutralDeck;
    public List<Card> riskyDeck;
    public List<Card> currentlySelectedDeck = null;

    public Button safeDeckButton;
    public Button neutralDeckButton;
    public Button riskyDeckButton;

    public TMP_Text description;

    public Button startButton;

    void Start()
    {
        safeDeckButton.onClick.AddListener(() => SelectDeck(safeDeck));
        neutralDeckButton.onClick.AddListener(() => SelectDeck(neutralDeck));
        riskyDeckButton.onClick.AddListener(() => SelectDeck(riskyDeck));

        startButton.onClick.AddListener(() => GameStart());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SelectDeck(List<Card> deck) 
    {
        currentlySelectedDeck = deck;
        //todo: description.text = deck description?,,,idk
    }

    private void GameStart() 
    {
        if (currentlySelectedDeck == null) return;

        //TODO: DO REST OF STARTING STUFF HERE :) 
        //ex copy over currently selected cards to player deck
        //display map 
    }
}
