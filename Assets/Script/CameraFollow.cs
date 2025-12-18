using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;   // Player 的 Transform

    [Header("Z Follow Settings")]
    public float zOffset = 8f;   // Camera 與 Player 的距離（通常是負值）
    public float followSpeed = 10f; // 跟隨平滑度（越大越快）

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 camPos = transform.position;
        float targetZ = player.position.z + zOffset;

        // 只改 Z 軸
        camPos.z = Mathf.Lerp(camPos.z, targetZ, followSpeed * Time.deltaTime);

        transform.position = camPos;
    }
}
