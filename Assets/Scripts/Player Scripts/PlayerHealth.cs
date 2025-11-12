using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public TMP_Text healthText;
    public Animator healTextAnim;

    private void Start()
    {
        healthText.text = "HP: " + StatsManager.Instance.currentHealth + " / " + StatsManager.Instance.maxHealth;
    }

    public void ChangeHealth(int amount)
    {
        StatsManager.Instance.currentHealth += amount;
        healTextAnim.Play("TextUpdate");
        healthText.text = "HP: " + StatsManager.Instance.currentHealth + " / " + StatsManager.Instance.maxHealth;
        if (StatsManager.Instance.currentHealth <= 0)
        {
            //gameObject.SetActive(false);
            SceneManager.LoadScene("Game_Over");
        }
    }
}
