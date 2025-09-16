using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePotion : MonoBehaviour
{

    public Player player;

    private void Start()
    {
        player = GameObject.Find("PLAYER").GetComponent<Player>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.tag == "Player")
        {
            player.currScore += 10;
            Destroy(gameObject);
        }

        if (collision.collider.gameObject.tag == "Water")
        {
            Destroy(gameObject);
        }
    }
}
