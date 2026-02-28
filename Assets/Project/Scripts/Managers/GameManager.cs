using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Board Settings")]
    [SerializeField] public int rows = 4;
    [SerializeField] public int columns = 4;
    [SerializeField] private RectTransform boardArea;
    [SerializeField] private float spacing = 10f;
    [SerializeField] private float cardAspect = 2f / 3f;

    [Header("Card Settings")]
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform cardParent;
    [SerializeField] private Sprite[] sprites;

    [Header("Preview")]
    [SerializeField] private float previewDuration = 2f;

    [SerializeField] private List<Card> cards = new List<Card>();
    [SerializeField] private List<Card> selectedCards = new List<Card>();
    private Coroutine previewCoroutine;
    [SerializeField] private Sprite cardBackSprite;
    [SerializeField] private int matchesRemaining;

    // Events 
    public static System.Action<int, int> OnScoreUpdated;
    public static System.Action<int, float, float> OnGameWon;
    public static System.Action OnMatch;
    public static System.Action OnMismatch;
    public static System.Action OnGameStart;

    [SerializeField] public TMP_Text TriesText;
    [SerializeField] public TMP_Text CorrectText;
    [SerializeField] public TMP_Text RemainingText;
    [SerializeField] public TMP_Text BestAccuracyText;

    private Card firstSelected;
    private Card secondSelected;
    private bool inputLocked;



    [Header("References")]
    private int totalTries;
    private int correctMatches;
    private int totalMatches;


    private enum GameState
    {
        Preview,
        Playing,
        Resolving,
        GameOver
    }

    private void Awake()
    {
        Instance = this;
      
    }

    private void Start()
    {
        SaveData data = SaveManager.Instance.Load();

        if (data != null && data.currentGame != null)
        {
            UIManager.Instance.ShowResumePanel();
        }
        else
        {
            UIManager.Instance.ShowHome();
        }
    }

    #region Board Generation

    public void StartGame()
    {
        if (previewCoroutine != null)
        {
            StopCoroutine(previewCoroutine);
            previewCoroutine = null;
        }
        ClearBoard();
        selectedCards.Clear();
        if (ScoreManager.Instance == null)
            return;

        ScoreManager.Instance.ResetScore();

        SaveData data = SaveManager.Instance != null
            ? SaveManager.Instance.Load()
            : null;

        if (data != null && data.currentGame != null)
        {
            RestoreGame(data.currentGame);
        }
        else
        {
            GenerateBoard();
            previewCoroutine = StartCoroutine(PreviewPhase());
        }

        if (data != null && BestAccuracyText != null)
            BestAccuracyText.text = data.bestAccuracy.ToString("F1") + "%";
    }

    private void GenerateBoard()
    {
        ClearBoard();
        int totalCards = rows * columns;

        matchesRemaining = totalCards / 2;
        totalMatches = matchesRemaining;

        List<int> spriteIDs = GenerateSpritePairs(totalCards);

        for (int i = 0; i < totalCards; i++)
        {
            CreateCard(spriteIDs[i]);
        }

        PositionCards(rows, columns);
        TriesText.text = "0";
        CorrectText.text = "0";
        RemainingText.text = matchesRemaining.ToString();
        OnScoreUpdated?.Invoke(0, 0);

        SaveData data = SaveManager.Instance.Load();
    if (data != null)
            BestAccuracyText.text = data.bestAccuracy.ToString("F1") + "%";
    else
        {
            BestAccuracyText.text = "0.0%";
        }


    }

    private void CreateCard(int spriteID)
    {
        Card newCard = Instantiate(cardPrefab, boardArea);

        CardData data = new CardData
        {
            id = spriteID,
            frontSprite = sprites[spriteID]
        };

        newCard.Init(data, cardBackSprite, OnCardSelected);
        cards.Add(newCard);
    }

    private List<int> GenerateSpritePairs(int totalCards)
    {
        List<int> ids = new List<int>();

        for (int i = 0; i < totalCards / 2; i++)
        {
            int spriteIndex = Random.Range(0, sprites.Length);
            ids.Add(spriteIndex);
            ids.Add(spriteIndex);
        }

        for (int i = 0; i < ids.Count; i++)
        {
            int randomIndex = Random.Range(0, ids.Count);
            (ids[i], ids[randomIndex]) = (ids[randomIndex], ids[i]);
        }

        return ids;
    }
    private void ClearBoard()
    {
        if (previewCoroutine != null)
        {
            StopCoroutine(previewCoroutine);
            previewCoroutine = null;
        }

        foreach (var c in cards)
        {
            if (c != null)
            {
                c.StopAllCoroutines();
                Destroy(c.gameObject);
            }
        }

        cards.Clear();
        selectedCards.Clear();
    }
    #endregion   

    #region Continuous Flipping
    private void OnCardSelected(Card selected)
    {
        if (selected.IsFaceUp || selected.IsMatched)
            return;

        selected.FlipUp();
        selectedCards.Add(selected);

        if (selectedCards.Count == 2)
        {
            StartCoroutine(ResolvePair(selectedCards[0], selectedCards[1]));
            selectedCards.Clear();
        }       
    }
    private IEnumerator ResolvePair(Card first, Card second)
    {
        ScoreManager.Instance.AddTry();
        TriesText.text = ScoreManager.Instance.TotalTries.ToString();
        OnScoreUpdated?.Invoke(ScoreManager.Instance.TotalTries,ScoreManager.Instance.CorrectMatches);

        yield return new WaitForSeconds(0.4f);

        if (first.Data.id == second.Data.id)
        {
            ScoreManager.Instance.AddCorrect();
            CorrectText.text = ScoreManager.Instance.CorrectMatches.ToString();
            matchesRemaining--;
            RemainingText.text = matchesRemaining.ToString();

            first.SetMatched();
            second.SetMatched();

            OnMatch?.Invoke();

            OnScoreUpdated?.Invoke(ScoreManager.Instance.TotalTries,ScoreManager.Instance.CorrectMatches);

            if (matchesRemaining == 0)
                HandleGameWin();
        }
        else
        {
            first.FlipDown();
            second.FlipDown();
            OnMismatch?.Invoke();
        }
    }

    #endregion

    #region Win Handling
    private void HandleGameWin()
    {
        float accuracy = ScoreManager.Instance.GetAccuracy();
        SaveData data = SaveManager.Instance.Load() ?? new SaveData();
        // Compare and update
        if (accuracy > data.bestAccuracy)
        {
            data.bestAccuracy = accuracy;
        }

        data.currentGame = null;
        SaveManager.Instance.Save(data);  
        SaveManager.Instance.DeleteSave();
        OnGameWon?.Invoke(ScoreManager.Instance.TotalTries,accuracy,data.bestAccuracy);        
        AudioManager.Instance.Play(AudioManager.SFX.GameWin);
    }
    #endregion

    #region Save / Resume
    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveCurrentGame();
    }
    private void OnApplicationQuit()
    {
        SaveCurrentGame();
    }
    private void SaveCurrentGame()
    {
        if (matchesRemaining <= 0) return;

        SaveData data = SaveManager.Instance.Load() ?? new SaveData();
        GameSaveState state = new GameSaveState();

        state.rows = rows;
        state.columns = columns;
        state.totalTries = ScoreManager.Instance.TotalTries;
        state.correctMatches = ScoreManager.Instance.CorrectMatches;
        state.matchesRemaining = matchesRemaining;
        foreach (var card in cards)
        {
            state.spriteIDs.Add(card.Data.id);
            state.matchedStates.Add(card.IsMatched);
            //state.faceUpStates.Add(card.IsFaceUp);
        }
        data.currentGame = state;
        SaveManager.Instance.Save(data);
    }
    private void RestoreGame(GameSaveState state)
    {
        rows = state.rows;
        columns = state.columns;
        matchesRemaining = state.matchesRemaining;

        ClearBoard();

        for (int i = 0; i < state.spriteIDs.Count; i++)
        {
            CreateCard(state.spriteIDs[i]);

            if (state.matchedStates[i])
                cards[i].SetMatchedInstant();
            else 
                cards[i].ResetCard();            
        }

        PositionCards(rows, columns);

        ScoreManager.Instance.TotalTries = state.totalTries;
        ScoreManager.Instance.CorrectMatches = state.correctMatches;
        TriesText.text = state.totalTries.ToString();
        CorrectText.text = state.correctMatches.ToString();
        RemainingText.text = matchesRemaining.ToString();
        selectedCards.Clear();
        OnScoreUpdated?.Invoke(state.totalTries, state.correctMatches);
    }

    #endregion

    #region Layout
    private void PositionCards(int rows, int cols)
    {
        float boardWidth = boardArea.rect.width;
        float boardHeight = boardArea.rect.height;

        float totalSpacingX = spacing * (cols - 1);
        float totalSpacingY = spacing * (rows - 1);

        float cellWidth = (boardWidth - totalSpacingX) / cols;
        float cellHeight = (boardHeight - totalSpacingY) / rows;
        float cardWidth;
        float cardHeight;
        cardHeight = cellHeight;
        cardWidth = cardHeight * cardAspect;

        if (cardWidth > cellWidth)
        {
            cardWidth = cellWidth;
            cardHeight = cardWidth / cardAspect;
        }

        float gridWidth = cardWidth * cols + totalSpacingX;
        float gridHeight = cardHeight * rows + totalSpacingY;

        float startX = -gridWidth / 2f + cardWidth / 2f;
        float startY = gridHeight / 2f - cardHeight / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            float x = startX + col * (cardWidth + spacing);
            float y = startY - row * (cardHeight + spacing);

            RectTransform rt = cards[i].GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(cardWidth, cardHeight);
            rt.anchoredPosition = new Vector2(x, y);
        }
    }

    #endregion

    #region"Transitions"
    private IEnumerator PreviewPhase()
    {
        List<Card> snapshot = new List<Card>(cards);

        foreach (var card in snapshot)
        {
            if (card != null && card.gameObject.activeInHierarchy)
                card.FlipUp();
        }
        yield return new WaitForSeconds(previewDuration);
        // Close all cards
        foreach (var card in snapshot)
        {
            if (card != null && card.gameObject.activeInHierarchy)
                card.FlipDown();
        }

        OnGameStart?.Invoke();
        AudioManager.Instance.Play(AudioManager.SFX.GameStart);
    }    
    public void StopGame()
    {
        ClearBoard();
        ScoreManager.Instance.ResetScore();
        //   AudioManager.Instance.Play(AudioManager.SFX.GameOver);

    }
    public void RestartGame()
    {
        if (previewCoroutine != null)
        {
            StopCoroutine(previewCoroutine);
            previewCoroutine = null;
        }
        SaveData data = SaveManager.Instance.Load();
        if (data != null)
        {
            data.currentGame = null;
            SaveManager.Instance.Save(data);
        }
        ClearBoard();
        ScoreManager.Instance.ResetScore();
        GenerateBoard();
        StartCoroutine(RestartWithPreview());
       // previewCoroutine = StartCoroutine(PreviewPhase());
        UIManager.Instance.ShowGame();
    }
    private IEnumerator RestartWithPreview()
    {
        UIManager.Instance.ShowGame();

        yield return null;          
        yield return new WaitForEndOfFrame(); 

        previewCoroutine = StartCoroutine(PreviewPhase());
    }
    public void ReturnHome()
    {
        UIManager.Instance.ShowHome();
    }    
    private float GetAccuracy()
    {
        if (totalTries == 0) return 0;
        return (float)correctMatches / totalTries * 100f;
    }
    public void ShowHome()
    {
        UIManager.Instance.ShowHome();
    }
    public void ShowGame()
    {
        UIManager.Instance.ShowGame();
    }
    public void ShowWin()
    {
        UIManager.Instance.ShowWin();
    } 
    public int GetTotalTries()
    {
        return totalTries;
    }
    public int GetCorrectMatches()
    {
        return correctMatches;
    }
    public int GetTotalMatches()
    {
        return totalMatches;
    }
    public float GetAccuracyPercentage()
    {
        if (totalTries == 0) return 0f;
        return (float)correctMatches / totalTries * 100f;
    }
    #endregion
}