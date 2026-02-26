using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ScoreManager;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    public void Save(SaveData data)
    {
        PlayerPrefs.SetString("SAVE", JsonUtility.ToJson(data));
    }

    public SaveData Load()
    {
        if (!PlayerPrefs.HasKey("SAVE")) return null;
        return JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("SAVE"));
    }
   
}
[System.Serializable]
public class SaveData
{
    public int score;
    public float bestAccuracy;
    public bool[] matchedCards;
}
