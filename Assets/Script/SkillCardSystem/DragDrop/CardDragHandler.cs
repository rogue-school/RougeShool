using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Utility;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.DragDrop
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

        /// <summary>카드의 원래 형제 순서 (Index)</summary>
        private int originalSiblingIndex;

        [HideInInspector]
        public bool droppedSuccessfully = false;

		private Canvas canvas;
		private CanvasGroup canvasGroup;
		private RectTransform rectTransform;
		private Vector3 originalScale = Vector3.one;
		private Tween moveTween;
		private Tween scaleTween;
		private Tween fadeTween;
        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합

        /// <summary>
        /// CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합
        /// </summary>
        public void Inject()
        {
            // CombatSlotManager 제거됨
        }

        #region 유니티 생명주기 메서드

		private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();
			originalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;

            if (canvasGroup == null)
                GameLogger.LogError("[CardDragHandler] CanvasGroup이 없습니다!", GameLogger.LogCategory.SkillCard);
        }

        #endregion

        #region 드래그 이벤트 핸들러

        /// <summary>
        /// 드래그 시작 시 호출됩니다.
        /// 카드 투명도 조절 및 raycast 대상 비활성화
        /// </summary>
		public void OnBeginDrag(PointerEventData eventData)
        {
            // 좌클릭만 허용
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!CanDrag()) return;

            // 원래 부모와 순서 저장
            OriginalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            OriginalWorldPosition = transform.position;
			canvasGroup.alpha = 1f;
			canvasGroup.blocksRaycasts = false;

            foreach (var img in GetComponentsInChildren<Image>())
                img.raycastTarget = false;

            // 드래그 중에만 최상위로 올리기 (다른 UI 요소 위에 표시)
            transform.SetParent(canvas.transform, true);
            transform.SetAsLastSibling();
            
            // SkillCardUI에 드래그 시작 알림
            var skillCardUI = GetComponent<SkillCardUI>();
            if (skillCardUI != null)
            {
                skillCardUI.OnDragStart();
            }
            
            // 드래그 시작 애니메이션 호출
            PlayDragStartAnimation();
        }

        /// <summary>
        /// 드래그 중 호출됩니다. 마우스 위치에 따라 카드 위치 이동
        /// </summary>
		public void OnDrag(PointerEventData eventData)
        {
            // 좌클릭만 허용
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!CanDrag()) return;

			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
				canvas.transform as RectTransform,
				eventData.position,
				eventData.pressEventCamera,
				out Vector3 worldPoint))
			{
				moveTween?.Kill(false);
				moveTween = rectTransform.DOMove(worldPoint, 0.08f).SetEase(Ease.OutQuad);
			}
        }

        /// <summary>
        /// 드래그 종료 시 호출됩니다.
        /// 유효하지 않은 위치일 경우 카드 원래 위치로 복원
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            // 좌클릭만 허용
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!CanDrag())
            {
                ResetToOrigin(GetComponent<SkillCardUI>());
                return;
            }

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

            bool dropFailed = !droppedSuccessfully || !validDropTargetFound;
            
            // SkillCardUI에 드래그 종료 알림 (성공/실패 관계없이)
            var skillCardUI = GetComponent<SkillCardUI>();
            if (skillCardUI != null)
            {
                skillCardUI.OnDragEnd();
            }
            
            if (dropFailed)
            {
                // 드롭 실패 시 원래 자리로 돌아가는 애니메이션 실행
                ResetToOrigin(skillCardUI);
                // 상호작용 복원은 PlayDropFailAnimation에서 처리됨
            }
            else
            {
                // 드롭 성공 시에만 드래그 종료 애니메이션 호출
                PlayDragEndAnimation();
                
                // UI 상태 리셋 (드롭 성공 시에만)
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                droppedSuccessfully = false;

                foreach (var img in GetComponentsInChildren<Image>())
                    img.raycastTarget = true;
            }
        }

        #endregion

        #region 내부 메서드

        /// <summary>
        /// 드래그 가능 여부 확인 (플레이어 카드만 드래그 가능)
        /// </summary>
        private bool CanDrag()
        {
            var skillCardUI = GetComponent<SkillCardUI>();
            if (skillCardUI == null)
            {
                return false;
            }

            var card = skillCardUI.GetCard();
            if (card == null)
            {
                return false;
            }

            // 플레이어 카드만 드래그 가능
            return card.IsFromPlayer();
        }

        /// <summary>
        /// 카드를 원래 위치로 되돌립니다. (드롭 실패 시 부드러운 애니메이션과 함께)
        /// </summary>
        /// <param name="cardUI">복원할 카드 UI</param>
		public void ResetToOrigin(SkillCardUI cardUI)
        {
            if (cardUI == null)
            {
                GameLogger.LogWarning("[CardDragHandler] ResetToOrigin 실패 - cardUI가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

			// 드롭 실패 애니메이션 실행
			PlayDropFailAnimation(cardUI);
        }

        /// <summary>
        /// 드롭 실패 시 원래 자리로 돌아가는 애니메이션
        /// </summary>
		private void PlayDropFailAnimation(SkillCardUI cardUI)
        {
            // 애니메이션 중 상호작용 차단
            canvasGroup.blocksRaycasts = false;
            foreach (var img in GetComponentsInChildren<Image>())
                img.raycastTarget = false;
			
			// 월드 위치로 부드럽게 복귀 후 원상 복원
			moveTween?.Kill(false);
			scaleTween?.Kill(false);
			fadeTween?.Kill(false);
			var duration = 0.18f;
			moveTween = rectTransform.DOMove(OriginalWorldPosition, duration).SetEase(Ease.OutQuad);
			scaleTween = rectTransform.DOScale(originalScale, duration).SetEase(Ease.OutQuad);
			fadeTween = canvasGroup.DOFade(1f, duration * 0.8f).SetEase(Ease.OutQuad);
			moveTween.onComplete = () =>
			{
				CardSlotHelper.ResetCardToOriginal(cardUI);
				canvasGroup.alpha = 1f;
				canvasGroup.blocksRaycasts = true;
				foreach (var img in GetComponentsInChildren<Image>())
					img.raycastTarget = true;
			};
        }

        /// <summary>
        /// 드래그 시작 애니메이션 실행
        /// </summary>
		private void PlayDragStartAnimation()
        {
            var skillCardUI = GetComponent<SkillCardUI>();
            if (skillCardUI != null)
            {
                var card = skillCardUI.GetCard();
                if (card != null)
                {
					// 확대 + 약간의 투명도 조정
					moveTween?.Kill(false);
					scaleTween?.Kill(false);
					fadeTween?.Kill(false);
					scaleTween = rectTransform.DOScale(originalScale * 1.08f, 0.12f).SetEase(Ease.OutQuad);
					fadeTween = canvasGroup.DOFade(0.9f, 0.12f).SetEase(Ease.OutQuad);
                }
            }
        }

        /// <summary>
        /// 드래그 종료 애니메이션 실행
        /// </summary>
		private void PlayDragEndAnimation()
        {
            var skillCardUI = GetComponent<SkillCardUI>();
            if (skillCardUI != null)
            {
                var card = skillCardUI.GetCard();
                if (card != null)
                {
					// 원래 스케일/불투명도로 복귀 (작은 탄성)
					scaleTween?.Kill(false);
					fadeTween?.Kill(false);
					scaleTween = rectTransform.DOScale(originalScale, 0.12f).SetEase(Ease.OutQuad);
					fadeTween = canvasGroup.DOFade(1f, 0.1f).SetEase(Ease.OutQuad);
                }
            }
        }

        #endregion
    }
}
