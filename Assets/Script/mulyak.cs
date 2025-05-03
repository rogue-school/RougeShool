using UnityEngine;
using TMPro;

public class ButtonListener : MonoBehaviour
{
    public int MaxHp = 20;
    public int currentHp;
    public int mulyak = 3;
    public TextMeshProUGUI infoText;
    public GameObject targetObjectToDisable;

    public void OnButtonClicked()
    {
        // 물약이 없으면 아무 동작도 하지 않음
        if (mulyak <= 0)
        {
            infoText.text = "물약 없음";

            if (targetObjectToDisable != null)
            {
                targetObjectToDisable.SetActive(false);
            }

            return;
        }

        // 체력 회복 (최대 체력 초과 방지)
        if (currentHp < MaxHp)
        {
            currentHp += 1;
        }

        // 물약 사용
        mulyak -= 1;

        // 텍스트 갱신: 물약이 남아 있으면 수 표시, 없으면 "물약 없음"
        if (mulyak > 0)
        {
            infoText.text = $"mulyak X{mulyak}";
        }
        else
        {
            infoText.text = "물약 없음";

            if (targetObjectToDisable != null)
            {
                targetObjectToDisable.SetActive(false);
            }
        }
    }
}