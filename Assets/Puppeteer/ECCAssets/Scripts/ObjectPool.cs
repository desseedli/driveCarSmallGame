using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class ObjectPool 
{
    private readonly Queue<GameObject> pool;
    private GameObject prefab;
    public string prefabName ;
    private int expandCount ;
    public Transform root;
    public ObjectPool(string prefabName, int count)
    {
        pool = new Queue<GameObject>();
        this.expandCount = count;
        this.prefabName = prefabName;
        GameObject obj = new GameObject(this.prefabName + "_pool");
        root = obj.transform;
        AsyncOperationHandle <GameObject> handle = Addressables.LoadAssetAsync<GameObject>(this.prefabName);
        handle.Completed += OnLoadCompleted;
    }

    void OnLoadCompleted(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            this.prefab = handle.Result;
            ExpandPool();
        }
    }
    public GameObject GetObject()
    {
        if (pool.Count == 0)
        {
            ExpandPool();
        }
        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    private void ExpandPool()
    {
        for (int i = 0; i < expandCount; ++i)
        {
            GameObject newObj = GameObject.Instantiate(prefab);
            newObj.transform.parent = root;
            pool.Enqueue(newObj);
            newObj.SetActive(false);
        }
    }
}
