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
    [Tooltip("A->B 或 B->A 走完一次要多久（秒）")]
    [Min(0.01f)]
    public float duration = 3f;

    [Tooltip("是否來回擺動（A<->B）。關掉則只會 A->B 到頂就停。")]
    public bool pingPong = true;

    [Header("Trigger")]
    public string playerTag = "Player";

    [Header("Preview")]
    [Tooltip("在編輯模式也即時預覽旋轉（不進 Play 也會動）")]
    public bool previewInEditMode = false;

    [Range(0f, 1f)]
    [Tooltip("手動預覽位置：0=A, 1=B（僅在未播放或未啟用旋轉時方便看角度）")]
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
            // 初始就放在 A
            spotLight.localEulerAngles = angleA;
            t = 0f;
            forward = true;
        }
    }

    void Update()
    {
        if (spotLight == null) return;

        // 編輯模式預覽：只要你勾 previewInEditMode，就會跟著 previewT 顯示
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
            // 不轉時也可以用 previewT 方便你在遊戲中手動調位置（可選）
            // ApplyRotation(previewT);
            return;
        }

        float delta = Time.deltaTime / duration;
        t += forward ? delta : -delta;

        if (t >= 1f)
        {
            t = 1f;
            if (pingPong) forward = false;
            else isActive = false; // 只走一次就停
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

    // ====== 下面是你要的「介面調整角度」超好用工具 ======

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
