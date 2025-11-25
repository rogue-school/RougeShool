using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Game.CoreSystem.Utility;
using Game.CombatSystem;
using Game.CombatSystem.Manager;
using Game.CoreSystem.Statistics;
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
        // 통계 매니저 (DI)
        [Inject(Optional = true)] private GameSessionStatistics _gameSessionStatistics;
        [Inject(Optional = true)] private IStatisticsManager _statisticsManager;
        [Inject(Optional = true)] private ILeaderboardManager _leaderboardManager;

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
        public async void Show(CombatStatsSnapshot snapshot)
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
                await ShowScoreAndRank();
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
        private async System.Threading.Tasks.Task ShowScoreAndRank()
        {
            if (scoreAndRankText == null)
            {
                return;
            }

            if (_gameSessionStatistics == null)
            {
                _gameSessionStatistics = Game.CoreSystem.Statistics.GameSessionStatisticsLocator.Instance;
            }

            if (_leaderboardManager == null)
            {
                _leaderboardManager = Game.CoreSystem.Statistics.LeaderboardManagerLocator.Instance;
            }

            if (_gameSessionStatistics == null)
            {
                GameLogger.LogWarning("[VictoryUI] GameSessionStatistics를 찾을 수 없습니다. 점수 표시를 건너뜁니다.", GameLogger.LogCategory.UI);
                if (scoreAndRankText != null)
                {
                    scoreAndRankText.gameObject.SetActive(true);
                    scoreAndRankText.text = "🎉 처음 클리어를 하였습니다! 🎉\n\n통계 정보를 불러올 수 없습니다.";
                }
                if (statsText != null)
                {
                    statsText.text = "🎉 처음 클리어를 하였습니다! 🎉";
                }
                return;
            }

            // 점수 계산 전에 세션 종료 및 요약 계산 보장
            if (_gameSessionStatistics.IsSessionActive)
            {
                GameLogger.LogInfo("[VictoryUI] 점수 계산 전 세션 종료 처리", GameLogger.LogCategory.UI);
                _gameSessionStatistics.EndSession(true);
            }

            var sessionData = _gameSessionStatistics.GetCurrentSessionData();
            if (sessionData == null)
            {
                GameLogger.LogWarning("[VictoryUI] 세션 데이터가 null입니다. 점수 표시를 건너뜁니다.", GameLogger.LogCategory.UI);
                if (scoreAndRankText != null)
                {
                    scoreAndRankText.gameObject.SetActive(true);
                    scoreAndRankText.text = "🎉 처음 클리어를 하였습니다! 🎉\n\n세션 데이터를 불러올 수 없습니다.";
                }
                if (statsText != null)
                {
                    statsText.text = "🎉 처음 클리어를 하였습니다! 🎉";
                }
                return;
            }

            // 점수 계산
            var scoreData = ScoreCalculator.CalculateScore(sessionData);

            // 처음 클리어 메시지 확인 (점수 추가 전에 확인)
            bool isFirstClear = false;
            if (_leaderboardManager != null)
            {
                string characterName = sessionData.selectedCharacterName;
                isFirstClear = _leaderboardManager.IsFirstClear(characterName);
            }

            // 리더보드에 추가
            if (_leaderboardManager != null)
            {
                await _leaderboardManager.AddScore(sessionData, scoreData);
            }

            // 점수 및 순위 표시 (하나의 텍스트 필드에 통합)
            scoreAndRankText.gameObject.SetActive(true);
            var sb = new System.Text.StringBuilder(512);
            
            if (isFirstClear)
            {
                sb.AppendLine("🎉 처음 클리어! 🎉");
                sb.AppendLine();
            }
            
            sb.AppendLine($"총 점수: {scoreData.totalScore:N0}");
            sb.AppendLine($"  - 기본 점수: 10,000");
            if (scoreData.turnEfficiencyScore < 0)
            {
                sb.AppendLine($"  - 턴 수 차감: {scoreData.turnEfficiencyScore:N0}");
            }
            if (scoreData.damageEfficiencyScore < 0)
            {
                sb.AppendLine($"  - 받은 총 데미지 차감: {scoreData.damageEfficiencyScore:N0}");
            }
            if (scoreData.healthBonus < 0)
            {
                sb.AppendLine($"  - 회복량 차감: {scoreData.healthBonus:N0}");
            }
            if (scoreData.stageClearBonus < 0)
            {
                sb.AppendLine($"  - 사용한 엑티브 아이템 차감: {scoreData.stageClearBonus:N0}");
            }
            if (scoreData.speedRunBonus < 0)
            {
                sb.AppendLine($"  - 자원 획득 차감: {scoreData.speedRunBonus:N0}");
            }
            // 보너스 표시: 적에게 준 총 데미지 보너스
            if (scoreData.noDamageBonus > 0)
            {
                sb.AppendLine($"  - 적에게 준 총 데미지 보너스: +{scoreData.noDamageBonus:N0}");
            }

            if (_leaderboardManager != null)
            {
                int totalClearCountAll = _leaderboardManager.GetTotalClearCountAllCharacters();
                
                GameLogger.LogInfo($"[VictoryUI] ShowScoreAndRank: 전체클리어횟수={totalClearCountAll}", GameLogger.LogCategory.UI);
                
                sb.AppendLine();
                sb.AppendLine($"총 클리어 횟수: {totalClearCountAll}회");
            }
            else
            {
                GameLogger.LogWarning("[VictoryUI] ShowScoreAndRank: _leaderboardManager가 null입니다", GameLogger.LogCategory.UI);
            }

            scoreAndRankText.text = sb.ToString();
        }

        /// <summary>
        /// 리더보드 표시 (최종 승리 시, 오른쪽에 배치)
        /// </summary>
        private void ShowLeaderboard()
        {
            if (_leaderboardManager == null || _gameSessionStatistics == null)
            {
                GameLogger.LogWarning("[VictoryUI] ShowLeaderboard: LeaderboardManager 또는 GameSessionStatistics가 주입되지 않았습니다.", GameLogger.LogCategory.UI);
            }

            if (_leaderboardManager == null || _gameSessionStatistics == null)
            {
                GameLogger.LogWarning("[VictoryUI] ShowLeaderboard: LeaderboardManager 또는 GameSessionStatistics를 찾을 수 없습니다.", GameLogger.LogCategory.UI);
                HideLeaderboard();
                return;
            }

            // 모든 캐릭터 통합 최고 점수 가져오기 및 표시
            int bestScore = _leaderboardManager.GetBestScoreAllCharacters();
            if (leaderboardBestScoreText != null)
            {
                leaderboardBestScoreText.gameObject.SetActive(true);
                if (bestScore > 0)
                {
                    leaderboardBestScoreText.text = $"최고 점수: {bestScore:N0}점";
                }
                else
                {
                    leaderboardBestScoreText.text = "최고 점수: -";
                }
            }

            // 모든 캐릭터 통합 상위 10개 항목 가져오기
            var topEntries = _leaderboardManager.GetTopEntriesAllCharacters(10);

            // 슬롯에 순위와 점수 표시 (1~10위 고정 슬롯)
            for (int i = 0; i < leaderboardSlots.Length; i++)
            {
                if (leaderboardSlots[i] == null)
                    continue;

                // 모든 슬롯 활성화
                leaderboardSlots[i].gameObject.SetActive(true);

                if (i < topEntries.Count)
                {
                    // 기록이 있는 경우 (점수와 캐릭터 이름 표시)
                    var entry = topEntries[i];
                    leaderboardSlots[i].text = $"{i + 1}위: {entry.characterName} - {entry.totalScore:N0}점";
                }
                else
                {
                    // 기록이 없는 경우 빈 슬롯 표시
                    leaderboardSlots[i].text = $"{i + 1}위: -";
                }
            }

            GameLogger.LogInfo($"[VictoryUI] ShowLeaderboard: 통합 리더보드 표시 완료. 최고점수={bestScore}, 상위항목수={topEntries.Count}", GameLogger.LogCategory.UI);
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

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private async void OnNextStageClicked()
        {
            GameLogger.LogInfo("[VictoryUI] 다음 스테이지 버튼 클릭", GameLogger.LogCategory.UI);
            
            bool isFinal = IsFinalVictory();
            
            // 최종 승리 시 통계 완전 종료 및 저장
            if (isFinal)
            {
                await SaveStatisticsSession(true);
            }
            
            var stm = _sceneTransitionManager;
            if (stm == null)
            {
                GameLogger.LogWarning("[VictoryUI] SceneTransitionManager를 찾을 수 없습니다", GameLogger.LogCategory.UI);
                return;
            }
            
            if (isFinal)
            {
                _ = stm.TransitionToMainScene();
            }
            else
            {
                _ = stm.TransitionToStageScene();
            }
        }
        
        /// <summary>
        /// 통계 세션 저장
        /// </summary>
        private async System.Threading.Tasks.Task SaveStatisticsSession(bool finalEnd)
        {
            GameLogger.LogInfo($"[VictoryUI] 통계 세션 저장 시도 (완전 종료: {finalEnd})", GameLogger.LogCategory.Save);
            
            if (_gameSessionStatistics == null)
            {
                GameLogger.LogWarning("[VictoryUI] GameSessionStatistics가 주입되지 않았습니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                return;
            }
            
            if (_statisticsManager == null)
            {
                GameLogger.LogWarning("[VictoryUI] StatisticsManager가 주입되지 않았습니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                return;
            }
            
            if (_gameSessionStatistics == null)
            {
                GameLogger.LogWarning("[VictoryUI] GameSessionStatistics를 찾을 수 없습니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                return;
            }
            
            if (_statisticsManager == null)
            {
                GameLogger.LogWarning("[VictoryUI] StatisticsManager를 찾을 수 없습니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                return;
            }
            
            // 이미 저장된 세션이면 건너뛰기
            if (_gameSessionStatistics.IsSaved && finalEnd)
            {
                GameLogger.LogInfo("[VictoryUI] 세션이 이미 저장되었습니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                return;
            }
            
            // 세션이 활성화되어 있으면 종료 처리
            if (_gameSessionStatistics.IsSessionActive)
            {
                _gameSessionStatistics.EndSession(finalEnd);
                var sessionData = _gameSessionStatistics.GetCurrentSessionData();
                
                if (sessionData == null)
                {
                    GameLogger.LogWarning("[VictoryUI] 세션 데이터가 null입니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                    return;
                }
                
                await _statisticsManager.SaveSessionStatistics(sessionData);
                
                if (finalEnd)
                {
                    _gameSessionStatistics.MarkAsSaved();
                }
                
                GameLogger.LogInfo($"[VictoryUI] 통계 세션 저장 완료 (완전 종료: {finalEnd})", GameLogger.LogCategory.Save);
            }
            else
            {
                // 세션이 이미 종료되었어도 데이터가 있으면 저장 시도
                var sessionData = _gameSessionStatistics.GetCurrentSessionData();
                if (sessionData != null)
                {
                    await _statisticsManager.SaveSessionStatistics(sessionData);
                    if (finalEnd)
                    {
                        _gameSessionStatistics.MarkAsSaved();
                    }
                    GameLogger.LogInfo($"[VictoryUI] 세션이 이미 종료되었지만, 기존 세션 데이터를 저장했습니다. (완전 종료: {finalEnd})", GameLogger.LogCategory.Save);
                }
            }
        }

        private bool IsFinalVictory()
        {
            // StageManager가 주입되지 않았으면 안전하게 찾아봅니다.
            var sm = _stageManager;
            if (sm != null)
            {
                // 1) 이미 게임 완료 플래그가 올라간 경우
                if (sm.IsGameCompleted) return true;

                // 2) 현재 스테이지가 마지막 스테이지인지 확인
                int currentStageNumber = sm.GetCurrentStageNumber();
                if (currentStageNumber <= 0)
                {
                    // 스테이지가 초기화되지 않았거나 0이면 최종 승리가 아님
                    return false;
                }

                // 3) 다음 스테이지가 없는 경우
                if (!sm.HasNextStage()) return true;

                // 4) 스테이지가 더 있다고 표기되지만, 다음 스테이지 데이터가 미등록된 경우도 최종 승리로 간주
                var nextData = sm.GetStageDataPublic(currentStageNumber + 1);
                if (nextData == null) return true;

                return false;
            }
            
            // StageManager를 찾을 수 없으면 최종 승리가 아님 (안전하게 false 반환)
            return false;
        }
    }
}


