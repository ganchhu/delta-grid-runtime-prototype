using UnityEngine;

public interface ISaveRepository
{
    void Save(GameSaveRoot data);
    GameSaveRoot Load();
}

public class PlayerPrefsSaveRepository : ISaveRepository
{
    private const string KEY = "MEMORY_GAME_SAVE";

    public void Save(GameSaveRoot data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    public GameSaveRoot Load()
    {
        if (!PlayerPrefs.HasKey(KEY))
            return new GameSaveRoot();

        return JsonUtility.FromJson<GameSaveRoot>(PlayerPrefs.GetString(KEY));
    }
}