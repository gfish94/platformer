using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthIncrease : MonoBehaviour
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
            if (player.currHealth < player.maxHealth)
            {
                player.currHealth += 1;
            }
            else
            {
                player.SetInvincible();
            }
            Destroy(gameObject);
        }

        if (collision.collider.gameObject.tag == "Water")
        {
            Destroy(gameObject);
        }
    }
}
