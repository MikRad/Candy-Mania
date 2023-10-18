using System.Collections.Generic;
using UnityEngine;

public class VfxController : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private Transform _vfxContainer;
    
    [Header("Vfx prefabs")]

    [SerializeField] private VfxBase[] _itemDetonationvfxPrefabs;
    [SerializeField] private VfxBase[] _otherVfxPrefabs;
    
    [SerializeField] private VfxBase _fallenItemVfxPrefab;
    
    [Header("Vfx pools size")] 
    [SerializeField] private int _defaultVfxPoolSize = 10;
    [SerializeField] private int _itemDetonationVfxPoolSize = 20;
    [SerializeField] private int _fallenItemsPoolSize = 100;

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
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
    
    public ItemSelectionVfx AddItemSelectionVfx()
    {
        ItemSelectionVfx vfx = GetVfx(VfxType.ItemSelection) as ItemSelectionVfx;
        if (vfx != null)
        {
            vfx.Init(Vector3.zero, Quaternion.identity);
        }
        
        return vfx;
    }
    
    public FallenItemVfx AddFallenItemVfx()
    {
        FallenItemVfx vfx = GetVfx(VfxType.FallenItem) as FallenItemVfx;
        if (vfx != null)
        {
            vfx.Init(Vector3.zero, Quaternion.identity);
        }
        
        return vfx;
    }
    
    public FlyingMessage AddFlyingScoreVfx(Vector3 position, Quaternion rotation)
    {
        FlyingMessage vfx = GetVfx(VfxType.FlyingScore) as FlyingMessage;
        if (vfx != null)
        {
            vfx.Init(position, rotation);
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
        
        _vfxPoolsMap.Add(_fallenItemVfxPrefab.Type, new Pool<VfxBase>(_fallenItemVfxPrefab, _fallenItemsPoolSize, _vfxContainer));
    }
}
