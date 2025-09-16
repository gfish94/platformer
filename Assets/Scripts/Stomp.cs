using UnityEngine;

public class Stomp : MonoBehaviour
{

    private Player player;

    private void Start()
    {
        player = GameObject.Find("PLAYER").GetComponent<Player>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.tag == "WeakPoint")
        {
            Destroy(collision.gameObject);
            if (collision.collider.GetComponent<Enemy>())
            {
                player.IncreaseScore(collision.collider.GetComponent<Enemy>().Score);
            }
        }
    }
}
