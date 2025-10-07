using TMPro;
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    public TMP_Text healthText;
    public Animator healTextAnim;

    private void Start()
    {
        healthText.text = "HP: " + currentHealth + " / " + maxHealth;
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        healTextAnim.Play("TextUpdate");
        healthText.text = "HP: " + currentHealth + " / " + maxHealth;
        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
