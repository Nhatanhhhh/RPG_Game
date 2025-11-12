using TMPro;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;
    public TMP_Text healthText;
    public StatsUI statsUI;

    [Header("Combat Stats")]
    public int damage;
    public float weaponRange;
    public float knockbackForce;
    public float knockbackTime;
    public float stunTime;

    [Header("Movement Stats")]
    public int speed;

    [Header("Health Stats")]
    public int maxHealth;
    public int currentHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void UpdateMaxDame(int amount)
    {
        damage += amount;
    }
    public void UpdateMaxHealth(int amount)
    {
        maxHealth += amount;
        healthText.text = "Hp: " + currentHealth + "/" + maxHealth; 
    }

    public void UpdateMaxSpeed(int amount)
    {  
        speed += amount;
    
    }
    public void UpdateHealth(int amount)
    {
        currentHealth += amount;
        if(currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        healthText.text = "Hp: " + currentHealth + "/" + maxHealth;
    }
    public void UpdateSpeed(int amount)
    {
        speed += amount;
        statsUI.UpdateAllStats();
    }
}
