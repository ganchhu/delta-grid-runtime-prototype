using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveRoot
{
    public List<DifficultySaveData> difficultySaves = new();
}

[Serializable]
public class DifficultySaveData
{
    public string difficultyId;
    public float bestAccuracy;
    public GameSessionData session;
}

[Serializable]
public class GameSessionData
{
    public int rows;
    public int columns;

    public int totalTries;
    public int correctMatches;
    public int matchesRemaining;

    public int[] spriteLayout;
    public bool[] matchedStates;

    public bool hasUnfinishedGame;
}