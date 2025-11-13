using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GifFromUrl : MonoBehaviour
{
    [Header("GIF URL")]
    [Tooltip("URL của file GIF")]
    public string gifUrl = "https://raw.githubusercontent.com/Adriano-97/pixelImages/refs/heads/main/BigBanner_02.gif";

    [Header("Component Settings")]
    public RawImage rawImage;

    [Header("Playback Settings")]
    public bool playOnStart = true;
    public bool loop = true;
    [Range(0.1f, 5f)]
    public float speedMultiplier = 1f;

    [Header("Loading Settings")]
    [Tooltip("Hiển thị ảnh placeholder trong khi chờ")]
    public Texture2D placeholderTexture;
    [Tooltip("Chờ preload manager nếu có")]
    public bool waitForPreload = true;

    private List<UniGif.GifTexture> gifTextures;
    private int currentFrame = 0;
    private bool isPlaying = false;
    private bool isLoaded = false;

    void Awake()
    {
        // Auto-detect RawImage
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();
    }

    void Start()
    {
        if (rawImage == null)
        {
            Debug.LogError("Không tìm thấy RawImage component!");
            return;
        }

        // Hiển thị placeholder
        if (placeholderTexture != null)
        {
            rawImage.texture = placeholderTexture;
        }

        StartCoroutine(LoadGifInstant());
    }

    IEnumerator LoadGifInstant()
    {
        // Kiểm tra xem GifPreloadManager có tồn tại không
        if (waitForPreload && GifPreloadManager.Instance != null)
        {
            // Đợi preload hoàn tất (nếu chưa xong)
            while (!GifPreloadManager.Instance.isPreloadComplete)
            {
                Debug.Log($"⏳ Đang chờ preload... {GifPreloadManager.Instance.preloadProgress * 100:F0}%");
                yield return new WaitForSeconds(0.1f);
            }

            // Lấy textures đã preload
            gifTextures = GifPreloadManager.Instance.GetGifTextures(gifUrl);

            if (gifTextures != null && gifTextures.Count > 0)
            {
                isLoaded = true;
                Debug.Log($"⚡ INSTANT LOAD! GIF đã sẵn sàng: {gifTextures.Count} frames");

                if (playOnStart)
                {
                    Play();
                }
                yield break;
            }
            else
            {
                Debug.LogWarning("Preload Manager không có GIF này, fallback sang load thông thường...");
            }
        }

        // Fallback: Load thông thường nếu không có preload
        Debug.LogWarning("Không có GifPreloadManager! Đang load GIF theo cách thông thường (chậm)...");
        Debug.LogWarning("Để load nhanh, hãy thêm GifPreloadManager vào scene đầu tiên!");

        // Load thông thường ở đây (nếu cần)
    }

    public void Play()
    {
        if (!isLoaded)
        {
            Debug.LogWarning("GIF chưa sẵn sàng!");
            return;
        }

        if (gifTextures == null || gifTextures.Count == 0)
        {
            Debug.LogError("Không có frame để play!");
            return;
        }

        if (!isPlaying)
        {
            isPlaying = true;
            StartCoroutine(PlayAnimation());
        }
    }

    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Resume()
    {
        if (isLoaded && !isPlaying)
        {
            isPlaying = true;
            StartCoroutine(PlayAnimation());
        }
    }

    IEnumerator PlayAnimation()
    {
        while (isPlaying)
        {
            if (gifTextures == null || gifTextures.Count == 0)
                yield break;

            var currentGifTexture = gifTextures[currentFrame];

            // Hiển thị frame hiện tại
            if (rawImage != null)
            {
                rawImage.texture = currentGifTexture.m_texture2d;
            }

            // Đợi theo delay của frame
            float delay = currentGifTexture.m_delaySec / speedMultiplier;
            if (delay < 0.01f) delay = 0.1f;

            yield return new WaitForSeconds(delay);

            // Chuyển frame tiếp theo
            currentFrame++;

            if (currentFrame >= gifTextures.Count)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    isPlaying = false;
                    yield break;
                }
            }
        }
    }
}