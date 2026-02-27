using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField] public TMP_Text TriesText;
    [SerializeField] public TMP_Text CorrectText;
    [SerializeField] public TMP_Text RemainingText;
    [SerializeField] public TMP_Text BestAccuracyText;
    public static GameManager Instance;
    [Header("Board Settings")]
    [SerializeField] public int rows = 4;
    [SerializeField] public int columns = 4;
    [SerializeField] private RectTransform boardArea;
    [SerializeField] private float spacing = 10f;

    [Header("Card Settings")]
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform cardParent;
    [SerializeField] private Sprite[] sprites;
    //[SerializeField] private int gridSize = 4;
    [SerializeField] private Sprite cardBackSprite;
    private List<Card> cards = new List<Card>();
    private Card firstSelected;
    private Card secondSelected;
    private int matchesRemaining;
    private bool inputLocked;

    [Header("Preview")]
    [SerializeField] private float previewDuration = 2f;
    private GameState currentState;


    [Header("References")]

    private int totalTries;
    private int correctMatches;
    private int totalMatches;

    //public event System.Action OnGameStarted;
    //public event System.Action OnGameWon;
    //public event System.Action<int, int> OnScoreUpdated;
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
      //  UIManager.Instance.ShowHome();
    }

    private void Start()
    {
      //  StartGame();
    }



    public void StartGame()
    {
        UIManager.Instance.ShowGame();
        ScoreManager.Instance.ResetScore();
        GenerateBoard();
        StartCoroutine(PreviewPhase());
        SaveData data = SaveManager.Instance.Load();
        BestAccuracyText.text = data.bestAccuracy.ToString("F1") + "%";
    }

    private void GenerateBoard()
    {
        totalMatches = matchesRemaining;
        totalTries = 0;
        correctMatches = 0;
          

        foreach (var c in cards)
            Destroy(c.gameObject);

        cards.Clear();

        int totalCards = rows * columns;

        if (totalCards % 2 != 0)
        {
            Debug.LogError("Total cards must be even!");
            return;
        }

        matchesRemaining = totalCards / 2;

        List<int> spriteIDs = GenerateSpritePairs(totalCards);

        for (int i = 0; i < totalCards; i++)
        {
            Card newCard = Instantiate(cardPrefab, boardArea);

            CardData data = new CardData
            {
                id = spriteIDs[i],
                frontSprite = sprites[spriteIDs[i]]
            };

            newCard.Init(data, cardBackSprite, OnCardSelected);
            cards.Add(newCard);
        }

        PositionCards(rows, columns);
        TriesText.text = "0";
        CorrectText.text = "0";
        RemainingText.text = (rows * columns / 2).ToString();
    }
    [SerializeField] private float cardAspect = 2f / 3f;
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

        //float size = Mathf.Min(cellWidth, cellHeight);

       // float gridWidth = size * cols + totalSpacingX;
       // float gridHeight = size * rows + totalSpacingY;
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

    private List<int> GenerateSpritePairs(int totalCards)
    {
        List<int> ids = new List<int>();

        for (int i = 0; i < totalCards / 2; i++)
        {
            int spriteIndex = Random.Range(0, sprites.Length);
            ids.Add(spriteIndex);
            ids.Add(spriteIndex);
        }

        // Shuffle
        for (int i = 0; i < ids.Count; i++)
        {
            int randomIndex = Random.Range(0, ids.Count);
            (ids[i], ids[randomIndex]) = (ids[randomIndex], ids[i]);
        }

        return ids;
    }

    private IEnumerator PreviewPhase()
    {
        currentState = GameState.Preview;
        inputLocked = true;

        // Open all cards
        foreach (var card in cards)
        {
            card.FlipUp();
            card.SetInteractable(false);
        }    
        yield return new WaitForSeconds(previewDuration);
        // Close all cards
        foreach (var card in cards)
        {
            card.FlipDown();
            card.SetInteractable(true);
        }
        inputLocked = false;
        currentState = GameState.Playing;
        AudioManager.Instance.Play(AudioManager.SFX.GameStart);
    }


    
    public void StopGame()
    {
        ScoreManager.Instance.ResetScore();
        currentState = GameState.GameOver;
        inputLocked = true;
        StopAllCoroutines();
        foreach (var card in cards)
            Destroy(card.gameObject);
        cards.Clear();
     //   AudioManager.Instance.Play(AudioManager.SFX.GameOver);

    }

    private void OnCardSelected(Card selected)
    {
        if (currentState != GameState.Playing) return;
        if (inputLocked) return;
        if (selected.IsFaceUp) return;
        if (selected.IsMatched) return;

        selected.FlipUp();

        if (firstSelected == null)
        {
            firstSelected = selected;
            return;
        }

        secondSelected = selected;
        StartCoroutine(ResolveSelection());
    }
    private IEnumerator ResolveSelection()
    {
        currentState = GameState.Resolving;
        inputLocked = true;
        ScoreManager.Instance.AddTry();
        // totalTries++;
         TriesText.text = ScoreManager.Instance.GetTries().ToString();

        yield return new WaitForSeconds(0.4f);

        if (firstSelected.Data.id == secondSelected.Data.id)
        {
            ScoreManager.Instance.AddCorrect();
            CorrectText.text = ScoreManager.Instance.GetCorrectMatches().ToString();
            firstSelected.SetMatched();
            secondSelected.SetMatched();

            matchesRemaining--;
            RemainingText.text = matchesRemaining.ToString();
            AudioManager.Instance.Play(AudioManager.SFX.Match);
            if (matchesRemaining == 0)
                HandleGameWin();
            //GameOver();
        }
        else
        {
            AudioManager.Instance.Play(AudioManager.SFX.Mismatch);
            firstSelected.FlipDown();
            secondSelected.FlipDown();
        }

        firstSelected = null;
        secondSelected = null;

        yield return new WaitForSeconds(0.3f);

        inputLocked = false;
        currentState = GameState.Playing;
    }
    private void HandleGameWin()
    {
        currentState = GameState.GameOver;

        float accuracy = ScoreManager.Instance.GetAccuracy();
        SaveData data = SaveManager.Instance.Load();
        if (data == null)
        {
            data = new SaveData();
        }

        // Compare and update
        if (accuracy > data.bestAccuracy)
        {
            data.bestAccuracy = accuracy;
        }

 
        data.score = ScoreManager.Instance.TotalTries;
        Debug.Log("Saving Score: " + data.score + ", Accuracy: " + data.bestAccuracy);
        SaveManager.Instance.Save(data);
        UIManager.Instance.ShowWinPanel(ScoreManager.Instance.TotalTries,accuracy,data.bestAccuracy);
        AudioManager.Instance.Play(AudioManager.SFX.GameWin);
    }
    public void RestartGame()
    {
        StopAllCoroutines();
        UIManager.Instance.ShowGame();

        foreach (var card in cards)
            Destroy(card.gameObject);
        cards.Clear();
        StartGame();
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


    private void GameOver()
    {
        currentState = GameState.GameOver;

        float accuracy = GetAccuracy();
        //triesText.text = "Tries: " + totalTries;
        //correctText.text = "Correct Matches: " + correctMatches;
        //accuracyText.text = "Accuracy: " + accuracy.ToString("F1") + "%";
        UIManager.Instance.ShowWinPanel(totalTries,correctMatches, accuracy);

  
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

}