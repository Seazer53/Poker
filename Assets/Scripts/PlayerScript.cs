using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // --- This script is for BOTH player and dealer

    // Get other scripts
    public CardScript cardScript;   
    public DeckScript deckScript;

    // Total value of player/dealer's hand
    public int handValue = 0;

    // Betting money
    private int money = 1000;

    // Array of card objects on table
    public GameObject[] hand;
    // Index of next card to be turned over
    public int cardIndex = 0;

    public void StartHand()
    {
        GetCard();
        GetCard();
    }

    public void DealPublicCard()
    {
        GetCard();
    }

    // Add a hand to the player/dealer's hand
    public int GetCard()
    {
        // Get a card, use deal card to assign sprite and value to card on table
        int cardValue = deckScript.DealCard(hand[cardIndex].GetComponent<CardScript>());

        // Show card on game screen
        hand[cardIndex].GetComponent<Renderer>().enabled = true;

        // Add card value to running total of the hand
        handValue += cardValue;
        
        cardIndex++;
        return handValue;
    }

    // Add or subtract from money, for bets
    public void AdjustMoney(int amount)
    {
        money += amount;
    }

    // Output players current money amount
    public int GetMoney()
    {
        return money;
    }

    // Hides all cards, resets the needed variables
    public void ResetHand()
    {
        for(int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard();
            hand[i].GetComponent<Renderer>().enabled = false;
        }

        cardIndex = 0;
        handValue = 0;
    }

    public List<string> GetCardNames()
    {
        List<string> cardList = new List<string>();
        
        for (int i = 0; i < hand.Length; i++)
        {
            cardList.Add(hand[i].gameObject.GetComponent<SpriteRenderer>().sprite.name);
        }

        return cardList;
    }
}
