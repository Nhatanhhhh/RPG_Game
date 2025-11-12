using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // This function is called from a Button
    public void LoadSceneByName(string sceneName)
    {
        // Ensure Time.timeScale = 1 when changing scenes (if paused)
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Game has quit!");
        Application.Quit();
    }
}