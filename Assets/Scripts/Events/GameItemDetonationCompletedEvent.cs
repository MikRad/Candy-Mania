public struct GameItemDetonationCompletedEvent : IEvent
{
    public GameItem Item { get; private set; }
    
    public GameItemDetonationCompletedEvent(GameItem item)
    {
        Item = item;
    }
}
