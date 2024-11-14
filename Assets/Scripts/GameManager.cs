using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using JSAM;

public enum GameState { ShuffleCards, HandCards, PlayerChoice, ChangeCards, FlipCards, ShowWinner }
public enum HandType { FiveOfAKind, FourOfAKind, FullHouse, ThreeOfAKind, TwoPairs, OnePair, Junk }
public enum TurnResults { PlayerWins, LuigiWins, Tie }

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform[] playerCardPos;
    [SerializeField] private Transform[] luigiCardPos;
    [SerializeField] private Image winText;
    [SerializeField] private Sprite[] text;
    [SerializeField] private Animator luigiAnimator;
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text starsText;
    [SerializeField] private TMP_Text betText;
    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] private CardValueHolder cardValueHolder;

    [SerializeField] HandResultUI luigiHandResultUI;
    [SerializeField] HandResultUI playerHandResultUI;

    private int coins = 10;
    private int stars = 0;
    private int betAmount = 1;

    private CardType[] luigiHand;
    private CardType[] playerHand;
    private Card[] luigiCards;
    private Card[] playerCards;
    private GameState GameState = GameState.ShuffleCards;

    private CardGenerator cardGenerator = new CardGenerator();

    private void Start()
    {
        // Initialize hands and card positions
        luigiHand = new CardType[5];
        playerHand = new CardType[5];
        luigiCards = new Card[5];
        playerCards = new Card[5];

        AudioManager.PlayMusic(DefaultMusic.CasinoTheme);
        cardGenerator.InitializeDeck();
        cardValueHolder.Initialize();

        gameOverPanel.SetActive(false);

        UpdateUI();

        GenerateNewHand();
    }

    private void UpdateUI()
    {
        coinsText.text = $"{coins}x";
        starsText.text = $"{stars}x";
        betText.text = $"{betAmount}x"; 
    }

    private IEnumerator HandCards()
    {
        GameState = GameState.HandCards;

        luigiAnimator.SetTrigger("BeginShuffle");
        yield return new WaitForSeconds(2.5f);

        for (int i = 0; i < 5; i++)
        {
            luigiCards[i] = Instantiate(cardPrefab, luigiCardPos[i]).GetComponent<Card>();
            luigiCards[i].InitializeCard(luigiHand[i]);
            luigiCards[i].FlipCard(true, false);

            playerCards[i] = Instantiate(cardPrefab, playerCardPos[i]).GetComponent<Card>();
            playerCards[i].InitializeCard(playerHand[i]);
            playerCards[i].FlipCard(true, false);

            luigiAnimator.Play("anim0");

            AudioManager.PlaySound(DefaultSounds.HandCard);

            yield return new WaitForSeconds(0.26667f);
        }

        yield return new WaitForSeconds(0.2f);
        luigiAnimator.Play("anim2");

        AudioManager.PlaySound(DefaultSounds.LuigiYa);

        foreach (Card card in playerCards)
        {
            card.FlipCard(false, true);
        }

        GameState = GameState.PlayerChoice;

        foreach (Card card in playerCards)
        {
            card.CanCardBeSelected = true;
        }
    }

    private IEnumerator ReplaceSelectedCards()
    {
        GameState = GameState.ChangeCards;

        if (playerCards.Any(card => card.IsSelected))
        {
            luigiAnimator.SetTrigger("BeginShuffle");
            yield return new WaitForSeconds(1.5f);

            List<Card> changedCards = new List<Card>();

            for (int i = 0; i < 5; i++)
            {
                if (playerCards[i].IsSelected)
                {
                    playerHand[i] = cardGenerator.GetRandomCard();
                    playerCards[i].InitializeCard(playerHand[i]);
                    playerCards[i].FlipCard(true, false);
                    playerCards[i].IsSelected = false;
                    changedCards.Add(playerCards[i]);

                    luigiAnimator.Play("anim0");

                    AudioManager.PlaySound(DefaultSounds.HandCard);

                    yield return new WaitForSeconds(0.26667f);
                }
            }

            luigiAnimator.SetTrigger("PlayIdle");

            yield return new WaitForSeconds(0.05f);

            foreach (Card card in changedCards)
            {
                card.FlipCard(false, true);
            }

            yield return new WaitForSeconds(0.7f);
        }

        if (luigiCards.Any(card => card.IsSelected))
        {
            luigiAnimator.SetTrigger("BeginShuffle");
            yield return new WaitForSeconds(1.5f);

            for (int i = 0; i < 5; i++)
            {
                if (luigiCards[i].IsSelected)
                {
                    luigiHand[i] = cardGenerator.GetRandomCard();
                    luigiCards[i].InitializeCard(playerHand[i]);
                    luigiCards[i].FlipCard(true, false);
                    luigiCards[i].IsSelected = false;

                    luigiAnimator.Play("anim0");

                    AudioManager.PlaySound(DefaultSounds.HandCard);

                    yield return new WaitForSeconds(0.26667f);
                }
            }

            luigiAnimator.SetTrigger("PlayIdle");
        }

        yield return new WaitForSeconds(0.5f);

        PlayHand();
    }

    private void ChooseLuigiCardsToReplace()
    {
        Dictionary<CardType, int> cardCounts = new Dictionary<CardType, int>();

        // Count occurrences of each card type in Luigi's hand
        foreach (CardType card in luigiHand)
        {
            if (cardCounts.ContainsKey(card))
            {
                cardCounts[card]++;
            }
            else
            {
                cardCounts[card] = 1;
            }
        }

        // Select cards that appear only once and have the lowest value
        List<CardType> cardsToReplace = cardCounts
            .Where(pair => pair.Value == 1)
            .OrderBy(pair => (int)pair.Key)
            .Select(pair => pair.Key)
            .ToList();

        // Mark the selected cards for replacement
        foreach (Card card in luigiCards)
        {
            if (cardsToReplace.Contains(card.CardType))
            {
                card.IsSelected = true;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnPlayButtonPressed();
        }
    }

    public void OnPlayButtonPressed()
    {
        if (GameState == GameState.PlayerChoice)
        {
            foreach (Card card in playerCards)
            {
                card.CanCardBeSelected = false;
            }

            AudioManager.PlaySound(DefaultSounds.Confirm);

            ChooseLuigiCardsToReplace();
            StartCoroutine(ReplaceSelectedCards());
        }
    }

    private void GenerateNewHand()
    {
        // Generate hands for Luigi and the player
        luigiHand = cardGenerator.GenerateHand();
        playerHand = cardGenerator.GenerateHand();

        // Clean up previous cards in the scene
        for (int i = 0; i < 5; i++)
        {
            if (luigiCards[i] != null)
                Destroy(luigiCards[i].gameObject);

            if (playerCards[i] != null)
                Destroy(playerCards[i].gameObject);
        }

        StartRound();
    }

    private void PlayHand()
    {
        GameState = GameState.FlipCards;

        HandInfo luigiHandInfo = GetHandInfo(luigiHand);
        HandInfo playerHandInfo = GetHandInfo(playerHand);

        luigiHand = SortHand(luigiHand, luigiHandInfo);
        playerHand = SortHand(playerHand, playerHandInfo);

        for (int i = 0; i < 5; i++)
        {
            luigiCards[i].InitializeCard(luigiHand[i]);

            playerCards[i].InitializeCard(playerHand[i]);
        }

        foreach (Card card in luigiCards)
        {
            card.FlipCard(true, false); // Workaround
            card.FlipCard(false, true);
        }

        Debug.Log("Luigi's hand: " + string.Join(", ", luigiHandInfo.types) + " - " + luigiHandInfo.handType);

        Debug.Log("Player's hand: " + string.Join(", ", playerHandInfo.types) + " - " + playerHandInfo.handType);

        TurnResults results = DetectWinner(luigiHandInfo, playerHandInfo);
        GameState = GameState.ShowWinner;

        HandleRoundResults(results, playerHandInfo.handType);

        winText.sprite = text[(int)results];

        luigiHandResultUI.ShowResult(luigiHandInfo.handType);
        playerHandResultUI.ShowResult(playerHandInfo.handType);

        StartCoroutine(TurnEnded());
    }

    private void HandleRoundResults(TurnResults results, HandType playerHandType)
    {
        int rewardMultiplier = GetRewardMultiplier(playerHandType);

        switch (results)
        {
            case TurnResults.PlayerWins:
                int winnings = betAmount * rewardMultiplier;
                coins += winnings;
                stars++;
                luigiAnimator.SetTrigger("PlayerWins");

                AudioManager.PlaySound(DefaultSounds.Win);
                AudioManager.PlaySound(DefaultSounds.LuigiHappy);

                Debug.Log($"Player wins {winnings} coins!");
                break;
            case TurnResults.LuigiWins:
                if (stars > 0) stars--;  // Deduct a star on loss, if possible
                luigiAnimator.SetTrigger("LuigiWins");
                Debug.Log("Luigi wins! Player loses the bet.");

                AudioManager.PlaySound(DefaultSounds.Lose);
                AudioManager.PlaySound(DefaultSounds.LuigiNo);

                break;
            case TurnResults.Tie:
                coins += betAmount;  // Return the bet on a tie
                luigiAnimator.SetTrigger("LuigiWins");
                Debug.Log("It's a tie! Bet returned.");

                AudioManager.PlaySound(DefaultSounds.Lose);
                AudioManager.PlaySound(DefaultSounds.LuigiNo);


                break;
        }

        UpdateUI();
    }

    private int GetRewardMultiplier(HandType handType)
    {
        return handType switch
        {
            HandType.OnePair => 2,
            HandType.TwoPairs => 3,
            HandType.ThreeOfAKind => 4,
            HandType.FullHouse => 6,
            HandType.FourOfAKind => 8,
            HandType.FiveOfAKind => 16,
            _ => 1
        };
    }

    private IEnumerator TurnEnded()
    {
        cameraAnimator.SetBool("FocusOnCards", true);

        yield return new WaitForSeconds(1f);

        winText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        winText.gameObject.SetActive(false);

        luigiHandResultUI.HideResult();
        playerHandResultUI.HideResult();

        cameraAnimator.SetBool("FocusOnCards", false);

        betAmount = 1;

        if (coins <= 0)
        {
            EndGame();
        }
        else
        {
            GenerateNewHand();
        }
    }

    private TurnResults DetectWinner(HandInfo luigiHandInfos, HandInfo playerHandInfos)
    {
        // Convert hand types to integer values for comparison
        int luigiHandValue = GetHandValue(luigiHandInfos);
        int playerHandValue = GetHandValue(playerHandInfos);

        // Compare the hand values
        if (luigiHandValue > playerHandValue)
        {
            // Luigi wins
            return TurnResults.LuigiWins;
        }
        else if (playerHandValue > luigiHandValue)
        {
            // Player wins
            return TurnResults.PlayerWins;
        }
        else
        {
            // If both hands have the same value, it's a tie
            return TurnResults.Tie;
        }
    }

    private int GetHandValue(HandInfo handInfo)
    {
        // Assign values to different hand types for comparison
        int handValue = 0;
        switch (handInfo.handType)
        {
            case HandType.FiveOfAKind:
                handValue += 600;
                break;
            case HandType.FourOfAKind:
                handValue += 500;
                break;
            case HandType.FullHouse:
                handValue += 400;
                break;
            case HandType.ThreeOfAKind:
                handValue += 300;
                break;
            case HandType.TwoPairs:
                handValue += 200;
                break;
            case HandType.OnePair:
                handValue += 100;
                break;
            default:
                break;
        }

        foreach (CardType type in handInfo.types)
        {
            handValue += GetCardWeight(type);
        }
        return handValue;
    }

    private int GetCardWeight(CardType card)
    {
        switch (card)
        {
            case CardType.Star:
                return 29;
            case CardType.Mario:
                return 17;
            case CardType.Luigi:
                return 13;
            case CardType.Flower:
                return 11;
            case CardType.Mushroom:
                return 7;
            case CardType.Cloud:
                return 5;
            default:
                return 0; // Fallback value if the card type isn't recognized
        }
    }


    private CardType[] SortHand(CardType[] startingHand, HandInfo handInfo)
    {
        CardType[] sortedHand = new CardType[5];
        List<CardType> handList = startingHand.ToList();

        // Sort by hand type priority first
        List<CardType> priorityCards = new List<CardType>();
        foreach (var type in handInfo.types)
        {
            // Add cards of the current hand type to priorityCards
            priorityCards.AddRange(handList.Where(card => card == type));
        }

        // Remove priority cards from handList
        foreach (var card in priorityCards)
        {
            handList.Remove(card);
        }

        // Sort priority cards by hand type first, then by value within each hand type
        priorityCards = priorityCards
            .OrderBy(card => handInfo.handType) // Assuming handType gives priority of hand type
            .ThenBy(card => (int)card) // Sort by card value in descending order
            .ToList();

        // Sort remaining cards by value in descending order
        handList.Sort((a, b) => ((int)a).CompareTo((int)b));

        // Combine priority cards and remaining sorted cards
        priorityCards.AddRange(handList);

        for (int i = 0; i < 5; i++)
        {
            sortedHand[i] = priorityCards[i];
        }

        return sortedHand;
    }


    private HandInfo GetHandInfo(CardType[] hand)
    {
        Dictionary<CardType, int> cardCounts = new Dictionary<CardType, int>();

        // Count occurrences of each card type
        foreach (CardType card in hand)
        {
            if (cardCounts.ContainsKey(card))
            {
                cardCounts[card]++;
            }
            else
            {
                cardCounts[card] = 1;
            }
        }

        bool hasThreeOfAKind = false;
        bool hasPair = false;
        HandType handType = HandType.Junk;
        List<CardType> handTypes = new List<CardType>();

        // Determine hand types based on counts
        foreach (var cardCount in cardCounts)
        {
            switch (cardCount.Value)
            {
                case 2:
                    handTypes.Add(cardCount.Key);
                    hasPair = true;
                    handType = HandType.OnePair;
                    break;
                case 3:
                    handTypes.Add(cardCount.Key);
                    hasThreeOfAKind = true;
                    handType = HandType.ThreeOfAKind;
                    break;
                case 4:
                    handTypes.Add(cardCount.Key);
                    handType = HandType.FourOfAKind;
                    break;
                case 5:
                    handTypes.Add(cardCount.Key);
                    handType = HandType.FiveOfAKind;
                    break;
            }
        }

        // Check for Full House
        if (hasThreeOfAKind && hasPair)
        {
            handType = HandType.FullHouse;
        }

        // Check for Two Pairs
        if (handTypes.Count == 2 && handType == HandType.OnePair)
        {
            handType = HandType.TwoPairs;
        }

        return new HandInfo(handTypes, handType);
    }

    private void StartRound()
    {
        if (coins < betAmount)
        {
            Debug.Log("Not enough coins to continue the game.");
            EndGame();
            return;
        }

        AudioManager.PlaySound(DefaultSounds.LuigiLetsGo);

        coins -= betAmount;
        UpdateUI();
        StartCoroutine(HandCards());
    }

    private void EndGame()
    {
        Debug.Log("Game Over! You've run out of coins.");
        // Add any end game logic, like displaying a restart option
        gameOverPanel.SetActive(true);
    }

    public void ResetGame()
    {
        gameOverPanel.SetActive(false);
        coins = 10;
        stars = 0;
        betAmount = 1;
        UpdateUI();
        GenerateNewHand();
    }

    public void IncreaseBet()
    {
        if (GameState != GameState.PlayerChoice)
            return;

        if (betAmount < 5 && coins > 0)
        {
            betAmount++;
            coins--;
            UpdateUI();

            AudioManager.PlaySound(DefaultSounds.BetCoin);

            Debug.Log("Bet increased to: " + betAmount);
        }
    }

    public void DecreaseBet()
    {
        if (GameState != GameState.PlayerChoice)
            return;

        if (betAmount > 1)
        {
            betAmount--;
            coins++;
            UpdateUI();
            Debug.Log("Bet decreased to: " + betAmount);
        }
    }
}

public class HandInfo
{
    public List<CardType> types;
    public HandType handType;

    public HandInfo(List<CardType> types, HandType handType)
    {
        this.types = types;
        this.handType = handType;
    }

    public HandInfo(CardType type, HandType handType)
    {
        types = new List<CardType> { type };
        this.handType = handType;
    }
}
