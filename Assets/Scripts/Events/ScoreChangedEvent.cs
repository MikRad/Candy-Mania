public struct ScoreChangedEvent : IEvent
{
    public int ScoreDelta { get; private set; }
    public int TotalScore { get; private set; }
    public Cell SourceCell { get; private set; }
    
    public ScoreChangedEvent(int scoreDelta, int totalScore, Cell sourceCell)
    {
        ScoreDelta = scoreDelta;
        TotalScore = totalScore;
        SourceCell = sourceCell;
    }
}
