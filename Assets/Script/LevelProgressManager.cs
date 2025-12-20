using UnityEngine;

public static class LevelProgressManager
{
    private const string KEY = "GameProgress";
    private const float MIN_PROGRESS = 0.5f;

    public static float Get()
    {
        float value = PlayerPrefs.GetFloat(KEY, MIN_PROGRESS);
        value = Mathf.Max(value, MIN_PROGRESS);

        Debug.Log($"[Progress] Get = {value}");
        return value;
    }
    public static void ForceSet(float value)
    {
        value = Mathf.Clamp(value, 0.5f, float.MaxValue);

        PlayerPrefs.SetFloat(KEY, value);
        PlayerPrefs.Save();

        Debug.Log($"[Progress] ForceSet = {value}");
    }
    /// <summary>
    /// 設定進度（不倒退、不超過 max、不小於 1）
    /// </summary>
    public static void SetTo(float target, float max)
    {
        float current = Get();
        float finalValue = Mathf.Clamp(
            Mathf.Max(current, target),
            MIN_PROGRESS,
            max
        );

        PlayerPrefs.SetFloat(KEY, finalValue);
        PlayerPrefs.Save();

        Debug.Log($"[Progress] SetTo: {current} → {finalValue}");
    }

    public static void Reset()
    {
        PlayerPrefs.SetFloat(KEY, MIN_PROGRESS);
        PlayerPrefs.Save();
        Debug.Log("[Progress] Reset to 1");
    }
}
