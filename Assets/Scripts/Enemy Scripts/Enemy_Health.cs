using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy_Health : MonoBehaviour
{
    [Header("Loại Enemy")]
    public bool isBoss = false;
    [Tooltip("ID duy nhất cho boss, ví dụ: 'MAP1_Dragon'")]
    public string persistentID = "";
    public float respawnTime = 30f;

    [Header("EXP Settings")]
    public int expReward = 3;

    public static event Action<int> OnMonsterDefeated;

    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;
    private Vector3 originalPosition;
    private bool isDead = false;

    [Header("Health Bar UI")]
    public GameObject healthBarCanvas;
    public Slider healthSlider;

    [Header("Loot Drop")]
    public LootItem[] lootTable;
    public float spawnRadius = 1f;

    private Collider2D col;
    private SpriteRenderer rend;
    private Rigidbody2D rb;
    private Animator anim;
    private Enemy_Movement movementScript;
    private Enemy_Combat combatScript;
    private Enemy_Knockback knockbackScript;

    private void Start()
    {
        Respawn();
    }

    private void Awake()
    {
        if (isBoss && !string.IsNullOrEmpty(persistentID))
        {
            if (GameManager.Instance.defeatedEnemies.Contains(persistentID))
            {
                Destroy(gameObject);
                return;
            }
        }

        originalPosition = transform.position;
        col = GetComponent<Collider2D>();
        rend = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        movementScript = GetComponent<Enemy_Movement>();
        combatScript = GetComponent<Enemy_Combat>();
        knockbackScript = GetComponent<Enemy_Knockback>();

        if (healthBarCanvas != null)
        {
            Canvas canvas = healthBarCanvas.GetComponent<Canvas>();

            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }

    public void ChangeHealth(int amount)
    {
        if (isDead) return; 
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if (healthBarCanvas != null && !healthBarCanvas.activeInHierarchy) healthBarCanvas.SetActive(true);
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnMonsterDefeated?.Invoke(expReward);

        SpawnLoot();

        if (isBoss && !string.IsNullOrEmpty(persistentID))
        {
            GameManager.Instance.defeatedEnemies.Add(persistentID);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(RespawnCoroutine());
        }
    }

    private IEnumerator RespawnCoroutine()
    {

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (movementScript != null) movementScript.enabled = false;
        if (combatScript != null) combatScript.enabled = false;
        if (knockbackScript != null) knockbackScript.enabled = false;
        if (anim != null) anim.enabled = false;


        if (col != null) col.enabled = false;
        if (rend != null) rend.enabled = false;
        if (healthBarCanvas != null) healthBarCanvas.SetActive(false); 

        yield return new WaitForSeconds(respawnTime);

        Respawn();
    }

    private void Respawn()
    {
        isDead = false;
        transform.position = originalPosition; 
        currentHealth = maxHealth; 

        if (col != null) col.enabled = true;
        if (rend != null) rend.enabled = true;

        if (movementScript != null) movementScript.enabled = true;
        if (combatScript != null) combatScript.enabled = true;
        if (knockbackScript != null) knockbackScript.enabled = true;
        if (anim != null) anim.enabled = true;

        if (movementScript != null)
        {
            movementScript.ChangeState(EnemyState.Idle);
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (healthBarCanvas != null)
        {
             healthBarCanvas.SetActive(true);
            //healthBarCanvas.SetActive(false);
        }
        // ---------------------
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