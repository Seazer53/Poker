using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    // Game Buttons
    public Button raiseBtn;
    public Button checkBtn;
    public Button foldBtn;
    public Button incrementBtn;
    public Button decrementBtn;
    public Button playBtn;

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
    private int roundCount = 0;

    // Ai check variables
    private float checkProb = 0.1f;
    private float randomValue;
    private float foldProb = 0.0f;

    // How many cards are dealed
    private int cardCount;
    
    //Sprite for card background
    [SerializeField] private GameObject hideCard1;
    [SerializeField] private GameObject hideCard2;

    // End game menu
    [SerializeField] private GameObject gameOverPanel;

    // Boolean to check if game started or not
    private bool gameStarted;
    private bool publicDeal;
    
    //Boolean to check bets whether betted or not
    private bool aiRaised;
    private bool playerRaised;
    private bool aiCheck;
    private bool playerCheck;
    private bool playerFold;
    private bool aiFold;
    private bool init = true;
    private bool firstBet;
    private bool aiWillCheck;
    private bool callAIBet = false;
    private bool gameOver = false;
    private bool bankrupt = false;

    private string logString = "";

    private void Start()
    {
        // Add on click listeners to the buttons
        raiseBtn.onClick.AddListener(() => RaiseClicked());
        checkBtn.onClick.AddListener(() => CheckClicked());
        foldBtn.onClick.AddListener(() => FoldClicked());
        incrementBtn.onClick.AddListener(() => IncrementClicked());
        decrementBtn.onClick.AddListener(() => DecrementClicked());
        playBtn.onClick.AddListener(() => PlayAgain());

        logString = "Game Started!\n";
        mainText.text = logString;

        firstBet = true;
        AIBet();
    }
    
    private void PlayAgain()
    {
        pot = 0;
        aiBet = 0;
        roundCount = 0;
        checkProb = 0.1f;
        foldProb = 0.0f;
        cardCount = 0;

        gameStarted = false;
        publicDeal = false;

        aiRaised = false;
        playerRaised = false;
        aiCheck = false;
        playerCheck = false;
        playerFold = false;
        aiFold = false;
        
        init = true;
        firstBet = true;
        
        aiWillCheck = false;
        callAIBet = false;
        
        gameOver = false;
        
        logString = "";
        mainText.text = logString;
        
        potText.text = "Pot: ₺" + 0;
        bettedText.text = "Bet: ₺" + 0;

        playerScript.ResetHand();
        publicScript.ResetHand();
        dealerScript.ResetHand();

        hideCard1.GetComponent<SpriteRenderer>().enabled = false;
        hideCard2.GetComponent<SpriteRenderer>().enabled = false;
        
        gameOverPanel.SetActive(false);

        checkBtn.interactable = true;
        
        AIBet();
    }

    private void AIBet()
    {
        randomValue = Random.value;
        roundCount++;
        checkProb *= roundCount;
        foldProb += roundCount / 50f;

        if (playerRaised && aiBet == bet && randomValue < checkProb)
        {
            aiWillCheck = true;
            playerRaised = false;
        }
            
        else if (randomValue < foldProb)
        {
            aiFold = true;
            gameOver = true;
            RoundOver();
            return;
        }

        else if (aiBet > bet && !init)
        {
            bet = aiBet;
        }

        else if (aiBet < bet && !init)
        {
            aiBet = bet;
        }

        else if(init)
        {
            bet = 10;
        }

        if (!aiWillCheck)
        {
            if (gameStarted)
            {
                List<string> aiCards = dealerScript.GetCardNames();
                List<int> aiCardNumbers = new List<int>();

                foreach (var card in aiCards)
                {
                    int valueIndex = card.IndexOf("s", StringComparison.Ordinal);

                    switch (card.Substring(valueIndex + 1))
                    {
                        case "J":
                            aiCardNumbers.Add(11);
                            break;
                        case "Q":
                            aiCardNumbers.Add(12);
                            break;
                        case "K":
                            aiCardNumbers.Add(13);
                            break;
                        case "A":
                            aiCardNumbers.Add(14);
                            break;
                        default:
                        {
                            var num = int.Parse(card.Substring(valueIndex + 1));
                            aiCardNumbers.Add(num);
                            break;
                        }
                    }
                }

                int sum = aiCardNumbers[0] + aiCardNumbers[1];

                if (sum < 5)
                {
                    int randomMultiplier = Random.Range(0, 4);
                    aiBet = bet + (randomMultiplier * 10);
                }
            
                else if (sum < 8)
                {
                    int randomMultiplier = Random.Range(1, 4);
                    aiBet = bet + (randomMultiplier * 10);
                }
            
                else if (sum < 11)
                {
                    int randomMultiplier = Random.Range(1, 5);
                    aiBet = bet + (randomMultiplier * 10);
                }
            
                else if (sum < 14)
                {
                    int randomMultiplier = Random.Range(2, 5);
                    aiBet = bet + (randomMultiplier * 10);
                }
            
                else if (sum < 17)
                {
                    int randomMultiplier = Random.Range(2, 6);
                    aiBet = bet + (randomMultiplier * 10);
                }
            
                else if (sum < 21)
                {
                    int randomMultiplier = Random.Range(3, 7);
                    aiBet = bet + (randomMultiplier * 10);
                }
            
                else if (sum < 25)
                {
                    int randomMultiplier = Random.Range(3, 9);
                    aiBet = bet + (randomMultiplier * 10);
                }
            
                else if (sum < 29)
                {
                    int randomMultiplier = Random.Range(4, 11);
                    aiBet = bet + (randomMultiplier * 10);
                }
            }
            
            else
            {
                aiBet = bet;
                checkProb *= 6f;
            }
        }

        if (init)
        {
            bet = aiBet;
            betText.text = "₺" + aiBet;
            init = false;
                
            dealerScript.AdjustMoney(-aiBet);
            pot += aiBet;
            aiCheck = true;

            logString += "AI Raised: " + aiBet + "\n";
            mainText.text = logString;
                
        }

        else
        {
            if ((randomValue < checkProb || playerCheck) && aiWillCheck)
            {
                checkBtn.interactable = true;
                aiCheck = true;
                aiWillCheck = false;

                if (!gameStarted)
                {
                    checkProb = 0.1f;
                }

                logString += "AI Checked! \n";
                mainText.text = logString;
                    
                bet = aiBet;
                betText.text = "₺" + bet;
            }

            else
            {
                dealerScript.AdjustMoney(-aiBet);
                pot += aiBet;
                aiRaised = true;

                checkBtn.interactable = false;

                logString += "AI Raised: " + aiBet + "\n";
                mainText.text = logString;

                bet = aiBet;
                betText.text = "₺" + bet;
                    
            }
        }

        callAIBet = false;
            
        if (callAIBet)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (playerRaised || playerCheck)
        {
            pot += bet;
            playerScript.AdjustMoney(-bet);
        
            bettedText.text = "Bet: ₺" + bet;
            cashText.text = "Money: ₺" + playerScript.GetMoney();
        }
        
        potText.text = "Pot: ₺" + pot;

        if (playerRaised)
        {
            aiCheck = false;
        }
        
        if (aiRaised && !firstBet)
        {
            playerCheck = false;
            aiRaised = false;
            checkBtn.interactable = false;
        }
        
        else if(aiCheck)
        {
            checkBtn.interactable = true;
        }

        if (aiCheck && playerCheck && !gameOver)
        {
            roundCount = 0;
            checkProb = 0.1f;
            foldProb = 0.0f;

            if (cardCount != 5)
            {
                logString += "Dealing Cards... \n";
                mainText.text = logString;
            }

            aiCheck = false;
            aiRaised = false;
            aiFold = false;
            
            playerCheck = false;
            playerRaised = false;
            
            firstBet = false;

            aiBet = 10;
            bet = 10;

            callAIBet = true;
            
            ShuffleAndDealCards();
        }
        
        else if (playerCheck || playerRaised)
        {
            callAIBet = true;
        }

        if (callAIBet && !aiFold && !gameOver)
        {
            AIBet();
        }
    }

    private void IncrementClicked()
    {
        if (playerScript.GetMoney() > 0)
        {
            bet += 10;
            betText.text = "₺" + bet;
        }
    }
    
    private void DecrementClicked()
    {
        if (bet > aiBet)
        {
            bet += -10;
            betText.text = "₺" + bet;
        }
        
    }

    private void RaiseClicked()
    {
        playerRaised = true;

        if (playerScript.GetMoney() - bet < 0)
        {
            bankrupt = true;
            gameOver = true;
            RoundOver();
        }

        else
        {
            logString += "Player Raised: " + bet + "\n";
            mainText.text = logString;
        }

        if (!gameOver)
        {
            UpdateUI();
        }
    }
    
    private void CheckClicked()
    {
        if (playerScript.GetMoney() - bet < 0)
        {
            bankrupt = true;
            gameOver = true;
            RoundOver();
        }

        else
        {
            checkBtn.interactable = false;
            playerCheck = true;
        
            logString += "Player Checked!\n";
            mainText.text = logString;
        }

        if (!gameOver)
        {
            UpdateUI();
        }
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
                gameOver = true;
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
        gameOver = true;
        
        logString += "Player Fold!\n";
        mainText.text = logString;
        
        RoundOver();
    }

    private void RoundOver()
    {
        if (bankrupt)
        {
            logString += "Player BANKRUPT!\n";
            mainText.text = logString;
        }
        
        else if (playerFold)
        {
            logString += "Player Folds, AI WIN!\n";
            mainText.text = logString;
            
            dealerScript.AdjustMoney(pot);
        }
        
        else if (aiFold)
        {
            logString += "AI Folds, Player WIN!\n";
            mainText.text = logString;
            
            playerScript.AdjustMoney(pot);
        }

        else
        {
            List<string> playerCards = playerScript.GetCardNames();
            List<string> aiCards = dealerScript.GetCardNames();
            List<string> publicCards = publicScript.GetCardNames();
        
            var playerHandValue = EvaluateHand(playerCards, publicCards);
            
            publicCards.Clear();
            publicCards = publicScript.GetCardNames();
            
            var aiHandValue = EvaluateHand(aiCards, publicCards);
            
            hideCard1.GetComponent<SpriteRenderer>().enabled = false;
            hideCard2.GetComponent<SpriteRenderer>().enabled = false;

            if (playerHandValue > aiHandValue)
            {
                logString += "YOU WIN!\n";
                mainText.text = logString;
            }

            else if(aiHandValue > playerHandValue)
            {
                logString += "YOU LOST!\n";
                mainText.text = logString;
            }

            else
            {
                logString += "DRAW! POT SHARED!\n";
                mainText.text = logString;
                
                playerScript.AdjustMoney(pot / 2);
                dealerScript.AdjustMoney(pot / 2);
                
                cashText.text = "Money: ₺" + playerScript.GetMoney();
                potText.text = "Pot: ₺" + 0;
            }
        }
        
        gameOverPanel.SetActive(true);

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
                                cardNumbers.Sort();
                            
                                if (cardNumbers.Zip(cardNumbers.Skip(1), (a, b) => (a + 1) == b).All(x => x))
                                {
                                    score = 9;
                                }

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
                    
                    var countDuplicates = from x in duplicates
                        group x by x into g
                        let count = g.Count()
                        orderby count descending
                        select new {Value = g.Key, Count = count};

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

                        if (score < 3)
                        {
                            foreach (var countDuplicate in countDuplicates)
                            {
                                if (countDuplicate.Value.Count() == 2)
                                {
                                    countPairs++;
                                }
                            }
                        
                            if (countPairs == 2)
                            {
                                score = 3;
                            }

                            if (countPairs == 1 && score < 2)
                            {
                                score = 2;
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
