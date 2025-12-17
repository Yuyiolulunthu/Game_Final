using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Ladder : MonoBehaviour
{
    [Header("Drop Settings")]
    public float dropSpeed = 2f;
    public string playerTag = "Player";

    [Header("Debug")]
    public bool showDebug = false;
    public float logInterval = 0.1f;

    private float ladderBottomY;
    private Transform playerTf;
    private Rigidbody playerRb;
    private PlayerMove playerMove;
    private float nextLogTime;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        ladderBottomY = col.bounds.min.y;

        if (showDebug) { 
            //Debug.Log($"[Ladder] bottomY={ladderBottomY:F2} name={name}");
            }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerTf = other.transform;
        playerRb = other.attachedRigidbody;
        playerMove = other.GetComponentInParent<PlayerMove>();

        // 1️⃣ 先停掉 PlayerMove，避免它立刻把位置拉回
        if (playerMove) playerMove.enabled = false;

        // 2️⃣ 進梯子瞬間，先往前推 0.5
        if (playerRb != null)
        {
           
            Vector3 forwardOffset = new Vector3(0.5f, 0f, 0f);
            playerRb.MovePosition(playerRb.position + forwardOffset);
        }
        else
        {
            // 保底（理論上不會用到）
            playerTf.position += playerTf.forward * 0.5f;
        }

        if (showDebug)
        {
            //Debug.Log("[Ladder] ENTER: push forward 0.5, disable PlayerMove");
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (playerMove) playerMove.enabled = true;

        playerTf = null;
        playerRb = null;
        playerMove = null;

        if (showDebug)
        {
            //Debug.Log("[Ladder] EXIT: enable PlayerMove");
        }
    }

    void FixedUpdate()
    {
        if (playerTf == null) return;

        // 沒有 Rigidbody 就直接改 transform（但你的 Player 通常會有 rb）
        if (playerRb == null)
        {
            Vector3 p = playerTf.position;
            if (p.y > ladderBottomY)
            {
                p.y = Mathf.Max(ladderBottomY, p.y - dropSpeed * Time.fixedDeltaTime);
                playerTf.position = p;
            }
            return;
        }

        // 用 rb.MovePosition 才不會被物理系統「打回去」
        Vector3 pos = playerRb.position;

        if (pos.y > ladderBottomY)
        {
            float beforeY = pos.y;
            float newY = Mathf.Max(ladderBottomY, pos.y - dropSpeed * Time.fixedDeltaTime);

            playerRb.velocity = new Vector3(playerRb.velocity.x, 0f, playerRb.velocity.z); // 避免重力/跳躍干擾
            playerRb.MovePosition(new Vector3(pos.x, newY, pos.z));

            if (showDebug && Time.time >= nextLogTime)
            {
                nextLogTime = Time.time + logInterval;
                //Debug.Log($"[Ladder] Dropping: y {beforeY:F2} -> {newY:F2}");
            }
        }
        else
        {
            if (showDebug && Time.time >= nextLogTime)
            {
                nextLogTime = Time.time + logInterval;
                //Debug.Log("[Ladder] Reached bottom");
            }
        }
    }
}
