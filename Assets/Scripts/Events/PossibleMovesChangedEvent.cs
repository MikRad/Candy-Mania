public struct PossibleMovesChangedEvent : IEvent
{
    public int PossibleMoves { get; private set; }
    
    public PossibleMovesChangedEvent(int possibleMoves)
    {
        PossibleMoves = possibleMoves;
    }
}
