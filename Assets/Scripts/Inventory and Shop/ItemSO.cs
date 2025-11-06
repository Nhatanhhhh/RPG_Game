using System.IO;
using UnityEngine;
[CreateAssetMenu(fileName = "New Item")]
public class ItemSO : ScriptableObject
{
    public string ItemName;
    public string ItemDescription;
    public Sprite icon;


    public bool isGold;
    public int stackSize = 3;

    [Header("States")]
    public int currentHealth;
    public int maxHealth;
    public int speed;
    public int damage;

    [Header("For Temporary Items")]
    public float duaration;
}
