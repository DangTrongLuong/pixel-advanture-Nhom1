using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    public string sceneName;

    public void LoadThisLevel()
    {
        PersistentPlayerManager.instance.SelectRandomPlayer();
        SceneManager.LoadScene(sceneName); 
    }
}
