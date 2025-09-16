using UnityEngine;

public class Crate : MonoBehaviour
{
    public GameObject scorePotionPrefab;
    public GameObject healthPotionPrefab;

    public PrefabPool healthPotionPool;
    public PrefabPool scorePotionPool;

    private void Awake()
    {
        // Initialize the pools with the respective prefabs
        healthPotionPool = new PrefabPool(healthPotionPrefab, 5);
        scorePotionPool = new PrefabPool(scorePotionPrefab, 5);
    }
    public void Break()
    {
        Vector3 pos = transform.position;
        int rand = Random.Range(0, 10);
        if (rand > 6)
        {
            scorePotionPool.Get(new Vector3(pos.x, pos.y, 0), Quaternion.identity);
        }
        else
        {
            healthPotionPool.Get(new Vector3(pos.x, pos.y, 0), Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
