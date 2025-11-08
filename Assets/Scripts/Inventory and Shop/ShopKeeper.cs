using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    public static ShopKeeper currentShopKeeper;
    public Animator anim;
    public CanvasGroup shopCanvasGroup;
    public ShopManager shopManager;

    [SerializeField] private List<ShopItems> shopItems;
    [SerializeField] private List<ShopItems> shopWeapon;
    [SerializeField] private List<ShopItems> shopArmour;

    public static event Action<ShopManager, bool> OnShopStateChanged;
    private bool PlayerInRange;
    private bool isShopOpen;

    void Update()
    {
        if (PlayerInRange)
        {
            if (Input.GetButtonDown("Interact"))
            {
                if (!isShopOpen)
                {
                    Time.timeScale = 0;
                    isShopOpen = true;
                    OnShopStateChanged?.Invoke(shopManager, true);
                    shopCanvasGroup.alpha = 1;
                    shopCanvasGroup.blocksRaycasts = true;
                    shopCanvasGroup.interactable = true;
                    OpenItemShop();
                } 
            }
            else if (Input.GetButtonDown("Cancel"))
                {
                Time.timeScale = 1;
                isShopOpen = false;
                currentShopKeeper = null;
                OnShopStateChanged?.Invoke(shopManager, false);
                shopCanvasGroup.alpha = 0;
                shopCanvasGroup.blocksRaycasts = false;
                shopCanvasGroup.interactable = false;
            }
            
        }
    }

    public void OpenItemShop()
    {
        shopManager.PopulateShopItems(shopItems);
    }

    public void OpenWeapenShop()
    {
        shopManager.PopulateShopItems(shopWeapon);
    }

    public void OpenArmourShop()
    {
        shopManager.PopulateShopItems(shopArmour);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            anim.SetBool("PlayerInRange", true);
            PlayerInRange = true;
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            anim.SetBool("PlayerInRange", false);
            PlayerInRange = false;
        }
    }


}
