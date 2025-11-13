using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Singleton Manager để preload và cache tất cả GIF
/// Đặt script này vào scene đầu tiên của game (hoặc persistent scene)
/// </summary>
public class GifPreloadManager : MonoBehaviour
{
    public static GifPreloadManager Instance { get; private set; }

    [Header("Preload Settings")]
    [Tooltip("Danh sách URL GIF cần preload")]
    public List<string> gifUrlsToPreload = new List<string>()
    {
        "https://raw.githubusercontent.com/Adriano-97/pixelImages/refs/heads/main/BigBanner_02.gif"
    };

    [Header("Status")]
    public bool isPreloadComplete = false;
    public float preloadProgress = 0f;

    // Cache lưu trữ GIF data và textures
    private Dictionary<string, byte[]> gifDataCache = new Dictionary<string, byte[]>();
    private Dictionary<string, List<UniGif.GifTexture>> gifTexturesCache = new Dictionary<string, List<UniGif.GifTexture>>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        StartCoroutine(PreloadAllGifs());
    }

    IEnumerator PreloadAllGifs()
    {
        Debug.Log("🚀 Bắt đầu preload GIFs...");

        int totalGifs = gifUrlsToPreload.Count;
        int loadedCount = 0;

        foreach (string gifUrl in gifUrlsToPreload)
        {
            yield return StartCoroutine(PreloadSingleGif(gifUrl));
            loadedCount++;
            preloadProgress = (float)loadedCount / totalGifs;
        }

        isPreloadComplete = true;
        Debug.Log("✅ Preload hoàn tất! Tất cả GIF đã sẵn sàng.");
    }

    IEnumerator PreloadSingleGif(string gifUrl)
    {
        string cacheFilePath = GetCacheFilePath(gifUrl);
        byte[] gifData = null;

        // Kiểm tra cache trên ổ đĩa
        if (File.Exists(cacheFilePath))
        {
            Debug.Log($"⚡ Loading từ disk cache: {gifUrl}");
            gifData = File.ReadAllBytes(cacheFilePath);
        }
        else
        {
            // Download từ URL
            Debug.Log($"📥 Downloading: {gifUrl}");
            UnityWebRequest request = UnityWebRequest.Get(gifUrl);
            request.timeout = 30;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Lỗi download GIF: {request.error}");
                yield break;
            }

            gifData = request.downloadHandler.data;

            // Lưu vào disk cache
            try
            {
                File.WriteAllBytes(cacheFilePath, gifData);
                Debug.Log($"💾 Cached to disk: {cacheFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Không thể cache: {e.Message}");
            }
        }

        // Lưu vào memory cache
        gifDataCache[gifUrl] = gifData;

        // Decode GIF thành textures
        Debug.Log($"🎬 Decoding GIF: {gifUrl}");
        bool decodeDone = false;

        yield return StartCoroutine(
            UniGif.GetTextureListCoroutine(
                gifData,
                (texList, loopCount, width, height) =>
                {
                    gifTexturesCache[gifUrl] = texList;
                    decodeDone = true;
                    Debug.Log($"✓ Decoded: {texList.Count} frames, {width}x{height}px");
                },
                FilterMode.Point,
                TextureWrapMode.Clamp
            )
        );

        // Đợi decode hoàn tất
        while (!decodeDone)
        {
            yield return null;
        }
    }

    // Public API để các script khác lấy GIF data
    public byte[] GetGifData(string gifUrl)
    {
        if (gifDataCache.ContainsKey(gifUrl))
        {
            return gifDataCache[gifUrl];
        }
        return null;
    }

    public List<UniGif.GifTexture> GetGifTextures(string gifUrl)
    {
        if (gifTexturesCache.ContainsKey(gifUrl))
        {
            return gifTexturesCache[gifUrl];
        }
        return null;
    }

    public bool IsGifReady(string gifUrl)
    {
        return gifTexturesCache.ContainsKey(gifUrl);
    }

    private string GetCacheFilePath(string url)
    {
        string hash = GetUrlHash(url);
        return Path.Combine(Application.persistentDataPath, $"GifCache_{hash}.gif");
    }

    private string GetUrlHash(string url)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url));
            return System.BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
        }
    }

    // Clear cache nếu cần
    public void ClearAllCache()
    {
        foreach (string url in gifUrlsToPreload)
        {
            string cacheFilePath = GetCacheFilePath(url);
            if (File.Exists(cacheFilePath))
            {
                File.Delete(cacheFilePath);
            }
        }

        gifDataCache.Clear();

        // Cleanup textures
        foreach (var kvp in gifTexturesCache)
        {
            foreach (var gifTex in kvp.Value)
            {
                if (gifTex.m_texture2d != null)
                {
                    Destroy(gifTex.m_texture2d);
                }
            }
        }
        gifTexturesCache.Clear();

        Debug.Log("🗑️ Đã xóa tất cả cache");
    }
}