using System.Collections.Generic;
using UnityEngine;

public class VfxController : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private Transform _vfxContainer;
    
    [Header("Vfx prefabs")]

    [SerializeField] private VfxBase[] _itemDetonationvfxPrefabs;
    [SerializeField] private VfxBase[] _otherVfxPrefabs;
    
    [Header("Vfx pools size")] 
    [SerializeField] private int _defaultVfxPoolSize = 10;
    [SerializeField] private int _itemDetonationVfxPoolSize = 20;

    private readonly Dictionary<VfxType, Pool<VfxBase>> _vfxPoolsMap = new Dictionary<VfxType, Pool<VfxBase>>();

    private Vector2 _gameFieldCenterPosition;
    
    public static VfxController Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        FillVfxPoolsMap();

        AddEventHandlers();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            RemoveEventHandlers();
            Instance = null;
        }
    }

    public void Init(Vector2 gameFieldCenterPos)
    {
        _gameFieldCenterPosition = gameFieldCenterPos;
    }
    
    private void AddEventHandlers()
    {
        EventBus.Get.Subscribe<ScoreChangedEvent>(HandleScoreChanged);
        EventBus.Get.Subscribe<LevelCompletedEvent>(HandleLevelCompleted);
        EventBus.Get.Subscribe<ComboCollectedEvent>(HandleComboCollected);
        EventBus.Get.Subscribe<LevelTimeExpiringEvent>(HandleLevelTimeExpiring);
        EventBus.Get.Subscribe<LevelTimeExpiredEvent>(HandleLevelTimeExpired);
        EventBus.Get.Subscribe<NoMoreMovesEvent>(HandleNoMoreMoves);
        EventBus.Get.Subscribe<GameItemDetonationStartedEvent>(HandleGameItemDetonation);
    }

    private void RemoveEventHandlers()
    {
        EventBus.Get.Unsubscribe<ScoreChangedEvent>(HandleScoreChanged);
        EventBus.Get.Unsubscribe<LevelCompletedEvent>(HandleLevelCompleted);
        EventBus.Get.Unsubscribe<ComboCollectedEvent>(HandleComboCollected);
        EventBus.Get.Unsubscribe<LevelTimeExpiringEvent>(HandleLevelTimeExpiring);
        EventBus.Get.Unsubscribe<LevelTimeExpiredEvent>(HandleLevelTimeExpired);
        EventBus.Get.Unsubscribe<NoMoreMovesEvent>(HandleNoMoreMoves);
        EventBus.Get.Unsubscribe<GameItemDetonationStartedEvent>(HandleGameItemDetonation);
    }
    
    private void HandleScoreChanged(ScoreChangedEvent ev)
    {
        FlyingMessage vfx = GetVfx(VfxType.FlyingScore) as FlyingMessage;
        if (vfx != null)
        {
            FlyingMessage.MessageType msgType = (ev.ScoreDelta > 0)
                ? FlyingMessage.MessageType.Positive
                : FlyingMessage.MessageType.Negative;
            string msgPrefix = (ev.ScoreDelta > 0) ? "+" : "-";
            
            vfx.Init(ev.SourceCell.CachedTransform.position, Quaternion.identity);
            vfx.SetText($"{msgPrefix}{ev.ScoreDelta}", msgType);
        }
    }
    
    private void HandleLevelCompleted()
    {
        FlyingMessage message = GetMessageVfx();
        if (message != null)
        {
            message.SetText($"{LevelMessages.LevelCompleted}", FlyingMessage.MessageType.Positive);
        }
    }
    
    private void HandleComboCollected(ComboCollectedEvent ev)
    {
        FlyingMessage message = GetMessageVfx();
        if (message != null)
        {
            message.SetText($"{LevelMessages.Combo} {ev.ComboCount} x", FlyingMessage.MessageType.Positive);
        }
    }
    
    private void HandleLevelTimeExpiring()
    {
        FlyingMessage message = GetMessageVfx();
        if (message != null)
        {
            message.SetText($"{LevelMessages.TimeExpiring}", FlyingMessage.MessageType.Warning);
        }
    }

    private void HandleLevelTimeExpired()
    {
        FlyingMessage message = GetMessageVfx();
        if (message != null)
        {
            message.SetText($"{LevelMessages.TimeExpired}", FlyingMessage.MessageType.Negative);
        }
    }
    
    private void HandleNoMoreMoves()
    {
        FlyingMessage message = GetMessageVfx();
        if (message != null)
        {
            message.SetText($"{LevelMessages.NoMoreMoves}", FlyingMessage.MessageType.Negative);
        }
    }

    private void HandleGameItemDetonation(GameItemDetonationStartedEvent ev)
    {
        GameItem item = ev.Item;
        Vector3 position = item.CachedTransform.position;

        if (item.IsUsual())
        {
            AddGameItemDetonationVfx(item.BaseItemType, position);
        }
        else
        {
            if (item.IsStar())
            {
                AddStarCollectVfx(position);
                AddFlyingStarVfx(position);
            }
            else
            {
                AddBombDetonationVfx(position);
            }
        }
    }
    
    private void AddBombDetonationVfx(Vector2 position)
    {
        GetVfx(VfxType.BombDetonation).Init(position, Quaternion.identity);
    }

    private void AddStarCollectVfx(Vector2 position)
    {
        GetVfx(VfxType.StarCollect).Init(position, Quaternion.identity);
    }

    private void AddFlyingStarVfx(Vector2 position)
    {
        GetVfx(VfxType.FlyingStar).Init(position, Quaternion.identity);
    }

    private void AddGameItemDetonationVfx(GameItemType gameItemBaseType, Vector2 position)
    {
        VfxType vfxType = gameItemBaseType switch
        {
            GameItemType.Item1 => VfxType.Item1Detonation,
            GameItemType.Item2 => VfxType.Item2Detonation,
            GameItemType.Item3 => VfxType.Item3Detonation,
            GameItemType.Item4 => VfxType.Item4Detonation,
            GameItemType.Item5 => VfxType.Item5Detonation,
            _ => VfxType.Item6Detonation
        };
        
        GetVfx(vfxType).Init(position, Quaternion.identity);
    }

    private Pool<VfxBase> GetPool(VfxType type)
    {
        if (_vfxPoolsMap.TryGetValue(type, out Pool<VfxBase> pool))
            return pool;
        
        return null;
    }
    
    private FlyingMessage GetMessageVfx()
    {
        FlyingMessage message = GetPool(VfxType.FlyingMessage).GetFreeElement() as FlyingMessage;

        if (message != null)
        {
            message.Init(_gameFieldCenterPosition, Quaternion.identity);
        }

        return message;
    }
    
    private VfxBase GetVfx(VfxType type)
    {
        Pool<VfxBase> pool = GetPool(type);
        
        return pool.GetFreeElement();
    }
    
    private void FillVfxPoolsMap()
    {
        foreach (VfxBase vfx in _itemDetonationvfxPrefabs)
        {
            if (!_vfxPoolsMap.ContainsKey(vfx.Type))
                _vfxPoolsMap.Add(vfx.Type, new Pool<VfxBase>(vfx, _itemDetonationVfxPoolSize, _vfxContainer));
        }
        foreach (VfxBase vfx in _otherVfxPrefabs)
        {
            if (!_vfxPoolsMap.ContainsKey(vfx.Type))
                _vfxPoolsMap.Add(vfx.Type, new Pool<VfxBase>(vfx, _defaultVfxPoolSize, _vfxContainer));
        }
    }
}
