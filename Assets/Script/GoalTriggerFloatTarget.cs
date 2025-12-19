using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalTriggerFloatTarget : MonoBehaviour
{
    [Header("Detect")]
    public string playerTag = "Player";

    [Header("Target To Float")]
    public FloatUpOnGoalTouch target; // 把要上浮的物件（上面那支腳本）拖進來

    void Reset()
    {
        // Goal 建議用 Trigger
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (target == null) return;

        target.TriggerByGoal();
    }
}
