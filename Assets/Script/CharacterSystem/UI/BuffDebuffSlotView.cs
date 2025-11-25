using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.SkillCardSystem.UI.Mappers;
using Game.ItemSystem.Constants;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 버프/디버프 슬롯의 간단한 뷰.
    /// 아이콘과 남은 턴 수만 표시하며, 0턴이 되면 자동으로 비활성화한다.
    /// 호버 시 툴팁을 표시하여 현재 효과 정보를 보여준다.
    /// </summary>
    public class BuffDebuffSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Serialized Fields

        [Header("아이콘")]
        [Tooltip("효과 아이콘 이미지")]
        [SerializeField] private Image iconImage;

        [Header("남은 턴 텍스트")]
        [Tooltip("남은 턴 수를 표시할 TextMeshProUGUI")]
        [SerializeField] private TextMeshProUGUI turnText;

        [Header("표시 옵션")]
        [Tooltip("0턴 이하일 때 자동 비활성화 여부")]
        [SerializeField] private bool autoHideOnZeroTurn = true;

        [Header("툴팁 설정")]
        // 툴팁 지연 시간은 ItemConstants에서 관리 (코드로 제어)
        private float tooltipDelay;

        #endregion

        #region Properties

        /// <summary>
        /// 현재 남은 턴 수.
        /// </summary>
        public int RemainingTurns { get; private set; }

        /// <summary>
        /// 현재 스택 수(선택 사용).
        /// </summary>
        public int CurrentStack { get; private set; } = 1;

        /// <summary>
        /// 현재 적용된 효과 데이터.
        /// </summary>
        public IPerTurnEffect CurrentEffect { get; private set; }

        #endregion

        #region Private Fields

        private Coroutine tooltipCoroutine;
        private bool isHovering = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 툴팁 지연 시간을 상수에서 초기화
            tooltipDelay = ItemConstants.TOOLTIP_SHOW_DELAY;
        }

        #endregion

        #region Public API

        /// <summary>
        /// 아이콘과 남은 턴을 설정한다.
        /// </summary>
        /// <param name="icon">표시할 아이콘</param>
        /// <param name="remainingTurns">남은 턴 수</param>
        public void SetData(Sprite icon, int remainingTurns)
        {
            if (iconImage != null)
                iconImage.sprite = icon;

            UpdateTurns(remainingTurns);
        }

        /// <summary>
        /// 효과 데이터를 설정한다.
        /// </summary>
        /// <param name="effect">적용된 효과</param>
        public void SetEffectData(IPerTurnEffect effect)
        {
            CurrentEffect = effect;
            
            if (effect != null)
            {
                if (iconImage != null)
                    iconImage.sprite = effect.Icon;
                
                UpdateTurns(effect.RemainingTurns);
                // SubTooltipModel에 값-페어를 전달할 준비(남은 턴 등)
            }
        }

        /// <summary>
        /// 남은 턴 수를 갱신한다. 0 이하이면 자동으로 비활성화한다.
        /// </summary>
        /// <param name="remainingTurns">남은 턴 수</param>
        public void UpdateTurns(int remainingTurns)
        {
            RemainingTurns = remainingTurns;

            if (turnText != null)
            {
                // 10 이상이면 9+ 같은 방식이 필요하면 여기서 포맷 조정 가능
                turnText.text = Mathf.Max(0, remainingTurns).ToString();
            }

            if (autoHideOnZeroTurn)
            {
                bool shouldShow = remainingTurns > 0;
                if (gameObject.activeSelf != shouldShow)
                    gameObject.SetActive(shouldShow);
            }
        }

        /// <summary>
        /// 스택 수를 설정한다(선택 기능).
        /// 필요 시 아이콘 오버레이/작은 텍스트로 확장 가능.
        /// </summary>
        /// <param name="stack">스택 수(1 이상)</param>
        public void SetStack(int stack)
        {
            if (stack < 1)
            {
                GameLogger.LogWarning("[BuffDebuffSlotView] 스택은 1 이상이어야 합니다.", GameLogger.LogCategory.UI);
                stack = 1;
            }
            CurrentStack = stack;
            // 현재는 시각 반영 없음. 필요 시 별도 텍스트/배지 추가
        }

        #endregion

        #region Tooltip Events

        /// <summary>
        /// 마우스가 슬롯에 진입했을 때 호출됩니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CurrentEffect == null || isHovering) return;

            isHovering = true;

            // 지연된 툴팁 표시 시작
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
            }
            tooltipCoroutine = StartCoroutine(ShowTooltipDelayed());
        }

        /// <summary>
        /// 마우스가 슬롯에서 이탈했을 때 호출됩니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isHovering) return;

            isHovering = false;

            // 진행 중인 툴팁 표시 코루틴 중지
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
                tooltipCoroutine = null;
            }

            // 툴팁 숨김
            HideTooltip();
        }

        #endregion

        #region Tooltip Coroutines

        /// <summary>
        /// 지연된 툴팁 표시를 처리합니다.
        /// </summary>
        private System.Collections.IEnumerator ShowTooltipDelayed()
        {
            // 툴팁 지연 시작
            yield return new WaitForSeconds(tooltipDelay);
            
            // 지연 후에도 여전히 호버 중이면 툴팁 표시
            if (isHovering && CurrentEffect != null)
            {
                ShowTooltip();
            }
            tooltipCoroutine = null;
        }

        #endregion

        #region Tooltip Management

        /// <summary>
        /// 버프/디버프 툴팁을 표시합니다.
        /// </summary>
        private void ShowTooltip()
        {
            if (CurrentEffect == null) return;

            // 버프/디버프 툴팁 매니저를 통해 툴팁 표시
            if (BuffDebuffTooltipManager.Instance != null)
            {
                // 자신의 RectTransform을 전달하여 정확한 위치 계산
                var rectTransform = GetComponent<RectTransform>();
                BuffDebuffTooltipManager.Instance.ShowBuffDebuffTooltip(CurrentEffect, transform.position, rectTransform);
                GameLogger.LogInfo($"[BuffDebuffSlotView] 툴팁 표시: {GetEffectDisplayName(CurrentEffect)}", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("[BuffDebuffSlotView] BuffDebuffTooltipManager를 찾을 수 없습니다.", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 버프/디버프 툴팁을 숨깁니다.
        /// </summary>
        private void HideTooltip()
        {
            if (BuffDebuffTooltipManager.Instance != null)
            {
                BuffDebuffTooltipManager.Instance.HideBuffDebuffTooltip();
            }
        }

        /// <summary>
        /// 효과의 표시 이름을 반환합니다.
        /// </summary>
        /// <param name="effect">효과</param>
        /// <returns>표시 이름</returns>
        private string GetEffectDisplayName(IPerTurnEffect effect)
        {
            if (effect == null) return "알 수 없는 효과";

            // 효과 타입에 따른 이름 반환
            string effectTypeName = effect.GetType().Name;
            
            // 일반적인 효과 이름 매핑
            switch (effectTypeName)
            {
                case "BleedEffect":
                    return "출혈";
                case "StunEffect":
                case "StunDebuff":
                    return "기절";
                case "GuardBuff":
                    return "가드";
                case "CounterBuff":
                    return "반격";
                case "HealEffect":
                    return "치유";
                default:
                    return effectTypeName.Replace("Effect", "").Replace("Buff", "");
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
                tooltipCoroutine = null;
            }
        }

        #endregion
    }
}


