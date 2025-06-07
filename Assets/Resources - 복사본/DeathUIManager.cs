using UnityEngine;
using TMPro;

public class DeathUIManager : MonoBehaviour
{
    public GameObject deathPanel;
    public TMP_Text defeatTitleText;
    public TMP_Text stageText;
    public TMP_Text weaponText;
    public GameObject Panel;

    void Start()
    {
        deathPanel.SetActive(false);
        Panel.SetActive(false);
        defeatTitleText.gameObject.SetActive(false);
    }

    public void ShowDeathUI()
    {
        int stage = GameManager.Instance.currentStage;
        string weapon = GameManager.Instance.selectedWeapon;

        stageText.text = $"최종 도달한 스테이지: {stage}";
        weaponText.text = $"자신이 선택했던 무기: {weapon}";

        deathPanel.SetActive(true);
        Panel.SetActive(true);
        defeatTitleText.gameObject.SetActive(true);
    }
}



