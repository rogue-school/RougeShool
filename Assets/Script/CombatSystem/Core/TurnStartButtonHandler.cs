using UnityEngine;
using UnityEngine.UI;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 턴 시작 버튼을 제어하는 스크립트입니다.
    /// 두 슬롯이 모두 카드로 채워졌을 때만 버튼이 활성화됩니다.
    /// 버튼은 한 번만 클릭되도록 설정됩니다.
    /// </summary>
    public class TurnStartButtonHandler : MonoBehaviour
    {
        [SerializeField] private Button turnStartButton;

        private void Start()
        {
            if (turnStartButton == null)
                turnStartButton = GetComponent<Button>();

            if (turnStartButton != null)
                turnStartButton.onClick.AddListener(OnTurnStartClicked);

            SetInteractable(false);
        }

        private void Update()
        {
            UpdateButtonState();
        }

        /// <summary>
        /// 버튼 활성화 여부 갱신
        /// </summary>
        private void UpdateButtonState()
        {
            if (CombatTurnManager.Instance == null || turnStartButton == null)
            {
                SetInteractable(false);
                return;
            }

            // 카드가 두 장 모두 준비된 경우만 활성화
            bool shouldBeInteractable = CombatTurnManager.Instance.AreBothSlotsReady();

            // 버튼이 이미 클릭되어 비활성화된 경우 다시 활성화하지 않음
            if (!turnStartButton.interactable && shouldBeInteractable)
                SetInteractable(true);
        }

        /// <summary>
        /// 버튼 상태 설정
        /// </summary>
        private void SetInteractable(bool value)
        {
            if (turnStartButton != null)
                turnStartButton.interactable = value;
        }

        /// <summary>
        /// 턴 시작 버튼 클릭 시 전투 실행
        /// </summary>
        private void OnTurnStartClicked()
        {
            if (CombatTurnManager.Instance != null)
            {
                // 즉시 비활성화하여 중복 클릭 방지
                SetInteractable(false);
                CombatTurnManager.Instance.ExecuteCombat();
            }
        }
    }
}
