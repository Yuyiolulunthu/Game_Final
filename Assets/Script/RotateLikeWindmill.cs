using UnityEngine;

public class RotateLikeWindmill : MonoBehaviour
{
    [Header("Windmill Rotation")]
    [Tooltip("每秒旋轉角度（360 = 一秒一圈）")]
    public float speed = 180f;

    [Tooltip("是否使用 local Z 軸旋轉")]
    public bool useLocalRotation = true;

    void Update()
    {
        float angle = speed * Time.deltaTime;

        if (useLocalRotation)
        {
            // 像風車一樣，沿著自己的 Z 軸轉
            transform.Rotate(0f, 0f, angle, Space.Self);
        }
        else
        {
            // 沿世界 Z 軸轉
            transform.Rotate(0f, 0f, angle, Space.World);
        }
    }
}
