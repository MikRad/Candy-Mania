public struct ScoreChangedEvent : IEvent
{
    public int LevelScore { get; private set; }    
    public int TotalScore { get; private set; }
    
    public ScoreChangedEvent(int levelScore, int totalScore)
    {
        LevelScore = levelScore;
        TotalScore = totalScore;
    }
}
