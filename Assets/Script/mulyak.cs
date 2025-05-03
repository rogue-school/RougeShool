using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 추가

public class ButtonListener : MonoBehaviour
{
    public int MaxHp = 20;
    public int currentHp;
    public int mulyak = 3;
    public TextMeshProUGUI infoText; // TextMeshPro용 텍스트 컴포넌트

    public void OnButtonClicked()
    {
        print("물약을 사용했습니다.");

        // 텍스트 먼저 설정
        if (mulyak == 3)
        {
            infoText.text = "mulyak X3";
        }
        else if (mulyak == 2)
        {
            infoText.text = "mulyak X2";
        }
        else if (mulyak == 1)
        {
            infoText.text = "mulyak X1";
        }
        else
        {
            infoText.text = "none mulyak";
            return; // 물약이 없으면 종료
        }

        // 체력 회복
        if (currentHp < MaxHp)
        {
            currentHp += 1;
        }

        // 물약 소모
        mulyak -= 1;
    }
}