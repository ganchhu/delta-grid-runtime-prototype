using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ScoreManager;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private const string SAVE_KEY = "SAVE";
    private void Awake()
    {
        Instance = this;
      //  Clear();
    }
    public void Save(SaveData data)
    {
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public SaveData Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return null;
        return JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SAVE_KEY));
    }
    public void Clear()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.DeleteAll();
    }
    public void ClearResume()
    {

        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;

        SaveData data = Load();
        if (data == null) return;
        data.currentGame = null;
        Save(data);
        
    }

    public void DeleteSave()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
        }
    }
}
[System.Serializable]
public class SaveData
{
   
    public float bestAccuracy;
    public GameSaveState currentGame;
}
[System.Serializable]
public class GameSaveState
{
    public int rows;
    public int columns;
    public int totalTries;
    public int correctMatches;
    public int matchesRemaining;

    public List<int> spriteIDs = new List<int>();
    public List<bool> matchedStates = new List<bool>();
    //public List<bool> faceUpStates = new List<bool>();
}
