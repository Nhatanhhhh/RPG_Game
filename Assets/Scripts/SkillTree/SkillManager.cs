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

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
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
                Debug.LogWarning("MaxDame: " + StatsManager.Instance.damage);
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
