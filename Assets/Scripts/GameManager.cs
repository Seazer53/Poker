using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    private int pot;
    private int bet;
    private int aiBet;

    // How many cards are dealed
    private int cardCount;
    
    //Sprite for card background
    [SerializeField] private GameObject hideCard1;
    [SerializeField] private GameObject hideCard2;

    // Boolean to check if game started or not
    private bool gameStarted;
    private bool publicDeal;
    
    //Boolean to check bets whether betted or not
    private bool aiBetted;
    private bool playerBetted;
    private bool aiCheck;
    private bool playerFold;
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
            potText.text = "Pot: ₺" + pot;
            
            bet = aiBet;
            playerScript.AdjustMoney(-bet);
            cashText.text = "Money: ₺" + playerScript.GetMoney();
            betText.text = "₺" + bet;
            
            aiBetted = false;
        }
        
        else if (playerBetted)
        {
            pot += bet;
            potText.text = "Pot: ₺" + pot;
            bettedText.text = "Bet: ₺" + bet;
            cashText.text = "Money: ₺" + playerScript.GetMoney();
            betText.text = "₺" + bet;
        }
    }

    private void IncrementClicked()
    {
        if (playerScript.GetMoney() > 0)
        {
            bet += 10;
            playerScript.AdjustMoney(-10);
            cashText.text = "Money: ₺" + playerScript.GetMoney();
            betText.text = "₺" + bet;
        }
    }
    
    private void DecrementClicked()
    {
        if (bet > aiBet && bet > 0)
        {
            bet += -10;
            playerScript.AdjustMoney(10);
            cashText.text = "Money: ₺" + playerScript.GetMoney();
            betText.text = "₺" + bet;
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
            
            hideCard1.GetComponent<SpriteRenderer>().enabled = true;
            hideCard2.GetComponent<SpriteRenderer>().enabled = true;

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
        playerFold = true;
        RoundOver();
    }

    private void RoundOver()
    {
        if (playerFold)
        {
            Debug.Log("PLAYER FOLDS, AI WON!");
        }

        else
        {
            List<string> playerCards = playerScript.GetCardNames();
            List<string> aiCards = dealerScript.GetCardNames();
            List<string> publicCards = publicScript.GetCardNames();
        
            var playerHandValue = EvaluateHand(playerCards, publicCards);
            var aiHandValue = EvaluateHand(aiCards, publicCards);

            if (playerHandValue > aiHandValue)
            {
                Debug.Log("Player won!");
            }

            else
            {
                Debug.Log("YOU LOST!");
            }
        }

    }

    // Check for winner and loser, hand is over
    private int EvaluateHand(List<string> playerCards, List<string> publicCards)
    {
        bool exists;
        bool ace = false;
        
        int heartsCount = 0;
        int diamondsCount = 0;
        int spadesCount = 0;
        int clubsCount = 0;

        int cardCount = 0;
        int score = 0;
        
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
                int valueIndex = card.IndexOf("s", StringComparison.Ordinal);

                switch (card.Substring(valueIndex + 1))
                {
                    case "J":
                        cardNumbers.Add(11);
                        cardCount++;
                        break;
                    case "Q":
                        cardNumbers.Add(12);
                        cardCount++;
                        break;
                    case "K":
                        cardNumbers.Add(13);
                        cardCount++;
                        break;
                    case "A":
                        cardNumbers.Add(14);
                        cardCount++;
                        ace = true;
                        break;
                    default:
                    {
                        var num = int.Parse(card.Substring(valueIndex + 1));
                        cardNumbers.Add(num);
                        
                        if (num == 10)
                        {
                            cardCount++;
                        }
                        
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

                if (card == cards.Last())
                {
                    if (heartsCount == 5 || diamondsCount == 5 || spadesCount == 5 || clubsCount == 5)
                    {
                        if (cardCount == 5)
                        {
                            score = 10;
                            return score;
                        }
                        
                        if (score < 9)
                        {
                            if (ace)
                            {
                                cardNumbers.Remove(14);
                                cardNumbers.Add(1);
                            
                                cardNumbers.Sort();
                            
                                if (cardNumbers.Zip(cardNumbers.Skip(1), (a, b) => (a + 1) == b).All(x => x))
                                {
                                    score = 9;
                                }
                            
                            }

                            else
                            {
                                cardNumbers.Sort();

                                if (cardNumbers.Zip(cardNumbers.Skip(1), (a, b) => (a + 1) == b).All(x => x))
                                {
                                    score = 9;
                                }
                            }
                        }

                        if (score < 6)
                        {
                            score = 6;
                        }
                    }
                    
                    var duplicates = cardNumbers.GroupBy(x => x).Where(x => x.Skip(1).Any());

                    int countPairs = 0;

                    if ((heartsCount > 0 && heartsCount < 3) && (diamondsCount > 0 && diamondsCount < 3) && (spadesCount > 0 && spadesCount < 3) && (clubsCount > 0 && clubsCount < 3))
                    {
                        List<int> countList = new List<int> { heartsCount, diamondsCount, spadesCount, clubsCount};
                        int twoCount = 0;

                        foreach (var count in countList)
                        {
                            if (count == 2)
                            {
                                twoCount++;
                            }
                        }

                        if (duplicates.Count() == 4 && twoCount == 1 && score < 8)
                        {
                            score = 8;
                        }

                        if (duplicates.Count() == 3 && score < 4)
                        {
                            score = 4;
                        }

                        if (score < 7)
                        {
                            bool threeExists = false;
                            bool twoExists = false;
                        
                            var countDuplicates = from x in duplicates
                                group x by x into g
                                let count = g.Count()
                                orderby count descending
                                select new {Value = g.Key, Count = count};

                            foreach (var countDuplicate in countDuplicates)
                            {
                                if (countDuplicate.Value.Count() == 3)
                                {
                                    threeExists = true;
                                }

                                if (countDuplicate.Value.Count() == 2 && threeExists)
                                {
                                    score = 7;
                                }

                                if (countDuplicate.Value.Count() == 2)
                                {
                                    twoExists = true;
                                    countPairs++;
                                }

                                if (countDuplicate.Value.Count() == 3 && twoExists)
                                {
                                    score = 7;
                                }
                            }
                        }

                        if (countPairs == 2 && score < 3)
                        {
                            score = 3;
                        }

                        if (countPairs == 1 && score < 2)
                        {
                            score = 2;
                        }
                        
                    }

                    else
                    {
                        if (score < 5)
                        {
                            if (ace)
                            {
                                cardNumbers.Remove(14);
                                cardNumbers.Add(1);
                            
                                cardNumbers.Sort();
                            
                                if (cardNumbers.Zip(cardNumbers.Skip(1), (a, b) => (a + 1) == b).All(x => x))
                                {
                                    score = 5;
                                }
                            
                            }

                            else
                            {
                                cardNumbers.Sort();

                                if (cardNumbers.Zip(cardNumbers.Skip(1), (a, b) => (a + 1) == b).All(x => x))
                                {
                                    score = 5;
                                }
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
        }
        
        return score;
    }

    private static IEnumerable<T[]> Combinations<T>(IEnumerable<T> source) {
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
