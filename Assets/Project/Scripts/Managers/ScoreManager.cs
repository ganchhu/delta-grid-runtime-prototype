using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int TotalTries;
    public int CorrectMatches;


    private void Awake()
    {
        Instance = this;
    }

    public void ResetScore()
    {
        TotalTries = 0;
        CorrectMatches = 0;
    }

    public void AddTry() => TotalTries++;
    public void AddCorrect() => CorrectMatches++;

    public float GetAccuracy()
    {
        if (TotalTries == 0) return 0;
        return (float)CorrectMatches / TotalTries * 100f;
    }
    public int GetTries()
    {
        if (TotalTries == 0) return 0;
        return  TotalTries;
    }
    public int GetCorrectMatches()
    {
        if (CorrectMatches == 0) return 0;
        return CorrectMatches;
    }



}

