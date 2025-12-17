using UnityEngine;

public class TestHint : MonoBehaviour
{
    void Start()
    {
        HintPanel.Instance.ShowHint("Try to touch items to collect them!");
    }
}
