using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonControl : MonoBehaviour
{
    /* ======================
     * 關閉遊戲
     * ====================== */
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 在 Unity Editor 中停止播放
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在正式 Build 中關閉遊戲
        Application.Quit();
#endif
    }

    /* ======================
     * 回到指定場景
     * ====================== */
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void Restart()
    {
        BGMManager.RestartBGM();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
