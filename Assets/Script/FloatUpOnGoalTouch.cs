using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatUpOnGoalTouch : MonoBehaviour
{
    [Header("Trigger Condition")]
    [Tooltip("觸發目標物件名稱（預設 Goal）")]
    public string goalObjectName = "Goal";

    [Tooltip("玩家 Tag（預設 Player）")]
    public string playerTag = "Player";

    [Header("Float Up Motion")]
    [Tooltip("向上浮起的高度")]
    public float floatHeight = 1.5f;

    [Tooltip("浮起所需時間（秒）")]
    public float floatDuration = 1.0f;

    [Tooltip("浮起動畫曲線（控制快慢）")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("VFX")]
    [Tooltip("觸發時播放的特效 Prefab（ParticleSystem）")]
    public ParticleSystem vfxPrefab;

    [Tooltip("特效生成位置（可留空，預設物件本身）")]
    public Transform vfxSpawnPoint;

    [Tooltip("特效是否跟隨此物件")]
    public bool vfxFollowThisObject = true;

    [Header("Optional: Fade Out Then Disable")]
    [Tooltip("浮起完成後是否淡出並關閉物件")]
    public bool fadeOutAndDisable = false;

    [Tooltip("淡出時間（秒）")]
    public float fadeDuration = 0.5f;

    // ===== Internal =====
    private bool triggered = false;
    private Vector3 startPos;
    private Vector3 endPos;

    void Awake()
    {
        startPos = transform.position;
        endPos = startPos + Vector3.up * floatHeight;
    }

    /*
     * 若你希望「玩家直接碰到這個物件就觸發」
     * 可以打開這段邏輯
     *（目前你的流程是由 GoalTrigger 來呼叫 TriggerByGoal）
     */
    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        // 若要直接觸發，取消註解
        // TriggerByGoal();
    }

    /// <summary>
    /// 由 Goal 物件或其他腳本呼叫的正式入口
    /// </summary>
    public void TriggerByGoal()
    {
        if (triggered) return;
        triggered = true;

        Debug.Log("[FloatUp] Triggered");

        // 重新取得起點（避免物件已被移動過）
        startPos = transform.position;
        endPos = startPos + Vector3.up * floatHeight;

        PlayVFX();
        StopAllCoroutines();
        StartCoroutine(FloatRoutine());
    }

    private void PlayVFX()
    {
        if (vfxPrefab == null) return;

        Vector3 pos = (vfxSpawnPoint != null) ? vfxSpawnPoint.position : transform.position;
        Transform parent = vfxFollowThisObject ? transform : null;

        ParticleSystem vfx = Instantiate(vfxPrefab, pos, Quaternion.identity, parent);
        vfx.Play();

        // 自動銷毀特效
        float life = 2f;
        var main = vfx.main;
        life = main.duration + main.startLifetime.constantMax;
        Destroy(vfx.gameObject, life + 0.2f);
    }

    private IEnumerator FloatRoutine()
    {
        float t = 0f;

        while (t < floatDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / floatDuration);
            float eased = easeCurve.Evaluate(u);

            transform.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        transform.position = endPos;

        if (fadeOutAndDisable)
            yield return FadeOutThenDisable();
    }

    private IEnumerator FadeOutThenDisable()
    {
        // 取得所有 Renderer（包含子物件）
        var renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            gameObject.SetActive(false);
            yield break;
        }

        float t = 0f;

        // 收集所有材質（避免每幀 new）
        var mats = new List<Material>();
        foreach (var r in renderers)
        {
            foreach (var m in r.materials)
                mats.Add(m);
        }

        // 記錄原始顏色
        Color[] startColors = new Color[mats.Count];
        for (int i = 0; i < mats.Count; i++)
        {
            startColors[i] = mats[i].HasProperty("_Color") ? mats[i].color : Color.white;
        }

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / fadeDuration);
            float alpha = 1f - u;

            for (int i = 0; i < mats.Count; i++)
            {
                if (!mats[i] || !mats[i].HasProperty("_Color")) continue;
                Color c = startColors[i];
                c.a = alpha;
                mats[i].color = c;
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
