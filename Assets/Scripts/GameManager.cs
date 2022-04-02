using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Game Buttons
    public Button raiseBtn;
    public Button checkBtn;
    public Button foldBtn;
    public Button incrementBtn;
    public Button decrementBtn;

    // Access the player and dealer's script
    public PlayerScript playerScript;
    public PlayerScript dealerScript;
    public PlayerScript publicScript;

    // public Text to access and update - hud
    public Text bettedText;
    public Text cashText;
    public Text mainText;
    public Text potText;
    public Text betText;

    // How much is bet and pot
    private int pot = 0;
    private int bet = 0;
    private int aiBet = 0;

    // How many cards are dealed
    private int cardCount = 0;
    
    //Sprite for card background
    [SerializeField] Sprite cardBackground;

    // Boolean to check if game started or not
    private bool gameStarted = false;
    private bool publicDeal = false;
    
    //Boolean to check bets whether betted or not
    private bool aiBetted = false;
    private bool playerBetted = false;
    private void Start()
    {
        // Add on click listeners to the buttons
        raiseBtn.onClick.AddListener(() => RaiseClicked());
        checkBtn.onClick.AddListener(() => CheckClicked());
        foldBtn.onClick.AddListener(() => FoldClicked());
        incrementBtn.onClick.AddListener(() => IncrementClicked());
        decrementBtn.onClick.AddListener(() => DecrementClicked());
        
        AIBet();
    }

    private void AIBet()
    {
        if (!gameStarted)
        {
            checkBtn.interactable = false;
            aiBet = 10;
            dealerScript.AdjustMoney(-10);
        }

        else
        {
            aiBet = 0;
        }

        aiBetted = true;
        pot += aiBet;
        
        if (aiBet == 0)
        {
            checkBtn.interactable = true;
        }

        else
        {
            checkBtn.interactable = false;
        }

        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (gameStarted)
        {
            if (bet < aiBet)
            {
                raiseBtn.interactable = false;
            }
        }

        if (aiBetted)
        {
            potText.text = "Pot: ₺" + pot.ToString();
            
            bet = aiBet;
            playerScript.AdjustMoney(-bet);
            cashText.text = "Money: ₺" + playerScript.GetMoney().ToString();
            betText.text = "₺" + bet.ToString();
            
            aiBetted = false;
        }
        
        else if (playerBetted)
        {
            pot += bet;
            potText.text = "Pot: ₺" + pot.ToString();
            bettedText.text = "Bet: ₺" + bet.ToString();
            cashText.text = "Money: ₺" + playerScript.GetMoney().ToString();
            betText.text = "₺" + bet.ToString();
            playerBetted = false;
        }
    }

    private void IncrementClicked()
    {
        if (playerScript.GetMoney() > 0)
        {
            checkBtn.interactable = false;
            bet += 10;
            playerScript.AdjustMoney(-10);
            cashText.text = "Money: ₺" + playerScript.GetMoney().ToString();
            betText.text = "₺" + bet.ToString();
        }
    }
    
    private void DecrementClicked()
    {
        if (bet > aiBet)
        {
            bet += -10;
            playerScript.AdjustMoney(10);
            cashText.text = "Money: ₺" + playerScript.GetMoney().ToString();
            betText.text = "₺" + bet.ToString();

            if (bet == 0 && aiBet == 0)
            {
                checkBtn.interactable = true;
            }
            
        }
        
    }

    private void RaiseClicked()
    {
        playerBetted = true;
        UpdateUI();
        ShuffleAndDealCards();
    }
    
    private void CheckClicked()
    {
        playerBetted = true;
        UpdateUI();
        ShuffleAndDealCards();
    }

    private void ShuffleAndDealCards()
    {
        if (!gameStarted)
        {
            // Reset round, hide text, prep for new hand
            playerScript.ResetHand();
            dealerScript.ResetHand();

            GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();

            playerScript.StartHand();
            dealerScript.StartHand();

            for (int i = 0; i < dealerScript.hand.Length; i++)
            {
                dealerScript.hand[i].GetComponent<SpriteRenderer>().sprite = cardBackground;
            }

            gameStarted = true;
            AIBet();
        }

        else
        {
            AIBet();

            if (cardCount == 3)
            {
                publicDeal = true;
            }

            if (cardCount == 5)
            {
                RoundOver();
            }

            else
            {
                if (!publicDeal)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        publicScript.DealPublicCard();
                        cardCount++;
                    }
                }

                else
                {
                    publicScript.DealPublicCard();
                    cardCount++;
                }
            }
        }
    }

    private void FoldClicked()
    {
        RoundOver();
    }

    // Check for winnner and loser, hand is over
    void RoundOver()
    {
        // Booleans (true/false) for bust and blackjack/21
        bool playerBust = playerScript.handValue > 21;
        bool dealerBust = dealerScript.handValue > 21;

        // If stand has been clicked less than twice, no 21s or busts, quit function
        
        bool roundOver = true;

        // All bust, bets returned
        if (playerBust && dealerBust)
        {
            mainText.text = "Tie! Bet splitted";
            playerScript.AdjustMoney(pot / 2);
        }

        // if player busts, dealer didnt, or if dealer has more points, dealer wins
        else if (playerBust || (!dealerBust && dealerScript.handValue > playerScript.handValue))
        {
            mainText.text = "YOU LOST!";
        }

        // if dealer busts, player didnt, or player has more points, player wins
        else if (dealerBust || playerScript.handValue > dealerScript.handValue)
        {
            mainText.text = "YOU WIN!";
            playerScript.AdjustMoney(pot);
        }

        //Check for tie, return bets
        else if (playerScript.handValue == dealerScript.handValue)
        {
            mainText.text = "Tie: Bet splitted";
            playerScript.AdjustMoney(pot / 2);
        }

        else
        {
            roundOver = false;
        }

        // Set ui up for next move / hand / turn
        if (roundOver)
        {
            raiseBtn.gameObject.SetActive(false);
            checkBtn.gameObject.SetActive(false);
            foldBtn.gameObject.SetActive(true);
            mainText.gameObject.SetActive(true);
            cashText.text = "Money: ₺" + playerScript.GetMoney().ToString();
            gameStarted = false;
        }
    }
}
