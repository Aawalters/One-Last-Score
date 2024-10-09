using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{

    public List<Card> allPurchasableCards;
    public Button card0Button;
    public Button card1Button;
    public Button card2Button;
    public GameManager GM;
    public Button duplicateButton;
    public Button destroyButton;
    public GameObject deckDisplayPanel; // Panel containing the player's current deck of cards
    public GameObject cardButtonPrefab; // Prefab to create buttons for each card in the deck
    public float dupPrice;
    public float destPrice;


    // Start is called before the first frame update
    void Start()
    {
        SetUpShop();
        duplicateButton.onClick.AddListener(DisplayDeckForDuplication);
        destroyButton.onClick.AddListener(DisplayDeckForDestruction);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    * sets up the cards in the shop to be random & configures the button sprites & listeners
    * to be called whenever a level is beaten 
    */
    void SetUpShop() 
    { 
        int[] _randCards = new int[3];
        for (int i = 0; i < 3; i++) 
        {
            _randCards[i] = Random.Range(0, allPurchasableCards.Count);
        }
        card0Button.GetComponent<Image>().sprite = allPurchasableCards[0].cardImage;
        card0Button.onClick.AddListener(() => PurchaseCard(_randCards[0], card0Button));
        card1Button.GetComponent<Image>().sprite = allPurchasableCards[1].cardImage;
        card1Button.onClick.AddListener(() => PurchaseCard(_randCards[1], card1Button));
        card2Button.GetComponent<Image>().sprite = allPurchasableCards[2].cardImage;
        card2Button.onClick.AddListener(() => PurchaseCard(_randCards[2], card2Button));
    }

    private void PurchaseCard(int _cardIndex, Button button) 
    {
        Card selectedCard = allPurchasableCards[_cardIndex];
        //TODO: change to be player money? idk tbh
        if(GM.wager < selectedCard.price) 
        {
            //player is too broke to buy card! do nothing. maybe play a sad trumpet sound 
            return;
        }
        else
        {
            //yippee the player has money
            GM.wager -= selectedCard.price;
            //TODO: change so the player has the deck, cahnge .currentDeck to that 
            GM.deckController.DeckAdd(selectedCard, GM.deckController.currentDeck);
            button.gameObject.SetActive(false);
        }
    }

    void DisplayDeckForDuplication()
    {
        DisplayDeck(true);
    }

    void DisplayDeckForDestruction()
    {
        DisplayDeck(false);
    }

    void DisplayDeck(bool duplicateCard)
    {
        deckDisplayPanel.SetActive(true);

        //clear prev. buttons
        foreach (Transform child in deckDisplayPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Card card in GM.deckController.currentDeck)
        {
            GameObject cardButton = Instantiate(cardButtonPrefab, deckDisplayPanel.transform);
            cardButton.GetComponent<Image>().sprite = card.cardImage;
            cardButton.GetComponent<Button>().onClick.AddListener(() => 
            {
                if (duplicateCard)
                {
                    DuplicateCard(card);
                }
                else
                {
                    DestroyCard(card);
                }
                deckDisplayPanel.SetActive(false); //hides after selection
            });
        }

        //TODO: add a button to close the display?
    }

    void DuplicateCard(Card card)
    {
        if (GM.wager < dupPrice) 
        {
            //BROKKKEEEEE
            return;
        }
        else
        {
            GM.wager -= dupPrice;
            GM.deckController.DeckAdd(card, GM.deckController.currentDeck);
        }
    }

    void DestroyCard(Card card)
    {
        if (GM.wager < destPrice) 
        {
            //BROKKKEEEEEEEEEEEEEEEEEEEEEE
            return;
        }
        else
        {
            GM.wager -= destPrice;
            GM.deckController.DeckAdd(card, GM.deckController.currentDeck);
        }
    }
}