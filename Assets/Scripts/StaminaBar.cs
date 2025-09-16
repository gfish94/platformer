using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [Tooltip("Reference to the player script.")]
    public Player player;

    [Tooltip("Reference to the slider UI element.")]
    public Slider staminaSlider;

    [Tooltip("Reference to the fill image of the slider.")]
    public Image fillImage;

    private void Start()
    {
        if (staminaSlider == null)
        {
            Debug.LogError("StaminaSlider is not assigned! Ensure it is assigned in the Unity Inspector.");
        }

        if (fillImage == null)
        {
            Debug.LogError("FillImage is not assigned! Ensure it is assigned in the Unity Inspector.");
        }
    }

    public void SetMaxStamina(float maxStamina)
    {
        if (staminaSlider == null) return;

        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = maxStamina;
    }

    public void SetStamina(float currentStamina)
    {
        if (staminaSlider == null)
        {
            Debug.LogError("StaminaSlider is not assigned!");
            return;
        }

        staminaSlider.value = currentStamina;

        // Change color based on stamina percentage
        if (fillImage != null)
        {
            float percentage = currentStamina / staminaSlider.maxValue;
            fillImage.color = Color.Lerp(Color.red, Color.green, percentage);
        }
    }
    public void UpdateStaminaBar(float currentStamina, bool isExhausted)
    {
        if (staminaSlider == null || fillImage == null) return;

        staminaSlider.value = currentStamina;

        // Change color based on exhaustion state
        if (isExhausted)
        {
            fillImage.color = Color.red; // Red for exhaustion
        }
        else
        {
            float percentage = currentStamina / staminaSlider.maxValue;
            fillImage.color = Color.Lerp(Color.red, Color.green, percentage); // Gradient from red to green
        }
    }
}
