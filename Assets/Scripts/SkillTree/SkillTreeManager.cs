using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager Instance;
    public SkillSlot[] skillSlots;
    public TMP_Text pointsText;
    public int availablePoints;

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

    private void Start()
    {
        foreach(SkillSlot slot in skillSlots)
        {
            slot.skillButton.onClick.AddListener(() => CheckAvailablePoints(slot));
            
        }
        UpdateAbilityPoints(0);
    }

    private void CheckAvailablePoints(SkillSlot skillSlot)
    {
        if (availablePoints > 0)
        {
            skillSlot.TryUpgradeSkill();
        }
    }

    private void OnEnable()
    {
        SkillSlot.OnAbilityPointSpent += HandleAbilityPointsSpent;
        SkillSlot.OnSkillMaxed += HandleSkillMaxed;
        ExeManager.OnLevelUp += UpdateAbilityPoints;
    
    }

    private void OnDisable()
    {
        SkillSlot.OnAbilityPointSpent -= HandleAbilityPointsSpent;
        SkillSlot.OnSkillMaxed -= HandleSkillMaxed;
        ExeManager.OnLevelUp -= UpdateAbilityPoints;
    }

    private void HandleAbilityPointsSpent(SkillSlot skillSlot)
    {
        if (availablePoints > 0) 
        {
            UpdateAbilityPoints(-1);
        }

    }

    private void HandleSkillMaxed(SkillSlot skillSlot)
    {
        foreach(SkillSlot slot in skillSlots)
        {
            if (!slot.isUnlocked && slot.CanUnlockSkill())
            {
                slot.Unlock();
            }
        }    
    }

    public void UpdateAbilityPoints(int amount)
    {
        availablePoints += amount;
        pointsText.text = "Point: " + availablePoints;
    }
}
