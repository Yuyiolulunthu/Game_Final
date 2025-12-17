using UnityEngine;

public class ResetHints : MonoBehaviour
{
    void Update()
    {
        // 按 R 鍵清除所有提示記錄
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("All hints reset!");
        }
    }
}