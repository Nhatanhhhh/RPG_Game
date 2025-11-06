using UnityEngine;
using System.Collections;
public class UseItem : MonoBehaviour
{
    public void ApplyItemEffects(ItemSO itemSO)
    {
        if(itemSO.currentHealth > 0)
        {
             StatsManager.Instance.UpdateHealth(itemSO.currentHealth); 
        }
        if (itemSO.maxHealth > 0)
        {
            StatsManager.Instance.UpdateMaxHealth(itemSO.maxHealth);
        }
        if (itemSO.speed > 0)
        {
            StatsManager.Instance.UpdateSpeed(itemSO.speed);
        }
        if (itemSO.duaration > 0)
        {
            StartCoroutine(EffectTimer(itemSO, itemSO.duaration));
        }
    }
    private IEnumerator EffectTimer(ItemSO itemSO, float duaration)
    {
        yield return new WaitForSeconds(duaration);

        if (itemSO.currentHealth > 0)
        {
            StatsManager.Instance.UpdateHealth(-itemSO.currentHealth);
        }
        if (itemSO.maxHealth > 0)
        {
            StatsManager.Instance.UpdateMaxHealth(-itemSO.maxHealth);
        }
        if (itemSO.speed > 0)
        {
            StatsManager.Instance.UpdateSpeed(-itemSO.speed);
        }

    }

}
