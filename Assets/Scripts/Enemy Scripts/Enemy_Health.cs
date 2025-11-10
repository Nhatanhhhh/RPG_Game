using System;
//using System.Diagnostics;
using UnityEngine;

[Serializable]
public class LootItem
{
    public GameObject lootPrefab;         
    [Range(0f, 1f)] public float dropChance = 1f;
    public int minAmount = 1;
    public int maxAmount = 1;
}

public class Enemy_Health : MonoBehaviour
{
    [Header("EXP Settings")]
    public int expReward = 3;
    [Header("Gold Settings")]
    public int gold = 3;

    public static event Action<int> OnMonsterDefeated;

    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Loot Drop")]
    public LootItem[] lootTable;
    public float spawnRadius = 1f;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
 
        OnMonsterDefeated?.Invoke(expReward);
        OnMonsterDefeated.Invoke(gold);
       
        SpawnLoot();

    
        Destroy(gameObject);
    }

    private void SpawnLoot()
    {
     

        if (lootTable == null || lootTable.Length == 0)
            return;

        foreach (LootItem loot in lootTable)
        {

            if (loot == null || loot.lootPrefab == null)
                continue;

            Debug.Log("Spawning loot: " + loot.lootPrefab.name);
            if (UnityEngine.Random.value <= loot.dropChance)
            {
                int quantity = UnityEngine.Random.Range(loot.minAmount, loot.maxAmount + 1);

    
                for (int i = 0; i < quantity; i++)
                {
                    Vector2 spawnPos = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * spawnRadius;
                    Instantiate(loot.lootPrefab, spawnPos, Quaternion.identity);
                }
            }
        }
    }
}
