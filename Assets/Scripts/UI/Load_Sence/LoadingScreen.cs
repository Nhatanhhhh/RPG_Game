using UnityEngine;
using UnityEngine.UI;
using TMPro; // Thêm namespace cho TextMeshPro
using System.Collections;

/// <summary>
/// Loading screen hiển thị progress khi preload GIFs
/// Attach vào Canvas trong scene đầu tiên
/// Hỗ trợ cả Text và TextMeshPro
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingPanel;
    public Slider progressBar;

    [Header("Text (Chọn 1 trong 2 loại)")]
    [Tooltip("Dùng cho Text cũ (legacy)")]
    public Text progressText;
    public Text statusText;

    [Tooltip("Dùng cho TextMeshPro")]
    public TMP_Text progressTextTMP;
    public TMP_Text statusTextTMP;

    [Header("Settings")]
    public bool autoHideWhenComplete = true;
    public float fadeOutDuration = 0.5f;

    void Start()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        StartCoroutine(MonitorPreloadProgress());
    }

    IEnumerator MonitorPreloadProgress()
    {
        // Đợi GifPreloadManager khởi tạo
        while (GifPreloadManager.Instance == null)
        {
            yield return null;
        }

        // Cập nhật progress
        while (!GifPreloadManager.Instance.isPreloadComplete)
        {
            float progress = GifPreloadManager.Instance.preloadProgress;

            // Update progress bar
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            // Update progress text (hỗ trợ cả 2 loại)
            string progressString = $"{progress * 100:F0}%";
            if (progressText != null)
            {
                progressText.text = progressString;
            }
            if (progressTextTMP != null)
            {
                progressTextTMP.text = progressString;
            }

            // Update status text
            string statusString = "Loading Assets...";
            if (statusText != null)
            {
                statusText.text = statusString;
            }
            if (statusTextTMP != null)
            {
                statusTextTMP.text = statusString;
            }

            yield return null;
        }

        // Hoàn tất
        if (progressBar != null)
        {
            progressBar.value = 1f;
        }

        if (progressText != null)
        {
            progressText.text = "100%";
        }
        if (progressTextTMP != null)
        {
            progressTextTMP.text = "100%";
        }

        if (statusText != null)
        {
            statusText.text = "Complete!";
        }
        if (statusTextTMP != null)
        {
            statusTextTMP.text = "Complete!";
        }

        // Đợi một chút rồi ẩn
        yield return new WaitForSeconds(0.3f);

        if (autoHideWhenComplete)
        {
            yield return StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        CanvasGroup canvasGroup = loadingPanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = loadingPanel.AddComponent<CanvasGroup>();
        }

        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
            yield return null;
        }

        loadingPanel.SetActive(false);
    }
}