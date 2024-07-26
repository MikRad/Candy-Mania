public struct LevelFailedEvent : IEvent
{
    public int LevelScore { get; private set; }
    public float TimePlayed { get; private set; }
    
    public LevelFailedEvent(int levelScore, float timePlayed)
    {
        LevelScore = levelScore;
        TimePlayed = timePlayed;
    }
}
