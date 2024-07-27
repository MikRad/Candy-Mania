using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelController : MonoBehaviour
{
    private enum LevelState { Initialisation, Idle, ItemsSwap, ItemsDetonation, ItemsFall, Finalisation }
    private enum LevelResult { Uncertain, AllConditionsReached, NoMoreMoves, TimeExpired }

    [Header("Prefabs")] 
    [SerializeField] private Cell _cellPrefab;

    [Header("Containers")] 
    [SerializeField] private Transform _cellsParent;

    [Header("Hints")] 
    [SerializeField] private float _hintsInterval = 10f;

    private Vector2 _gameFieldCenter;
    private Vector2 _gameFieldOffset;
    private float _cellSize;
    private int _maxRowsNumber;
    private int _maxColumnsNumber;
    private int _minMatchNumber;

    private int _levelNumber;
    private Cell[,] _cells;

    private int _currentComboCount = 1;
    private int _maxComboCount = 1;
    
    private float _currentTimeToHint;

    private readonly LinkedList<GameItem> _hintedGameItems = new LinkedList<GameItem>();
    private readonly LinkedList<GameItem> _swappingGameItems = new LinkedList<GameItem>();
    private LinkedList<Cell> _detonatingItemsCells;
    private LinkedList<Cell> _fallingOldItemsCells;
    private List<CellsPair> _possibleSuccessfulMoves;
    private int _fallingItemsNumber;

    private LevelState _currentState;
    private LevelResult _levelResult;

    private LevelTimer _levelTimer;
    private CellsProcessor _cellsProcessor;
    private MoveProcessor _moveProcessor;
    private GameItemSpawner _gameItemSpawner;
    private LevelPassConditionsUpdater _levelPassConditionsUpdater;
    private UIViewsController _uiViewsController;
    private GameProgressController _gameProgressController;
    private CameraHolder _cameraHolder;
    private GamefieldShakerVfx _gamefieldShaker;

    private bool IsLevelFinished => _levelResult != LevelResult.Uncertain;

    private void Update()
    {
        UpdateTimer();
        UpdateCurrentState();
    }

    private void OnDestroy()
    {
        RemoveEventHandlers();
    }

    public void Init(AppSettings settings, GameProgressController progressController)
    {
        _gameFieldOffset = settings._gameFieldOffset;
        _cellSize = settings._cellSize;
        _maxRowsNumber = settings._maxRowsNumber;
        _maxColumnsNumber = settings._maxColumnsNumber;
        _minMatchNumber = settings._minMatchNumber;
        _gameProgressController = progressController;
        
        _gameItemSpawner = GetComponentInChildren<GameItemSpawner>();
        _gamefieldShaker = GetComponentInChildren<GamefieldShakerVfx>();

        _cameraHolder = CameraHolder.Instance;
        _uiViewsController = UIViewsController.Instance;
        _levelPassConditionsUpdater = new LevelPassConditionsUpdater();
        _cellsProcessor = new CellsProcessor(_maxRowsNumber, _maxColumnsNumber, _minMatchNumber);
        _moveProcessor = new MoveProcessor(_cellsProcessor, settings._minDragDelta, PerformMove);
        _levelTimer = new LevelTimer(settings._levelTimeAlarm);
        
        _gameItemSpawner.Init(_cellsProcessor);
        _gameFieldCenter = GetPositionFromFieldIndex(new FieldIndex(_maxRowsNumber / 2, _maxColumnsNumber / 2));
        
        VfxController.Instance.Init(_gameFieldCenter);

        DOTween.SetTweensCapacity(200, 100);
        
        AddEventHandlers();
    }

    public void InitLevel(int levNumber)
    {
        SetState(LevelState.Initialisation);

        _levelNumber = levNumber;
        _maxComboCount = _currentComboCount = 1;
        LevelData levelData = LevelData.Load("level" + _levelNumber);

        RemoveOldCells();

        _gameItemSpawner.InitProbabilities(levelData);
        InitConditions(levelData);
        InitCells(levelData);

        UpdatePossibleMoves();
        
        _uiViewsController.InitLevelInfoData(levelData._levelPassConditions,
            _gameProgressController.TotalScore, _levelTimer.TimeRemained);

        _currentTimeToHint = _hintsInterval;
        
        SetState(LevelState.Idle);
        _levelResult = LevelResult.Uncertain;
    }

    private void UpdateTimer()
    {
        if (IsLevelFinished)
            return;

        _levelTimer.Tick();

        _uiViewsController.SetLevelInfoTime(_levelTimer.TimeRemained);
    }

    private void UpdateCurrentState()
    {
        switch (_currentState)
        {
            case LevelState.Idle:
                UpdateIdle();
                break;
        }
    }

    private void SetState(LevelState newState)
    {
        if (_currentState == newState)
            return;

        _currentState = newState;

        switch (_currentState)
        {
            case LevelState.ItemsSwap:
                StartSwap();
                break;
            case LevelState.ItemsDetonation:
                StartDetonation();
                break;
            case LevelState.ItemsFall:
                StartFalling();
                break;
        }
    }

    private void UpdateIdle()
    {
        if (Game.Instance.IsPaused)
            return;

        CheckLevelResult();
        UpdatePossibleHint();

        if (!IsMouseOverGameField(out Vector2 mouseWorldPos))
            return;

        if (Input.GetMouseButtonDown(0))
        {
            FieldIndex fIdx = GetFieldIndexFromMousePosition(mouseWorldPos);
            _moveProcessor.TrySelectCell(fIdx, mouseWorldPos);
        }

        _moveProcessor.UpdatePossibleDrag(mouseWorldPos);
    }

    private void PerformMove()
    {
        if (_hintedGameItems.Count > 0)
        {
            TerminateHint();
        }

        SetState(LevelState.ItemsSwap);
    }

    private void UpdatePossibleHint()
    {
        if (_currentTimeToHint > 0)
        {
            _currentTimeToHint -= Time.deltaTime;
            if (_currentTimeToHint <= 0)
            {
                MakeHint();
            }
        }
    }

    private void MakeHint()
    {
        int rndIndex = Random.Range(0, _possibleSuccessfulMoves.Count);
        CellsPair suggestedMove = _possibleSuccessfulMoves[rndIndex];
        suggestedMove.FirstCell.GameItem.SetHinted(true);
        suggestedMove.SecondCell.GameItem.SetHinted(true, 0.1f);
        
        _hintedGameItems.AddLast(suggestedMove.FirstCell.GameItem);
        _hintedGameItems.AddLast(suggestedMove.SecondCell.GameItem);
    }

    private void StartSwap()
    {
        _swappingGameItems.AddLast(_moveProcessor.SelectedCells.FirstCell.GameItem);
        _swappingGameItems.AddLast(_moveProcessor.SelectedCells.SecondCell.GameItem);
        _detonatingItemsCells = _cellsProcessor.SwapItemsBetweenCells(_moveProcessor.SelectedCells);
    }

    private void StartDetonation()
    {
        _gamefieldShaker.Shake();

        foreach (Cell cell in _detonatingItemsCells)
        {
            if (cell.HandleItemDetonation())
            {
                _levelPassConditionsUpdater.UpdateVictoryCondition(LevelPassCondition.Type.CellDetonate);
            }
        }
    }

    private void StartFalling()
    {
        _fallingOldItemsCells = _cellsProcessor.CheckFallingItemsCells();
        int fallingNewItemsNumber = GenerateNewItemsForEmptyCells();
        _fallingItemsNumber = _fallingOldItemsCells.Count + fallingNewItemsNumber;
    }

    private void HandleHintCompleted(GameItemHintCompletedEvent ev)
    {
        _hintedGameItems.Remove(ev.Item);
        
        if (_hintedGameItems.Count == 0)
        {
            _currentTimeToHint = _hintsInterval / 2;
        }
    }

    private void TerminateHint()
    {
        foreach (GameItem gItem in _hintedGameItems)
        {
            gItem.SetHinted(false);
        }
        _hintedGameItems.Clear();
    }

    private void HandleItemSwapCompleted(GameItemSwapCompletedEvent ev)
    {
        _swappingGameItems.Remove(ev.Item);
        
        if (_swappingGameItems.Count == 0)
        {
            // unsuccessful move
            if (_detonatingItemsCells.Count == 0)
            {
                _moveProcessor.ResetSelectedCells();
                SetState(LevelState.Idle);
            }
            else
            {
                _moveProcessor.ResetSelectedCells();
                SetState(LevelState.ItemsDetonation);
            }
        }
    }

    private void HandleItemFallCompleted()
    {
        _fallingItemsNumber--;

        if (_fallingItemsNumber == 0)
        {
            _detonatingItemsCells = _cellsProcessor.CheckCellsForDetonation(_fallingOldItemsCells);
            _fallingOldItemsCells.Clear();
            if (_detonatingItemsCells.Count == 0)
            {
                UpdatePossibleMoves();

                SetState(LevelState.Idle);
                _currentTimeToHint = _hintsInterval;
                _currentComboCount = 1;
            }
            else
            {
                _currentComboCount++;
                EventBus.Get.RaiseEvent(this, new ComboCollectedEvent(_currentComboCount));

                if (_currentComboCount > _maxComboCount)
                    _maxComboCount = _currentComboCount;

                SetState(LevelState.ItemsDetonation);
            }
        }
    }

    private void HandleItemDetonationCompleted(GameItemDetonationCompletedEvent ev)
    {
        foreach (Cell cell in _detonatingItemsCells)
        {
            if (cell.GameItem == ev.Item)
            {
                _levelPassConditionsUpdater.UpdateVictoryCondition(ev.Item.GetPassConditionType());
                cell.RemoveGameItem();
                cell.IsAddedForDetonation = false;
                cell.DetonationDelayFactor = 0;

                if (cell.CausedDetonationsNumber > 0)
                {
                    _gameProgressController.AddScore(cell);
                    cell.CausedDetonationsNumber = 0;
                }

                _detonatingItemsCells.Remove(cell);

                if (cell.UpgradedGameItemType != GameItemType.Empty)
                {
                    _gameItemSpawner.CreateItemForCell(cell, cell.UpgradedGameItemType, true);
                    cell.UpgradedGameItemType = GameItemType.Empty;
                }

                break;
            }
        }

        if (_detonatingItemsCells.Count == 0)
        {
            SetState(LevelState.ItemsFall);
        }
    }

    private void InitCells(LevelData levelData)
    {
        _cells = new Cell[_maxRowsNumber, _maxColumnsNumber];
        _cellsProcessor.Cells = _cells;

        for (int i = 0; i < _maxRowsNumber; i++)
        {
            for (int j = 0; j < _maxColumnsNumber; j++)
            {
                CellData cData = levelData._cellDatas[i * _maxRowsNumber + j];
                Cell cell = Instantiate(_cellPrefab, _cellsParent);
                FieldIndex fIdx = new FieldIndex(i, j);
                cell.Init(fIdx, cData._detonationsToClear, GetPositionFromFieldIndex(fIdx));
                _cells[i, j] = cell;

                if (!cell.IsAvailable)
                    continue;

                if (cData._gameItemType != GameItemType.Empty)
                {
                    _gameItemSpawner.CreateItemForCell(cell, cData._gameItemType);
                }
                else
                {
                    _gameItemSpawner.GenerateRandomItemForCell(cell);
                }
            }
        }
    }

    private void CheckLevelResult()
    {
        if (!IsLevelFinished)
            return;
        
        if (_levelResult == LevelResult.AllConditionsReached)
        {
            SetState(LevelState.Finalisation);
            
            EventBus.Get.RaiseEvent(this, new LevelCompletedEvent(_gameProgressController.CurrentLevelScore, 
                _gameProgressController.TotalScore, _maxComboCount, _levelTimer.TimePlayed));
            return;
        }
            
        EventBus.Get.RaiseEvent(this, new LevelFailedEvent(_gameProgressController.CurrentLevelScore, _levelTimer.TimePlayed));
        
        SetState(LevelState.Finalisation);
    }

    private void InitConditions(LevelData levelData)
    {
        _levelTimer.Init(levelData._timeLimit);
        _levelPassConditionsUpdater.Init(levelData._levelPassConditions);
    }

    private void RemoveOldCells()
    {
        if (_cells == null)
            return;

        for (int i = 0; i < _maxRowsNumber; i++)
        {
            for (int j = 0; j < _maxColumnsNumber; j++)
            {
                Cell c = _cells[i, j];

                c.RemoveGameItem();
                Destroy(c.gameObject);
            }
        }

        _cells = null;
    }

    private int GenerateNewItemsForEmptyCells()
    {
        int newItemsNumber = 0;
        for (int j = 0; j < _maxColumnsNumber; j++)
        {
            for (int i = _maxRowsNumber - 1; i >= 0; i--)
            {
                Cell cell = _cells[i, j];
                if (cell.IsAvailable && cell.IsEmpty)
                {
                    Vector2 pos = GetPositionFromFieldIndex(cell.Index);
                    pos.y = (_maxRowsNumber - cell.Index._i) * _cellSize;
                    _gameItemSpawner.GenerateRandomItemForCell(cell, true, pos);
                    newItemsNumber++;
                }
            }
        }

        return newItemsNumber;
    }

    private void UpdatePossibleMoves()
    {
        _possibleSuccessfulMoves = _cellsProcessor.GetPossibleSuccessfulMoves();
        EventBus.Get.RaiseEvent(this, new PossibleMovesChangedEvent(_possibleSuccessfulMoves.Count / 2));
    }
    
    private void HandleTimeExpired()
    {
        _levelResult = LevelResult.TimeExpired;
    }
    
    private void HandleAllLevelPassConditionsReached()
    {
        _levelResult = LevelResult.AllConditionsReached;
    }
    
    private void HandleNoMoreMoves()
    {
        _levelResult = LevelResult.NoMoreMoves;
    }
    
    private void AddEventHandlers()
    {
        EventBus.Get.Subscribe<LevelTimeExpiredEvent>(HandleTimeExpired);
        EventBus.Get.Subscribe<AllLevelPassConditionsReachedEvent>(HandleAllLevelPassConditionsReached);
        EventBus.Get.Subscribe<NoMoreMovesEvent>(HandleNoMoreMoves);
        
        EventBus.Get.Subscribe<GameItemDetonationCompletedEvent>(HandleItemDetonationCompleted);
        EventBus.Get.Subscribe<GameItemFallCompletedEvent>(HandleItemFallCompleted);
        EventBus.Get.Subscribe<GameItemSwapCompletedEvent>(HandleItemSwapCompleted);
        EventBus.Get.Subscribe<GameItemHintCompletedEvent>(HandleHintCompleted);
    }

    private void RemoveEventHandlers()
    {
        EventBus.Get.Unsubscribe<LevelTimeExpiredEvent>(HandleTimeExpired);
        EventBus.Get.Unsubscribe<AllLevelPassConditionsReachedEvent>(HandleAllLevelPassConditionsReached);
        EventBus.Get.Unsubscribe<NoMoreMovesEvent>(HandleNoMoreMoves);
        
        EventBus.Get.Unsubscribe<GameItemDetonationCompletedEvent>(HandleItemDetonationCompleted);
        EventBus.Get.Unsubscribe<GameItemFallCompletedEvent>(HandleItemFallCompleted);
        EventBus.Get.Unsubscribe<GameItemSwapCompletedEvent>(HandleItemSwapCompleted);
        EventBus.Get.Unsubscribe<GameItemHintCompletedEvent>(HandleHintCompleted);
    }
    
    private Vector2 GetPositionFromFieldIndex(FieldIndex fIdx)
    {
        return new Vector2((fIdx._j * _cellSize) + _cellSize / 2, (-fIdx._i * _cellSize) - _cellSize / 2) +
               _gameFieldOffset;
    }

    private FieldIndex GetFieldIndexFromMousePosition(Vector2 mousePos)
    {
        mousePos.y *= -1;
        mousePos.x -= _gameFieldOffset.x;
        mousePos.y += _gameFieldOffset.y;

        int i = (int)((int)mousePos.y / _cellSize);
        int j = (int)((int)mousePos.x / _cellSize);

        return new FieldIndex(i, j);
    }

    private bool IsMouseOverGameField(out Vector2 mouseWorldPos)
    {
        mouseWorldPos = GetMouseWorldPosition();

        return ((mouseWorldPos.x > _gameFieldOffset.x) &&
                (mouseWorldPos.x < (_gameFieldOffset.x + _maxColumnsNumber * _cellSize)) &&
                (mouseWorldPos.y < _gameFieldOffset.y) &&
                (mouseWorldPos.y > (_gameFieldOffset.y - _maxRowsNumber * _cellSize)));
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 mousePixelsPos = Input.mousePosition;
        return _cameraHolder.ScreenToWorldPoint(mousePixelsPos);
    }
}