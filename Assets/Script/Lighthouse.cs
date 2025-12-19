using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Lighthouse : MonoBehaviour
{
    [Header("Target")]
    public Transform spotLight;

    [Header("Rotation Range (Local Euler Angles)")]
    [Tooltip("起點角度（Local Euler）")]
    public Vector3 angleA = new Vector3(174.855f, 35.033f, 10.63699f);

    [Tooltip("終點角度（Local Euler）")]
    public Vector3 angleB = new Vector3(134.891f, 7.108002f, -0.757995f);

    [Header("Motion")]
    [Tooltip("A->B 或 B->A 單程要幾秒（秒）")]
    [Min(0.01f)]
    public float duration = 3f;

    [Tooltip("是否來回運動（A<->B）。關閉則只會 A->B 到底就停。")]
    public bool pingPong = true;

    [Header("Trigger")]
    public string playerTag = "Player";

    [Header("Preview")]
    [Tooltip("在編輯模式即時預覽角度（不用 Play 也會動）")]
    public bool previewInEditMode = false;

    [Range(0f, 1f)]
    [Tooltip("動畫預覽位置：0=A, 1=B（僅在編輯器或未啟動時方便查看角度）")]
    public float previewT = 0f;

    bool isActive = false;
    float t = 0f;
    bool forward = true;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        if (spotLight != null)
        {
            // 初始停在 A
            spotLight.localEulerAngles = angleA;
            t = 0f;
            forward = true;
        }
    }

    void Update()
    {
        if (spotLight == null) return;

        // 編輯模式預覽：只要勾選 previewInEditMode，就會根據 previewT 旋轉
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (previewInEditMode)
                ApplyRotation(previewT);
            return;
        }
#endif

        if (!isActive)
        {
            // 停止時也可以用 previewT 方便在遊戲中調整位置（可選）
            // ApplyRotation(previewT);
            return;
        }

        float delta = Time.deltaTime / duration;
        t += forward ? delta : -delta;

        if (t >= 1f)
        {
            t = 1f;
            if (pingPong) forward = false;
            else isActive = false; // 單程一次就停
        }
        else if (t <= 0f)
        {
            t = 0f;
            if (pingPong) forward = true;
        }

        ApplyRotation(t);
    }

    void ApplyRotation(float lerpT)
    {
        Vector3 euler = Vector3.Lerp(angleA, angleB, lerpT);
        spotLight.localEulerAngles = euler;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        isActive = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        isActive = false;
    }

    // ====== 以下是方便在編輯器調整角度的工具 ======

    [ContextMenu("Capture Angle A From Current Light")]
    void CaptureAngleAFromCurrent()
    {
        if (spotLight == null) return;
        angleA = spotLight.localEulerAngles;
        previewT = 0f;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Capture Angle B From Current Light")]
    void CaptureAngleBFromCurrent()
    {
        if (spotLight == null) return;
        angleB = spotLight.localEulerAngles;
        previewT = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Snap Light To Angle A")]
    void SnapToA()
    {
        if (spotLight == null) return;
        spotLight.localEulerAngles = angleA;
        previewT = 0f;
    }

    [ContextMenu("Snap Light To Angle B")]
    void SnapToB()
    {
        if (spotLight == null) return;
        spotLight.localEulerAngles = angleB;
        previewT = 1f;
    }
}