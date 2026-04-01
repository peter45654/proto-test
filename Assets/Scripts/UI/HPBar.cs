using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI; // Required for UI elements

public class HPBar : MonoBehaviour
{
    public Slider healthBarSlider; // Drag your Slider here in the Inspector

    public Player player; // Reference to the Player script
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();

        if (player == null)
        {
            Debug.LogError("找不到Player腳本，請確保玩家物件已正確設置標籤並附加Player腳本！");
            enabled = false; // Disable this script if Player is not found
            return;
        }
        
        currentHealth = player.CurrentHealth; // Initialize current health from the player
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = player.MaxHealth;
            healthBarSlider.value = currentHealth;
        }
    }

    void Update()
    {
        if (player != null)
        {
            currentHealth = player.CurrentHealth; // Update current health from the player
            currentHealth = Mathf.Clamp(currentHealth, 0, player.MaxHealth); // Prevent health from going below 0 or above max
            if (healthBarSlider != null)
            {
                healthBarSlider.value = currentHealth; // Update the UI slider
            }
        }
    }

    // public void TakeDamage(float damage)
    // {
    //     currentHealth -= damage;
        

    //     if (healthBarSlider != null)
    //     {
    //         healthBarSlider.value = currentHealth; // Update the UI slider
    //     }

    //     if (currentHealth <= 0)
    //     {
    //         Debug.Log("Player Died!");
    //         // Handle player death
    //     }
    // }
}
