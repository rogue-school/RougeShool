using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Game.CoreSystem.Utility;
using Game.CombatSystem;
using Game.CombatSystem.Manager;
using Game.CoreSystem.Interface;

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

        [Header("점수 및 순위 UI (최종 승리 시 표시)")]
        [Tooltip("점수 및 순위 표시 텍스트 (최종 승리 시에만 사용, 선택적)")]
        [SerializeField] private TextMeshProUGUI scoreAndRankText;

        [Header("리더보드 UI (최종 승리 시 표시)")]
        [Tooltip("리더보드 제목 텍스트")]
        [SerializeField] private TextMeshProUGUI leaderboardTitleText;
        
        [Tooltip("리더보드 최고 점수 텍스트")]
        [SerializeField] private TextMeshProUGUI leaderboardBestScoreText;
        
        [Tooltip("리더보드 슬롯 (1~10위, 순서대로 배치)")]
        [SerializeField] private TextMeshProUGUI[] leaderboardSlots = new TextMeshProUGUI[10];

        // 통계 제공자 (DI)
        [Inject(Optional = true)] private ICombatStatsProvider _statsProvider;
        // 스테이지 진행/전환 매니저 (DI)
        [Inject(Optional = true)] private Game.StageSystem.Manager.StageManager _stageManager;
        [Inject(Optional = true)] private Game.CoreSystem.Manager.SceneTransitionManager _sceneTransitionManager;
        // SaveSystem 및 Statistics 제거됨

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void OnEnable()
        {
            CombatEvents.OnVictory += HandleVictory;
            CombatEvents.OnCombatStarted += HandleCombatStarted;
            
            // 새 게임 시작 시 패널 숨기기
            if (panel != null) panel.SetActive(false);
        }

        private void OnDisable()
        {
            CombatEvents.OnVictory -= HandleVictory;
            CombatEvents.OnCombatStarted -= HandleCombatStarted;
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
            
            // 새 게임 시작 시 패널 숨기기
            if (panel != null) panel.SetActive(false);
        }

        /// <summary>
        /// 전투 시작 핸들러 (새 게임 시작 시 패널 숨기기)
        /// </summary>
        private void HandleCombatStarted()
        {
            if (panel != null && panel.activeSelf)
            {
                panel.SetActive(false);
                GameLogger.LogInfo("[VictoryUI] 전투 시작 - 승리 패널 숨김", GameLogger.LogCategory.UI);
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
                    // 최종 승리 시에는 ShowScoreAndRank에서 처리
                    if (isFinal)
                    {
                        statsText.text = "게임을 클리어했습니다!";
                    }
                    else
                {
                    statsText.text = "통계 정보를 불러올 수 없습니다.";
                    }
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

            // 최종 승리 시 점수 및 순위 표시
            if (isFinal)
            {
                ShowScoreAndRank();
                ShowLeaderboard();
            }
            else
            {
                // 스테이지 클리어 시 점수/순위 UI 숨기기
                if (scoreAndRankText != null) scoreAndRankText.gameObject.SetActive(false);
                HideLeaderboard();
            }

            GameLogger.LogInfo("[VictoryUI] 승리 패널 표시", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 점수 및 순위 표시 (최종 승리 시)
        /// </summary>
        private void ShowScoreAndRank()
        {
            if (scoreAndRankText == null)
            {
                return;
            }

            // Statistics 제거됨 - 간단한 승리 메시지만 표시
            scoreAndRankText.gameObject.SetActive(true);
            scoreAndRankText.text = "🎉 승리! 🎉\n\n스테이지를 클리어했습니다!";
            
            if (statsText != null)
            {
                statsText.text = "🎉 승리! 🎉";
            }
        }

        /// <summary>
        /// 리더보드 표시 (최종 승리 시, 오른쪽에 배치)
        /// </summary>
        private void ShowLeaderboard()
        {
            // Statistics 제거됨 - 리더보드 비활성화
            HideLeaderboard();
        }

        /// <summary>
        /// 리더보드 숨기기
        /// </summary>
        private void HideLeaderboard()
        {
            if (leaderboardBestScoreText != null)
                leaderboardBestScoreText.gameObject.SetActive(false);

            for (int i = 0; i < leaderboardSlots.Length; i++)
            {
                if (leaderboardSlots[i] != null)
                    leaderboardSlots[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 승리 UI 패널을 숨깁니다.
        /// </summary>
        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void OnNextStageClicked()
        {
            GameLogger.LogInfo("[VictoryUI] 다음 스테이지 버튼 클릭", GameLogger.LogCategory.UI);
            
            bool isFinal = IsFinalVictory();
            
            // 최종 승리 시 처리
            if (isFinal)
            {
                // Statistics 제거됨
                
                // _sceneTransitionManager는 DI로 주입받음
                if (_sceneTransitionManager != null)
                {
                    _ = _sceneTransitionManager.TransitionToMainScene();
                }
                return;
            }
            
            // 스테이지 클리어 시: 자동 진행이 이미 되었으면 패널만 숨기고, 아니면 다음 스테이지로 진행
            // _stageManager는 DI로 주입받음
            if (_stageManager != null)
            {
                var currentStage = _stageManager.GetCurrentStage();
                if (currentStage != null && currentStage.autoProgressToNext)
                {
                    // 자동 진행이 이미 완료되었으므로 패널만 숨김
                    GameLogger.LogInfo("[VictoryUI] 자동 진행 완료 - 패널 숨김", GameLogger.LogCategory.UI);
                    Hide();
                }
                else
                {
                    // 수동 진행: 다음 스테이지로 진행
                    if (_stageManager.ProgressToNextStage())
                    {
                        GameLogger.LogInfo("[VictoryUI] 다음 스테이지로 수동 진행", GameLogger.LogCategory.UI);
                        
                        // SaveSystem 제거됨
                        
                        // 스테이지 시작
                        _stageManager.StartStage();
                        
                        // 패널 숨김
                        Hide();
                    }
                    else
                    {
                        GameLogger.LogWarning("[VictoryUI] 다음 스테이지로 진행할 수 없습니다", GameLogger.LogCategory.UI);
                    }
                }
            }
            else
            {
                GameLogger.LogWarning("[VictoryUI] StageManager를 찾을 수 없습니다", GameLogger.LogCategory.UI);
            }
        }
        
        // Statistics 제거됨 - SaveStatisticsSession 메서드 제거

        private bool IsFinalVictory()
        {
            // _stageManager는 DI로 주입받음
            if (_stageManager != null)
            {
                // 1) 이미 게임 완료 플래그가 올라간 경우
                if (_stageManager.IsGameCompleted) return true;

                // 2) 현재 스테이지 데이터를 가져와서 IsLastStage 확인
                var currentStageData = _stageManager.GetCurrentStage();
                if (currentStageData != null && currentStageData.IsLastStage)
                {
                    // 현재 스테이지가 마지막 스테이지인 경우
                    return true;
                }

                // 그 외의 경우는 최종 승리가 아님
                return false;
            }
            
            // StageManager를 찾을 수 없으면 최종 승리가 아님 (안전하게 false 반환)
            return false;
        }
    }
}


