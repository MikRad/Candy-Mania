public struct LevelCompletedEvent : IEvent
{
    public int LevelScore { get; private set; }
    public int TotalScore { get; private set; }
    public int MaxCombo { get; private set; }
    public float TimePlayed { get; private set; }
    
    public LevelCompletedEvent(int levelScore, int totalScore, int maxCombo, float timePlayed)
    {
        LevelScore = levelScore;
        TotalScore = totalScore;
        MaxCombo = maxCombo;
        TimePlayed = timePlayed;
    }
}
