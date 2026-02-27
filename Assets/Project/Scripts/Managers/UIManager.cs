using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private int totalTries;
   // private int correctMatches;
    private int totalMatches;
    [SerializeField] public TMP_Text triesText; 
    [SerializeField] public TMP_Text accuracyText;
    [SerializeField] public TMP_Text BestAccracy;
    [SerializeField] GameObject[] panels;

    private void Awake()
    {
        Instance = this;
        ShowHome();
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
        panels[3].SetActive(true);
    }
    public void ShowWin()
    {
        showpanel(2);
        Getthedetails();
    }
   
    private void Getthedetails()
    {
        totalTries = GameManager.Instance.GetTotalTries();
        triesText.text = GameManager.Instance.GetTotalTries().ToString();
        accuracyText.text = GameManager.Instance.GetAccuracyPercentage().ToString();
        BestAccracy.text= ScoreManager.Instance.GetAccuracy().ToString("F1") + "%";

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
    public void ShowWinPanel(int tries, float correct, float accuracy)
    {
        ShowWin();
        triesText.text =  tries.ToString();
        accuracyText.text = correct.ToString();
        BestAccracy.text = accuracy.ToString("F1") + "%";
    }




}
