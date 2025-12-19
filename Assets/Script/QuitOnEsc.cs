using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitOnEsc : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            // 在 Unity 編輯器中停止播放
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // 在 Build 後的遊戲中關閉程式
            Application.Quit();
#endif
        }
    }
}
