using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotAirBalloonLoop : MonoBehaviour
{
    [Header("Path Settings")]
    public Vector3 startPos = new Vector3(56.9f, 38.6f, 5.4f);
    public Vector3 endPos = new Vector3(-100f, 38.6f, 5.4f);

    [Header("Movement")]
    [Tooltip("移動速度（單位：每秒）")]
    public float speed = 2f;

    void Start()
    {
        // 確保一開始在起點
        transform.position = startPos;
    }

    void Update()
    {
        // 往終點直線移動
        transform.position = Vector3.MoveTowards(
            transform.position,
            endPos,
            speed * Time.deltaTime
        );

        // 到達終點 → 瞬間重置回起點
        if (Vector3.Distance(transform.position, endPos) < 0.01f)
        {
            transform.position = startPos;
        }
    }
}
