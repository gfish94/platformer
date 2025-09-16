using UnityEngine;

public class EnemyDmg : MonoBehaviour
{
    [Tooltip("Reference to the Player script")]
    public Player player;

    [Tooltip("Amount of damage this enemy deals")]
    public int damage = 1;

    private void Start()
    {
        // Ensure the player reference is assigned
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<Player>();
            }
            else
            {
                Debug.LogError("Player object not found! Ensure the Player has the 'Player' tag.");
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the player and this is not a weak point
        if (collision.collider.CompareTag("Player") && !CompareTag("WeakPoint"))
        {
            if (player != null)
            {
                // Apply knockback and damage
                player.knockbackCounter = player.knockbackClock;
                player.hitFromRight = collision.transform.position.x <= transform.position.x;
                player.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("Player reference is null. Cannot apply damage or knockback.");
            }
        }
    }
}
