using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.DragDrop
{
    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Transform OriginalParent { get; set; }
        public Vector3 OriginalWorldPosition { get; set; }

        [HideInInspector]
        public bool droppedSuccessfully = false;

        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private ICombatFlowCoordinator flowCoordinator;

        public void Inject(ICombatFlowCoordinator coordinator)
        {
            this.flowCoordinator = coordinator;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();

            if (canvasGroup == null)
                Debug.LogError("[CardDragHandler] CanvasGroup이 없습니다!");
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanDrag()) return;

            OriginalWorldPosition = transform.position;
            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;

            foreach (var img in GetComponentsInChildren<Image>())
                img.raycastTarget = false;

            transform.SetParent(canvas.transform, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!CanDrag()) return;

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
            if (!CanDrag())
            {
                ResetToOrigin(GetComponent<SkillCardUI>());
                return;
            }

            Debug.Log("[CardDragHandler] OnEndDrag 호출됨");

            bool validDropTargetFound = false;
            var raycastResults = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            foreach (var result in raycastResults)
            {
                if (result.gameObject.GetComponent<IDropHandler>() != null)
                {
                    validDropTargetFound = true;
                    break;
                }
            }

            if (!droppedSuccessfully || !validDropTargetFound)
            {
                ResetToOrigin(GetComponent<SkillCardUI>());
            }

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            droppedSuccessfully = false;

            foreach (var img in GetComponentsInChildren<Image>())
                img.raycastTarget = true;
        }

        private bool CanDrag()
        {
            return flowCoordinator != null && flowCoordinator.IsPlayerInputEnabled();
        }

        public void ResetToOrigin(SkillCardUI cardUI)
        {
            if (cardUI == null)
            {
                Debug.LogWarning("[CardDragHandler] ResetToOrigin 실패 - cardUI가 null입니다.");
                return;
            }

            CardSlotHelper.ResetCardToOriginal(cardUI);
        }
    }
}
