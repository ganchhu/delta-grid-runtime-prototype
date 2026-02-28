using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

  //  private int totalTries;
   // private int correctMatches;
    //private int totalMatches;
    [SerializeField] public TMP_Text triesText; 
    [SerializeField] public TMP_Text accuracyText;
    [SerializeField] public TMP_Text BestAccracy;
    [SerializeField] GameObject[] panels;

    [Header("game panel")]
    [SerializeField] public TMP_Text TriesText;
    [SerializeField] public TMP_Text CorrectText;
    [SerializeField] public TMP_Text RemainingText;

    [SerializeField] private GameObject resumePanel;


    private void Awake()
    {
        Instance = this;
        ShowHome();
    }
    private void OnEnable()
    {
        GameManager.OnScoreUpdated += UpdateScoreUI;
        GameManager.OnGameWon += ShowWinPanel;
    }

    public void ShowResumePanel()
    {
        resumePanel.SetActive(true);
    }


    public void ResumeGame()
    {
        resumePanel.SetActive(false);
        ShowGame();
        GameManager.Instance.StartGame(); 
    }

    public void StartNewGameFromResume()
    {
        SaveManager.Instance.DeleteSave();
        resumePanel.SetActive(false);
        ShowHome();
    }

    private void OnDisable()
    {
        GameManager.OnScoreUpdated -= UpdateScoreUI;
        GameManager.OnGameWon -= ShowWinPanel;
    }

    public void ShowHome()
    {
        showpanel(0);       
    }

    public void ShowGame()
    {
        showpanel(1);
    }
    public void ShowSetting()
    {
        if (panels.Length > 3)
            panels[3].SetActive(true);
    }
    public void ShowWin()
    {
        showpanel(2);
      
    }
    private void showpanel(int value)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == value);
        }

    }
    public void ShowWinPanel(int tries, float accuracy, float bestAccuracy)
    {
        ShowWin();
        triesText.text = tries.ToString();
        accuracyText.text = accuracy.ToString("F1") + "%";
        BestAccracy.text = bestAccuracy.ToString("F1") + "%";
    }
    private void UpdateScoreUI(int tries, int correct)
    {
        triesText.text = tries.ToString();
        accuracyText.text = correct.ToString(); // if this is Remaining text
    }
    public void UpdateRemaining(int remaining)
    {
        RemainingText.text = remaining.ToString();
    }
    public void setthegame(int value)
    {
        SaveManager.Instance.ClearResume();
        switch (value)
        {
            case 0:
                GameManager.Instance.rows = 2;
                GameManager.Instance.columns = 2;               
                break;
            case 1:
                GameManager.Instance.rows = 2;
                GameManager.Instance.columns = 3;
                break;
            case 2:
                GameManager.Instance.rows = 5;
                GameManager.Instance.columns = 6;
                break;
            default:
            GameManager.Instance.rows = 6;
            GameManager.Instance.columns = 6;
            break;
          
        }
        ShowGame();
        GameManager.Instance.StartGame();
    }
    public void restart()
    {
        GameManager.Instance.RestartGame();
        ShowGame();
    }
    public void Backfromgame()
    {
        GameManager.Instance.StopGame();
               ShowHome();

    }
    
    public void clearsavedData()
    {
        SaveManager.Instance.DeleteSave();
    }
  




}
