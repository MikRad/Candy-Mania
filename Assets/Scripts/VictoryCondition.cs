using System;

[Serializable]
public class VictoryCondition
{
    public enum Type { CellDetonate = 0, Item1Collect = 1, Item2Collect, Item3Collect, Item4Collect, Item5Collect, Item6Collect, ItemStarCollect }

    public Type _victoryConditionType;
    public int _numberNeededToComplete;

    public VictoryCondition(Type type, int numToComplete)
    {
        _victoryConditionType = type;
        _numberNeededToComplete = numToComplete;
    }
}
