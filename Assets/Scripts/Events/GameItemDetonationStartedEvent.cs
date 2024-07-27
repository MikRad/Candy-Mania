public struct GameItemDetonationStartedEvent : IEvent
{
    public GameItem Item { get; private set; }
    
    public GameItemDetonationStartedEvent(GameItem item)
    {
        Item = item;
    }
}
