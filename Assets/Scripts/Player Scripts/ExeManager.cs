using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class ExeManager : MonoBehaviour


{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int level;
    public int currentExp;
    public int expToLevel = 10;
    public float expGrowthMultiplier = 1.2f;
    public Slider expSlider;
    public TMP_Text currentLevelText;

    public static event Action<int> OnLevelUp;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GainExperience(2);
        }
    }

    public void GainExperience(int amount)
    {
        currentExp += amount;
        if(currentExp >= expToLevel )
        {
            LevelUp();
        }
        UpdateUI();
    }
    private void OnEnable()
    {
        Enemy_Health.OnMonsterDefeated += GainExperience;
    }
    private void OnDisable  ()
    {
        Enemy_Health.OnMonsterDefeated -= GainExperience;
    }

    private void LevelUp()
    {
        level++;
        currentExp -= expToLevel;
        expToLevel = Mathf.RoundToInt(expToLevel * expGrowthMultiplier);
        OnLevelUp?.Invoke(1);
    }


    public void UpdateUI()
    {
        expSlider.maxValue = expToLevel;
        expSlider.value = currentExp;
        currentLevelText.text = "Level: " + level; 
    }
}
