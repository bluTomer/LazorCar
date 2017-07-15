using System;
using System.Collections.Generic;
using UnityEngine;

public class MasterPooler : MonoBehaviour
{
    #region Poolable Class

    public class PoolableData
    {
        public PoolableData(GameObject objectPrefab)
        {
            ObjectPrefab = objectPrefab;
            TotalObjectPool = new Queue<GameObject>();
            CurrentObjectPool = new Queue<GameObject>();
        }

        public Transform Parent;
        public GameObject ObjectPrefab;
        public Queue<GameObject> TotalObjectPool;
        public Queue<GameObject> CurrentObjectPool;
    }

    #endregion


    #region Fields

    private static Dictionary<Type, PoolableData> _poolables;
    private static Transform _transform;

    #endregion // Fields


    #region Pool Interface

    public static int ActiveObjectCount<T>()
    {
        if (!_poolables.ContainsKey(typeof(T)))
        {
            throw new Exception(string.Format("[MasterPooler] Trying to get item ({0}) from an uninitiated pool.", typeof(T)));
        }

        return _poolables[typeof(T)].TotalObjectPool.Count - _poolables[typeof(T)].CurrentObjectPool.Count;

    }

    public static void InitPool<T>(T objectPrefab) where T : MonoBehaviour, IPoolable
    {
        _poolables[typeof(T)] = new PoolableData(objectPrefab.gameObject);
        _poolables[typeof(T)].Parent = new GameObject(typeof(T).ToString()).transform;
        _poolables[typeof(T)].Parent.SetParent(_transform);
    }

    public static void Return<T>(T poolable) where T : MonoBehaviour, IPoolable
    {
        // Sanity
        if (!_poolables.ContainsKey(typeof(T)))
        {
            throw new Exception(string.Format("[MasterPooler] Trying to return item ({0}) to an uninitiated pool.", typeof(T)));
        }

        // Prepare and return to queue
        poolable.Reset();
        poolable.gameObject.SetActive(false);
        _poolables[typeof(T)].CurrentObjectPool.Enqueue(poolable.gameObject);
    }

    public static T Get<T>(Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolable
    {
        // Sanity
        if (!_poolables.ContainsKey(typeof(T)))
        {
            throw new Exception(string.Format("[MasterPooler] Trying to get item ({0}) from an uninitiated pool.", typeof(T)));
        }

        // Get or create an object
        var availableQueue = _poolables[typeof(T)].CurrentObjectPool;
        var totalQueue = _poolables[typeof(T)].TotalObjectPool;

        // Create a new object if there are none available
        if (availableQueue.Count < 1)
        {
            var newObject = Instantiate(_poolables[typeof(T)].ObjectPrefab);
            newObject.transform.SetParent(_poolables[typeof(T)].Parent);
            availableQueue.Enqueue(newObject);
            totalQueue.Enqueue(newObject);
        }

        // Get and position object
        var poolable = availableQueue.Dequeue();
        poolable.SetActive(true);
        poolable.transform.position = position;
        poolable.transform.rotation = rotation;
        poolable.GetComponent<IPoolable>().Init();
        return poolable.GetComponent<T>();
    }

    public static void ClearPool<T>()
    {
        // Sanity
        if (!_poolables.ContainsKey(typeof(T)))
        {
            throw new Exception(string.Format("[MasterPooler] Trying to clear an uninitiated pool.", typeof(T)));
        }

        // Get and destroy all objects
        var allObjects = _poolables[typeof(T)].TotalObjectPool;
        foreach (var poolable in allObjects)
        {
            Destroy(poolable.gameObject);
        }

        // Remove poolable from dictionary
        _poolables.Remove(typeof(T));
    }

    #endregion // Pool Interface


    #region BaseBehaviour

    protected void Awake()
    {
        _poolables = new Dictionary<Type, PoolableData>();
        _transform = GetComponent<Transform>();

        // Check for duplicates
        if (FindObjectsOfType<MasterPooler>().Length > 1)
        {
            throw new Exception("More than one instance of MasterPooler exists in scene");
        }
    }

    #endregion // BaseBehaviour
}
