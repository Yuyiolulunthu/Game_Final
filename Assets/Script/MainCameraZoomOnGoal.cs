using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraZoomOnGoal : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("玩家物件（用來比對 tag 也可）")]
    public string playerTag = "Player";

    [Header("Zoom Settings")]
    [Tooltip("拉近後 Camera 的世界座標位置")]
    public Vector3 zoomPosition;

    [Tooltip("拉近後 Camera 的世界旋轉")]
    public Vector3 zoomEulerRotation;

    [Tooltip("拉近移動所需時間（秒）")]
    public float moveDuration = 1.0f;

    [Tooltip("是否同時套用旋轉")]
    public bool applyRotation = true;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isMoving = false;
    private float timer = 0f;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (!isMoving) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / moveDuration);

        transform.position = Vector3.Lerp(originalPosition, zoomPosition, t);

        if (applyRotation)
        {
            Quaternion targetRot = Quaternion.Euler(zoomEulerRotation);
            transform.rotation = Quaternion.Slerp(originalRotation, targetRot, t);
        }

        if (t >= 1f)
            isMoving = false;
    }

    //  給 Goal 呼叫
    public void ZoomIn()
    {
        timer = 0f;
        isMoving = true;
    }
}
