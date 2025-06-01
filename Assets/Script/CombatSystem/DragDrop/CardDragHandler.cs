using UnityEngine;
using UnityEngine.EventSystems;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
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
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!PlayerInputGuard.CanProceed(flowCoordinator)) return;

            OriginalParent ??= transform.parent;
            OriginalWorldPosition = transform.position;

            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;
            transform.SetParent(canvas.transform, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!PlayerInputGuard.CanProceed(flowCoordinator)) return;

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
                CardSlotHelper.ResetCardToOriginal(GetComponent<SkillCardUI>());

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            droppedSuccessfully = false;
        }
    }
}
