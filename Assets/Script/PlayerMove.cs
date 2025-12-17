using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("moving parameter")]
    public float moveSpeed = 1f;
    public float drag = 8f;

    [Header("shadow check")]
    public Shadow shadowChecker;       
    public LayerMask groundMask = ~0;  
    public float footRayHeight = 2.0f; 
    public float footLift = 0.01f;    
    public PlayerShadowFromObject dynamicShadow;

    [Header("push settings")]
    public float pushCheckDistance = 30f;
    public LayerMask pushableMask;
    public string pushingParam = "IsPushing";

    private bool isPushing = false;

    [Header("animation (Animator)")]
    public Animator animator;                 
    public string speedParam = "MovingSpeed";

    [Header("step up")]
    public float stepHeight = 0.5f;     // 能跨的最大台階高度
    public float stepCheckDist = 0.3f;   // 前方檢查距離
    public float stepUpSpeed = 3f;       // 抬升速度
    public LayerMask obstacleMask = ~0;  // 台階 / 牆 的 layer

    [Header("step debug")]
    public bool showStepDebug = true;
    public bool verboseStepLog = false;

    private bool stepLowHit;
    private bool stepHighBlocked;
    private string stepLowName = "-";
    private float stepLowDist;



    private Rigidbody rb;
    private Camera cam;
    private Vector3 moveDir;
    private bool blocked;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; rb.useGravity = true; rb.drag = drag;
        cam = Camera.main;

        // player has no shadow
        foreach (var r in GetComponentsInChildren<Renderer>())
        { r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = true; }

        // find animator
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (animator) animator.applyRootMotion = false;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v).normalized;

        Vector3 camF = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(cam.transform.right,   Vector3.up).normalized;
        moveDir = (camF * input.z + camR * input.x).normalized;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            if (animator) animator.SetFloat(speedParam, 1f);
            transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
        }    
        else
        {
            if (animator) animator.SetFloat(speedParam, 0f);
        }
    }

    void FixedUpdate()
    {           
        if (moveDir.sqrMagnitude < 1e-4f)
        { rb.velocity = new Vector3(0f, rb.velocity.y, 0f); blocked = false; return; }

        // 1) predict next step
        Vector3 step   = moveDir * moveSpeed * Time.fixedDeltaTime;
        Vector3 nextXZ = rb.position + new Vector3(step.x, 0f, step.z);

        // 2) origin position
        Vector3 rayOrigin = nextXZ + Vector3.up * footRayHeight;
        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit,
                             footRayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {   // edge -> dont move
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            blocked = true; return;
        }

        Vector3 footPoint = hit.point + Vector3.up * footLift;

        // 3) check in shadow
        bool inShadow = true;
        if (shadowChecker) inShadow = shadowChecker.IsInShadow(footPoint);
        if (!inShadow && dynamicShadow && dynamicShadow.IsFrozen)
        {
            if (dynamicShadow.IsPointOnShadow(footPoint))
                inShadow = true;
        }

        if (!inShadow)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            blocked = true;
            return;
        }

       
        // 4) step up try（新增）
        TryStepUp();
        // 4) move
        rb.MovePosition(new Vector3(nextXZ.x, rb.position.y, nextXZ.z));
        blocked = false;
    }

    // for debug
    void OnGUI()
    {
        bool here = false;
        if (Physics.Raycast(transform.position + Vector3.up * footRayHeight, Vector3.down,
                             out var hit, footRayHeight * 2f, groundMask))
            here = shadowChecker ? shadowChecker.IsInShadow(hit.point + Vector3.up * footLift) : true;

        GUI.Label(new Rect(10,10,520,24), $"Here Shadow: {(here ? "YES":"NO")} | Blocked: {(blocked?"YES":"NO")} | Pushing: {(isPushing ? "YES" : "NO")}");
    }

    void OnCollisionStay(Collision other)
    {
        // 判斷對方是不是在 pushableMask 裡
        if ((pushableMask.value & (1 << other.gameObject.layer)) != 0)
        {
            // 有在輸入方向才算在推
            if (moveDir.sqrMagnitude > 0.01f)
            {
                SetPushing(true);
            }
            else
            {
                SetPushing(false);
            }
        }
    }

    void OnCollisionExit(Collision other)
    {
        if ((pushableMask.value & (1 << other.gameObject.layer)) != 0)
        {
            SetPushing(false);
        }
    }

    void SetPushing(bool value)
    {
        if (isPushing == value) return;

        isPushing = value;
        if (animator)
            animator.SetBool(pushingParam, isPushing);

        if (isPushing)
            animator.SetFloat(speedParam, 0f);
    }

    void TryStepUp()
    {
        // reset debug info every call
        stepLowHit = false;
        stepHighBlocked = false;
        stepLowName = "-";
        stepLowDist = 0f;

        Vector3 dir = Vector3.ProjectOnPlane(moveDir, Vector3.up).normalized;
        if (dir.sqrMagnitude < 1e-4f) return;

        Vector3 lowOrigin = rb.position + Vector3.up * 0.05f;
        Vector3 highOrigin = rb.position + Vector3.up * stepHeight;

        // --- Debug rays (Scene view) ---
        if (showStepDebug)
        {
            Debug.DrawRay(lowOrigin, dir * stepCheckDist, Color.red);
            Debug.DrawRay(highOrigin, dir * stepCheckDist, Color.green);
        }

        // 1) Low ray: is there an obstacle in front near the feet?
        if (Physics.Raycast(lowOrigin, dir, out RaycastHit lowHit,
                            stepCheckDist, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            stepLowHit = true;
            stepLowName = lowHit.collider ? lowHit.collider.name : "(no collider)";
            stepLowDist = lowHit.distance;

            // 2) High ray: if high is ALSO blocked, it's a wall -> cannot step
            bool blockedHigh = Physics.Raycast(highOrigin, dir, stepCheckDist,
                                               obstacleMask, QueryTriggerInteraction.Ignore);
            stepHighBlocked = blockedHigh;

            if (blockedHigh)
            {
                if (verboseStepLog)
                    Debug.Log($"[StepUp] WALL: low hit {stepLowName} dist={stepLowDist:F2}");
                return;
            }

            // 3) Step up!
            float up = stepUpSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + Vector3.up * up);

            if (verboseStepLog)
                Debug.Log($"[StepUp] STEP: low hit {stepLowName} dist={stepLowDist:F2} -> stepping up {up:F3}");
        }
        else
        {
            // no low obstacle -> normal movement
            stepLowHit = false;
        }
    }



}
