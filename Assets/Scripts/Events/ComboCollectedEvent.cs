public struct ComboCollectedEvent : IEvent
{
    public int ComboCount { get; private set; }
    
    public ComboCollectedEvent(int comboCount)
    {
        ComboCount = comboCount;
    }
}
