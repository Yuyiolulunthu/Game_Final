using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatUpOnGoalTouch : MonoBehaviour
{
    [Header("Trigger Condition")]
    [Tooltip("要碰到的目標物件名稱（預設 Goal）")]
    public string goalObjectName = "Goal";

    [Tooltip("玩家 Tag（預設 Player）")]
    public string playerTag = "Player";

    [Header("Float Up Motion")]
    [Tooltip("往上浮起的距離（世界座標）")]
    public float floatHeight = 1.5f;

    [Tooltip("上浮所需時間（秒）")]
    public float floatDuration = 1.0f;

    [Tooltip("上浮曲線（越右上越快/越平滑）")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("VFX")]
    [Tooltip("觸發時播放的特效 Prefab（ParticleSystem）")]
    public ParticleSystem vfxPrefab;

    [Tooltip("特效生成位置（不填則用本物件位置）")]
    public Transform vfxSpawnPoint;

    [Tooltip("特效生成時是否跟著本物件")]
    public bool vfxFollowThisObject = true;

    [Header("Optional: Fade Out Then Disable")]
    [Tooltip("上浮結束後淡出並停用物件")]
    public bool fadeOutAndDisable = false;

    [Tooltip("淡出時間（秒）")]
    public float fadeDuration = 0.5f;

    // internal
    private bool triggered = false;
    private Vector3 startPos;
    private Vector3 endPos;

    void Awake()
    {
        startPos = transform.position;
        endPos = startPos + Vector3.up * floatHeight;
    }

    // 由 Goal 物件的 Collider/Trigger 觸發：Goal 上需有 Collider（可 Trigger 或非 Trigger）
    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        // 只接受「Player」進入本物件的 trigger
        if (!other.CompareTag(playerTag)) return;

        // 確認玩家現在碰到的是 Goal（用 "玩家目前碰到的是 Goal" 的方式做判定）
        // 注意：這個腳本掛在「要上浮的物件」上，所以必須由 Goal 來觸發它才合理。
        // 若你希望由 Goal 來觸發，請把此腳本也掛在 Goal，並在 inspector 指定 targetToFloat。
    }

    // 建議做法：由 Goal 觸發（更直觀）
    // 下面提供一個公開方法，讓 Goal 物件去呼叫
    public void TriggerByGoal()
    {
        if (triggered) return;
        triggered = true;

        // 重新抓一次，避免你在場景中移動過此物件
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

        // 自動清掉特效物件
        float life = 2f;
        var main = vfx.main;
        life = main.duration + main.startLifetime.constantMax;
        Destroy(vfx.gameObject, life + 0.2f);
    }

    private System.Collections.IEnumerator FloatRoutine()
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

    private System.Collections.IEnumerator FadeOutThenDisable()
    {
        // 找所有 Renderer（包含子物件）
        var renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            gameObject.SetActive(false);
            yield break;
        }

        // 只處理有 _Color 的材質（常見於 Standard/URP Lit）
        float t = 0f;

        // 先把所有材質抓出來（避免每 frame new）
        var mats = new System.Collections.Generic.List<Material>();
        foreach (var r in renderers)
        {
            foreach (var m in r.materials) // material => 會 instantiate，適合做獨立淡出
                mats.Add(m);
        }

        // 記錄初始顏色
        Color[] startColors = new Color[mats.Count];
        for (int i = 0; i < mats.Count; i++)
        {
            startColors[i] = mats[i].HasProperty("_Color") ? mats[i].color : Color.white;
        }

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / fadeDuration);
            float a = 1f - u;

            for (int i = 0; i < mats.Count; i++)
            {
                if (!mats[i] || !mats[i].HasProperty("_Color")) continue;
                Color c = startColors[i];
                c.a = a;
                mats[i].color = c;
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }
}

