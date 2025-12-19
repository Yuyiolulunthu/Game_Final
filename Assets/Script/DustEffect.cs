using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustEffect : MonoBehaviour
{
    [Header("Dust Effect")]
    [Tooltip("開始移動時播放的塵土特效 Prefab")]
    public ParticleSystem dustEffectPrefab;

    [Header("Detect Movement")]
    [Tooltip("判定為開始移動的最小位移")]
    public float moveThreshold = 0.001f;

    [Tooltip("使用 localPosition 判斷移動")]
    public bool useLocalPosition = false;

    [Header("Spawn Offset (Local Space)")]
    [Tooltip("特效相對於物體的 local 座標位移 (X:左右, Y:上下, Z:前後；負Z = 背後)")]
    public Vector3 localSpawnOffset = new Vector3(0f, -0.5f, -0.3f);

    [Tooltip("是否讓特效跟著物體")]
    public bool followTarget = false;

    private Vector3 lastPos;
    private bool hasPlayed = false;

    void Start()
    {
        lastPos = GetPosition();
    }

    void Update()
    {
        if (hasPlayed) return;

        Vector3 currentPos = GetPosition();
        float distance = Vector3.Distance(currentPos, lastPos);

        // 從靜止 → 開始移動
        if (distance > moveThreshold)
        {
            PlayDust();
            hasPlayed = true;
        }

        lastPos = currentPos;
    }

    Vector3 GetPosition()
    {
        return useLocalPosition ? transform.localPosition : transform.position;
    }

    void PlayDust()
    {
        if (dustEffectPrefab == null) return;

        // local → world，完全跟著物體方向
        Vector3 spawnPos = transform.TransformPoint(localSpawnOffset);

        ParticleSystem ps;
        if (followTarget)
            ps = Instantiate(dustEffectPrefab, spawnPos, Quaternion.identity, transform);
        else
            ps = Instantiate(dustEffectPrefab, spawnPos, Quaternion.identity);

        ps.Play();

        float lifetime = GetLifetime(ps);
        Destroy(ps.gameObject, lifetime + 0.1f);
    }

    float GetLifetime(ParticleSystem ps)
    {
        var main = ps.main;
        float duration = main.duration;

        var lt = main.startLifetime;
        float lifetimeMax = 0f;

        switch (lt.mode)
        {
            case ParticleSystemCurveMode.Constant:
                lifetimeMax = lt.constant;
                break;
            case ParticleSystemCurveMode.TwoConstants:
                lifetimeMax = lt.constantMax;
                break;
            default:
                lifetimeMax = 1f; // 保守值
                break;
        }

        return duration + lifetimeMax;
    }
}
