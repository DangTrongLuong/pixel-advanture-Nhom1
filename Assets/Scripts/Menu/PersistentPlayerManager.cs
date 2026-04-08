using UnityEngine;
using System.Collections.Generic;

public class PersistentPlayerManager : MonoBehaviour
{
    public static PersistentPlayerManager instance;

    [Header("Players")]
    [SerializeField] private List<GameObject> playerPrefabs = new List<GameObject>();

    private GameObject selectedPrefab;

    private GameObject currentInstance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SelectRandomPlayer()
    {
        if (playerPrefabs == null || playerPrefabs.Count == 0)
        {
            Debug.LogError("[PPM] Chưa gán prefab nào vào playerPrefabs!");
            return;
        }

        int index = Random.Range(0, playerPrefabs.Count);
        selectedPrefab = playerPrefabs[index];
        Debug.Log("[PPM] Đã chọn prefab: " + selectedPrefab.name);
    }

    public void UpdatePlayersForNewScene()
    {
        if (selectedPrefab == null)
        {
            Debug.LogWarning("[PPM] Chưa có prefab nào được chọn, tự chọn ngẫu nhiên.");
            SelectRandomPlayer();
            if (selectedPrefab == null) return;
        }

        GameObject spawnPointObj = GameObject.FindGameObjectWithTag("SpawnPoint");
        Vector3 spawnPos = spawnPointObj != null
            ? spawnPointObj.transform.position
            : Vector3.zero;

        if (currentInstance != null)
            Destroy(currentInstance);

        currentInstance = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        Debug.Log("[PPM] Đã spawn: " + currentInstance.name + " tại " + spawnPos);
    }

    public PlayerController GetSelectedController()
    {
        if (currentInstance == null) return null;
        return currentInstance.GetComponent<PlayerController>();
    }

    public void ResetSelection()
    {
        selectedPrefab = null;
        currentInstance = null;
    }
}