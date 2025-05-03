using UnityEngine;
using TMPro;

public class ButtonListener : MonoBehaviour
{
    public int MaxHp = 20;
    public int currentHp;
    public int mulyak = 3;
    private int prevMulyak; // 이전 물약 수 기억용

    public TextMeshProUGUI infoText;
    public GameObject targetObjectToToggle;

    void Start()
    {
        prevMulyak = mulyak; // 초기값 설정
        UpdateMulyakText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            mulyak += 1;
        }

        // 물약 수가 바뀌었을 때만 텍스트 갱신
        if (mulyak != prevMulyak)
        {
            UpdateMulyakText();
            prevMulyak = mulyak;
        }
    }

    public void OnButtonClicked()
    {
        if (mulyak <= 0)
        {
            return;
        }

        if (currentHp < MaxHp)
        {
            currentHp += 1;
        }

        mulyak -= 1;
    }

    private void UpdateMulyakText()
    {
        if (mulyak > 0)
        {
            infoText.text = $"mulyak X{mulyak}";
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
