using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pressText;   // 「press any button」文字
    public GameObject mainPanel;   // 主選單面板

    [Header("Buttons")]
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    [Header("Scene Names")]
    public string scene1Name;
    public string scene2Name;
    public string scene3Name;
    public string scene4Name;

    private bool hasPressed = false;

    void Start()
    {
        // 初始狀態：只顯示 press 提示
        pressText.SetActive(true);
        mainPanel.SetActive(false);

        // 綁定按鈕事件
        if (button1 != null) button1.onClick.AddListener(() => LoadScene(scene1Name));
        if (button2 != null) button2.onClick.AddListener(() => LoadScene(scene2Name));
        if (button3 != null) button3.onClick.AddListener(() => LoadScene(scene3Name));
        if (button4 != null) button4.onClick.AddListener(() => LoadScene(scene4Name));
    }

    void Update()
    {
        if (!hasPressed && Input.anyKeyDown)
        {
            hasPressed = true;
            pressText.SetActive(false);
            mainPanel.SetActive(true);
        }
    }

    void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("⚠ 尚未設定場景名稱！");
        }
    }
}
