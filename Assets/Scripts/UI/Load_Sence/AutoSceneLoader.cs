using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Tự động load scene tiếp theo khi preload hoàn tất
/// Attach vào GameObject trong PreloadScene
/// </summary>
public class AutoSceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Tên scene cần load (VD: Main_Menu)")]
    public string targetSceneName = "Main_Menu";

    [Header("Delay Settings")]
    [Tooltip("Đợi bao lâu sau khi preload xong (giây)")]
    public float delayAfterPreload = 0.5f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    void Start()
    {
        StartCoroutine(WaitAndLoadScene());
    }

    IEnumerator WaitAndLoadScene()
    {
        if (showDebugLogs)
        {
            Debug.Log("🎮 AutoSceneLoader started");
        }

        // Đợi GifPreloadManager khởi tạo
        while (GifPreloadManager.Instance == null)
        {
            yield return null;
        }

        if (showDebugLogs)
        {
            Debug.Log("⏳ Đang chờ preload hoàn tất...");
        }

        // Đợi preload hoàn tất
        while (!GifPreloadManager.Instance.isPreloadComplete)
        {
            yield return null;
        }

        if (showDebugLogs)
        {
            Debug.Log($"✅ Preload hoàn tất! Đợi {delayAfterPreload}s trước khi chuyển scene...");
        }

        // Đợi thêm một chút (để loading screen fade out)
        yield return new WaitForSeconds(delayAfterPreload);

        // Kiểm tra scene có tồn tại trong Build Settings không
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneName == targetSceneName)
            {
                sceneExists = true;
                break;
            }
        }

        if (!sceneExists)
        {
            Debug.LogError($"❌ Scene '{targetSceneName}' không tồn tại trong Build Settings!");
            Debug.LogError("Vui lòng thêm scene vào: File → Build Settings → Add Open Scenes");
            yield break;
        }

        // Load scene chính
        if (showDebugLogs)
        {
            Debug.Log($"🎮 Loading scene: {targetSceneName}");
        }

        SceneManager.LoadScene(targetSceneName);
    }

    // Hiển thị danh sách scenes trong Build Settings
    [ContextMenu("Show Available Scenes")]
    void ShowAvailableScenes()
    {
        Debug.Log("=== Scenes trong Build Settings ===");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"{i}: {sceneName}");
        }
    }
}