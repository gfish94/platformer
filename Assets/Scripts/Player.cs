using System.Collections;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    // Cached components
    private Rigidbody2D rb;
    private Collider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;

    // Player state
    private Vector2 respawnPoint;
    private float moveInput;
    private float timer;

    // Public properties
    public int currScore;
    public int savedScore;

    public StaminaBar staminaBar; // Reference to the stamina bar UI

    [Range(0.5f, 25f)] public float runSpeed;
    [Range(0.5f, 25f)] public float sprintMultiplier = 1.5f; // Multiplier for sprint speed
    [Range(0.5f, 25f)] public float jumpForce;
    [Range(1.01f, 25f)] public float jumpDecay;
    public float invincibleTime = 3.0f;

    public float knockbackForce;
    public float knockbackCounter;

    public bool hitFromRight;

    public int maxHealth = 3;
    public int currHealth;

    public bool isInvincible;

    private bool isGrounded;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int Airborne = Animator.StringToHash("airborne");
    private static readonly int JumpTrigger = Animator.StringToHash("jump");

    private float extraHeight = 0.25f;
    public LayerMask groundMask;

    private UserInput userInput;
    public float knockbackClock = 0.5f; // Duration of knockback effect
    public float maxStamina = 100f; // Maximum stamina
    public float currentStamina;   // Current stamina
    public float staminaRegenRate = 15f; // Stamina regeneration rate per second
    public float staminaDrainRate = 35f; // Stamina drain rate per second while sprinting
    private bool isExhausted = false;

    public TMP_Text scoreText;
    public TMP_Text highscoreText;

    private void Start()
    {
        // Cache components
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        userInput = UserInput.instance;

        SetRespawnPoint((Vector2)transform.position, 0);

        currHealth = maxHealth;
        currScore = savedScore;
        currentStamina = maxStamina;
        isInvincible = false;

        // Initialize the stamina bar
        staminaBar?.SetMaxStamina(maxStamina);
        staminaBar?.UpdateStaminaBar(currentStamina, isExhausted);

        // Perform an initial ground check
        isGrounded = GroundCheck();

        UpdateScoreUI();
        UpdateHighscoreUI();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > 1)
        {
            IncreaseScore(1);
            timer = 0;
        }

        if (knockbackCounter > 0 && !isInvincible)
        {
            ApplyKnockback();
        }
        else
        {
            HandleMovement();
            HandleJump();
        }

        // Perform ground check only when necessary
        if (rb.velocity.y != 0)
        {
            isGrounded = GroundCheck();
        }

        UpdateSpriteColor();
    }

    public void IncreaseScore(int amount)
    {
        currScore += amount;
        UpdateScoreUI();

        // Check and update highscore
        int currentHighscore = GetHighscore();
        if (currScore > currentHighscore)
        {
            HighscoreManager.AddScore(currScore);
            UpdateHighscoreUI();
        }
    }

    private void ApplyKnockback()
    {
        rb.velocity = new Vector2(hitFromRight ? -knockbackForce : knockbackForce, knockbackForce);
        knockbackCounter -= Time.deltaTime;
    }

    private void UpdateSpriteColor()
    {
        if (isExhausted) return; // Skip color updates if exhausted

        // Avoid redundant color updates
        Color targetColor = isInvincible ? new Color(1, 0, 0, 1) : new Color(1, 1, 1, 1);
        if (sprite.color != targetColor)
        {
            sprite.color = targetColor;
        }
    }

    public void SetRespawnPoint(Vector2 position, int score)
    {
        respawnPoint = position;
        savedScore = score;
    }

    #region Damage

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            currHealth -= damage;

            if (currHealth <= 0)
            {
                Respawn();
                UpdateHighscoreUI();
            }
        }
    }

    private void Respawn()
    {
        var mapGen = FindObjectOfType<MapGen>();

        // Reset platforms first
        if (mapGen != null)
        {
            mapGen.ResetPlatforms(transform.position);
            // Force platform generation at the respawn x position
            mapGen.GeneratePlatforms(transform.position.x);
        }

        // Now get respawn position above platform
        Vector3 respawnPos = mapGen != null ? mapGen.GetRespawnPositionAbovePlatform(transform.position.x) : (Vector3)respawnPoint;

        transform.position = respawnPos;

        // Reset the camera position
        FindObjectOfType<CameraController>()?.ResetToPlayer();

        // Reset health and other states
        currHealth = maxHealth;

        // Reset the player's score
        currScore = 0;
    }

    public void SetInvincible()
    {
        if (!isInvincible)
        {
            isInvincible = true;
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            TakeDamage(maxHealth);
        }
    }

    #endregion

    #region Movement

    private void HandleMovement()
    {
        if (userInput == null) return;

        moveInput = userInput.moveInput.x;

        // Check if sprint input is active, stamina is available, and the player is not exhausted
        bool isSprinting = userInput.controls.Movement.Sprint.IsPressed() && currentStamina > 0 && moveInput != 0 && !isExhausted;

        // Calculate current speed
        float currentSpeed = isSprinting ? runSpeed * sprintMultiplier : runSpeed;

        // Drain stamina if sprinting
        if (isSprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0); // Ensure stamina doesn't go below 0

            // Trigger exhaustion if stamina is fully depleted
            if (currentStamina <= 0)
            {
                isExhausted = true;
                UpdatePlayerColor(); // Update color when exhausted
                StartCoroutine(WaitForStaminaRefill());
            }
        }
        else
        {
            // Regenerate stamina when not sprinting
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina); // Ensure stamina doesn't exceed max
        }

        // Update the stamina bar
        staminaBar?.UpdateStaminaBar(currentStamina, isExhausted);

        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);

        // Update animation only when necessary
        anim.SetBool(IsWalking, moveInput != 0);

        // Flip sprite based on movement direction only when moving
        if (moveInput != 0)
        {
            sprite.flipX = moveInput < 0;
        }
    }

    private void HandleJump()
    {
        if (userInput == null) return;

        if (userInput.controls.Movement.Jump.WasPressedThisFrame() && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            anim.SetTrigger(JumpTrigger);
        }

        if (userInput.controls.Movement.Jump.WasReleasedThisFrame())
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / jumpDecay);
        }
    }

    #endregion

    #region State Check

    private bool GroundCheck()
    {
        RaycastHit2D groundHit = Physics2D.BoxCast(
            coll.bounds.center,
            coll.bounds.size,
            0f,
            Vector2.down,
            coll.bounds.extents.y + extraHeight,
            groundMask
        );

        anim.SetBool(Airborne, groundHit.collider == null);

        return groundHit.collider != null;
    }

    private IEnumerator WaitForStaminaRefill()
    {
        // Wait until stamina is fully refilled
        while (currentStamina < maxStamina)
        {
            yield return null; // Wait for the next frame
        }

        // Reset exhaustion state
        isExhausted = false;
        UpdatePlayerColor(); // Reset color after exhaustion ends
    }

    private void UpdatePlayerColor()
    {
        if (sprite != null)
        {
            // Change color to yellow when exhausted, otherwise reset to white
            sprite.color = isExhausted ? new Color(1f, 1f, 0f, 1f) : Color.white; // Yellow for exhaustion
        }
    }

    #endregion

    void UpdateScoreUI()
    {
        scoreText.text = $"{currScore}";
    }

    void UpdateHighscoreUI()
    {
        int highscore = GetHighscore();
        if (currScore > 0 && currScore >= highscore)
        {
            highscoreText.color = Color.red;
            scoreText.color = Color.red;
        }
        else
        {
            highscoreText.color = Color.white;
            scoreText.color = Color.white;
        }
        highscoreText.text = $"Highscore: {highscore}";
    }

    int GetHighscore()
    {
        var scores = HighscoreManager.GetHighscores();
        return scores.Count > 0 ? scores[0] : 0;
    }
}
