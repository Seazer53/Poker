using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    private bool aiCheck = false;
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
        if (playerBetted && bet == aiBet)
        {
            checkBtn.interactable = true;
            aiCheck = true;
        }

        else
        {
            checkBtn.interactable = false;
            aiCheck = false;
        }
        
        if (!aiCheck)
        {
            aiBet = 10;
            dealerScript.AdjustMoney(-10);
            
            aiBetted = true;
            pot += aiBet;
        }

        playerBetted = false;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
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
        }
    }

    private void IncrementClicked()
    {
        if (playerScript.GetMoney() > 0)
        {
            bet += 10;
            playerScript.AdjustMoney(-10);
            cashText.text = "Money: ₺" + playerScript.GetMoney().ToString();
            betText.text = "₺" + bet.ToString();
        }
    }
    
    private void DecrementClicked()
    {
        if (bet > aiBet && bet > 0)
        {
            bet += -10;
            playerScript.AdjustMoney(10);
            cashText.text = "Money: ₺" + playerScript.GetMoney().ToString();
            betText.text = "₺" + bet.ToString();
        }
        
    }

    private void RaiseClicked()
    {
        playerBetted = true;
        UpdateUI();
        AIBet();
    }
    
    private void CheckClicked()
    {
        checkBtn.interactable = false;
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
        }

        else
        {
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

    // Check for winner and loser, hand is over
    void RoundOver()
    {
        bool roundOver = false;
        bool exists = false;
        bool ace = false;
        
        bool royalFlush = false;
        bool straightFlush = false;
        bool fourOfAKind = false;
        bool fullHouse = false;
        bool flush = false;
        bool straight = false;
        bool threeOfAKind = false;
        bool twoPair = false;
        bool onePair = false;
        

        int heartsCount = 0;
        int diamondsCount = 0;
        int spadesCount = 0;
        int clubsCount = 0;

        int cardCount = 0;
        
        List<string> publicCards = publicScript.GetCardNames();
        List<string> playerCards = playerScript.GetCardNames();
        List<string[]> combinationCards = new List<string[]>();
        List<int> cardNumbers = new List<int>();

        publicCards.Insert(0, playerCards[1]);
        publicCards.Insert(0, playerCards[0]);

        var result = Combinations(publicCards.ToArray());

        foreach (var item in result)
        {
            if (item.Length == 5)
            {
                var a = string.Join(", ", item);
                exists = combinationCards.Any(s => s.Contains(a));
                
                if (!exists)
                {
                    combinationCards.Add(a.Split(','));
                }
            }
        }

        foreach (var cards in combinationCards)
        {
            foreach (var card in cards)
            {
                int index = card.IndexOf("s", StringComparison.Ordinal);

                switch (card.Substring(index + 1))
                {
                    case "J":
                        cardNumbers.Add(11);
                        break;
                    case "Q":
                        cardNumbers.Add(12);
                        break;
                    case "K":
                        cardNumbers.Add(13);
                        break;
                    case "A":
                        cardNumbers.Add(14);
                        ace = true;
                        break;
                    default:
                    {
                        int num = Int32.Parse(card.Substring(index + 1));
                        cardNumbers.Add(num);
                        break;
                    }
                }

                if (card.Contains("Hearts"))
                {
                    heartsCount++;
                }
                
                else if (card.Contains("Diamonds"))
                {
                    diamondsCount++;
                }
                
                else if (card.Contains("Spades"))
                {
                    spadesCount++;
                }
                
                else if (card.Contains("Clubs"))
                {
                    clubsCount++;
                }
                
                if (card.Contains("10"))
                {
                    cardCount++;
                }
                
                else if (card.Contains("J"))
                {
                    cardCount++;
                }
                
                else if (card.Contains("Q"))
                {
                    cardCount++;
                }
                
                else if (card.Contains("K"))
                {
                    cardCount++;
                }
                        
                else if (card.Contains("A"))
                {
                    cardCount++;
                }

                if (card == cards.Last())
                {
                    if (heartsCount == 5 || diamondsCount == 5 || spadesCount == 5 || clubsCount == 5)
                    {
                        if (cardCount == 5)
                        {
                            royalFlush = true;
                            break;
                        }
                        
                        cardNumbers.Sort();

                        if (cardNumbers.Zip(cardNumbers.Skip(1), (a, b) => (a + 1) == b).All(x => x))
                        {
                            straightFlush = true;
                            break;
                        }

                        if (ace)
                        {
                            cardNumbers.Remove(14);
                            cardNumbers.Add(0);
                            
                            if (cardNumbers.Zip(cardNumbers.Skip(1), (a, b) => (a + 1) == b).All(x => x))
                            {
                                straightFlush = true;
                                break;
                            }
                            
                        }
                    }

                    heartsCount = 0;
                    diamondsCount = 0;
                    spadesCount = 0;
                    clubsCount = 0;
                    cardCount = 0;
                    ace = false;
                    cardNumbers.Clear();

                }
            }

            if (royalFlush || straightFlush)
            {
                roundOver = true;
                break;
            }
            
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
    
    public static IEnumerable<T[]> Combinations<T>(IEnumerable<T> source) {
        if (null == source)
            throw new ArgumentNullException(nameof(source));

        T[] data = source.ToArray();

        return Enumerable
            .Range(1, 1 << (data.Length))
            .Select(index => data
                .Where((v, i) => (index & (1 << i)) != 0)
                .ToArray());
    }
}
