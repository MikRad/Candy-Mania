public struct GameItemHintCompletedEvent : IEvent
{
    public GameItem Item { get; private set; }
    
    public GameItemHintCompletedEvent(GameItem item)
    {
        Item = item;
    }
}
