using UnityEngine;

public class ProximityPanelTrigger : MonoBehaviour
{
    [Header("玩家與距離")]
    public Transform player;
    public float detectDistance = 3f;

    [Header("顯示物件")]
    public GameObject hintTextObject; // SPACE for more
    public GameObject panelObject;    // 跳出的 Panel

    private bool isNear = false;
    private bool isPanelOpen = false;

    void Start()
    {
        if (hintTextObject != null)
            hintTextObject.SetActive(false);

        if (panelObject != null)
            panelObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        isNear = distance <= detectDistance;

        // 只有「靠近 + Panel 沒開」才顯示提示
        if (hintTextObject != null)
            hintTextObject.SetActive(isNear && !isPanelOpen);

        // 按 Space 打開 Panel
        if (isNear && !isPanelOpen && Input.GetKeyDown(KeyCode.Space))
        {
            OpenPanel();
        }
    }

    void OpenPanel()
    {
        isPanelOpen = true;

        if (panelObject != null)
            panelObject.SetActive(true);

        if (hintTextObject != null)
            hintTextObject.SetActive(false);

        // 暫停遊戲
        Time.timeScale = 0f;
    }

    // 👉 給「關閉按鈕」用
    public void ClosePanel()
    {
        isPanelOpen = false;

        if (panelObject != null)
            panelObject.SetActive(false);

        // 恢復遊戲
        Time.timeScale = 1f;
    }
}
