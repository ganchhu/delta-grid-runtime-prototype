using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private int totalTries;
    private int correctMatches;
    private int totalMatches;
    [SerializeField] private TMP_Text triesText;
    [SerializeField] private TMP_Text correctText;
    [SerializeField] private TMP_Text accuracyText;
    //[SerializeField] private GameObject homePanel;
    //[SerializeField] private GameObject gamePanel;
    //[SerializeField] private GameObject winPanel;
    [SerializeField] GameObject[] panels;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowHome()
    {
        showpanel(0);
    }

    public void ShowGame()
    {
        showpanel(1);
    }

    public void ShowWin()
    {
        showpanel(2);
        Getthedetails();

    }
   
    private void Getthedetails()
    {
        totalTries = GameManager.Instance.GetTotalTries();
        triesText.text = $"Tries: {GameManager.Instance.GetTotalTries()}";
        correctMatches = GameManager.Instance.GetCorrectMatches();
        correctText.text = $"Correct Matches: {GameManager.Instance.GetCorrectMatches()}";
        totalMatches = GameManager.Instance.GetTotalMatches();
        accuracyText.text = $"Accuracy: {((float)correctMatches / totalTries * 100):F1}%";
    }


    public void setthegame(int value)
    {
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
    private void showpanel(int value)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i==value);
        }
            
    }
    public void ShowWinPanel(int tries, int correct, float accuracy)
    {
        ShowWin();
        Debug.LogError("came here" + panels[2].activeSelf.ToString());
        triesText.text = "Tries: " + tries;
        correctText.text = "Correct: " + correct;
        accuracyText.text = "Accuracy: " + accuracy.ToString("F1") + "%";
    }




}
