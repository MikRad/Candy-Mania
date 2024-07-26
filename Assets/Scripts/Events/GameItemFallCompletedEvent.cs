public struct GameItemFallCompletedEvent : IEvent
{
    public GameItem Item { get; private set; }
    
    public GameItemFallCompletedEvent(GameItem item)
    {
        Item = item;
    }
}
