using System;
using UnityEngine;

public class MoveProcessor
{
    private readonly CellsProcessor _cellsProcessor;

    private bool _isDraggingItem;
    private Vector2 _dragItemStartPosition;
    private readonly float _minDragDelta;
    
    public CellsPair SelectedCells { get; }
    
    private readonly Action _performMoveCallback;
    
    public MoveProcessor(CellsProcessor cellsProcessor, float minDragDelta, Action performMoveCallback)
    {
        _cellsProcessor = cellsProcessor;
        SelectedCells = new CellsPair();
        _minDragDelta = minDragDelta;
        _performMoveCallback = performMoveCallback;
    }

    public void TrySelectCell(FieldIndex fIdx, Vector2 mousePosition)
    {
        Cell cell = _cellsProcessor.Cells[fIdx._i, fIdx._j];
        if(cell.IsAvailable && !cell.IsEmpty)
        {
            if (SelectedCells.FirstCell == null)
            {
                _isDraggingItem = true;
                _dragItemStartPosition = mousePosition;
                cell.SetSelected(true);
                SelectedCells.FirstCell = cell;
            }
            else
            {
                if(cell == SelectedCells.FirstCell)
                {
                    _isDraggingItem = true;
                    _dragItemStartPosition = mousePosition;
                    return;
                }

                cell.SetSelected(true);
                SelectedCells.SecondCell = cell;
                if(_cellsProcessor.IsCorrectPossibleMove(SelectedCells))
                {
                    _performMoveCallback?.Invoke();
                }
                else
                {
                    ResetSelectedCells();
                }
            }
        }
        else
        {
            ResetSelectedCells();
        }
    }
    
    public void UpdatePossibleDrag(Vector2 mousePosition)
    {
        if (!_isDraggingItem)
            return;
        
        if (Input.GetMouseButtonUp(0))
        {
            _isDraggingItem = false;
            return;
        }
        
        Vector2 dragDelta = mousePosition - _dragItemStartPosition;
        if (dragDelta.magnitude >= _minDragDelta)
        {
            int deltaI = 0, deltaJ = 0;
            
            if(Mathf.Abs(dragDelta.x) >= Mathf.Abs(dragDelta.y)) // horizontal drag
            {
                deltaJ = (dragDelta.x > 0) ? 1 : -1;
            }
            else // vertical drag
            {
                deltaI = (dragDelta.y > 0) ? -1 : 1;
            }
            
            FieldIndex fIdx = new FieldIndex(SelectedCells.FirstCell.Index._i + deltaI, SelectedCells.FirstCell.Index._j + deltaJ);
            if(_cellsProcessor.IsCorrectFieldIndex(fIdx))
            {
                Cell cell = _cellsProcessor.Cells[fIdx._i, fIdx._j];
                if (cell.IsAvailable && !cell.IsEmpty)
                {
                    cell.SetSelected(true);
                    SelectedCells.SecondCell = cell;
                    if (_cellsProcessor.IsCorrectPossibleMove(SelectedCells))
                    {
                        _performMoveCallback?.Invoke();
                    }
                    else
                    {
                        ResetSelectedCells();
                    }
                }
                else
                {
                    ResetSelectedCells();
                }
            }
            else
            {
                ResetSelectedCells();
            }

            _isDraggingItem = false;
        }
    }

    public void ResetSelectedCells()
    {
        SelectedCells.FirstCell?.SetSelected(false);
        SelectedCells.FirstCell = null;
        
        SelectedCells.SecondCell?.SetSelected(false);
        SelectedCells.SecondCell = null;
    }
}
