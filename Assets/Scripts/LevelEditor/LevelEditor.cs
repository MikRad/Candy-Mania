using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditor : MonoBehaviour
{
    [Header("App settings")]
    [SerializeField] private AppSettings _settings;

    [Header("Conditions & probabilities")]
    [SerializeField] private int _specialItemGenerationProbability = 5;
    [SerializeField] private int _bombItemGenerationProbability = 20;
    [SerializeField] private int _vertBombItemGenerationProbability = 20;
    [SerializeField] private int _horBombItemGenerationProbability = 20;
    [SerializeField] private int _starItemGenerationProbability = 40;
    [SerializeField] private float _timeLimit = 180f;
    [SerializeField] private List<VictoryCondition> _victoryConditions;

    [Header("View settings")]
    [SerializeField] private GameObject _gameFieldBack;
    [SerializeField] private int _itemDefaultOrderInLayer = 3;
    [SerializeField] private int _itemSelectedOrderInLayer = 4;
    [SerializeField] private int _cellSelectedOrderInLayer = 2;

    [Header("UI")]
    [SerializeField] private Text _levelNameText;
    [SerializeField] private Button _loadLevelButton;
    [SerializeField] private Button _saveLevelButton;
    [SerializeField] private Button _createLevelButton;

    [Header("Prefabs")]
    [SerializeField] private Cell[] _cellPrefabs;
    [SerializeField] private GameItem[] _usualGameItemPrefabs;
    [SerializeField] private GameItem[] _bombGameItemPrefabs;
    [SerializeField] private GameItem[] _vertBombGameItemPrefabs;
    [SerializeField] private GameItem[] _horBombGameItemPrefabs;
    [SerializeField] private GameItem[] _starGameItemPrefabs;
    
    [Header("Containers")]
    [SerializeField] private Transform _cellsContainer;
    [SerializeField] private Transform _gameItemsContainer;

    private LevelData _levelData;
    private Cell[,] _cells;
    private GameObject _selectedTemplate;

    private CameraHolder _cameraHolder;
    
    private Vector2 GameFieldOffset => _settings._gameFieldOffset;
    private float CellSize => _settings._cellSize;
    private int MaxRowsNumber => _settings._maxRowsNumber;
    private int MaxColumnsNumber => _settings._maxColumnsNumber;
    private string DefaultLevelName => _settings._defaultLevelName;

    private void OnEnable()
    {
        _loadLevelButton.onClick.AddListener(HandleLoadLevelClick);
        _saveLevelButton.onClick.AddListener(HandleSaveLevelClick);
        _createLevelButton.onClick.AddListener(HandleCreateLevelClick);
    }

    private void OnDisable()
    {
        _loadLevelButton.onClick.RemoveListener(HandleLoadLevelClick);
        _saveLevelButton.onClick.RemoveListener(HandleSaveLevelClick);
        _createLevelButton.onClick.RemoveListener(HandleCreateLevelClick);
    }

    private void Start()
    {
        _cameraHolder = CameraHolder.Instance;
        
        InitBack();

        LoadLevel(true);
    }

    private void Update()
    {
        UpdateSelectedTemplatePosition();

        if (IsMouseOnGameField())
        {
            if (Input.GetMouseButton(0))
            {
                TryGameFieldDraw();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                TrySelectNewTemplate();
            }
        }
    }

    private void LoadLevel(bool isInitLoad)
    {
#if UNITY_EDITOR
        if (isInitLoad)
        {
            _levelData = LevelData.Load(DefaultLevelName);
        }
        else
        {
            string filePath = EditorUtility.OpenFilePanel("Load level", "Assets/Resources/Levels", "asset");
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            _levelData = LevelData.Load(fileName);
        }

        InitLevel(false);
#endif
    }

    private void SaveLevel()
    {
#if UNITY_EDITOR

        if (_levelData == null)
        {
            _levelData = ScriptableObject.CreateInstance<LevelData>();
            _levelData._cellDatas = new List<CellData>();
            FillLevelData(true);

            string filePath = EditorUtility.SaveFilePanel("Save level", "Assets/Resources/Levels", "level0", "asset");
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            AssetDatabase.CreateAsset(_levelData, "Assets/Resources/Levels/" + fileName + ".asset");

            _levelNameText.text = _levelData.name.ToUpper();
        }
        else
        {
            FillLevelData(false);

            EditorUtility.SetDirty(_levelData);
            AssetDatabase.SaveAssets();
        }
#endif

    }

    private void FillLevelData(bool isNew)
    {
        int overallDetonationsToClear = 0;

        for (int i = 0; i < MaxRowsNumber; i++)
        {
            for (int j = 0; j < MaxColumnsNumber; j++)
            {
                Cell cell = _cells[i, j];
                GameItemType gItemType = (!cell.IsEmpty) ? cell.GameItem.ItemType : GameItemType.Empty;
                if(isNew)
                    _levelData._cellDatas.Add(new CellData(cell.Index, cell.DetonationsToClear, gItemType));
                else
                    _levelData._cellDatas[i * MaxRowsNumber + j] = new CellData(cell.Index, cell.DetonationsToClear, gItemType);

                if(cell.DetonationsToClear > 0)
                    overallDetonationsToClear += cell.DetonationsToClear;
            }
        }

        _victoryConditions[0]._numberNeededToComplete = overallDetonationsToClear;
        _levelData._victoryConditions = _victoryConditions;

        _levelData._specialItemGenerationProbability = _specialItemGenerationProbability;
        _levelData._bombItemGenerationProbability = _bombItemGenerationProbability;
        _levelData._vertBombItemGenerationProbability = _vertBombItemGenerationProbability;
        _levelData._horBombItemGenerationProbability = _horBombItemGenerationProbability;
        _levelData._starItemGenerationProbability = _starItemGenerationProbability;
        _levelData._timeLimit = _timeLimit;
    }

    private void RemoveOldCells()
    {
        if (_cells == null)
            return;

        for (int i = 0; i < MaxRowsNumber; i++)
        {
            for (int j = 0; j < MaxColumnsNumber; j++)
            {
                Cell cell = _cells[i, j];

                cell.RemoveGameItem();
                Destroy(cell.gameObject);
            }
        }

        _cells = null;
    }

    private void TryGameFieldDraw()
    {
        if (_selectedTemplate == null)
            return;

        Vector2 mousePos = GetMouseWorldPosition();
        FieldIndex fIdx = GetFieldIndexFromMousePosition(mousePos);

        if(_selectedTemplate.TryGetComponent(out Cell cellTemplate))
        {
            TryCellDraw(cellTemplate, fIdx);
            return;
        }
        if (_selectedTemplate.TryGetComponent(out GameItem gItemTemplate))
        {
            TryGameItemDraw(gItemTemplate, fIdx);
        }
    }

    private void TrySelectNewTemplate()
    {
        Ray ray = _cameraHolder.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            SelectNewTemplate(hit.collider.gameObject);
        }
    }

    private void SelectNewTemplate(GameObject selectedObject)
    {
        ReleaseOldTemplate();
        
        _selectedTemplate = Instantiate(selectedObject, transform);
        _selectedTemplate.GetComponent<Collider2D>().enabled = false;

        if(_selectedTemplate.TryGetComponent(out GameItem gItem))
        {
            gItem.SetOrderInLayer(_itemSelectedOrderInLayer);
            return;
        }
        if (_selectedTemplate.TryGetComponent(out Cell cell))
        {
            cell.SetOrderInLayer(_cellSelectedOrderInLayer);
        }
    }

    private void TryCellDraw(Cell cellTemplate, FieldIndex fIdx)
    {
        Cell cell = _cells[fIdx._i, fIdx._j];

        if (cellTemplate.DetonationsToClear == cell.DetonationsToClear)
            return;

        // Level victory conditions update
        if ((cellTemplate.DetonationsToClear >= 0) && (cell.DetonationsToClear >= 0))
        {
            _victoryConditions[0]._numberNeededToComplete += (cellTemplate.DetonationsToClear - cell.DetonationsToClear);
        }
        else if ((cellTemplate.DetonationsToClear > 0) && (cell.DetonationsToClear < 0))
        {
            _victoryConditions[0]._numberNeededToComplete += cellTemplate.DetonationsToClear;
        }
        else if ((cellTemplate.DetonationsToClear < 0) && (cell.DetonationsToClear > 0))
        {
            _victoryConditions[0]._numberNeededToComplete -= cell.DetonationsToClear;
        }
        //

        cell.SetDetonationsToClear(cellTemplate.DetonationsToClear);
    }

    private void TryGameItemDraw(GameItem gItemTemplate, FieldIndex fIdx)
    {
        Cell cell = _cells[fIdx._i, fIdx._j];

        if (cell.IsAvailable)
        {
            if ((!cell.IsEmpty) && (cell.GameItem.ItemType == gItemTemplate.ItemType))
                return;

            gItemTemplate.SetOrderInLayer(_itemDefaultOrderInLayer);
            cell.SetGameItem(gItemTemplate, _gameItemsContainer);
        }
    }

    private void UpdateSelectedTemplatePosition()
    {
        if (_selectedTemplate == null)
            return;

        Vector2 mousePos = GetMouseWorldPosition();
        if(IsMouseOnGameField())
        {
            // snapping to cells
            FieldIndex fIdx = GetFieldIndexFromMousePosition(mousePos);
            _selectedTemplate.transform.localPosition = GetPositionFromFieldIndex(fIdx);
        }
        else
        {
            _selectedTemplate.transform.position = mousePos;
        }
    }

    private void ReleaseOldTemplate()
    {
        if(_selectedTemplate != null)
        {
            Destroy(_selectedTemplate);
            _selectedTemplate = null;
        }
    }

    private void InitLevel(bool isDefault)
    {
        RemoveOldCells();

        if (!isDefault && _levelData != null)
        {
            InitCells();
            InitConditionsAndProbabilities();
            _levelNameText.text = _levelData.name.ToUpper();
        }
        else
        {
            _levelData = null;
            InitEmptyCells();
            InitDefaultConditionsAndProbabilities();
            _levelNameText.text = "DEFAULT_NAME";
        }
    }

    private void InitCells()
    {
        _cells = new Cell[MaxRowsNumber, MaxColumnsNumber];
        
        for (int i = 0; i < MaxRowsNumber; i++)
        {
            for (int j = 0; j < MaxColumnsNumber; j++)
            {
                CellData cData = _levelData._cellDatas[i * MaxRowsNumber + j];
                Cell cell = Instantiate(_cellPrefabs[0], _cellsContainer);
                Vector2 pos = GetPositionFromFieldIndex(new FieldIndex(i, j));
                cell.Init(new FieldIndex(i, j), cData._detonationsToClear, pos);

                if (cData._gameItemType != GameItemType.Empty)
                {
                    GameItem gItemPrefab = GetGameItemPrefab(cData._gameItemType);
                    cell.SetGameItem(gItemPrefab, _gameItemsContainer);
                }
                
                _cells[i, j] = cell;
            }
        }
    }

    private void InitEmptyCells()
    {
        _cells = new Cell[MaxRowsNumber, MaxColumnsNumber];
        
        for (int i = 0; i < MaxRowsNumber; i++)
        {
            for (int j = 0; j < MaxColumnsNumber; j++)
            {
                Cell cell = Instantiate(_cellPrefabs[0], _cellsContainer);
                Vector2 pos = GetPositionFromFieldIndex(new FieldIndex(i, j));
                cell.Init(new FieldIndex(i, j), 0, pos);
                
                _cells[i, j] = cell;
            }
        }
    }
    
    private void InitConditionsAndProbabilities()
    {
        _victoryConditions = _levelData._victoryConditions;
        _specialItemGenerationProbability = _levelData._specialItemGenerationProbability;
        _bombItemGenerationProbability = _levelData._bombItemGenerationProbability;
        _vertBombItemGenerationProbability = _levelData._vertBombItemGenerationProbability;
        _horBombItemGenerationProbability = _levelData._horBombItemGenerationProbability;
        _starItemGenerationProbability = _levelData._starItemGenerationProbability;
        _timeLimit = _levelData._timeLimit;
    }
    
    private void InitDefaultConditionsAndProbabilities()
    {
        _victoryConditions = new List<VictoryCondition> { new VictoryCondition(VictoryCondition.Type.CellDetonate, 0) };
        _specialItemGenerationProbability = 0;
        _bombItemGenerationProbability = 0;
        _vertBombItemGenerationProbability = 0;
        _horBombItemGenerationProbability = 0;
        _starItemGenerationProbability = 0;
        _timeLimit = 0;
    }

    private GameItem GetGameItemPrefab(GameItemType type)
    {
        if (GameItemExtension.IsUsual(type))
            return _usualGameItemPrefabs[GameItemExtension.GetBaseTypeInt(type) - 1];
        if (GameItemExtension.IsUsualBomb(type))
            return _bombGameItemPrefabs[GameItemExtension.GetBaseTypeInt(type) - 1];
        if (GameItemExtension.IsVerticalBomb(type))
            return _vertBombGameItemPrefabs[GameItemExtension.GetBaseTypeInt(type) - 1];
        if (GameItemExtension.IsHorizontalBomb(type))
            return _horBombGameItemPrefabs[GameItemExtension.GetBaseTypeInt(type) - 1];
        if (GameItemExtension.IsStar(type))
            return _starGameItemPrefabs[GameItemExtension.GetBaseTypeInt(type) - 1];

        return null;
    }
    
    private void HandleLoadLevelClick()
    {
        LoadLevel(false);
    }

    private void HandleSaveLevelClick()
    {
        SaveLevel();
    }

    private void HandleCreateLevelClick()
    {
        InitLevel(true);
    }

    private Vector2 GetPositionFromFieldIndex(FieldIndex fIdx)
    {
        return new Vector2((fIdx._j * CellSize) + CellSize / 2, (-fIdx._i * CellSize) - CellSize / 2) + GameFieldOffset;
    }

    private FieldIndex GetFieldIndexFromMousePosition(Vector2 mousePos)
    {
        mousePos.y *= -1;
        mousePos.x -= GameFieldOffset.x;
        mousePos.y += GameFieldOffset.y;

        int i = (int)((int)mousePos.y / CellSize);
        int j = (int)((int)mousePos.x / CellSize);

        return new FieldIndex(i, j);
    }

    private bool IsMouseOnGameField()
    {
        Vector2 mousePos = GetMouseWorldPosition();

        return ((mousePos.x > GameFieldOffset.x) && (mousePos.x < (GameFieldOffset.x + MaxColumnsNumber * CellSize)) &&
                (mousePos.y < GameFieldOffset.y) && (mousePos.y > (GameFieldOffset.y - MaxRowsNumber * CellSize)));
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 mousePixelsPos = Input.mousePosition;
        return _cameraHolder.ScreenToWorldPoint(mousePixelsPos);
    }

    private void InitBack()
    {
        _gameFieldBack.transform.localPosition = GameFieldOffset;
        _gameFieldBack.transform.localScale = new Vector3(MaxColumnsNumber, MaxRowsNumber, 0);
    }
}
