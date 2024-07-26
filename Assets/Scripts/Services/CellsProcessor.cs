using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellsProcessor
{
    public int RowsNumber { get; }
    public int ColumnsNumber { get; }
    private int MinMatchNumber { get; }

    public Cell[,] Cells { get; set; }

    public CellsProcessor(int rowsNumber, int columnsNumber, int minMatchNumber)
    {
        RowsNumber = rowsNumber;
        ColumnsNumber = columnsNumber;
        MinMatchNumber = minMatchNumber;
    }
    
    public LinkedList<Cell> GetMatches(Cell cell, GameItemType gItemType)
    {
        LinkedList<Cell> matches = GetHorizontalMatches(cell, gItemType);
        matches.AppendRange(GetVerticalMatches(cell, gItemType));

        return matches;
    }
    
    public bool IsCorrectPossibleMove(CellsPair selectedCells)
    {
        LinkedList<Cell> possibleMoveCells = GetPossibleMoveCells(selectedCells.FirstCell.Index);
        
        return possibleMoveCells.Contains(selectedCells.SecondCell);
    }

    public bool IsCorrectFieldIndex(FieldIndex fIdx)
    {
        return ((fIdx._i >= 0) && (fIdx._i < RowsNumber) && (fIdx._j >= 0) && (fIdx._j < ColumnsNumber));
    }
    
    public LinkedList<Cell> SwapItemsBetweenCells(CellsPair cPair)
    {
        SwapItems(cPair);
        LinkedList<Cell> matches1 = GetMatches(cPair.FirstCell);
        LinkedList<Cell> matches2 = GetMatches(cPair.SecondCell);
        bool isEnoughMatches1 = IsEnoughMatches(matches1);
        bool isEnoughMatches2 = IsEnoughMatches(matches2);
        bool isSuccessfulMove = isEnoughMatches1 || isEnoughMatches2;

        if (!isSuccessfulMove)
        {
            cPair.FirstCell.GameItem.SwapToAndReturn(cPair.SecondCell.GameItem.CachedTransform.localPosition);
            cPair.SecondCell.GameItem.SwapToAndReturn(cPair.FirstCell.GameItem.CachedTransform.localPosition);
            SwapItems(cPair);
            
            return new LinkedList<Cell>();
        }

        LinkedList<Cell> matchedCells = new LinkedList<Cell>();
        if(isEnoughMatches1)
        {
            AddMatchesToList(matchedCells, matches1, cPair.FirstCell);
            CheckCellItemForUpgrade(cPair.FirstCell, matches1.Count);

        }
        if (isEnoughMatches2)
        {
            AddMatchesToList(matchedCells, matches2, cPair.SecondCell);
            CheckCellItemForUpgrade(cPair.SecondCell, matches2.Count);
        }
        cPair.FirstCell.GameItem.SwapTo(cPair.SecondCell.GameItem.CachedTransform.localPosition);
        cPair.SecondCell.GameItem.SwapTo(cPair.FirstCell.GameItem.CachedTransform.localPosition);

        CheckCellsForExplosiveItems(matchedCells);

        return matchedCells;
    }
    
    public LinkedList<Cell> CheckFallingItemsCells()
    {
        LinkedList<Cell> fallingItemsCells = new LinkedList<Cell>();

        for (int i = RowsNumber - 2; i >= 0; i--)
        {
            for (int j = 0; j < ColumnsNumber; j++)
            {
                Cell cell = Cells[i, j];
                if (cell.IsAvailable && !cell.IsEmpty)
                {
                    CheckCellGameItemForFall(cell, fallingItemsCells);
                }
            }
        }
        return fallingItemsCells;
    }

    public LinkedList<Cell> CheckCellsForDetonation(IEnumerable<Cell> cellsToCheck)
    {
        LinkedList<Cell> matchedCells = new LinkedList<Cell>();

        foreach (Cell cell in cellsToCheck)
        {
            if (cell.IsAvailable && !matchedCells.Contains(cell))
            {
                LinkedList<Cell> matches = GetMatches(cell);
                if (IsEnoughMatches(matches))
                {
                    AddMatchesToList(matchedCells, matches, cell);
                    CheckCellItemForUpgrade(cell, matches.Count);
                }
            }
        }
        CheckCellsForExplosiveItems(matchedCells);
        
        return matchedCells;
    }

    public List<CellsPair> GetPossibleSuccessfulMoves()
    {
        List<CellsPair> possibleSuccessfulMoves = new List<CellsPair>();
        for (int i = 0; i < RowsNumber; i++)
        {
            for (int j = 0; j < ColumnsNumber; j++)
            {
                Cell cell = Cells[i, j];
                if (!cell.IsAvailable)
                    continue;

                LinkedList<Cell> possibleMoveCells = GetPossibleMoveCells(cell.Index);
                foreach (Cell possibleMoveCell in possibleMoveCells)
                {
                    CellsPair cPair = new CellsPair(cell, possibleMoveCell);
                    SwapItems(cPair);
                    if (IsEnoughMatches(GetMatches(possibleMoveCell)) || IsEnoughMatches(GetMatches(cell)))
                        possibleSuccessfulMoves.Add(cPair);                        
                    SwapItems(cPair);
                }
            }
        }

        if (possibleSuccessfulMoves.Count == 0)
        {
            EventBus.Get.RaiseEvent(this, new NoMoreMovesEvent());
        }
        
        return possibleSuccessfulMoves;
    }

    private void AddMatchesToList(ICollection<Cell> matchesList, IReadOnlyCollection<Cell> matchesToAdd, Cell matchSourceCell)
    {
        matchSourceCell.CausedDetonationsNumber = matchesToAdd.Count + 1;
        
        FillDetonationDelaysForMatches(matchSourceCell, matchesToAdd);
        AddCellToListIfUnique(matchesList, matchSourceCell);
        AddUniqueCellsToList(matchesList, matchesToAdd);
    }
    
    private LinkedList<Cell> GetPossibleMoveCells(FieldIndex fIdx)
    {
        LinkedList<Cell> neighbors = new LinkedList<Cell>();
        FieldIndex[] moveDeltas = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };

        foreach (FieldIndex delta in moveDeltas)
        {
            FieldIndex idx = fIdx + delta;
            
            if ((IsCorrectFieldIndex(idx)) && (Cells[idx._i, idx._j].IsAvailable))
                neighbors.AddLast(Cells[idx._i, idx._j]);
        }
        
        return neighbors;
    }
    
    private LinkedList<Cell> GetAdjacentCells(FieldIndex fIdx)
    {
        LinkedList<Cell> adjCells = new LinkedList<Cell>();

        FieldIndex[] adjDeltas =
        {
            new(0, 1), new(0, -1), new(1, 0), new(-1, 0),
            new(1, 1), new(1, -1), new(-1, 1), new(-1, -1),
        };

        foreach (FieldIndex delta in adjDeltas)
        {
            FieldIndex idx = fIdx + delta;
            
            if ((IsCorrectFieldIndex(idx)) && (Cells[idx._i, idx._j].IsAvailable))
                adjCells.AddLast(Cells[idx._i, idx._j]);
        }

        return adjCells;
    }

    private LinkedList<Cell> GetAvailableHorizontalCells(FieldIndex fIdx)
    {
        LinkedList<Cell> horCells = GetAvailableCellsForDirection(fIdx, new FieldIndex(0, -1));
        horCells.AppendRange(GetAvailableCellsForDirection(fIdx, new FieldIndex(0, 1)));
        
        return horCells;
    }
    
    private LinkedList<Cell> GetAvailableVerticalCells(FieldIndex fIdx)
    {
        LinkedList<Cell> vertCells = GetAvailableCellsForDirection(fIdx, new FieldIndex(-1, 0));
        vertCells.AppendRange(GetAvailableCellsForDirection(fIdx, new FieldIndex(1, 0)));
        
        return vertCells;
    }
    
    private LinkedList<Cell> GetAvailableCellsForDirection(FieldIndex fIdx, FieldIndex idxDelta)
    {
        LinkedList<Cell> availableCells = new LinkedList<Cell>();
    
        FieldIndex idx = fIdx + idxDelta;
        
        while (IsCorrectFieldIndex(idx))
        {
            Cell c = Cells[idx._i, idx._j];
            if ((c != null) && c.IsAvailable && !c.IsEmpty)
                availableCells.AddLast(c);
            else break;

            idx += idxDelta;
        }
        
        return availableCells;
    }
    
    private LinkedList<Cell> GetMatches(Cell cell)
    {
        return GetMatches(cell, cell.GameItem.BaseItemType);
    }

    private LinkedList<Cell> GetHorizontalMatches(Cell cell, GameItemType gItemType)
    {
        LinkedList<Cell> horMatches = new LinkedList<Cell>();
        
        FillMatches(horMatches, GetAvailableCellsForDirection(cell.Index, new FieldIndex(0, -1)), gItemType);
        FillMatches(horMatches, GetAvailableCellsForDirection(cell.Index, new FieldIndex(0, 1)), gItemType);
        
        if (!IsEnoughMatches(horMatches))
            horMatches.Clear();

        return horMatches;
    }

    private LinkedList<Cell> GetVerticalMatches(Cell cell, GameItemType gItemType)
    {
        LinkedList<Cell> vertMatches = new LinkedList<Cell>();
        
        FillMatches(vertMatches, GetAvailableCellsForDirection(cell.Index, new FieldIndex(-1, 0)), gItemType);
        FillMatches(vertMatches, GetAvailableCellsForDirection(cell.Index, new FieldIndex(1, 0)), gItemType);
        
        if (!IsEnoughMatches(vertMatches))
            vertMatches.Clear();

        return vertMatches;
    }

    private void FillMatches(ICollection<Cell> matchesToFill, IEnumerable<Cell> availableCells, GameItemType gItemType)
    {
        GameItemType baseType = GameItemExtension.GetBaseType(gItemType);
        
        foreach (Cell cell in availableCells)
        {
            if (cell.GameItem.IsSame(baseType))
                matchesToFill.Add(cell);
            
            else break;
        }
    }
    
    private void AddUniqueCellsToList(ICollection<Cell> listTo, IEnumerable<Cell> listFrom)
    {
        foreach (Cell cell in listFrom)
            AddCellToListIfUnique(listTo, cell);
    }

    private void AddCellToListIfUnique(ICollection<Cell> listTo, Cell cell)
    {
        if (!listTo.Contains(cell))
            listTo.Add(cell);
    }
    
    private void CheckCellGameItemForFall(Cell cell, ICollection<Cell> cellsListToFill)
    {
        int destinationI = -1;
        for (int i = cell.Index._i + 1; i < RowsNumber; i++)
        {
            Cell c = Cells[i, cell.Index._j];
            if (c.IsAvailable && c.IsEmpty)
            {
                destinationI = i;
            }
        }

        if(destinationI >= 0)
        {
            int halfJ = ColumnsNumber / 2;
            Cell destinationCell = Cells[destinationI, cell.Index._j];
            SwapItems(new CellsPair(cell, destinationCell));
            destinationCell.GameItem.FallTo(destinationCell.CachedTransform.position, Mathf.Abs(halfJ - destinationCell.Index._j));
            cellsListToFill.Add(destinationCell);
        }
    }

    private void CheckCellItemForUpgrade(Cell cell, int matchesCount)
    {
        if (matchesCount == MinMatchNumber)
        {
            cell.UpgradedGameItemType = cell.GameItem.GetUpgradedToBombType();
        }
        else if (matchesCount > MinMatchNumber)
        {
            cell.UpgradedGameItemType = cell.GameItem.GetUpgradedToLineBombType();
        }
    }

    private bool IsEnoughMatches(ICollection matches)
    {
        return (matches.Count >= (MinMatchNumber - 1));
    }

    private void CheckCellsForExplosiveItems(ICollection<Cell> listToCheck)
    {
        List<Cell> explodingCells = new List<Cell>();
        foreach (Cell cell in listToCheck)
        {
            if(cell.GameItem.IsExplosive() && !cell.IsAddedForDetonation)
            {
                cell.IsAddedForDetonation = true;
                IEnumerable<Cell> explCells = LaunchExplosiveItem(cell);
                AddUniqueCellsToList(explodingCells, explCells);
            }
        }
        if (explodingCells.Count > 0)
            AddUniqueCellsToList(listToCheck, explodingCells);
    }

    private IEnumerable<Cell> LaunchExplosiveItem(Cell cell)
    {
        if (cell.GameItem.IsUsualBomb())
            return LaunchUsualBomb(cell);
        
        return cell.GameItem.IsVerticalBomb() ? LaunchVerticalBomb(cell) : LaunchHorizontalBomb(cell);
    }

    private IEnumerable<Cell> LaunchUsualBomb(Cell cell)
    {
        LinkedList<Cell> adjacentCells = GetAdjacentCells(cell.Index);
        foreach (Cell c in adjacentCells)
        {
            if(c.DetonationDelayFactor == 0)
            {
                c.DetonationDelayFactor = cell.DetonationDelayFactor + 1;
            }
        }

        CheckCellsForExplosiveItems(adjacentCells);

        return adjacentCells;
    }

    private IEnumerable<Cell> LaunchVerticalBomb(Cell cell)
    {
        LinkedList<Cell> vertCells = GetAvailableVerticalCells(cell.Index);
        
        foreach (Cell c in vertCells)
        {
            if (c.DetonationDelayFactor == 0)
            {
                c.DetonationDelayFactor = cell.DetonationDelayFactor + Mathf.Abs(cell.Index._i - c.Index._i);
            }
        }

        CheckCellsForExplosiveItems(vertCells);

        return vertCells;
    }

    private IEnumerable<Cell> LaunchHorizontalBomb(Cell cell)
    {
        LinkedList<Cell> horCells = GetAvailableHorizontalCells(cell.Index);
        
        foreach (Cell c in horCells)
        {
            if (c.DetonationDelayFactor == 0)
            {
                c.DetonationDelayFactor = cell.DetonationDelayFactor + Mathf.Abs(cell.Index._j - c.Index._j);
            }
        }

        CheckCellsForExplosiveItems(horCells);

        return horCells;
    }

    private void FillDetonationDelaysForMatches(Cell cell, IEnumerable matches)
    {
        cell.DetonationDelayFactor = -1;

        foreach (Cell c in matches)
        {
            if (c.DetonationDelayFactor == 0)
            {
                if(c.Index._i == cell.Index._i)
                    c.DetonationDelayFactor = Mathf.Abs(cell.Index._j - c.Index._j);
                else if (c.Index._j == cell.Index._j)
                    c.DetonationDelayFactor = Mathf.Abs(cell.Index._i - c.Index._i);
            }
        }
    }

    private void SwapItems(CellsPair cPair)
    {
        FieldIndex idx1 = cPair.FirstCell.Index;
        FieldIndex idx2 = cPair.SecondCell.Index;

        GameItem tmpItem = Cells[idx1._i, idx1._j].GameItem;
        Cells[idx1._i, idx1._j].GameItem = Cells[idx2._i, idx2._j].GameItem;
        Cells[idx2._i, idx2._j].GameItem = tmpItem;
    }
}
