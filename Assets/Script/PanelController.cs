using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject panel;        // 띄울 패널
    public Button openButton;       // 버튼

    void Start()
    {
        // 처음에는 패널 비활성화
        panel.SetActive(false);

        // 버튼에 클릭 이벤트 연결
        openButton.onClick.AddListener(ShowPanel);
    }

    void ShowPanel()
    {
        panel.SetActive(true);
    }
}
