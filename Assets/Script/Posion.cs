using UnityEngine;
using TMPro;

public class ButtonListener : MonoBehaviour
{
    public int MaxHp = 20;
    public int currentHp;
    public int Posion = 3;
    private int prevMulyak; // 이전 물약 수 기억용

    public TextMeshProUGUI infoText;
    public GameObject targetObjectToToggle;

    void Start()
    {
        prevMulyak = Posion; // 초기값 설정
        UpdateMulyakText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Posion += 1;
        }

        // 물약 수가 바뀌었을 때만 텍스트 갱신
        if (Posion != prevMulyak)
        {
            UpdateMulyakText();
            prevMulyak = Posion;
        }
    }

    public void OnButtonClicked()
    {
        if (Posion <= 0)
        {
            return;
        }

        if (currentHp < MaxHp)
        {
            currentHp += 1;
        }

        Posion -= 1;
    }

    private void UpdateMulyakText()
    {
        if (Posion > 0)
        {
            infoText.text = $"Posion X{Posion}";
            if (targetObjectToToggle != null)
                targetObjectToToggle.SetActive(true);
        }
        else
        {
            infoText.text = "물약 없음";
            if (targetObjectToToggle != null)
                targetObjectToToggle.SetActive(false);
        }
    }
}
