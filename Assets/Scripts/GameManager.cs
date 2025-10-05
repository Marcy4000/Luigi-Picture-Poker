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
    public static GameManager instance;

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
    [SerializeField] private Slider betSlider;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CardValueHolder cardValueHolder;
    [SerializeField] HandResultUI luigiHandResultUI;
    [SerializeField] HandResultUI playerHandResultUI;

    [SerializeField] private AudioLibrary audioLibrary;
    [SerializeField] private TMP_Text highScoreText;

    private long coins = 10;
    private int stars = 0;
    private long betAmount = 1;

    private long highScoreCoins = 0;
    private int highScoreStars = 0;

    private CardType[] luigiHand;
    private CardType[] playerHand;
    private Card[] luigiCards;
    private Card[] playerCards;
    private GameState GameState = GameState.ShuffleCards;

    private CardGenerator cardGenerator = new CardGenerator();

    public long Coins => coins;
    public int Stars => stars;

    public event System.Action OnTurnEnded;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        highScoreCoins = PlayerPrefs.HasKey("HighScoreCoins") ? long.Parse(PlayerPrefs.GetString("HighScoreCoins", "0")) : 0;
        highScoreStars = PlayerPrefs.GetInt("HighScoreStars", 0);

        luigiHand = new CardType[5];
        playerHand = new CardType[5];
        luigiCards = new Card[5];
        playerCards = new Card[5];

        cardGenerator.InitializeDeck();
        cardValueHolder.Initialize();

        gameOverPanel.SetActive(false);

        UpdateUI();

        StartCoroutine(WaitForAudioLoad());

        betSlider.minValue = 1;
        betSlider.maxValue = coins > 0 ? coins + 1 : 1;
        betSlider.value = betAmount;
        betSlider.onValueChanged.AddListener(OnBetSliderChanged);
    }
    
    private IEnumerator WaitForAudioLoad()
    {
        yield return new WaitUntil(() => audioLibrary.IsLoaded());
        AudioManager.PlayMusic(DefaultMusic.CasinoTheme);

        GenerateNewHand();
    }

    private void UpdateUI()
    {
        coinsText.text = $"{coins}x";
        starsText.text = $"{stars}x";
        betText.text = $"{betAmount}x";
        betSlider.maxValue = coins + betAmount;
        betSlider.value = betAmount;
        
        if (highScoreText != null)
            highScoreText.text = $"High Score:\n <sprite name=\"Coin\">x{highScoreCoins} <sprite name=\"Star\">x{highScoreStars}";
    }

    private void UpdateHighScores()
    {
        if (coins > highScoreCoins)
        {
            highScoreCoins = coins;
            PlayerPrefs.SetString("HighScoreCoins", highScoreCoins.ToString());
        }
        
        if (stars > highScoreStars)
        {
            highScoreStars = stars;
            PlayerPrefs.SetInt("HighScoreStars", highScoreStars);
        }
        
        PlayerPrefs.Save();
        UpdateUI();
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

        List<CardType> cardsToReplace = cardCounts
            .Where(pair => pair.Value == 1)
            .OrderBy(pair => (int)pair.Key)
            .Select(pair => pair.Key)
            .ToList();

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
        luigiHand = cardGenerator.GenerateHand();
        playerHand = cardGenerator.GenerateHand();

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
            card.FlipCard(true, false);
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
                long winnings = betAmount * rewardMultiplier;
                coins = SafeAdd(coins, winnings);
                stars++;
                luigiAnimator.SetTrigger("PlayerWins");

                AudioManager.PlaySound(DefaultSounds.Win);
                AudioManager.PlaySound(DefaultSounds.LuigiHappy);

                Debug.Log($"Player wins {winnings} coins!");
                break;
            case TurnResults.LuigiWins:
                if (stars > 0) stars--;
                luigiAnimator.SetTrigger("LuigiWins");
                Debug.Log("Luigi wins! Player loses the bet.");

                AudioManager.PlaySound(DefaultSounds.Lose);
                AudioManager.PlaySound(DefaultSounds.LuigiNo);

                break;
            case TurnResults.Tie:
                coins = SafeAdd(coins, betAmount);
                luigiAnimator.SetTrigger("LuigiWins");
                Debug.Log("It's a tie! Bet returned.");

                AudioManager.PlaySound(DefaultSounds.Lose);
                AudioManager.PlaySound(DefaultSounds.LuigiNo);


                break;
        }

        UpdateUI();
        UpdateHighScores();
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

        betAmount = 1 + stars;

        OnTurnEnded?.Invoke();

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
        int luigiHandValue = GetHandValue(luigiHandInfos);
        int playerHandValue = GetHandValue(playerHandInfos);

        if (luigiHandValue > playerHandValue)
        {
            return TurnResults.LuigiWins;
        }
        else if (playerHandValue > luigiHandValue)
        {
            return TurnResults.PlayerWins;
        }
        else
        {
            return TurnResults.Tie;
        }
    }

    private int GetHandValue(HandInfo handInfo)
    {
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
                return 0;
        }
    }


    private CardType[] SortHand(CardType[] startingHand, HandInfo handInfo)
    {
        CardType[] sortedHand = new CardType[5];
        List<CardType> handList = startingHand.ToList();

        List<CardType> priorityCards = new List<CardType>();
        foreach (var type in handInfo.types)
        {
            priorityCards.AddRange(handList.Where(card => card == type));
        }

        foreach (var card in priorityCards)
        {
            handList.Remove(card);
        }

        priorityCards = priorityCards
            .OrderBy(card => handInfo.handType)
            .ThenBy(card => (int)card)
            .ToList();

        handList.Sort((a, b) => ((int)a).CompareTo((int)b));

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

        if (hasThreeOfAKind && hasPair)
        {
            handType = HandType.FullHouse;
        }

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

        coins = SafeSubtract(coins, betAmount);
        UpdateUI();
        betSlider.maxValue = coins + betAmount;
        betSlider.value = betAmount;
        StartCoroutine(HandCards());
    }

    private void OnBetSliderChanged(float value)
    {
        if (GameState != GameState.PlayerChoice)
        {
            betSlider.value = betAmount;
            return;
        }

        long newBet = Mathf.RoundToInt(value);
        long delta = newBet - betAmount;

        if (delta > 0 && coins >= delta)
        {
            coins = SafeSubtract(coins, delta);
            betAmount = newBet;
            AudioManager.PlaySound(DefaultSounds.BetCoin);
        }
        else if (delta < 0)
        {
            coins = SafeAdd(coins, -delta);
            betAmount = newBet;
        }

        UpdateUI();
    }

    private void EndGame()
    {
        Debug.Log("Game Over! You've run out of coins.");
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
        UpdateHighScores();
    }

    public void IncreaseBet()
    {
        if (GameState != GameState.PlayerChoice)
            return;

        if (coins > 0)
        {
            betAmount++;
            coins = SafeSubtract(coins, 1);
            UpdateUI();
            betSlider.value = betAmount;

            AudioManager.PlaySound(DefaultSounds.BetCoin);

            Debug.Log("Bet increased to: " + betAmount);
        }
    }

    public void AllIn()
    {
        if (GameState != GameState.PlayerChoice)
            return;

        if (coins > 0)
        {
            betAmount += coins;
            coins = 0;
            UpdateUI();
            betSlider.value = betAmount;

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
            coins = SafeAdd(coins, 1);
            UpdateUI();
            betSlider.value = betAmount;
            Debug.Log("Bet decreased to: " + betAmount);
        }
    }

    public void RemoveCoins(int amount)
    {
        if (coins >= amount)
        {
            coins = SafeSubtract(coins, amount);
            UpdateUI();
        }
    }

    private long SafeAdd(long a, long b)
    {
        if (b < 0 && a < long.MinValue - b) return 0;
        if (b > 0 && a > long.MaxValue - b) return long.MaxValue;
        long result = a + b;
        return result < 0 ? 0 : result;
    }

    private long SafeSubtract(long a, long b)
    {
        if (b < 0 && a > long.MaxValue + b) return long.MaxValue;
        if (b > 0 && a < long.MinValue + b) return 0;
        long result = a - b;
        return result < 0 ? 0 : result;
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
