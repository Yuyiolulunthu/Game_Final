using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    [SerializeField] private string hintKey = "hint_01";
    [TextArea]
    [SerializeField] private string hintMessage = "Hint message here";
    [SerializeField] private bool alwaysShow = false;  // 測試用

    void Start()
    {
        if (alwaysShow || !PlayerPrefs.HasKey(hintKey))
        {
            HintPanel.Instance.ShowHint(hintMessage);
            PlayerPrefs.SetInt(hintKey, 1);
            PlayerPrefs.Save();
        }
    }
}
