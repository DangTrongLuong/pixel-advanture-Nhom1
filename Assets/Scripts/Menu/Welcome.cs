using UnityEngine;
using UnityEngine.SceneManagement;

public class Welcome : MonoBehaviour
{
    public void PlayGame()
    {
        PersistentPlayerManager.instance.SelectRandomPlayer();
        SceneManager.LoadScene("Map1");
    }
    
}
