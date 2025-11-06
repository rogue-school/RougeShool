using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Game.CoreSystem.Utility;
using Game.CombatSystem;
using Game.CombatSystem.Manager;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 승리 화면을 표시하는 UI 컨트롤러입니다.
    /// 전투 종료 시점에 통계를 표시합니다.
    /// </summary>
    public class VictoryUI : MonoBehaviour
    {
        [Header("승리 UI 요소")]
        [Tooltip("승리 패널 (배경)")]
        [SerializeField] private GameObject panel;

        [Tooltip("타이틀 텍스트")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("통계 요약 텍스트")]
        [SerializeField] private TextMeshProUGUI statsText;

        [Tooltip("다음 스테이지로 이동 버튼")]
        [SerializeField] private Button nextStageButton;

        // 통계 제공자 (DI)
        [Inject(Optional = true)] private ICombatStatsProvider _statsProvider;
        // 스테이지 진행/전환 매니저 (DI)
        [Inject(Optional = true)] private Game.StageSystem.Manager.StageManager _stageManager;
        [Inject(Optional = true)] private Game.CoreSystem.Manager.SceneTransitionManager _sceneTransitionManager;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void OnEnable()
        {
            CombatEvents.OnVictory += HandleVictory;
        }

        private void OnDisable()
        {
            CombatEvents.OnVictory -= HandleVictory;
        }

        private void Start()
        {
            // 버튼 이벤트는 선택적으로 연결
            if (nextStageButton != null)
            {
                nextStageButton.onClick.AddListener(OnNextStageClicked);
                var label = nextStageButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = IsFinalVictory() ? "메인 메뉴" : "다음 스테이지";
                }
            }
        }

        private void HandleVictory()
        {
            var snapshot = _statsProvider != null ? _statsProvider.GetSnapshot() : null;
            Show(snapshot);
        }

        /// <summary>
        /// 승리 패널을 열고 통계를 표시합니다.
        /// </summary>
        public void Show(CombatStatsSnapshot snapshot)
        {
            if (panel != null) panel.SetActive(true);

            bool isFinal = IsFinalVictory();
            if (titleText != null)
            {
                titleText.text = isFinal ? "게임 클리어" : "스테이지 클리어";
            }
            if (nextStageButton != null)
            {
                var label = nextStageButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = isFinal ? "메인 메뉴" : "다음 스테이지";
                }
            }

            if (statsText != null)
            {
                if (snapshot == null)
                {
                    statsText.text = "통계 정보를 불러올 수 없습니다.";
                }
                else
                {
                    // 핵심 지표만 간결히 표시
                    var sb = new System.Text.StringBuilder(256);
                    sb.AppendLine($"전투 시간: {snapshot.battleDurationSeconds:F1}s");
                    sb.AppendLine($"총 턴 수: {snapshot.totalTurns}");
                    sb.AppendLine($"가한 피해: {snapshot.totalDamageDealtToEnemies}");
                    sb.AppendLine($"받은 피해: {snapshot.totalDamageTakenByPlayer}");
                    sb.AppendLine($"회복량: {snapshot.totalHealingToPlayer}");

                    if (!string.IsNullOrEmpty(snapshot.resourceName))
                    {
                        sb.AppendLine($"자원({snapshot.resourceName}): 시작 {snapshot.startResource} / 종료 {snapshot.endResource} / 최대 {snapshot.maxResource}");
                        sb.AppendLine($"자원 획득: {snapshot.totalResourceGained} / 자원 소모: {snapshot.totalResourceSpent}");
                    }

                    // 액티브 아이템 사용 요약 (상위 몇 개만 표시)
                    int shown = 0;
                    foreach (var kv in snapshot.activeItemUsageByName)
                    {
                        if (kv.Value <= 0) continue;
                        sb.AppendLine($"아이템 사용 - {kv.Key}: {kv.Value}");
                        if (++shown >= 8) break;
                    }

                    // 스킬 카드 사용 요약 (상위 몇 개만 표시)
                    shown = 0;
                    foreach (var kv in snapshot.playerSkillUsageByCardId)
                    {
                        if (kv.Value <= 0) continue;
                        sb.AppendLine($"스킬 사용 - {kv.Key}: {kv.Value}");
                        if (++shown >= 10) break;
                    }

                    statsText.text = sb.ToString();
                }
            }

            GameLogger.LogInfo("[VictoryUI] 승리 패널 표시", GameLogger.LogCategory.UI);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void OnNextStageClicked()
        {
            GameLogger.LogInfo("[VictoryUI] 다음 스테이지 버튼 클릭", GameLogger.LogCategory.UI);
            var stm = _sceneTransitionManager != null
                ? _sceneTransitionManager
                : FindFirstObjectByType<Game.CoreSystem.Manager.SceneTransitionManager>(FindObjectsInactive.Include);
            if (stm == null)
            {
                GameLogger.LogWarning("[VictoryUI] SceneTransitionManager를 찾을 수 없습니다", GameLogger.LogCategory.UI);
                return;
            }
            if (IsFinalVictory())
            {
                _ = stm.TransitionToMainScene();
            }
            else
            {
                _ = stm.TransitionToStageScene();
            }
        }

        private bool IsFinalVictory()
        {
            // StageManager가 주입되지 않았으면 안전하게 찾아봅니다.
            var sm = _stageManager != null ? _stageManager :
                FindFirstObjectByType<Game.StageSystem.Manager.StageManager>(FindObjectsInactive.Include);
            if (sm != null)
            {
                // 1) 이미 게임 완료 플래그가 올라간 경우
                if (sm.IsGameCompleted) return true;

                // 2) 다음 스테이지가 없는 경우
                if (!sm.HasNextStage()) return true;

                // 3) 스테이지가 더 있다고 표기되지만, 다음 스테이지 데이터가 미등록된 경우도 최종 승리로 간주
                int current = sm.GetCurrentStageNumber();
                var nextData = sm.GetStageDataPublic(current + 1);
                if (nextData == null) return true;

                return false;
            }
            return false;
        }
    }
}


