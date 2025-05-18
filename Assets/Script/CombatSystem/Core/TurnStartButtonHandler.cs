using UnityEngine;
using UnityEngine.UI;
using Game.CombatSystem.Interface;

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

        private ICombatTurnManager turnManager;

        public void Inject(ICombatTurnManager manager)
        {
            turnManager = manager;
        }

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

        private void UpdateButtonState()
        {
            if (turnManager == null || turnStartButton == null)
            {
                SetInteractable(false);
                return;
            }

            bool shouldBeInteractable = turnManager.AreBothSlotsReady();
            if (!turnStartButton.interactable && shouldBeInteractable)
                SetInteractable(true);
        }

        private void SetInteractable(bool value)
        {
            if (turnStartButton != null)
                turnStartButton.interactable = value;
        }

        private void OnTurnStartClicked()
        {
            if (turnManager != null)
            {
                SetInteractable(false);
                turnManager.ExecuteCombat();
            }
        }
    }
}
