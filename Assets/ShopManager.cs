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
    public GameObject Options; // Panel containing the player's current deck of cards
    public GameObject deckDisplayPanel; // Panel containing the player's current deck of cards
    public GameObject cardButtonPrefab; // Prefab to create buttons for each card in the deck
    public float dupPrice;
    public float destPrice;

    private void Awake()
    {
        // Buttons should be assigned in the Inspector, no need to assign them here unless necessary
    }

    // Start is called before the first frame update
    void Start()
    {
        SetUpShop();
        duplicateButton.onClick.AddListener(DisplayDeckForDuplication);
        destroyButton.onClick.AddListener(DisplayDeckForDestruction);
    }

    /*
     * Sets up the cards in the shop to be random & configures the button sprites & listeners
     * to be called whenever a level is beaten.
     */
    public void SetUpShop()
    {
        int[] _randCards = new int[3];
        for (int i = 0; i < 3; i++)
        {
            _randCards[i] = Random.Range(0, allPurchasableCards.Count);
        }
        // Using _randCards[i] instead of 0, 1, 2
        card0Button.GetComponent<Image>().sprite = allPurchasableCards[_randCards[0]].cardImage;
        card0Button.onClick.AddListener(() => PurchaseCard(_randCards[0], card0Button));

        card1Button.GetComponent<Image>().sprite = allPurchasableCards[_randCards[1]].cardImage;
        card1Button.onClick.AddListener(() => PurchaseCard(_randCards[1], card1Button));

        card2Button.GetComponent<Image>().sprite = allPurchasableCards[_randCards[2]].cardImage;
        card2Button.onClick.AddListener(() => PurchaseCard(_randCards[2], card2Button));
    }

    private void PurchaseCard(int _cardIndex, Button button)
    {
        Card selectedCard = allPurchasableCards[_cardIndex];
        if (GM.wager < selectedCard.price)
        {
            // Feedback for insufficient funds (e.g., play a sound or display a message)
            Debug.Log("Player doesn't have enough money to buy this card!");
            return;
        }
        else
        {
            // Player buys the card
            GM.wager -= selectedCard.price;
            GM.deckController.DeckAdd(selectedCard, GM.deckController.currentDeck);
            button.gameObject.SetActive(false); // Hide the button after purchase
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
        Options.SetActive(false);

        // Clear previous buttons
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
                deckDisplayPanel.SetActive(false); // Hide after selection
                Options.SetActive(true);
            });
        }
    }

    void DuplicateCard(Card card)
    {
        if (GM.wager < dupPrice)
        {
            Debug.Log("Not enough money to duplicate this card!");
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
            Debug.Log("Not enough money to destroy this card!");
            return;
        }
        else
        {
            GM.wager -= destPrice;
            GM.deckController.DeckRemove(card, GM.deckController.currentDeck); // Correcting to remove the card
        }
    }
}
