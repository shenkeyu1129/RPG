using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager
{
    private readonly Dictionary<string, ObjectPool> _pools = new();
    private Transform _poolRoot;

    public void Initialize()
    {
        _poolRoot = new GameObject("ObjectPool").transform;
        Object.DontDestroyOnLoad(_poolRoot.gameObject);
    }

    /// <summary>
    /// 获取对象池，如果不存在则创建一个新的对象池
    /// </summary> 
    /// <param name="prefab">对象池预制体</param>
    /// <param name="initSize">初始对象池大小</param>
    /// <param name="maxSize">对象池最大大小</param>
    public ObjectPool GetPool(GameObject prefab, int initSize = 10, int maxSize = 100)
    {
        string key = prefab.name;
        if(_pools.TryGetValue(key, out var pool))
        {
            return pool;
        }

        var poolTransform = new GameObject($"Pool_{key}").transform;
        poolTransform.SetParent(_poolRoot);
        var newPool = new ObjectPool(prefab, poolTransform, initSize, maxSize);
        _pools.Add(key, newPool);
        return newPool;
    }

    /// <summary>
    /// 释放对象池，清理池内所有对象
    /// </summary> 
    /// <param name="prefabName">对象池对应的预制体名称</param>
    public void ReleasePool(string prefabName)
    {
        if(_pools.TryGetValue(prefabName, out var pool))
        {
            pool.Clear();
            _pools.Remove(prefabName);
        }
    }

    /// <summary>
    /// 销毁对象池管理器，清理所有对象池和池内对象
    /// </summary>
    public void Destroy()
    {
        foreach (var pool in _pools.Values)
        {
            pool.Clear();
        }
        _pools.Clear();
        Object.Destroy(_poolRoot.gameObject);
    }
}

public class ObjectPool
{
    private readonly GameObject _prefab;
    private readonly Transform _parent;
    private readonly Queue<GameObject> _pool = new();
    private readonly int _maxSize;

    /// <summary>
    ///  创建对象池
    /// </summary>
    /// <param name="prefab">对象预制体</param>
    /// <param name="parent">父对象</param>
    /// <param name="initSize">初始大小</param>
    /// <param name="maxSize">最大大小</param>
    public ObjectPool(GameObject prefab, Transform parent, int initSize, int maxSize)
    {
        _prefab = prefab;
        _parent = parent;
        _maxSize = maxSize;

        for (int i = 0; i < initSize; i++)
        {
            CreateNewObject();
        }
    }

    /// <summary>
    /// 获取对象池中的一个对象，如果池中没有可用对象则创建一个新的对象
    /// </summary> 
    /// <returns>对象池中的一个对象</returns>
    public GameObject Get()
    {

        while (_pool.Count > 0)
        {
            var obj = _pool.Dequeue();
            if (obj != null)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        return CreateNewObject();
    }

        /// <summary> 
        /// 释放对象，将对象放回对象池中，如果对象池已满则销毁对象 
        /// </summary>
        /// <param name="obj">要释放的对象</param>
    public void Release(GameObject obj)
    {
        if(obj == null)return;
        if(_pool.Count >= _maxSize)
        {
            Object.Destroy(obj);
            return;
        }
        obj.SetActive(false);
        _pool.Enqueue(obj);

    }

        /// <summary>
        /// 清理对象池，销毁池内所有对象并清空池
        /// </summary>
    public void Clear()
    {
        foreach (var obj in _pool)
        {
            if(obj != null)
            {
                Object.Destroy(obj);
            }
        }
        _pool.Clear();
    }

    /// <summary>
    /// 创建一个新的对象并加入对象池
    /// </summary>
    /// <returns> 新创建的对象 </returns>
    private GameObject CreateNewObject()
    {
        var obj = Object.Instantiate(_prefab, _parent);
        obj.SetActive(false);
        _pool.Enqueue(obj);
        return obj;
    }
}