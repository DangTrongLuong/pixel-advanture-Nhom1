using UnityEngine;
using System.Collections.Generic;

public class PersistentPlayerManager : MonoBehaviour
{
    public static PersistentPlayerManager instance;
    private List<PlayerData> players = new List<PlayerData>();
    private GameObject selectedPlayer;

    [System.Serializable]
    private class PlayerData
    {
        public GameObject obj;
        public SpriteRenderer sr;
        public Collider2D col;
        public Rigidbody2D rb;
    }

    private void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        CachePlayers();
    }

    // -------------------------------------------------------------------------
    private void CachePlayers()
    {
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
        players.Clear();
        foreach (var p in foundPlayers)
        {
            players.Add(new PlayerData
            {
                obj = p,
                sr = p.GetComponent<SpriteRenderer>(),
                col = p.GetComponent<Collider2D>(),
                rb = p.GetComponent<Rigidbody2D>()
            });
        }
    }

    // -------------------------------------------------------------------------
    public void SelectRandomPlayer()
    {
        if (players.Count == 0)
        {
            Debug.LogWarning("Không có player nào trong list!");
            return;
        }

        int randomIndex = Random.Range(0, players.Count);
        selectedPlayer = players[randomIndex].obj;

        ApplyActiveState(randomIndex);

        Debug.Log("Chọn player: " + selectedPlayer.name);
    }

    // -------------------------------------------------------------------------
    private void ApplyActiveState(int activeIndex)
    {
        for (int i = 0; i < players.Count; i++)
        {
            bool active = (i == activeIndex);

            if (players[i].sr) players[i].sr.enabled = active;
            if (players[i].col) players[i].col.enabled = active;
            if (players[i].rb)
                players[i].rb.bodyType = active ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        }
    }

    // -------------------------------------------------------------------------
    public GameObject GetSelectedPlayer()
    {
        return selectedPlayer;
    }

    public void ResetSelection()
    {
        selectedPlayer = null;
        Debug.Log("Đã reset player selection → sẽ random lại sau khi scene load.");
    }

    // -------------------------------------------------------------------------
    public void UpdatePlayersForNewScene()
    {
        CachePlayers();

        if (selectedPlayer == null)
        {
            Debug.Log("Scene mới - chưa có player, random mới");
            SelectRandomPlayer();
            return;
        }

        PlayerData selectedData = players.Find(p => p.obj.name == selectedPlayer.name);

        if (selectedData != null)
        {
            selectedPlayer = selectedData.obj;

            int index = players.FindIndex(p => p.obj == selectedPlayer);
            ApplyActiveState(index);

            Debug.Log("Giữ nguyên player: " + selectedPlayer.name);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy player đã chọn ở scene này, random player mới");
            SelectRandomPlayer();
        }
    }
}