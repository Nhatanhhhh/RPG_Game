using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] itemSlots;
    public UseItem useItem;
    public int gold;
    public TMP_Text goldText;
    public GameObject lootPrelab;
    public Transform player;
    public static InventoryManager Instance;

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
        foreach (var slot in itemSlots)
        {
            slot.UpdateUI();
        }
    }


    private void OnEnable()
    {
        Loot.OnItemLooted += AddItem;
    }
    private void OnDisable()
    {
        Loot.OnItemLooted -= AddItem;
    }

    public void AddItem(ItemSO itemSo, int quantity)
    {
        if (itemSo.isGold)
        {
            gold += quantity;
            goldText.text = gold.ToString();
            return;
        }
        foreach (var slot in itemSlots)
        {
            if (slot.itemSO == itemSo && slot.quantity < itemSo.stackSize)
            {
                int availableSpace = itemSo.stackSize - slot.quantity;
                int amountToAdd = Mathf.Min(availableSpace, quantity);

                slot.quantity += amountToAdd;
                quantity -= amountToAdd;
                slot.UpdateUI();
                if (quantity <= 0)
                {
                    return;
                }
            }
        }
        foreach (var slot in itemSlots) //if item remain we will look at the empty slot
        {
            if (slot.itemSO == null)
            {
                int amountToAdd = Mathf.Min(itemSo.stackSize, quantity);
                slot.itemSO = itemSo;
                slot.quantity = quantity;
                slot.UpdateUI();
                return;
            }
        }
        if (quantity > 0)
        {
            DropLoot(itemSo, quantity);
        }

    }
    public void DropItem(InventorySlot slot)
    {
        DropLoot(slot.itemSO, 1);
        slot.quantity--;
        if (slot.quantity <= 0)
        {
            slot.itemSO = null;
        }
        slot.UpdateUI();
    }


    private void DropLoot(ItemSO itemSO, int quantity)
    {
        Loot loot = Instantiate(lootPrelab, player.position, Quaternion.identity).GetComponent<Loot>();
        loot.Initialize(itemSO, quantity);
    }

    public void UseItem(InventorySlot slot)
    {
        if (slot.itemSO != null && slot.quantity >= 0)
        {
            useItem.ApplyItemEffects(slot.itemSO);
            slot.quantity--;
            if (slot.quantity <= 0)
            {
                slot.itemSO = null;
            }
            slot.UpdateUI();
        }
    }
}