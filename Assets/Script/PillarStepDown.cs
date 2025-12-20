using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarStepDown : MonoBehaviour
{
    public List<Transform> pillars;
    public float dropDistance = 2f;
    public float dropSpeed = 2f;

    private int currentIndex;
    private bool isMoving = false;

    private const string SAVE_KEY = "LevelIndex";
    public PillarStepDown pillarStepDown;

    void Start()
    {
        // 讀取存檔
        currentIndex = PlayerPrefs.GetInt(SAVE_KEY, 0);

        // 啟動時直接把已完成的柱子移下去（不播動畫）
        for (int i = 0; i < currentIndex && i < pillars.Count; i++)
        {
            pillars[i].position += Vector3.down * dropDistance;
        }
    }
    private void Update()
        {
            // 數字鍵盤 +（Numpad +）
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                pillarStepDown.CompleteLevel();
                Debug.Log("DEBUG: Index +1");
            }
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                pillarStepDown.ResetProgress();
                Debug.Log("DEBUG: Reset");
            }
        }

    /// <summary>
    /// 過關時呼叫
    /// </summary>
    public void CompleteLevel()
    {
        if (isMoving) return;
        if (currentIndex >= pillars.Count) return;

        StartCoroutine(DropPillar(pillars[currentIndex]));
        currentIndex++;

        // ⭐ 存檔
        PlayerPrefs.SetInt(SAVE_KEY, currentIndex);
        PlayerPrefs.Save();
    }

    IEnumerator DropPillar(Transform pillar)
    {
        isMoving = true;

        Vector3 startPos = pillar.position;
        Vector3 targetPos = startPos + Vector3.down * dropDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * dropSpeed;
            pillar.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        pillar.position = targetPos;
        isMoving = false;
    }

    /// <summary>
    /// （測試用）重置進度
    /// </summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }
}
