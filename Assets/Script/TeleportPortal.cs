using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class TeleportPortal : MonoBehaviour
{
    public TeleportPortal otherPortal;   // 指向另一個 portal
    public string playerTag = "Player";
    public float cooldown = 0.2f;
    public float exitOffset = 1.0f;

    // 用 per-player cooldown，避免兩邊互相影響/設定錯亂
    private static readonly Dictionary<Transform, float> lastTpTime = new();

    void Reset()
    {
        // 確保是 trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        Debug.Log($"[Portal] ENTER {name}");

        if (otherPortal == null)
        {
            Debug.LogWarning($"[Portal] {name} otherPortal not set!");
            return;
        }

        var t = other.transform;

        if (lastTpTime.TryGetValue(t, out float last) && Time.time - last < cooldown)
        {
            Debug.Log($"[Portal] Cooldown for {t.name}");
            return;
        }

        lastTpTime[t] = Time.time;

        Vector3 targetPos = otherPortal.transform.position + otherPortal.transform.right * exitOffset;

        Rigidbody rb = t.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = targetPos;
        }
        else
        {
            t.position = targetPos;
        }

        Debug.Log($"[Portal] Teleport {t.name} -> {otherPortal.name} @ {targetPos}");
    }
}
