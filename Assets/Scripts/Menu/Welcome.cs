using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Welcome : MonoBehaviour
{

    public void PlayGame()
    {
        PersistentPlayerManager.instance.SelectRandomPlayer();
        SceneManager.LoadScene("Level");
    }

    
}
