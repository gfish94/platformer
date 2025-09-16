using System.Collections.Generic;
using UnityEngine;

public class PrefabPool
{
    private GameObject prefab;
    private Queue<GameObject> pool = new();
    private HashSet<GameObject> activeObjects = new();

    public PrefabPool(GameObject prefab, int initialSize = 15)
    {
        this.prefab = prefab;
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : GameObject.Instantiate(prefab);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.transform.SetParent(parent);
        obj.SetActive(true);
        activeObjects.Add(obj);
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return; // Already destroyed or missing
        if (obj.Equals(null)) return; // Unity's special null check
        obj.SetActive(false);
        pool.Enqueue(obj);
        activeObjects.Remove(obj);
    }

    public void ReturnAll()
    {
        foreach (var obj in new List<GameObject>(activeObjects))
        {
            if (obj == null || obj.Equals(null)) continue; // Skip destroyed
            Return(obj);
        }
        activeObjects.Clear();
    }
}
