using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarStepDown : MonoBehaviour
{
    [Header("Pillars（順序 = index）")]
    public List<Transform> pillars;

    [Header("動畫設定")]
    public float moveDistance = 2f;
    public float moveSpeed = 2f;

    // currentIndex = 目前「已升起」的柱子數量
    // 下限 = 1
    private int currentIndex = 1;
    private bool isMoving = false;

    /* ======================
     * Start：全部在地底 → 升到 progress
     * ====================== */
    void Start()
    {
        float progress = LevelProgressManager.Get(); // >= 1
        int targetIndex = Mathf.Clamp(Mathf.CeilToInt(progress), 1, pillars.Count);

        Debug.Log($"[Pillar] Enter Stage Select, targetIndex = {targetIndex}");

        // ⭐ 保證所有柱子都在地底（設計基準）
        // 這裡「不動位置」，假設你已經在場景中擺好地底狀態

        // ⭐ 播放進場升起動畫
        StartCoroutine(PlayEnterRise(targetIndex));
    }

    IEnumerator PlayEnterRise(int targetIndex)
    {
        isMoving = true;

        for (int i = 0; i < targetIndex; i++)
        {
            if (pillars[i] == null) continue;

            yield return MovePillar(pillars[i], Vector3.up);
        }

        currentIndex = targetIndex;
        isMoving = false;

        Debug.Log($"[Pillar] Enter animation finished, currentIndex = {currentIndex}");
    }

    /* ======================
     * Debug 操作
     * ====================== */
    void Update()
    {
        if (isMoving) return;

        // + 再多升一根
        if (Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Equals))
            StepUpOne();

        // - 降一根
        if (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus))
            StepDownOne();

        // 0 全部降回地底
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
            ResetAll();
    }

    /* ======================
     * 升 / 降 操作
     * ====================== */

    public void StepUpOne()
    {
        if (currentIndex >= pillars.Count)
            return;

        StartCoroutine(MovePillar(pillars[currentIndex], Vector3.up));
        currentIndex++;

        SaveProgress();
    }

    public void StepDownOne()
    {
        if (currentIndex <= 1)
            return;

        currentIndex--;
        StartCoroutine(MovePillar(pillars[currentIndex], Vector3.down));

        SaveProgress();
    }

    public void ResetAll()
    {
        StopAllCoroutines();
        StartCoroutine(ResetCoroutine());
    }

    IEnumerator ResetCoroutine()
    {
        isMoving = true;

        // 全部降回地底
        for (int i = currentIndex - 1; i > 0; i--)
        {
            if (pillars[i] == null) continue;
            yield return MovePillar(pillars[i], Vector3.down);
        }

        currentIndex = 1;
        LevelProgressManager.Reset();

        isMoving = false;
        Debug.Log("[Pillar] Reset to underground");
    }

    /* ======================
     * 單根動畫
     * ====================== */

    IEnumerator MovePillar(Transform pillar, Vector3 dir)
    {
        Vector3 start = pillar.position;
        Vector3 target = start + dir * moveDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            pillar.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        pillar.position = target;
    }

    /* ======================
     * 存檔
     * ====================== */

    void SaveProgress()
    {
        LevelProgressManager.ForceSet(currentIndex);
        Debug.Log($"[Pillar] Save progress = {currentIndex}");
    }
}
