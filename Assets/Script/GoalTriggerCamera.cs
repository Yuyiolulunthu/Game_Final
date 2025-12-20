using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalTriggerCamera : MonoBehaviour
{
    public string playerTag = "Player";
    public MainCameraZoomOnGoal mainCamera;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (mainCamera != null)
            mainCamera.ZoomIn();
    }
}
