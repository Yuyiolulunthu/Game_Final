using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class Ladder : MonoBehaviour
{
    [Header("Drop Settings")]
    public float dropSpeed = 2f;
    public string playerTag = "Player";

    [Header("Enter Push Settings")]
    public float enterPushDistance = 0.5f;
    public bool pushTowardLadder = true;
    public bool horizontalOnly = true;

    [Tooltip("相對方向倍率（例如 2 = player↔ladder 相對方向 *2）")]
    public float relativeDirMultiplier = 0.1f;

    [Header("Bottom Settings")]
    public bool updateBottomEveryFixed = true;
    public float bottomEpsilon = 0.01f;

    [Header("Anti-jitter (Debounce)")]
    public int enterStayFrames = 4;
    public float switchCooldown = 0.20f;
    public float bottomCooldown = 0.45f;
    public float acquireTinyCooldown = 0.02f;

    [Header("Debug")]
    public bool showDebug = false;
    public float logInterval = 0.1f;

    [Tooltip("Debug 畫線：player->ladder 方向線")]
    public bool drawDebugLines = true;

    [Tooltip("Debug 畫線顯示時間")]
    public float debugLineDuration = 0.05f;

    private float ladderBottomY;
    private Transform playerTf;
    private Rigidbody playerRb;
    private Collider playerCol;
    private PlayerMove playerMove;
    private float nextLogTime;

    private static readonly Dictionary<Transform, Ladder> ownerOfPlayer = new();
    private readonly Dictionary<Transform, int> stayCount = new();
    private static readonly Dictionary<Transform, float> nextAllowedTime = new();

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        ladderBottomY = col.bounds.min.y;

        if (showDebug)
            Debug.Log($"[Ladder] Awake bottomY={ladderBottomY:F2} name={name}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        Transform tf = other.transform;

        if (nextAllowedTime.TryGetValue(tf, out float t) && Time.time < t)
            return;

        stayCount[tf] = 0;

        if (showDebug)
            Debug.Log($"[Ladder] OnTriggerEnter (pre-acquire) ladder={name}, player={tf.name}");
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        Transform tf = other.transform;

        if (nextAllowedTime.TryGetValue(tf, out float t) && Time.time < t)
            return;

        if (ownerOfPlayer.TryGetValue(tf, out var owner) && owner != this)
            return;

        if (!stayCount.ContainsKey(tf))
            stayCount[tf] = 0;

        stayCount[tf]++;

        // ===== Debug 相對距離/方向（在還沒 Acquire 前也能看到）=====
        if (showDebug && Time.time >= nextLogTime)
        {
            nextLogTime = Time.time + logInterval;

            Vector3 playerPos = other.attachedRigidbody ? other.attachedRigidbody.position : tf.position;
            Vector3 ladderPos = transform.position;

            Vector3 toLadder = ladderPos - playerPos;
            Vector3 toLadderH = new Vector3(toLadder.x, 0f, toLadder.z);

            float dist3D = toLadder.magnitude;
            float distH = toLadderH.magnitude;

            float angle = Vector3.Angle(tf.forward, toLadderH.sqrMagnitude > 1e-6f ? toLadderH.normalized : tf.forward);

            Debug.Log(
                $"[Ladder] Stay ladder={name} player={tf.name} " +
                $"stay={stayCount[tf]}/{enterStayFrames} " +
                $"distH={distH:F3} dist3D={dist3D:F3} angle(playerFwd,toLadderH)={angle:F1}deg " +
                $"toLadder={toLadder}"
            );

            if (drawDebugLines)
            {
                Debug.DrawLine(playerPos, ladderPos, Color.yellow, debugLineDuration);
                Debug.DrawRay(playerPos, tf.forward * 1.0f, Color.cyan, debugLineDuration);
            }
        }

        if (stayCount[tf] < enterStayFrames)
            return;

        // 已經是 owner 就不用重進
        if (playerTf == tf && ownerOfPlayer.TryGetValue(tf, out var me) && me == this)
            return;

        AcquirePlayer(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        Transform tf = other.transform;
        stayCount.Remove(tf);

        if (ownerOfPlayer.TryGetValue(tf, out var owner) && owner == this)
        {
            ReleaseOwner(tf, switchCooldown, $"EXIT released owner ladder={name}");
        }
        else
        {
            nextAllowedTime[tf] = Time.time + switchCooldown;
            if (showDebug)
                Debug.Log($"[Ladder] EXIT ignored (not owner) ladder={name}");
        }
    }

    void FixedUpdate()
    {
        if (playerTf == null) return;

        if (ownerOfPlayer.TryGetValue(playerTf, out var owner) && owner != this)
            return;

        if (updateBottomEveryFixed)
            ladderBottomY = GetComponent<Collider>().bounds.min.y;

        float footY = (playerCol != null)
            ? playerCol.bounds.min.y
            : (playerRb != null ? playerRb.position.y : playerTf.position.y);

        if (footY <= ladderBottomY + bottomEpsilon)
        {
            if (showDebug && Time.time >= nextLogTime)
            {
                nextLogTime = Time.time + logInterval;
                Debug.Log($"[Ladder] Reached bottom (free fall) ladder={name}");
            }

            Transform tf = playerTf;
            ReleaseOwner(tf, bottomCooldown, $"Reached bottom -> release + cooldown {bottomCooldown:F2}s ladder={name}");
            return;
        }

        if (playerRb == null)
        {
            Vector3 p = playerTf.position;
            p.y -= dropSpeed * Time.fixedDeltaTime;
            playerTf.position = p;
            return;
        }

        Vector3 pos = playerRb.position;
        float newY = pos.y - dropSpeed * Time.fixedDeltaTime;

        playerRb.velocity = new Vector3(playerRb.velocity.x, 0f, playerRb.velocity.z);
        playerRb.MovePosition(new Vector3(pos.x, newY, pos.z));
    }

    // ================== Acquire / Release ==================

    private void AcquirePlayer(Collider other)
    {
        Transform tf = other.transform;

        if (ownerOfPlayer.TryGetValue(tf, out var owner) && owner != this)
            return;

        ownerOfPlayer[tf] = this;

        playerTf = tf;
        playerRb = other.attachedRigidbody;
        playerCol = other;
        playerMove = other.GetComponentInParent<PlayerMove>();

        ladderBottomY = GetComponent<Collider>().bounds.min.y;

        if (playerMove) playerMove.enabled = false;

        if (showDebug)
        {
            Vector3 ppos = playerRb ? playerRb.position : playerTf.position;
            Debug.Log($"[Ladder] ACQUIRE ladder={name}, player={playerTf.name}, playerPos={ppos}, bottomY={ladderBottomY:F2}");
        }

        PushPlayerRelative(enterPushDistance, pushTowardLadder);

        nextAllowedTime[tf] = Time.time + acquireTinyCooldown;
    }

    private void ReleaseOwner(Transform tf, float cooldown, string reasonLog)
    {
        nextAllowedTime[tf] = Time.time + cooldown;

        if (ownerOfPlayer.TryGetValue(tf, out var owner) && owner == this)
            ownerOfPlayer.Remove(tf);

        if (playerMove) playerMove.enabled = true;

        stayCount.Remove(tf);

        playerTf = null;
        playerRb = null;
        playerCol = null;
        playerMove = null;

        if (showDebug)
            Debug.Log($"[Ladder] {reasonLog}");
    }

    // ================== Debug Push Offset + Relative Dir ==================

    private void PushPlayerRelative(float dist, bool towardLadder)
    {
        if (dist <= 0f) return;
        if (playerTf == null) return;

        Vector3 beforePos = (playerRb != null) ? playerRb.position : playerTf.position;
        Vector3 ladderPos = transform.position;

        Vector3 dir = towardLadder ? (ladderPos - beforePos) : (beforePos - ladderPos);

        if (horizontalOnly)
            dir.y = 0f;

        bool usedFallback = false;
        if (dir.sqrMagnitude < 1e-6f)
        {
            dir = playerTf.forward;
            if (horizontalOnly) dir.y = 0f;
            usedFallback = true;
        }

        Vector3 dirN = dir.normalized;

        // ✅ 相對方向倍率（*2 就是在這裡）
        float m = Mathf.Max(0.01f, relativeDirMultiplier);
        Vector3 offset = dirN * (dist * m);

        if (playerRb != null)
            playerRb.MovePosition(beforePos + offset);
        else
            playerTf.position = beforePos + offset;

        Vector3 afterPos = (playerRb != null) ? playerRb.position : playerTf.position;
        float actualMoved = Vector3.Distance(beforePos, afterPos);

        if (showDebug)
        {
            Debug.Log(
                $"[Ladder] PUSH ladder={name} player={playerTf.name} " +
                $"toward={towardLadder} horizontalOnly={horizontalOnly} fallback={usedFallback} " +
                $"mult={m:F2} " +
                $"before={beforePos} ladderPos={ladderPos} " +
                $"dirN={dirN} dist={dist:F3} offset={offset} " +
                $"after={afterPos} actualMoved={actualMoved:F3}"
            );
        }

        if (drawDebugLines)
        {
            Debug.DrawRay(beforePos, dirN * (dist * m), Color.magenta, debugLineDuration);
        }
    }
}
