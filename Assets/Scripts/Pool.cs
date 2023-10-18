using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class Pool<T> where T : MonoBehaviour
{
    public T Prefab { get; }
    public bool IsAutoExpand { get; set; }
    public Transform Container { get; }

    private List<T> _pool;

    public Pool(T prefab, int poolSize)
    {
        Prefab = prefab;
        Container = null;
        IsAutoExpand = true;

        InitPool(poolSize);
    }

    public Pool(T prefab, int poolSize, Transform container)
    {
        Prefab = prefab;
        Container = container;
        IsAutoExpand = true;
        
        InitPool(poolSize);
    }

    public bool HasFreeElement(out T element)
    {
        foreach (T elem in _pool)
        {
            if (!elem.gameObject.activeInHierarchy)
            {
                elem.gameObject.SetActive(true);
                element = elem;
                return true;
            }
        }

        element = null;
        return false;
    }

    public T GetFreeElement()
    {
        if (HasFreeElement(out T element))
            return element;

        if (IsAutoExpand)
            return CreateElement(true);

        throw new Exception($"There is no free elements in pool of type {typeof(T)}");
    }
    
    private void InitPool(int poolSize)
    {
        _pool = new List<T>();

        for (int i = 0; i < poolSize; i++)
            CreateElement();
    }

    private T CreateElement(bool isActive = false)
    {
        T element = Object.Instantiate(Prefab, Container);
        element.gameObject.SetActive(isActive);
        _pool.Add(element);
        
        return element;
    }
}
