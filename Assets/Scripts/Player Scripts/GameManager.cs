using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string nextSpawnPointID;
    public HashSet<string> activatedSceneObjects = new HashSet<string>();
    public HashSet<string> defeatedEnemies = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(nextSpawnPointID))
        {
            return;
        }

        // GameManager "quan tâm" đến Player bằng cách gọi Instance của nó
        Transform playerTransform = PlayerMovement.Instance.transform;
        if (playerTransform == null)
        {
            Debug.LogError("GameManager: Không tìm thấy PlayerMovement.Instance!");
            return;
        }

        PlayerSpawnPoint[] allSpawnPoints = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
        Transform targetSpawn = null;

        foreach (PlayerSpawnPoint spawnPoint in allSpawnPoints)
        {
            if (spawnPoint.spawnPointID == nextSpawnPointID)
            {
                targetSpawn = spawnPoint.transform;
                break;
            }
        }

        if (targetSpawn != null)
        {
            playerTransform.position = targetSpawn.position;
        }
        else
        {
            Debug.LogWarning("GameManager: Không tìm thấy Spawn Point ID: " + nextSpawnPointID);
        }

        nextSpawnPointID = null;
    }
}