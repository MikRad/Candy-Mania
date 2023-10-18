public class CellsPair
{
    public Cell FirstCell { get; set; }
    public Cell SecondCell { get; set; }

    public CellsPair()
    {
    }

    public CellsPair(Cell fCell, Cell sCell)
    {
        FirstCell = fCell;
        SecondCell = sCell;
    }
}