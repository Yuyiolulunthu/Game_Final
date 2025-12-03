using UnityEngine;

public class DirectionalLightRotator : MonoBehaviour
{
    public Vector3 angleA = new Vector3(133f, 160f, 194f); // 554 → 194
    public Vector3 angleB = new Vector3(153f, 154f, 190f); // 550 → 190
    public float speed = 1f;

    private bool canRotate = false;
    private float t = 0f;

    // 給外部呼叫：開始與停止旋轉
    public void StartRotation()
    {
        canRotate = true;
    }

    public void StopRotation()
    {
        canRotate = false;
    }

    void Update()
    {
        if (!canRotate) return;

        t = Mathf.PingPong(Time.time * speed, 1f);

        Quaternion rotA = Quaternion.Euler(angleA);
        Quaternion rotB = Quaternion.Euler(angleB);

        transform.rotation = Quaternion.Lerp(rotA, rotB, t);
    }
}
