using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarStepDown : MonoBehaviour
{
    public List<Transform> pillars;
    public float moveDistance = 2f;
    public float moveSpeed = 2f;

    private int currentIndex;
    private bool isMoving = false;

    private const string SAVE_KEY = "LevelIndex";

    void Start()
    {
        currentIndex = PlayerPrefs.GetInt(SAVE_KEY, 0);
        currentIndex = Mathf.Clamp(currentIndex, 0, pillars.Count);

        // 啟動時直接套用狀態（不播動畫）
        for (int i = 0; i < currentIndex; i++)
        {
            if (pillars[i] != null)
                pillars[i].position += Vector3.down * moveDistance;
        }
    }

    void Update()
    {
        if (isMoving) return;

        // + 降
        if (Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Equals))
        {
            StepDown();
        }

        // - 升
        if (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus))
        {
            StepUp();
        }

        // 0 重置
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetAll();
        }
    }

    /// <summary>
    /// index +1，柱子下降
    /// </summary>
    void StepDown()
    {
        if (currentIndex >= pillars.Count) return;

        StartCoroutine(MovePillar(
            pillars[currentIndex],
            Vector3.down * moveDistance
        ));

        currentIndex++;
        SaveIndex();
    }

    /// <summary>
    /// index -1，柱子上升
    /// </summary>
    void StepUp()
    {
        if (currentIndex <= 0) return;

        currentIndex--;

        StartCoroutine(MovePillar(
            pillars[currentIndex],
            Vector3.up * moveDistance
        ));

        SaveIndex();
    }

    /// <summary>
    /// 全部重置（播動畫）
    /// </summary>
    void ResetAll()
    {
        StopAllCoroutines();
        StartCoroutine(ResetCoroutine());
    }

    IEnumerator ResetCoroutine()
    {
        isMoving = true;

        for (int i = currentIndex - 1; i >= 0; i--)
        {
            if (pillars[i] != null)
            {
                yield return StartCoroutine(
                    MovePillar(pillars[i], Vector3.up * moveDistance)
                );
            }
        }

        currentIndex = 0;
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();

        isMoving = false;
        Debug.Log("DEBUG: Reset All");
    }

    IEnumerator MovePillar(Transform pillar, Vector3 offset)
    {
        if (pillar == null) yield break;

        isMoving = true;

        Vector3 startPos = pillar.position;
        Vector3 targetPos = startPos + offset;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            pillar.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        pillar.position = targetPos;
        isMoving = false;
    }

    void SaveIndex()
    {
        PlayerPrefs.SetInt(SAVE_KEY, currentIndex);
        PlayerPrefs.Save();
        Debug.Log($"DEBUG: Index = {currentIndex}");
    }
}
