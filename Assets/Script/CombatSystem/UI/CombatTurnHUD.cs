using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CoreSystem.Utility;
using Game.Domain.Combat.Interfaces;
using Game.Domain.Combat.ValueObjects;
using Game.CombatSystem.Interface;
using Zenject;
using DomainTurnType = Game.Domain.Combat.ValueObjects.TurnType;
using DomainCombatPhase = Game.Domain.Combat.ValueObjects.CombatPhase;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 도메인 전투 턴 정보를 화면에 표시하는 HUD 컴포넌트입니다.
    /// </summary>
    public sealed class CombatTurnHUD : MonoBehaviour
    {
        // Members -----------------------------------------------------

        [Header("턴 표시 UI")]
        [Tooltip("현재 턴 정보를 표시할 TextMeshProUGUI (설정되지 않으면 Legacy Text를 사용합니다)")]
        [SerializeField] private TMP_Text _turnText;

        [Tooltip("TextMeshPro를 사용하지 않는 경우에 사용할 기본 Text 컴포넌트")]
        [SerializeField] private Text _legacyText;

        [Header("표시 형식")]
        [Tooltip("턴 정보 포맷 (예: \"턴 {0} - {1} ({2})\")")]
        [SerializeField] private string _format = "턴 {0} - {1} ({2})";

        // 도메인 턴 매니저 (DDD)
        private ITurnManager _domainTurnManager;

        // 기존 TurnController 이벤트를 활용하여 HUD 갱신
        private ITurnController _turnController;

        // Init --------------------------------------------------------

        [Inject]
        public void Construct(ITurnManager domainTurnManager, ITurnController turnController)
        {
            _domainTurnManager = domainTurnManager ?? throw new System.ArgumentNullException(nameof(domainTurnManager));
            _turnController = turnController;
        }

        private void OnEnable()
        {
            if (_turnController != null)
            {
                _turnController.OnTurnChanged += HandleTurnChanged;
                _turnController.OnTurnCountChanged += HandleTurnCountChanged;
            }

            RefreshHUD();
        }

        private void OnDisable()
        {
            if (_turnController != null)
            {
                _turnController.OnTurnChanged -= HandleTurnChanged;
                _turnController.OnTurnCountChanged -= HandleTurnCountChanged;
            }
        }

        // Update ------------------------------------------------------

        private void HandleTurnChanged(Game.CombatSystem.Interface.TurnType _)
        {
            RefreshHUD();
        }

        private void HandleTurnCountChanged(int _)
        {
            RefreshHUD();
        }

        // Functions ---------------------------------------------------

        private void RefreshHUD()
        {
            if (_domainTurnManager == null)
            {
                GameLogger.LogWarning("[CombatTurnHUD] 도메인 턴 매니저가 주입되지 않았습니다.", GameLogger.LogCategory.UI);
                return;
            }

            int turnNumber = _domainTurnManager.CurrentTurnNumber;
            DomainTurnType turnType = _domainTurnManager.CurrentTurnType;
            DomainCombatPhase phase = _domainTurnManager.Phase;

            string typeText = GetTurnTypeText(turnType);
            string phaseText = GetPhaseText(phase);

            string result = string.Format(_format, turnNumber, typeText, phaseText);

            if (_turnText != null)
            {
                _turnText.text = result;
            }
            else if (_legacyText != null)
            {
                _legacyText.text = result;
            }
        }

        private static string GetTurnTypeText(DomainTurnType turnType)
        {
            switch (turnType)
            {
                case DomainTurnType.Player:
                    return "플레이어 턴";
                case DomainTurnType.Enemy:
                    return "적 턴";
                default:
                    return "알 수 없음";
            }
        }

        private static string GetPhaseText(DomainCombatPhase phase)
        {
            switch (phase)
            {
                case DomainCombatPhase.None:
                    return "대기";
                case DomainCombatPhase.Preparation:
                    return "준비";
                case DomainCombatPhase.PlayerTurn:
                    return "플레이어";
                case DomainCombatPhase.EnemyTurn:
                    return "적";
                case DomainCombatPhase.Resolution:
                    return "해결";
                case DomainCombatPhase.Victory:
                    return "승리";
                case DomainCombatPhase.Defeat:
                    return "패배";
                case DomainCombatPhase.Ended:
                    return "종료";
                case DomainCombatPhase.Paused:
                    return "일시정지";
                default:
                    return "알 수 없음";
            }
        }
    }
}


