//using UnityEngine;

//public class PersistentObject : MonoBehaviour
//{
//    public string objectID;

//    private void Awake()
//    {
//        this.gameObject.name = objectID;

//        GameObject[] otherInstances = GameObject.FindGameObjectsWithTag(this.gameObject.tag);

//        foreach (GameObject obj in otherInstances)
//        {
//            if (obj != this.gameObject && obj.name == this.objectID)
//            {
//                Debug.Log($"Đã có {objectID}. Hủy bản sao này.");
//                Destroy(this.gameObject);
//                return; 
//            }
//        }

//        DontDestroyOnLoad(this.gameObject);
//        Debug.Log($"Giữ lại bản gốc: {objectID}");
//    }
//}


using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private static PersistentObject instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log($"Đã có {name} tồn tại. Hủy bản sao này.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"Giữ lại bản gốc: {name}");
    }
}
