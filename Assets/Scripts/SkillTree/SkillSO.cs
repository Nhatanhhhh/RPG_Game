using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "NewSkill", menuName = "SkillTree/Skill")]
public class SkillSO : ScriptableObject
{
    public string SkillName;
    public int maxLevel;
    public Sprite skillIcon;
        
}
