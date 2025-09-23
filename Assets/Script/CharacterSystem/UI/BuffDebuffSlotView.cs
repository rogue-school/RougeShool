using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CoreSystem.Utility;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 버프/디버프 슬롯의 간단한 뷰.
    /// 아이콘과 남은 턴 수만 표시하며, 0턴이 되면 자동으로 비활성화한다.
    /// </summary>
    public class BuffDebuffSlotView : MonoBehaviour
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
    }
}


