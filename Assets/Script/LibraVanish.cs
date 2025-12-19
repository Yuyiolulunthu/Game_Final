using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class LibraVanish : MonoBehaviour
{
    [Header("Trigger")]
    public float disappearHeight = -5f;   // 低於這個 Y 觸發
    public float fadeDuration = 1f;       // 變淡時間（秒）

    [Header("Effect")]
    public ParticleSystem disappearEffect; // 消失特效（可選）

    [Header("Options")]
    public bool useLocalY = false;

    Renderer rend;
    Material mat;
    bool isDisappearing = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material; // ⚠️ 會自動複製材質（安全）
    }

    void Update()
    {
        float y = useLocalY ? transform.localPosition.y : transform.position.y;

        if (!isDisappearing && y < disappearHeight)
        {
            StartCoroutine(FadeAndDestroy());
        }
    }

    IEnumerator FadeAndDestroy()
    {
        isDisappearing = true;

        // 停止所有同物件上的 script（包含 Libra_stage7）
        foreach (var mb in GetComponents<MonoBehaviour>())
        {
            if (mb != this)
                mb.enabled = false;
        }

        // 播放消失特效
        if (disappearEffect != null)
        {
            ParticleSystem ps = Instantiate(
                disappearEffect,
                transform.position,
                Quaternion.identity
            );
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        // 淡出
        Color c = mat.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            mat.color = c;
            yield return null;
        }

        // 最後銷毀整個物件（完全不再影響任何東西）
        Destroy(gameObject);
    }
}
