using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject panelA;
   

    // 1. 처음 버튼 누르면 PanelA 활성화
    public void ShowPanelA()
    {
        panelA.SetActive(true);
    }

  
}
