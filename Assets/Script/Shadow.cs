using UnityEngine;

public class Shadow : MonoBehaviour
{
    [Header("光源（不填就用 RenderSettings.sun）")]
    public Light mainLight;

    [Header("會擋光的圖層（牆、建築、樹等，需有 Collider）")]
    public LayerMask occluderMask;

    [Header("取樣&寬容度")]
    [Tooltip("從地面上方一點取樣，避免貼地誤差")]
    public float sampleHeight = 0.2f;
    [Tooltip("沿光線反方向退一點，避免邊界閃爍")]
    public float lightBias = 0.05f;
    [Tooltip("用有厚度的射線檢查")]
    public float sphereRadius = 0.06f;
    [Tooltip("Directional 光的射線長度")]
    public float maxRayDistance = 200f;

    [Header("方向修正")]
    [Tooltip("若判斷反了就打勾")]
    public bool invertDirectional = false;

    void Awake()
    {
        if (!mainLight) mainLight = RenderSettings.sun;
    }

    /// <summary> true = 陰影；false = 亮處 </summary>
    public bool IsInShadow(Vector3 worldPos)
    {
        if (!mainLight) return true;

        Vector3 p = worldPos + Vector3.up * sampleHeight;

        if (mainLight.type == LightType.Directional)
        {
            // 從點往「光」方向打（依需要反轉），並把點往陰影內側退一點
            Vector3 dirToLight = invertDirectional ? mainLight.transform.forward : -mainLight.transform.forward;
            dirToLight.Normalize();
            Vector3 biasedPoint = p - dirToLight * lightBias;

            // 有厚度的射線：撞到遮擋物 ? 在陰影
            return Physics.SphereCast(biasedPoint, sphereRadius, dirToLight,
                                      out _, maxRayDistance, occluderMask, QueryTriggerInteraction.Ignore);
        }
        else
        {
            // 點光 / 聚光：從光源往點打
            Vector3 from = mainLight.transform.position;
            Vector3 dir  = (p - from);
            float dist   = dir.magnitude;
            if (dist < 1e-4f) return false;
            dir /= dist;

            return Physics.SphereCast(from, sphereRadius, dir,
                                      out _, dist - 0.01f, occluderMask, QueryTriggerInteraction.Ignore);
        }
    }
}
