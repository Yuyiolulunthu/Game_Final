using UnityEngine;

public class PlayerShadowFromObject : MonoBehaviour
{
    [Header("基本組件")]
    public Transform player;
    public Transform lightObject;
    public Transform aimTarget;
    public GameObject shadowPrefab;

    [Header("Layer 設定")]
    public LayerMask groundMask;  // GroundMask = 只包含地面

    [Header("光線設定")]
    public float rayMaxDistance = 500f;
    public float afterHitBias = 0.05f;
    public float smallLift = 0.002f;
    public float rayRadius = 0.0f;  // SphereCast 半徑

    [Header("陰影尺寸控制")]
    public float baseMinorRadius = 0.45f;
    public float baseMajorRadius = 0.45f;
    public float minCos = 0.25f;
    public float maxStretch = 4.0f;

    [Header("控制")]
    public KeyCode toggleKey = KeyCode.Space;  // Space：凍結/恢復

    private GameObject shadowInstance;
    private bool _isFrozen = false;             // true=凍結中（不更新）
    private Renderer[] lightRenderers;          // 用來暫時隱藏
    private Renderer[] aimRenderers;

    // 新增：唯讀屬性（可從外部讀取目前狀態）
    public bool IsFrozen => _isFrozen;

    void Start()
    {
        if (!shadowPrefab)
        {
            Debug.LogError("請指定 shadowPrefab！");
            enabled = false;
            return;
        }

        // 快取可見性切換用的 Renderer 陣列（不影響 Transform）
        lightRenderers = lightObject ? lightObject.GetComponentsInChildren<Renderer>(true) : null;
        aimRenderers = aimTarget ? aimTarget.GetComponentsInChildren<Renderer>(true) : null;

        shadowInstance = Instantiate(shadowPrefab);
        shadowInstance.name = "DynamicShadow";

        // 不投、不收 Unity 光照陰影
        foreach (var r in shadowInstance.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        shadowInstance.SetActive(false);
    }

    void Update()
    {
        // Space：切換凍結狀態
        if (Input.GetKeyDown(toggleKey))
        {
            _isFrozen = !_isFrozen;
            SetVisible(lightRenderers, !_isFrozen);
            SetVisible(aimRenderers, !_isFrozen);
            // 不改 shadowInstance 的啟用狀態與 transform，
            // 保持目前畫面樣子（凍結）
        }
    }

    void LateUpdate()
    {
        if (!player || !lightObject || !aimTarget || !shadowInstance)
            return;

        // 凍結時：完全停止投影與更新
        if (_isFrozen) return;

        Vector3 origin = lightObject.position;
        Vector3 dir = (aimTarget.position - origin);
        if (dir.sqrMagnitude < 1e-8f)
        {
            shadowInstance.SetActive(false);
            return;
        }
        dir.Normalize();

        // Step 1: 使用 SphereCast() 檢查是否打到 player
        if (!Physics.SphereCast(origin, rayRadius, dir, out RaycastHit firstHit, rayMaxDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            shadowInstance.SetActive(false);
            return;
        }

        bool hitPlayer = IsPartOf(firstHit.transform, player);
        if (!hitPlayer)
        {
            shadowInstance.SetActive(false);
            return;
        }

        // Step 2: 從玩家命中點往地面打（普通 Raycast）
        Vector3 fromAfterPlayer = firstHit.point + dir * afterHitBias;
        if (!Physics.Raycast(fromAfterPlayer, dir, out RaycastHit groundHit, rayMaxDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            shadowInstance.SetActive(false);
            return;
        }

        // Step 3: 顯示並更新位置（不旋轉 prefab）
        shadowInstance.SetActive(true);
        Vector3 shadowPos = groundHit.point + groundHit.normal * smallLift;
        shadowInstance.transform.position = shadowPos;

        // 保留 prefab 原本旋轉（不改 rotation）

        // Step 4: 根據光線角度控制橢圓比例
        float cosTheta = Mathf.Clamp01(Vector3.Dot(-dir, groundHit.normal));
        cosTheta = Mathf.Max(cosTheta, minCos);
        float stretch = Mathf.Min(1.0f / cosTheta, maxStretch);

        shadowInstance.transform.localScale =
            new Vector3(baseMinorRadius * 2f, 1f, baseMajorRadius * stretch * 2f);

        // Debug 線顯示光路徑
        Debug.DrawLine(origin, firstHit.point, Color.red);
        Debug.DrawLine(firstHit.point, groundHit.point, Color.green);
    }

    private static void SetVisible(Renderer[] renderers, bool visible)
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i]) renderers[i].enabled = visible;
        }
    }

    private static bool IsPartOf(Transform t, Transform root)
    {
        if (!t || !root) return false;
        for (Transform cur = t; cur != null; cur = cur.parent)
            if (cur == root) return true;
        return false;
    }
    public bool IsPointOnShadow(Vector3 worldPos)
    {
        if (!shadowInstance || !shadowInstance.activeSelf) return false;
        Vector3 local = shadowInstance.transform.InverseTransformPoint(worldPos);
        float rx = shadowInstance.transform.localScale.x * 0.5f; 
        float rz = shadowInstance.transform.localScale.z * 0.5f; 

        if (Mathf.Abs(local.y) > 0.05f) return false;
        if (rx <= 1e-5f || rz <= 1e-5f) return false;
        float val = (local.x * local.x) / (rx * rx) + (local.z * local.z) / (rz * rz);

        return val <= 0.1f;
    }

}
