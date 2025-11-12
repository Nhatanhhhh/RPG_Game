using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string sceneToLoad;
    public string targetSpawnPointID;

    private bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isLoading)
        {
            isLoading = true;
            LoadTargetScene();
        }
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            isLoading = false;
            return;
        }

        GameManager.Instance.nextSpawnPointID = this.targetSpawnPointID;

        SceneManager.LoadScene(sceneToLoad);
    }
}