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

    [Header("Containers")]
    [SerializeField] private Transform _gameItemsContainer;

    private readonly Dictionary<GameItemType, Pool<GameItem>> _gameItemPoolsMap = new Dictionary<GameItemType, Pool<GameItem>>(GameItemPoolsMapSize);
    private CellsProcessor _cellsProcessor;
    
    private int _specialItemGenerationProbability;
    private int _bombProbabilityWeight;
    private int _vertBombProbabilityWeight;
    private int _horBombProbabilityWeight;
    private int _starProbabilityWeight;
    private int _totalSpecialItemProbabilityWeight;

    private const int GameItemPoolsMapSize = 50; 
    private const int UsualGameItemPoolSize = 20;
    private const int SpecialGameItemPoolSize = 10;
    
    private List<SpecialItemGenerationData> _specialItemsGenerationDatas;

    public event Action<GameItem> OnItemSpawned; 

    public void Init(CellsProcessor cellsProcessor)
    {
        _cellsProcessor = cellsProcessor;
        
        CreateGameItemPools();
    }
    
    public void InitProbabilities(LevelData levelData)
    {
        _specialItemGenerationProbability = levelData._specialItemGenerationProbability;
        _bombProbabilityWeight = levelData._bombItemGenerationProbability;
        _vertBombProbabilityWeight = levelData._vertBombItemGenerationProbability;
        _horBombProbabilityWeight = levelData._horBombItemGenerationProbability;
        _starProbabilityWeight = levelData._starItemGenerationProbability;
        _totalSpecialItemProbabilityWeight = _bombProbabilityWeight + _vertBombProbabilityWeight +
                                                      _horBombProbabilityWeight + _starProbabilityWeight;

        FillSpecialItemsGenerationData();
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
        if (isUsual)
        {
            int randIdx = Random.Range(0, _usualGameItemPrefabs.Length);
            
            return _usualGameItemPrefabs[randIdx];
        }

        int randProbability = Random.Range(0, _totalSpecialItemProbabilityWeight + 1);
        for (int i = 0; i < _specialItemsGenerationDatas.Count; i++)
        {
            if (_specialItemsGenerationDatas[i].cumulativeProbabilityWeight >= randProbability )
            {
                GameItem[] prefabs = _specialItemsGenerationDatas[i].prefabs;
                int randIdx = Random.Range(0, prefabs.Length);
                
                return prefabs[randIdx];
            }
        }

        return null;
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
            _gameItemPoolsMap.Add(_usualGameItemPrefabs[i].ItemType, new Pool<GameItem>(_usualGameItemPrefabs[i], UsualGameItemPoolSize, _gameItemsContainer));
            _gameItemPoolsMap.Add(_bombGameItemPrefabs[i].ItemType, new Pool<GameItem>(_bombGameItemPrefabs[i], SpecialGameItemPoolSize, _gameItemsContainer));
            _gameItemPoolsMap.Add(_vertBombGameItemPrefabs[i].ItemType, new Pool<GameItem>(_vertBombGameItemPrefabs[i], SpecialGameItemPoolSize, _gameItemsContainer));
            _gameItemPoolsMap.Add(_horBombGameItemPrefabs[i].ItemType, new Pool<GameItem>(_horBombGameItemPrefabs[i], SpecialGameItemPoolSize, _gameItemsContainer));
            _gameItemPoolsMap.Add(_starGameItemPrefabs[i].ItemType, new Pool<GameItem>(_starGameItemPrefabs[i], SpecialGameItemPoolSize, _gameItemsContainer));
        }
    }
    
    private void FillSpecialItemsGenerationData()
    {
        _specialItemsGenerationDatas = new List<SpecialItemGenerationData>();
        
        _specialItemsGenerationDatas.Add(
            new SpecialItemGenerationData(_bombProbabilityWeight, _bombGameItemPrefabs));
        _specialItemsGenerationDatas.Add(
            new SpecialItemGenerationData(_bombProbabilityWeight + _vertBombProbabilityWeight, _vertBombGameItemPrefabs));
        _specialItemsGenerationDatas.Add(
            new SpecialItemGenerationData(_bombProbabilityWeight + _vertBombProbabilityWeight + _horBombProbabilityWeight, _horBombGameItemPrefabs)); 
        _specialItemsGenerationDatas.Add(
            new SpecialItemGenerationData(_bombProbabilityWeight + _vertBombProbabilityWeight + _horBombProbabilityWeight + _starProbabilityWeight, _starGameItemPrefabs));
    }
    
    private struct SpecialItemGenerationData
    {
        public readonly int cumulativeProbabilityWeight;
        public readonly GameItem[] prefabs;

        public SpecialItemGenerationData(int cumProbabilityWeight, GameItem[] siPrefabs)
        {
            cumulativeProbabilityWeight = cumProbabilityWeight;
            prefabs = siPrefabs;
        }
    }
}
