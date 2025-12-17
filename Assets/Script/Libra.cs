using UnityEngine;

public class Libra : MonoBehaviour
{
    [Header("Target Objects")]
    public Transform ZZZ;        // 取得 z 軸的物件
    public Transform YYY;        // 正向移動的物件（y 軸）
    public Transform minus_YYY;  // 反向移動的物件（y 軸）

    [Header("Mapping")]
    public float scale = 1f;     // 比例（1 = z 減 1，y 加 1）

    private float baseZZZ_Z;
    private float baseYYY_Y;
    private float baseMinusYYY_Y;

    void Start()
    {
        if (ZZZ == null || YYY == null || minus_YYY == null)
        {
            Debug.LogError("[Libra] ZZZ / YYY / minus_YYY not assigned!");
            enabled = false;
            return;
        }

        // 記住初始位置作為基準
        baseZZZ_Z = ZZZ.position.z;
        baseYYY_Y = YYY.position.y;
        baseMinusYYY_Y = minus_YYY.position.y;
    }

    void Update()
    {
        float deltaZ = baseZZZ_Z - ZZZ.position.z;

        // --- 正向 YYY ---
        Vector3 posYYY = YYY.position;
        posYYY.y = baseYYY_Y + deltaZ * scale;
        YYY.position = posYYY;

        // --- 反向 minus_YYY ---
        Vector3 posMinus = minus_YYY.position;
        posMinus.y = baseMinusYYY_Y - deltaZ * scale;
        minus_YYY.position = posMinus;
    }
}
