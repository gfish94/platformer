using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Tooltip("Reference to the player script.")]
    public Player player;

    [Tooltip("Reference to the image of the health icons.")]
    public Image[] icons;

    private void Update()
    {
        CheckHealth();
    }
    private void CheckHealth()
    {
        for (int i = 0; i < player.maxHealth; i++)
        {
            icons[i].enabled = (player.currHealth > i);
        }
    }
}
