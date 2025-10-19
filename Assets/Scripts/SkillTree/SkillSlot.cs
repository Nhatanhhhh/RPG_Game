using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SkillSlot : MonoBehaviour
{
    public List<SkillSlot> prerequisiteSkillSlots;
    public SkillSO skillSO;

    public int currentLevel;
    public bool isUnlocked;

    public Image skillIcon;
    public TMP_Text LevelText;

    public Button skillButton;


    public static event Action<SkillSlot> OnAbilityPointSpent;
    public static event Action<SkillSlot> OnSkillMaxed;
    private void OnValidate()
    {
        if (skillSO != null && LevelText != null)
        {
            UpdateUI();
        }
    }

    public void TryUpgradeSkill()
    {
        if (isUnlocked && currentLevel < skillSO.maxLevel)
        {
            currentLevel++;
            OnAbilityPointSpent?.Invoke(this);
            if (currentLevel >= skillSO.maxLevel)
            {
                OnSkillMaxed?.Invoke(this);
            }

            UpdateUI();
        }

    }

    private void UpdateUI()
    {
        skillIcon.sprite = skillSO.skillIcon;

        if (isUnlocked)
        {
            skillButton.interactable = true;
            LevelText.text = currentLevel.ToString() + "/" + skillSO.maxLevel.ToString();
            skillIcon.color = Color.white;
        }
        else
        {
            skillButton.interactable = false;
            LevelText.text = "Locked";
            skillIcon.color = Color.grey;
        }
    }

    public bool CanUnlockSkill()
    {
        foreach (SkillSlot slot in prerequisiteSkillSlots)
        {
            if(!slot.isUnlocked || slot.currentLevel < slot.skillSO.maxLevel)
            {
                return false;
            }
        }
        return true;
    }

    public void Unlock()
    {
        isUnlocked = true;
        UpdateUI();
    }

}