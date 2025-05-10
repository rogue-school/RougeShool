using UnityEngine;
using UnityEngine.UI;
using Game.Managers;

namespace Game.UI
{
    /// <summary>
    /// 턴 시작 버튼을 제어하는 스크립트입니다.
    /// 두 슬롯이 모두 카드로 채워졌을 때만 버튼이 활성화됩니다.
    /// </summary>
    public class TurnStartButtonHandler : MonoBehaviour
    {
        [SerializeField] private Button turnStartButton;

        private void Start()
        {
            // 버튼 자동 참조
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

            SetInteractable(CombatTurnManager.Instance.AreBothSlotsReady());
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
                CombatTurnManager.Instance.ExecuteCombat();
        }
    }
}
