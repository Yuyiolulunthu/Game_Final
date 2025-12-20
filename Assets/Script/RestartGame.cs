using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    public void Restart()
    {
        BGMManager.RestartBGM();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}