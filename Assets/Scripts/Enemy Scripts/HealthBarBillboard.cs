using UnityEngine;

public class HealthBarBillboard : MonoBehaviour
{
    private Transform parentTransform;
    private Vector3 initialScale;

    void Awake()
    {
        initialScale = transform.localScale;

        parentTransform = transform.parent;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;


        if (parentTransform != null)
        {
            if (parentTransform.lossyScale.x < 0)
            {
                transform.localScale = new Vector3(-initialScale.x, initialScale.y, initialScale.z);
            }
            else
            {
                transform.localScale = initialScale;
            }
        }
    }
}