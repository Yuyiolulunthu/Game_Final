using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Movable Block")]
    public Transform targetBlock;

    [Header("Position Set")]
    public Vector3 hiddenPosition;   
    public Vector3 shownPosition;    
    public float moveSpeed = 2f;

    [Header("Activators")]
    public LayerMask activatorMask;  

    private int pressCount = 0;      

    void Start()
    {
        if (targetBlock != null)
        {
            targetBlock.position = hiddenPosition;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsActivator(other.gameObject))
        {
            pressCount++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsActivator(other.gameObject))
        {
            pressCount = Mathf.Max(0, pressCount - 1);
        }
    }

    void Update()
    {
        if (!targetBlock) return;

        // 有東西壓住 -> 浮出來，否則 -> 回到地下
        Vector3 targetPos = (pressCount > 0) ? shownPosition : hiddenPosition;

        targetBlock.position = Vector3.MoveTowards(
            targetBlock.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }

    bool IsActivator(GameObject obj)
    {
        // 用 Layer 判斷
        return ((1 << obj.layer) & activatorMask) != 0;
    }
}
