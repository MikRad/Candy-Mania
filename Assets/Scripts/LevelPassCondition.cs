using System;

[Serializable]
public class LevelPassCondition
{
    public enum Type { CellDetonate = 0, Item1Collect = 1, Item2Collect, Item3Collect, Item4Collect, Item5Collect, Item6Collect, ItemStarCollect }

    public Type _levelPassConditionType;
    public int _numberNeededToComplete;

    public LevelPassCondition(Type type, int numToComplete)
    {
        _levelPassConditionType = type;
        _numberNeededToComplete = numToComplete;
    }
}
