//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;
//using System;

//public class GoldManager : MonoBehaviour
//{
//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Return))
//        {
//            GainGold(2);
//        }
//    }
//    public TMP_Text goldtext;
//    public int currentGold;


//    public void GainGold(int amount)
//    {
//        currentGold += amount;
//        UpdateUI();
//    }

//    private void OnEnable()
//    {
//        Enemy_Health.OnMonsterDefeated += GainGold;
//    }
//    private void OnDisable()
//    {
//        Enemy_Health.OnMonsterDefeated -= GainGold;
//    }
//    public void UpdateUI()
//    {
//        AmountText.text = "Level: ";
//    }
//}
