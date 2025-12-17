using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HintPanel : MonoBehaviour
{
    public static HintPanel Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Button okButton;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        okButton.onClick.AddListener(CloseHint);
    }

    public void ShowHint(string message)
    {
        hintText.text = message;
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseHint()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
