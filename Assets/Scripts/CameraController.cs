using UnityEngine;
public class CameraController : MonoBehaviour
{
    [Tooltip("Speed at which the camera scrolls along the x-axis.")]
    public float scrollSpeed = 2f;

    [Tooltip("Distance behind the camera at which the player is considered to have fallen behind.")]
    public float killThreshold = 20f;

    [Tooltip("Reference to the Player script.")]
    public Player player;

    private Transform cameraTransform;
    private int lastScoreCheckpoint; // Tracks the last score checkpoint for speed increase
    private float initialScrollSpeed; // Stores the initial scroll speed

    private void Start()
    {
        // Cache the camera's transform for performance
        cameraTransform = transform;

        // Store the initial scroll speed
        initialScrollSpeed = scrollSpeed;

        // Find the player object if not assigned in the Inspector
        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.GetComponent<Player>();
            if (player == null)
            {
                Debug.LogError("Player object not found! Ensure the Player has the 'Player' tag.");
            }
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Auto-scroll the camera
        AutoScroll();

        // Check if the player has fallen behind
        CheckPlayerPosition();

        // Adjust scroll speed based on player's score
        AdjustScrollSpeed();
    }

    private void AutoScroll()
    {
        // Move the camera along the x-axis at a constant speed
        cameraTransform.position += Vector3.right * scrollSpeed * Time.deltaTime;
    }

    private void CheckPlayerPosition()
    {
        // Precompute the kill threshold position
        float killPosition = cameraTransform.position.x - killThreshold;

        // Check if the player has fallen behind the camera
        if (player.transform.position.x < killPosition)
        {
            player.TakeDamage(player.maxHealth); // Kill the player by dealing max damage
        }
    }

    private void AdjustScrollSpeed()
    {
        // Check if the player's score has reached the next multiple of 10
        if (player.currScore >= lastScoreCheckpoint + 10)
        {
            scrollSpeed += 0.35f; // Increase scroll speed
            lastScoreCheckpoint += 10; // Update the checkpoint
        }
    }

    public void ResetToPlayer()
    {
        if (player == null) return;

        // Directly center the camera on the player's position
        cameraTransform.position = new Vector3(
            player.transform.position.x,
            cameraTransform.position.y,
            cameraTransform.position.z
        );

        // Reset the scroll speed to its initial value
        scrollSpeed = initialScrollSpeed;

        // Reset the score checkpoint
        lastScoreCheckpoint = 0;
    }
}
