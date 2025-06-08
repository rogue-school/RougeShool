using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.DragDrop
{
    /// <summary>
    /// 카드 드래그 앤 드롭을 처리하는 핸들러입니다.
    /// 드래그 시작/중단/종료 시 UI 반응과 슬롯 배치를 제어합니다.
    /// </summary>
    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>카드의 원래 부모 트랜스폼</summary>
        public Transform OriginalParent { get; set; }

        /// <summary>카드의 드래그 전 월드 위치</summary>
        public Vector3 OriginalWorldPosition { get; set; }

        [HideInInspector]
        public bool droppedSuccessfully = false;

        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private ICombatFlowCoordinator flowCoordinator;

        /// <summary>
        /// 전투 흐름 관리자 주입 (입력 가능 여부 판단용)
        /// </summary>
        public void Inject(ICombatFlowCoordinator coordinator)
        {
            this.flowCoordinator = coordinator;
        }

        #region 유니티 생명주기 메서드

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();

            if (canvasGroup == null)
                Debug.LogError("[CardDragHandler] CanvasGroup이 없습니다!");
        }

        #endregion

        #region 드래그 이벤트 핸들러

        /// <summary>
        /// 드래그 시작 시 호출됩니다.
        /// 카드 투명도 조절 및 raycast 대상 비활성화
        /// </summary>
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

        /// <summary>
        /// 드래그 중 호출됩니다. 마우스 위치에 따라 카드 위치 이동
        /// </summary>
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

        /// <summary>
        /// 드래그 종료 시 호출됩니다.
        /// 유효하지 않은 위치일 경우 카드 원래 위치로 복원
        /// </summary>
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

        #endregion

        #region 내부 메서드

        /// <summary>
        /// 드래그 가능 여부 확인 (입력 차단 상태인지 확인)
        /// </summary>
        private bool CanDrag()
        {
            return flowCoordinator != null && flowCoordinator.IsPlayerInputEnabled();
        }

        /// <summary>
        /// 카드를 원래 위치로 되돌립니다.
        /// </summary>
        /// <param name="cardUI">복원할 카드 UI</param>
        public void ResetToOrigin(SkillCardUI cardUI)
        {
            if (cardUI == null)
            {
                Debug.LogWarning("[CardDragHandler] ResetToOrigin 실패 - cardUI가 null입니다.");
                return;
            }

            CardSlotHelper.ResetCardToOriginal(cardUI);
        }

        #endregion
    }
}
