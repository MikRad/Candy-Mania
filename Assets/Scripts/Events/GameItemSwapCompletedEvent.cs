public struct GameItemSwapCompletedEvent : IEvent
{
    public GameItem Item { get; private set; }
    
    public GameItemSwapCompletedEvent(GameItem item)
    {
        Item = item;
    }
}
