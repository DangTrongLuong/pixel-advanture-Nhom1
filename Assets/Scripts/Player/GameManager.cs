using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject RandomPlayer()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        if (allPlayers.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy player nào với tag 'Player'");
            return null;
        }

        int randomIndex = Random.Range(0, allPlayers.Length);
        GameObject selectedPlayer = allPlayers[randomIndex];

        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (i == randomIndex)
                allPlayers[i].SetActive(true);
            else
                allPlayers[i].SetActive(false);
        }

        return selectedPlayer;
    }
}