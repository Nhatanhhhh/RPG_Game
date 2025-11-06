using System;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public ItemSO itemSO;
    public SpriteRenderer sr;
    public Animator anim;

    public bool  canBePickUp = true;
    public static event Action<ItemSO, int> OnItemLooted; 
    public int quantity;

    private void OnValidate()
    {
        if (itemSO == null)
            return;
        
            sr.sprite = itemSO.icon;
            this.name = itemSO.ItemName;    
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && canBePickUp==true) {
            anim.Play("Loot_Pickup");
            OnItemLooted?.Invoke(itemSO, quantity);
            Destroy(gameObject, .5f);
        }  
    }

    private void UpdateAppearance()
    {
        sr.sprite = itemSO.icon;
        this.name = itemSO.ItemName;
    }

    public void Initialize(ItemSO itemSO, int quantity)
    {
        this.quantity = quantity;
        this.itemSO = itemSO;
        canBePickUp = false;
        UpdateAppearance();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            canBePickUp = true;
        }
    }
    

}
