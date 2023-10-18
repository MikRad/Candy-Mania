using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameItemSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameItem[] _usualGameItemPrefabs;
    [SerializeField] private GameItem[] _bombGameItemPrefabs;
    [SerializeField] private GameItem[] _vertBombGameItemPrefabs;
    [SerializeField] private GameItem[] _horBombGameItemPrefabs;
    [SerializeField] private GameItem[] _starGameItemPrefabs;

    [Header("Containers & game items pooling")]
    [SerializeField] private Transform _gameItemsContainer;
    [SerializeField] private int _usualGameItemPoolSize = 20;
    [SerializeField] private int _specialGameItemPoolSize = 10;

    private readonly Dictionary<GameItemType, Pool<GameItem>> _gameItemPoolsMap = new Dictionary<GameItemType, Pool<GameItem>>();
    private CellsProcessor _cellsProcessor;
    
    private int _specialItemGenerationProbability;
    private int _bombItemGenerationProbability;
    private int _vertBombItemGenerationProbability;
    private int _horBombItemGenerationProbability;
    private int _starItemGenerationProbability;
    private List<GameItem> _specialGameItemsPrefabs;

    public event Action<GameItem> OnItemSpawned; 

    public void Init(CellsProcessor cellsProcessor)
    {
        _cellsProcessor = cellsProcessor;
        
        CreateGameItemPools();
    }
    
    public void InitProbabilities(LevelData levelData)
    {
        _specialItemGenerationProbability = levelData._specialItemGenerationProbability;
        _bombItemGenerationProbability = levelData._bombItemGenerationProbability;
        _vertBombItemGenerationProbability = levelData._vertBombItemGenerationProbability;
        _horBombItemGenerationProbability = levelData._horBombItemGenerationProbability;
        _starItemGenerationProbability = levelData._starItemGenerationProbability;

        FillSpecialItemsPrefabs();
    }

    public void CreateItemForCell(Cell cell, GameItemType gItemType, bool isIgnoreMatches = false)
    {
        LinkedList<Cell> potentialMatches = _cellsProcessor.GetMatches(cell, gItemType);
        if(potentialMatches.Count == 0 || isIgnoreMatches)
        {
            GameItem gItem = GetGameItemFromPool(gItemType);
            gItem.transform.localPosition = cell.transform.localPosition;
            cell.GameItem = gItem;
            
            OnItemSpawned?.Invoke(gItem);
        }
        else if(potentialMatches.Count > 0)
        {
            if(GameItemExtension.IsUsual(gItemType))
                GenerateRandomUsualGameItem(cell);
            else
                GenerateRandomSpecialGameItem(cell);
        }
    }
    
    public void GenerateRandomItemForCell(Cell cell, bool isStartFall = false, Vector2 fallStartPosition = default(Vector2))
    {
        int randNum = Random.Range(1, 100);
        if(randNum > _specialItemGenerationProbability)
            GenerateRandomUsualGameItem(cell, isStartFall, fallStartPosition);
        else
            GenerateRandomSpecialGameItem(cell, isStartFall, fallStartPosition);
    }
    
    private GameItem GetRandomGameItemPrefab(bool isUsual)
    {
        int randIdx = (isUsual) ? Random.Range(0, _usualGameItemPrefabs.Length) : 
            Random.Range(0, _specialGameItemsPrefabs.Count);
        
        return (isUsual) ? _usualGameItemPrefabs[randIdx] : _specialGameItemsPrefabs[randIdx];
    }

    private void GenerateRandomUsualGameItem(Cell cell, bool isInitFall = false, Vector2 fallStartPosition = default)
    {
        GenerateGameItem(cell, true, isInitFall, fallStartPosition);
    }
    
    private void GenerateRandomSpecialGameItem(Cell cell, bool isInitFall = false, Vector2 fallStartPosition = default)
    {
        GenerateGameItem(cell, false, isInitFall, fallStartPosition);
    }

    private void GenerateGameItem(Cell cell, bool isUsual, bool isStartFall, Vector2 fallStartPosition)
    {
        GameItem prefab;
        while (true)
        {
            prefab = GetRandomGameItemPrefab(isUsual);

            LinkedList<Cell> potentialMatches = _cellsProcessor.GetMatches(cell, prefab.ItemType);
            if (potentialMatches.Count == 0)
                break;
        }

        GameItem gItem = GetGameItemFromPool(prefab.ItemType);
        if (isStartFall)
        {
            int halfJ = _cellsProcessor.ColumnsNumber / 2;
            gItem.transform.localPosition = fallStartPosition;
            gItem.FallTo(cell.CachedTransform.localPosition, Mathf.Abs(halfJ - cell.Index._j), true);
        }
        else
        {
            gItem.transform.localPosition = cell.CachedTransform.localPosition;
        }
        
        cell.GameItem = gItem;
        
        OnItemSpawned?.Invoke(gItem);
    }

    private GameItem GetGameItemFromPool(GameItemType type)
    {
        if (_gameItemPoolsMap.TryGetValue(type, out Pool<GameItem> itemsPool))
        {
            GameItem item = itemsPool.GetFreeElement();
            item.Init();
            return item;
        }

        return null;
    }
    
    private void CreateGameItemPools()
    {
        for (int i = 0; i < (int)GameItemType.Item6; i++)
        {
            _gameItemPoolsMap.Add(_usualGameItemPrefabs[i].ItemType, new Pool<GameItem>(_usualGameItemPrefabs[i], _usualGameItemPoolSize, _gameItemsContainer));
            _gameItemPoolsMap.Add(_bombGameItemPrefabs[i].ItemType, new Pool<GameItem>(_bombGameItemPrefabs[i], _specialGameItemPoolSize, _gameItemsContainer));
            _gameItemPoolsMap.Add(_vertBombGameItemPrefabs[i].ItemType, new Pool<GameItem>(_vertBombGameItemPrefabs[i], _specialGameItemPoolSize, _gameItemsContainer));
            _gameItemPoolsMap.Add(_horBombGameItemPrefabs[i].ItemType, new Pool<GameItem>(_horBombGameItemPrefabs[i], _specialGameItemPoolSize, _gameItemsContainer));
            _gameItemPoolsMap.Add(_starGameItemPrefabs[i].ItemType, new Pool<GameItem>(_starGameItemPrefabs[i], _specialGameItemPoolSize, _gameItemsContainer));
        }
    }
    
    private void FillSpecialItemsPrefabs()
    {
        _specialGameItemsPrefabs = new List<GameItem>();
        for (int i = 0; i < _bombItemGenerationProbability; i++)
        {
            int randIdx = Random.Range(0, _bombGameItemPrefabs.Length);
            _specialGameItemsPrefabs.Add(_bombGameItemPrefabs[randIdx]);
        }
        for (int i = 0; i < _vertBombItemGenerationProbability; i++)
        {
            int randIdx = Random.Range(0, _vertBombGameItemPrefabs.Length);
            _specialGameItemsPrefabs.Add(_vertBombGameItemPrefabs[randIdx]);
        }
        for (int i = 0; i < _horBombItemGenerationProbability; i++)
        {
            int randIdx = Random.Range(0, _horBombGameItemPrefabs.Length);
            _specialGameItemsPrefabs.Add(_horBombGameItemPrefabs[randIdx]);
        }
        for (int i = 0; i < _starItemGenerationProbability; i++)
        {
            int randIdx = Random.Range(0, _starGameItemPrefabs.Length);
            _specialGameItemsPrefabs.Add(_starGameItemPrefabs[randIdx]);
        }
    }
}
