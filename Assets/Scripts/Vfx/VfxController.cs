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

    public static VfxController Instance { get; private set; }

    private readonly Dictionary<VfxType, Pool<VfxBase>> _vfxPoolsMap = new Dictionary<VfxType, Pool<VfxBase>>();
    
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

    private void AddEventHandlers()
    {
        EventBus.Get.Subscribe<ScoreChangedEvent>(HandleScoreChanged);
    }

    private void RemoveEventHandlers()
    {
        EventBus.Get.Unsubscribe<ScoreChangedEvent>(HandleScoreChanged);
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
    
    public ScalerVfx AddScalerVfx(VfxType type)
    {
        ScalerVfx vfx = GetVfx(type) as ScalerVfx;
        if (vfx != null)
        {
            vfx.Init(Vector3.zero, Quaternion.identity);
        }
        
        return vfx;
    }
    
    public FlyingMessage AddFlyingMessageVfx(Vector3 position, Quaternion rotation)
    {
        FlyingMessage vfx = GetVfx(VfxType.FlyingMessage) as FlyingMessage;
        if (vfx != null)
        {
            vfx.Init(position, rotation);
        }
        
        return vfx;
    }
    
    public void AddBombDetonationVfx(Vector2 position)
    {
        GetVfx(VfxType.BombDetonation).Init(position, Quaternion.identity);
    }
    
    public void AddStarCollectVfx(Vector2 position)
    {
        GetVfx(VfxType.StarCollect).Init(position, Quaternion.identity);
    }
    
    public void AddFlyingStarVfx(Vector2 position)
    {
        GetVfx(VfxType.FlyingStar).Init(position, Quaternion.identity);
    }

    public void AddGameItemDetonationVfx(GameItemType gameItemBaseType, Vector2 position)
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
