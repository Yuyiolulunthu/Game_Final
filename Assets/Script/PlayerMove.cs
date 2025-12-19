using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    public static System.Action OnGameEnd;

    [Header("Moving Parameter")]
    public float moveSpeed = 1f;
    public float drag = 8f;

    [Header("Shadow Check")]
    public Shadow shadowChecker;       
    public LayerMask groundMask = 3;  
    public float footRayHeight = 2.0f; 
    public float footLift = 0.01f;    
    public PlayerShadowFromObject dynamicShadow;

    [Header("Push Settings")]
    public float pushCheckDistance = 30f;
    public LayerMask pushableMask;
    public string pushingParam = "IsPushing";

    private bool isPushing = false;

    [Header("Animation (Animator)")]
    public Animator animator;                 
    public string speedParam = "MovingSpeed";
    public string shadowParam = "InShadow";

    [Header("Goal End")]
    public string goalTag = "Goal";
    public string endParam = "IsEnd";    
    private bool reachedGoal = false;

    [Header("Step Up")]
    public float stepHeight = 0.8f;
    public float stepCheckDist = 0.3f;
    public float stepUpSpeed = 3f;
    public LayerMask obstacleMask = ~0;

    [Header("Step Debug")]
    public bool showStepDebug = true;
    public bool verboseStepLog = false;

    // ===== 新增：音效設定 =====
    [Header("Audio - General")]
    public AudioSource audioSource;           // 主要音效播放器（一次性音效）
    
    [Header("Audio - Footsteps")]
    public AudioClip[] footstepSounds;        // 腳步聲（多個隨機播放）
    public float footstepInterval = 0.35f;    // 腳步間隔
    [Range(0f, 1f)]
    public float footstepVolume = 0.5f;
    
    [Header("Audio - Push")]
    public AudioClip pushStartSound;          // 開始推的撞擊聲
    public AudioClip pushLoopSound;           // 推動中的摩擦聲（循環）
    [Range(0f, 1f)]
    public float pushVolume = 0.7f;
    
    [Header("Audio - Step Up")]
    public AudioClip stepUpSound;             // 踏上台階的聲音
    [Range(0f, 1f)]
    public float stepUpVolume = 0.6f;
    
    [Header("Audio - Goal")]
    public AudioClip goalReachedSound;        // 到達終點的音效
    
    private AudioSource loopAudioSource;      // 專門播循環音效
    private float footstepTimer;
    private bool wasSteppingUp = false;       // 追蹤是否正在踏階梯
    // ===== 音效設定結束 =====

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
        rb.freezeRotation = true;
        rb.useGravity = true;
        rb.drag = drag;
        cam = Camera.main;

        // player has no shadow
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = true;
        }

        // find animator
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (animator) animator.applyRootMotion = false;

        // ===== 音效初始化 =====
        // 如果沒有指定 AudioSource，嘗試取得或建立一個
        if (!audioSource)
        {
            audioSource = GetComponent<AudioSource>();
            if (!audioSource)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }
        
        // 建立第二個 AudioSource 專門給循環音效用
        loopAudioSource = gameObject.AddComponent<AudioSource>();
        loopAudioSource.playOnAwake = false;
        loopAudioSource.spatialBlend = 0f;
        loopAudioSource.loop = true;
        // ===== 音效初始化結束 =====
    }

    void Update()
    {
        if (reachedGoal)
        {
            moveDir = Vector3.zero;
            if (animator) animator.SetFloat(speedParam, 0f);
            return;
        }
        
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v).normalized;

        Vector3 camF = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
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
        if (reachedGoal)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            blocked = true;
            return;
        }
        
        if (moveDir.sqrMagnitude < 1e-4f)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            blocked = false;
            ResetFootstepTimer();
            return;
        }

        // 1) predict next step
        Vector3 step = moveDir * moveSpeed * Time.fixedDeltaTime;
        Vector3 nextXZ = rb.position + new Vector3(step.x, 0f, step.z);

        // 2) origin position
        Vector3 rayOrigin = nextXZ + Vector3.up * footRayHeight;
        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit,
                             footRayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            // edge -> don't move
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            blocked = true;
            ResetFootstepTimer();
            return;
        }

        Vector3 footPoint = hit.point;

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
            ResetFootstepTimer();
            return;
        }
        
        // 4) step up try
        TryStepUp();
        
        // 5) move
        rb.MovePosition(new Vector3(nextXZ.x, rb.position.y, nextXZ.z));
        blocked = false;
        
        // ===== 腳步聲 =====
        PlayFootstepSound();
    }

    // ===== 音效方法 =====
    
    /// <summary>
    /// 播放腳步聲（在移動時定時觸發）
    /// </summary>
    void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;
        if (isPushing) return; // 推東西時不播腳步聲
        
        footstepTimer -= Time.fixedDeltaTime;
        if (footstepTimer <= 0f)
        {
            int index = Random.Range(0, footstepSounds.Length);
            if (footstepSounds[index] != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(footstepSounds[index], footstepVolume);
            }
            footstepTimer = footstepInterval;
        }
    }
    
    /// <summary>
    /// 重置腳步計時器
    /// </summary>
    void ResetFootstepTimer()
    {
        footstepTimer = 0f;
    }
    
    /// <summary>
    /// 播放踏上台階的音效
    /// </summary>
    void PlayStepUpSound()
    {
        if (stepUpSound != null && !wasSteppingUp)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(stepUpSound, stepUpVolume);
        }
        wasSteppingUp = true;
    }
    
    /// <summary>
    /// 開始推動音效
    /// </summary>
    void StartPushSound()
    {
        // 播放開始推的撞擊聲
        if (pushStartSound != null)
        {
            audioSource.PlayOneShot(pushStartSound, pushVolume);
        }
        
        // 播放循環摩擦聲
        if (pushLoopSound != null && loopAudioSource != null)
        {
            loopAudioSource.clip = pushLoopSound;
            loopAudioSource.volume = pushVolume;
            loopAudioSource.Play();
        }
    }
    
    /// <summary>
    /// 停止推動音效
    /// </summary>
    void StopPushSound()
    {
        if (loopAudioSource != null && loopAudioSource.isPlaying)
        {
            loopAudioSource.Stop();
        }
    }
    
    /// <summary>
    /// 播放到達終點音效
    /// </summary>
    void PlayGoalSound()
    {
        if (goalReachedSound != null)
        {
            audioSource.PlayOneShot(goalReachedSound);
        }
    }
    
    // ===== 音效方法結束 =====

    void OnGUI()
    {
        bool here = false;
        if (Physics.Raycast(transform.position + Vector3.up * footRayHeight, Vector3.down,
                             out var hit, footRayHeight * 2f, groundMask))
            here = shadowChecker ? shadowChecker.IsInShadow(hit.point + Vector3.up * footLift) : true;
        animator.SetBool(shadowParam, here);
        GUI.Label(new Rect(10, 10, 520, 24), $"Here Shadow: {(here ? "YES" : "NO")} | Blocked: {(blocked ? "YES" : "NO")} | Pushing: {(isPushing ? "YES" : "NO")}");
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
        {
            animator.SetFloat(speedParam, 0f);
            StartPushSound();  // ← 開始推動音效
        }
        else
        {
            StopPushSound();   // ← 停止推動音效
        }
    }

    void TryStepUp()
    {
        // reset debug info every call
        stepLowHit = false;
        stepHighBlocked = false;
        stepLowName = "-";
        stepLowDist = 0f;

        Vector3 dir = Vector3.ProjectOnPlane(moveDir, Vector3.up).normalized;
        if (dir.sqrMagnitude < 1e-4f)
        {
            wasSteppingUp = false;
            return;
        }

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
                wasSteppingUp = false;
                return;
            }

            // 3) Step up!
            float up = stepUpSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + Vector3.up * up);
            
            // ===== 踏上台階音效 =====
            PlayStepUpSound();

            if (verboseStepLog)
                Debug.Log($"[StepUp] STEP: low hit {stepLowName} dist={stepLowDist:F2} -> stepping up {up:F3}");
        }
        else
        {
            // no low obstacle -> normal movement
            stepLowHit = false;
            wasSteppingUp = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (reachedGoal) return;
        if (!other.CompareTag(goalTag)) return;

        reachedGoal = true;
        SetPushing(false);
        if (animator) animator.SetBool(endParam, true);

        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        
        // ===== 到達終點音效 =====
        PlayGoalSound();
        
        OnGameEnd?.Invoke();
    }
}