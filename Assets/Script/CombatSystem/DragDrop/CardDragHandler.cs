using UnityEngine;
using UnityEngine.EventSystems;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.UI;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Vector3 OriginalPosition { get; private set; }
    public Transform OriginalParent { get; private set; }
    public bool droppedSuccessfully = false;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OriginalPosition = transform.localPosition;
        OriginalParent = transform.parent;
        droppedSuccessfully = false;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(GetComponent<Canvas>());

        if (!droppedSuccessfully)
        {
            transform.SetParent(OriginalParent);
            transform.localPosition = OriginalPosition;
            transform.localScale = Vector3.one;

            //  슬롯에서 나 자신을 제거해줘야 겹침 방지됨
            var slot = GetComponentInParent<ICombatCardSlot>();
            if (slot != null && slot.GetCardUI() == GetComponent<SkillCardUI>())
            {
                slot.Clear();
            }

            Debug.LogWarning("[CardDragHandler] 드롭 실패 - 위치 복귀 및 슬롯 정리 완료");
        }
    }
}
