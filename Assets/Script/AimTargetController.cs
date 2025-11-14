using UnityEngine;

[RequireComponent(typeof(Transform))]
public class AimTargetController : MonoBehaviour
{
    [Header("參考物件")]
    public Transform lightObject;     // 光源（父座標參考）

    [Header("鍵盤控制參數（垂直位移）")]
    public float moveSpeedY = 3f;                       // I/K 垂直位移速度（單位/秒）
    public Vector2 localYClamp = new Vector2(-2.5f, -1.1f);  // 在父座標下的高度範圍（最小~最大）

    [Header("Smooth 移動")]
    public float followLerp = 15f;    // 平滑度，越大越快

    // —— 內部狀態 —— 
    private float lockedLocalX;       // 開始時鎖定的 local X
    private float lockedLocalZ;       // 開始時鎖定的 local Z
    private float localY;             // 目前的 local Y（會被 I/K 改變）

    private int vertDir = 0;          // 0=不動, +1=往上, -1=往下
    private bool hasStarted = false;  // 在未按下 I 或 K 之前不移動

    void Start()
    {
        if (!lightObject)
        {
            Debug.LogError("[AimTargetController] 請指定 lightObject（父/光源中心）。");
            enabled = false;
            return;
        }

        // 讀取目前與父物件的相對座標，並「鎖定 X/Z」
        Vector3 local = lightObject.InverseTransformPoint(transform.position);
        lockedLocalX = local.x;
        lockedLocalZ = local.z;

        // 初始 localY 就取當前相對高度，並夾在範圍內
        localY = Mathf.Clamp(local.y, localYClamp.x, localYClamp.y);

        // 初始化位置：直接用鎖定的 X/Z 與當前 Y 回寫到世界座標
        Vector3 lockedLocal = new Vector3(lockedLocalX, localY, lockedLocalZ);
        Vector3 worldPos = lightObject.TransformPoint(lockedLocal);
        transform.position = worldPos;

        // 方向（可選）：看向從 lightObject 指向本物件的方向
        Vector3 dir = (transform.position - lightObject.position).normalized;
        if (dir.sqrMagnitude > 1e-6f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        bool holdI = Input.GetKey(KeyCode.I);
        bool holdK = Input.GetKey(KeyCode.K);

        // 第一次按下 I/K 才開始更新
        if (!hasStarted && (holdI || holdK))
            hasStarted = true;

        // 未開始前：完全不動
        if (!hasStarted)
            return;

        // I向下 K向上
        if (Input.GetKeyDown(KeyCode.I)) vertDir = +1;
        if (Input.GetKeyDown(KeyCode.K)) vertDir = -1;

        // 若兩鍵同時按或都沒按，則不動
        if (!(holdI ^ holdK)) vertDir = 0;

        // 垂直位移：只改 localY（X/Z 保持鎖定）
        if (vertDir != 0)
        {
            localY += vertDir * moveSpeedY * dt;

            // 邊界不動
            if (localY >= localYClamp.y)
            {
                localY = localYClamp.y;
                vertDir = 0; // 靜止
            }
            else if (localY <= localYClamp.x)
            {
                localY = localYClamp.x;
                vertDir = 0; // 靜止
            }
        }

        // 以鎖定 X/Z + 當前 localY 回寫世界座標
        Vector3 lockedLocal = new Vector3(lockedLocalX, localY, lockedLocalZ);
        Vector3 targetWorld = lightObject.TransformPoint(lockedLocal);

        transform.position = Vector3.Lerp(
            transform.position,
            targetWorld,
            1f - Mathf.Exp(-followLerp * dt)
        );

        // 方向（可選）：面向從 lightObject 指向本物件的方向
        Vector3 dir = (transform.position - lightObject.position);
        if (dir.sqrMagnitude > 1e-6f)
            transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
    }

    // Scene 視窗輔助線
    void OnDrawGizmos()
    {
        if (!lightObject) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(lightObject.position, transform.position);
        Gizmos.DrawWireSphere(transform.position, 0.08f);
    }
}
