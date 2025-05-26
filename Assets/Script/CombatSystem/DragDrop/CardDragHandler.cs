using UnityEngine;
using UnityEngine.EventSystems;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Core;
using Game.SkillCardSystem.UI;

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
        private ICombatFlowCoordinator flowCoordinator;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();
            flowCoordinator = Object.FindFirstObjectByType<CombatFlowCoordinator>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (flowCoordinator == null || !flowCoordinator.IsPlayerInputEnabled())
            {
                //Debug.LogWarning("[DragHandler] 현재 상태에서 드래그 불가");
                return;
            }

            if (OriginalParent == null)
            {
                OriginalParent = transform.parent;
                OriginalWorldPosition = transform.position;
                //Debug.Log($"[DragHandler] 최초 저장: 위치={OriginalWorldPosition}, 부모={OriginalParent?.name}");
            }

            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;
            transform.SetParent(canvas.transform, true);
            //Debug.Log($"[DragHandler] 드래그 시작: {gameObject.name}, 현재 위치: {transform.position}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canvas == null || flowCoordinator == null || !flowCoordinator.IsPlayerInputEnabled()) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                rectTransform.localPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!droppedSuccessfully)
            {
                CardSlotHelper.ResetCardToOriginal(GetComponent<SkillCardUI>());
                Debug.Log("[DragHandler] 유효 슬롯 위에 드롭되지 않음. 복귀 처리됨");
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
