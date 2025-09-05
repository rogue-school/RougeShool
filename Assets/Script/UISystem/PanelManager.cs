using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject panelA;

    // 비활성화할 오브젝트들 추가
    public GameObject panelToDisable;      // DeathPanel > Panel
    public GameObject defeatTextToDisable; // DeathPanel > DefeatText

    // 버튼을 눌렀을 때 실행
    public void ShowPanelA()
    {
        panelA.SetActive(true);

        if (panelToDisable != null)
            panelToDisable.SetActive(false);

        if (defeatTextToDisable != null)
            defeatTextToDisable.SetActive(false);
    }
}
