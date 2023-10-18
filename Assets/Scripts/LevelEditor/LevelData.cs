using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData : ScriptableObject
{
    public List<CellData> _cellDatas;
    public List<VictoryCondition> _victoryConditions;
    public int _specialItemGenerationProbability;
    public int _bombItemGenerationProbability;
    public int _vertBombItemGenerationProbability;
    public int _horBombItemGenerationProbability;
    public int _starItemGenerationProbability;
    public float _timeLimit;

    public static LevelData Load(string name)
    {
        return Resources.Load<LevelData>(GetLevelResourcePath(name));
    }

    private static string GetLevelResourcePath(string name)
    {
        return "Levels/" + name;
    }
}
