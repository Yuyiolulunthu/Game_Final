using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class GoalTriggerFloatTarget : MonoBehaviour
{
    [Header("Detect")]
    public string playerTag = "Player";

    [Header("Target To Float")]
    public FloatUpOnGoalTouch target;

    [Header("Progress")]
    [Tooltip("這一關的 index（第 0 關填 0，第 1 關填 1）")]
    public int setToIndex = 0;

    [Tooltip("最大 index（柱子數量）")]
    public int maxIndex = 6;

    [Header("Scene")]
    public string backToScene = "Stage-select";

    [Header("Delay")]
    public float delayBeforeReturn = 3f;

    private bool triggered = false;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;
        if (target == null) return;

        triggered = true;

        Debug.Log("[GoalTrigger] Player reached goal");

        target.TriggerByGoal();
        StartCoroutine(CompleteAfterDelay());
    }

    IEnumerator CompleteAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeReturn);

        // ⭐ 關卡完成狀態（.5）
        float targetProgress = setToIndex + 0.5f;

        LevelProgressManager.SetTo(targetProgress, maxIndex);

        SceneManager.LoadScene(backToScene);
    }
}
