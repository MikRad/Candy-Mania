public struct LevelPassConditionUpdatedEvent : IEvent
{
    public LevelPassCondition.Type PassConditionType { get; private set; }
    public int NumberToComplete { get; private set; }
    
    public LevelPassConditionUpdatedEvent(LevelPassCondition.Type lpcType, int numberToComplete)
    {
        PassConditionType = lpcType;
        NumberToComplete = numberToComplete;
    }
}
