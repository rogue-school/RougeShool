using UnityEngine;
using UnityEngine.EventSystems;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.DragDrop
{
    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Transform OriginalParent { get; set; }
        public Vector3 OriginalWorldPosition { get; set; }
        public bool droppedSuccessfully = false;

        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // 위치는 처음 드래그 시작할 때만 저장
            if (OriginalParent == null)
            {
                OriginalParent = transform.parent;
                OriginalWorldPosition = transform.position;
                Debug.Log($"[DragHandler] 최초 저장: 위치={OriginalWorldPosition}, 부모={OriginalParent.name}");
            }

            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;
            transform.SetParent(canvas.transform, true); // worldPosition 유지

            Debug.Log($"[DragHandler] 드래그 시작: {gameObject.name}, 현재 위치: {transform.position}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canvas == null) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );
            rectTransform.localPosition = localPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!droppedSuccessfully)
            {
                CardSlotHelper.ResetCardToOriginal(GetComponent<SkillCardSystem.UI.SkillCardUI>());
                Debug.Log($"[DragHandler] 유효 슬롯 위에 드롭되지 않음. 복귀 처리됨");
            }
            else
            {
                Debug.Log("[DragHandler] 드롭 성공");
            }

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            droppedSuccessfully = false;

            Debug.Log("[DragHandler] 드래그 종료");
        }
    }
}
