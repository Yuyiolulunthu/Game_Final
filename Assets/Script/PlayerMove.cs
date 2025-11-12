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
    public float footLift = 0.03f;    

    [Header("animation (Animator)")]
    public Animator animator;                 
    public string speedParam = "MovingSpeed"; 

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

        // 3) in shadow
        if (shadowChecker && !shadowChecker.IsInShadow(footPoint))
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            blocked = true; return;
        }

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

        GUI.Label(new Rect(10,10,520,24), $"Here Shadow: {(here ? "YES":"NO")} | Blocked: {(blocked?"YES":"NO")}");
    }
}
