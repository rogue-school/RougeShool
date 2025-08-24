using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProUGUI 사용

public class InventoryRandomizer : MonoBehaviour
{
    [Header("슬롯 이미지들")]
    public Image[] slots; // 빈 슬롯들 (Inspector에서 드래그 앤 드롭)

    [Header("슬롯 이름 텍스트들")]
    public TextMeshProUGUI[] slotNames; // 슬롯마다 이름 표시

    [Header("슬롯 설명 텍스트들")]
    public TextMeshProUGUI[] slotDescriptions; // 슬롯마다 설명 표시

    [Header("랜덤으로 넣을 아이템 스프라이트들")]
    public Sprite[] itemSprites; // 랜덤으로 넣을 아이템 스프라이트들

    [Header("아이템 이름 지정")]
    public string[] itemNames; // 각 스프라이트에 대응하는 이름

    [Header("아이템 설명 지정")]
    public string[] itemDescriptions; // 각 스프라이트에 대응하는 설명


    void Start()
    {
        FillRandomItems();
    }

    void FillRandomItems()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int rand = Random.Range(0, itemSprites.Length);

            // 슬롯 이미지 설정
            slots[i].sprite = itemSprites[rand];
            slots[i].enabled = true;

            // 슬롯 이름 설정
            if (i < slotNames.Length && rand < itemNames.Length)
                slotNames[i].text = itemNames[rand];

            // 슬롯 설명 설정
            if (i < slotDescriptions.Length && rand < itemDescriptions.Length)
                slotDescriptions[i].text = itemDescriptions[rand];
        }
    }
}