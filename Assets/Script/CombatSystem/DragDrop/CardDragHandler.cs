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

        [HideInInspector] // 인스펙터에서 제거하여 직렬화되지 않도록 방지
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

            droppedSuccessfully = false; // 안전한 기본값 초기화
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!PlayerInputGuard.CanProceed(flowCoordinator)) return;

            // OriginalParent는 카드 생성 시 이미 설정됨
            OriginalWorldPosition = transform.position;

            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;

            // 드래그 시 UI 최상위로 이동
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
            {
                CardSlotHelper.ResetCardToOriginal(GetComponent<SkillCardUI>());
            }

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            droppedSuccessfully = false; // 항상 다음 드래그를 위해 초기화
        }
    }
}
