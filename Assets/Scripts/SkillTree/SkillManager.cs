//using System.Diagnostics;
//using UnityEngine;

//public class SkillManager : MonoBehaviour
//{
//    private void OnEnable()
//    {
//        SkillSlot.OnAbilityPointSpend += HandleAbilityPointSpend;
//    }
//    private void OnEnable()
//    {
//        SkillSlot.OnAbilityPointSpend -= HandleAbilityPointSpend;
//    }

//    private void HandleAbilityPointSpend(SkillSlot slot)
//    {
//        string skillName = slot.skillSO.skillName;

//        swith(skillName)
//        {
//            case "MaxHealthBoost":
//                StatsManager.Instance.UpdateMaxHealth(1);
//                break;
//            default:
//                Debug.LogWarning("Unknow Skill : " + skillName);
//                break;



//        }
//    }
//}

using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;
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

    private void OnEnable()
    {

        SkillSlot.OnAbilityPointSpent += HandleAbilityPointSpend;
    }

    private void OnDisable()
    {
        
        SkillSlot.OnAbilityPointSpent -= HandleAbilityPointSpend;
    }

    private void HandleAbilityPointSpend(SkillSlot slot)
    {
        string skillName = slot.skillSO.SkillName;

        switch (skillName)
        {
            case "MaxHealthBoost":
                StatsManager.Instance.UpdateMaxHealth(1);
                break;

            case "DameBoost":
                StatsManager.Instance.UpdateMaxDame(1);
                Debug.Log("MaxDame: " + StatsManager.Instance.damage);
                break;

            case "MoveSpeed":
                StatsManager.Instance.UpdateMaxSpeed(1);
                break;

            default:
                Debug.LogWarning("Unknown Skill: " + skillName);
                break;
        }
    }
}
