using UnityEngine;
using UnityEngine.EventSystems;

public class PotionClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Transform inventoryParent; // 인벤토리 패널 Transform

    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭 시, 이 포션 오브젝트를 인벤토리 안으로 이동
        if (inventoryParent != null)
        {
            transform.SetParent(inventoryParent);
            // 위치 초기화 (필요 시 조절)
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            Debug.Log("포션이 인벤토리에 들어갔습니다.");
        }
        else
        {
            Debug.LogWarning("Inventory Parent가 할당되지 않았습니다.");
        }
    }
}