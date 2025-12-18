using UnityEngine;

public class Libra_stage7 : MonoBehaviour
{
    [Header("References")]
    public Transform libra;      // 讀取它的 position.x
    public Transform object1;    // y 跟 libra x 反向
    public Transform object2;    // y 跟 object1 相反

    [Header("Mapping")]
    public float ratio = 1f;     // 等比例倍率：1 = x變-1 -> y變+1
    public bool useLocalPosition = false; // 要用 localPosition 還是 position

    [Header("Options")]
    public bool clampY = false;
    public float minY = -10f;
    public float maxY = 10f;

    float lastLibraX;

    void Start()
    {
        if (libra == null || object1 == null || object2 == null)
        {
            Debug.LogError("[LibraToObjectsY] Missing references!");
            enabled = false;
            return;
        }

        lastLibraX = GetLibraX();
    }

    void Update()
    {
        float currentX = GetLibraX();
        float deltaX = currentX - lastLibraX;  // libra x 的變化量
        lastLibraX = currentX;

        // libra.x -1 => object1.y +1  (反向)  所以 object1DeltaY = -deltaX * ratio
        float deltaY1 = -deltaX * ratio;
        float deltaY2 = +deltaX * ratio; // object2 方向相反

        ApplyDeltaY(object1, deltaY1);
        ApplyDeltaY(object2, deltaY2);
    }

    float GetLibraX()
    {
        return useLocalPosition ? libra.localPosition.x : libra.position.x;
    }

    void ApplyDeltaY(Transform target, float deltaY)
    {
        if (useLocalPosition)
        {
            Vector3 p = target.localPosition;
            p.y += deltaY;

            if (clampY) p.y = Mathf.Clamp(p.y, minY, maxY);

            target.localPosition = p;
        }
        else
        {
            Vector3 p = target.position;
            p.y += deltaY;

            if (clampY) p.y = Mathf.Clamp(p.y, minY, maxY);

            target.position = p;
        }
    }
}
