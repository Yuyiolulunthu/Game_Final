using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableStepAssist : MonoBehaviour
{
    [Header("Grounding")]
    public LayerMask groundMask = ~0;
    public float groundCheckDistance = 0.6f;   // 往下找地板距離
    public float groundSnapSpeed = 12f;        // 貼地速度（越大越黏地）

    [Header("Step settings")]
    public float maxStepUp = 0.5f;             // 你說的 0.3~0.5，建議設 0.5
    public float maxStepDown = 0.6f;           // 允許往下落差
    public float stepCheckDist = 0.25f;        // 前方檢查距離（越大越早抬腳）
    public float stepUpSpeed = 6f;             // 抬升速度
    public float minMoveSpeedToAssist = 0.05f; // 太慢就不做 step（避免原地抖）

    [Header("Obstacle settings")]
    public LayerMask obstacleMask = ~0;        // 牆/台階/障礙物 layer
    public float skin = 0.02f;                 // 些微偏移，避免卡在表面
    public float maxWallNormalY = 0.2f;        // 判斷「像牆」的法線條件（越小越像垂直）

    [Header("Debug")]
    public bool showDebug = true;

    Rigidbody rb;
    Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // 建議設定（你也可以在 Inspector 自己設）
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void FixedUpdate()
    {
        Vector3 v = rb.velocity;
        Vector3 horizontal = Vector3.ProjectOnPlane(v, Vector3.up);

        // 太慢就不幫忙，避免小抖動一直觸發
        if (horizontal.magnitude < minMoveSpeedToAssist)
        {
            SnapDownToGround();
            return;
        }

        // 先嘗試爬上小台階（避免被低矮高度差卡死）
        TryStepUp(horizontal.normalized);

        // 再貼地（處理下坡/落差）
        SnapDownToGround();
    }

    // --- Step Up: 低處撞到、高處沒撞到 => 抬升 ---
    void TryStepUp(Vector3 dir)
    {
        Bounds b = col.bounds;
        float halfHeight = b.extents.y;
        float radius = Mathf.Min(b.extents.x, b.extents.z);

        // 從「接近底部」打一條前方低高度的 SphereCast
        Vector3 lowOrigin = new Vector3(rb.position.x, b.min.y + radius + skin, rb.position.z);
        Ray lowRay = new Ray(lowOrigin, dir);

        bool lowHit = Physics.SphereCast(lowRay, radius * 0.95f, out RaycastHit hitLow,
                                         stepCheckDist, obstacleMask, QueryTriggerInteraction.Ignore);

        if (showDebug)
        {
            Debug.DrawRay(lowOrigin, dir * stepCheckDist, lowHit ? Color.red : Color.gray);
        }

        if (!lowHit) return;

        // 如果前方撞到的表面不是「像牆的垂直面」，可能只是地板/斜坡，就不處理
        if (hitLow.normal.y > maxWallNormalY) return;

        // 從較高的位置再打一條 SphereCast：如果高處也被擋 => 真牆，上不去
        Vector3 highOrigin = lowOrigin + Vector3.up * maxStepUp;
        Ray highRay = new Ray(highOrigin, dir);
        bool highBlocked = Physics.SphereCast(highRay, radius * 0.95f, stepCheckDist,
                                              obstacleMask, QueryTriggerInteraction.Ignore);

        if (showDebug)
        {
            Debug.DrawRay(highOrigin, dir * stepCheckDist, highBlocked ? Color.yellow : Color.green);
        }

        if (highBlocked) return;

        // 抬升：用 MovePosition 做「輔助」抬高一點點（不改 XZ，不破壞推的方向）
        float up = stepUpSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + Vector3.up * up);
    }

    // --- Step Down / Ground Snap: 讓物體不會因為落差小就浮起或卡住 ---
    void SnapDownToGround()
    {
        Bounds b = col.bounds;
        float radius = Mathf.Min(b.extents.x, b.extents.z);

        // 從物體上方往下找地板
        Vector3 origin = new Vector3(rb.position.x, b.center.y + b.extents.y + 0.1f, rb.position.z);
        float maxDist = (b.extents.y + maxStepDown + groundCheckDistance);

        bool hit = Physics.SphereCast(origin, radius * 0.95f, Vector3.down, out RaycastHit gHit,
                                      maxDist, groundMask, QueryTriggerInteraction.Ignore);

        if (showDebug)
        {
            Debug.DrawRay(origin, Vector3.down * maxDist, hit ? Color.cyan : Color.magenta);
        }

        if (!hit) return;

        // 計算「底部應該貼到地板」的目標 y
        float targetBottomY = gHit.point.y + skin;
        float currentBottomY = b.min.y;

        float delta = targetBottomY - currentBottomY;

        // 只處理「往下貼」或小幅往上貼（避免穿透）
        // 往下：delta < 0；往上：delta > 0
        if (delta < 0f)
        {
            // 向下貼地：用速度方式更物理（不硬拉）
            float vy = Mathf.Lerp(rb.velocity.y, rb.velocity.y + delta / Time.fixedDeltaTime, groundSnapSpeed * Time.fixedDeltaTime);
            rb.velocity = new Vector3(rb.velocity.x, vy, rb.velocity.z);
        }
        else if (delta > 0f && delta <= maxStepUp * 0.5f)
        {
            // 小幅往上貼地（避免地板縫造成微卡）
            rb.MovePosition(rb.position + Vector3.up * Mathf.Min(delta, 0.02f));
        }
    }
}
