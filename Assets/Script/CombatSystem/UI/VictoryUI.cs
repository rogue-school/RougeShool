using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Game.CoreSystem.Utility;
using Game.CombatSystem;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 스테이지 승리 패널을 표시하는 UI 컨트롤러입니다.
    /// 스테이지 클리어 시 승리 메시지와 다음 스테이지 진행 버튼을 제공합니다.
    /// </summary>
    public class VictoryUI : MonoBehaviour
    {
        [Header("승리 UI 요소")]
        [Tooltip("승리 패널 (배경)")]
        [SerializeField] private GameObject panel;

        [Tooltip("타이틀 텍스트")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("다음 스테이지로 이동 버튼")]
        [SerializeField] private Button nextStageButton;

        // 스테이지 진행/전환 매니저 (DI)
        [Inject(Optional = true)] private Game.StageSystem.Manager.StageManager _stageManager;
        [Inject(Optional = true)] private Game.CoreSystem.Manager.SceneTransitionManager _sceneTransitionManager;

        // 승리 패널이 방금 표시되었는지 추적 (다음 스테이지 시작 시 즉시 닫히는 것을 방지)
        private bool _justShown = false;
        private float _showTime = 0f;

        /// <summary>
        /// StageManager가 주입되지 않았으면 Fallback 주입을 시도합니다.
        /// </summary>
        private void EnsureStageManagerInjected()
        {
            if (_stageManager != null) return;

            try
            {
                // 1. ProjectContext에서 SceneContextRegistry를 통해 찾기
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        var sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                        if (sceneContainer != null)
                        {
                            var resolvedStageManager = sceneContainer.TryResolve<Game.StageSystem.Manager.StageManager>();
                            if (resolvedStageManager != null)
                            {
                                _stageManager = resolvedStageManager;
                                GameLogger.LogInfo("[VictoryUI] StageManager를 SceneContext에서 찾아서 주입했습니다.", GameLogger.LogCategory.UI);
                                return;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[VictoryUI] SceneContext 주입 시도 실패: {ex.Message}", GameLogger.LogCategory.UI);
                    }
                }

                // 2. FindFirstObjectByType을 사용한 폴백
                _stageManager = UnityEngine.Object.FindFirstObjectByType<Game.StageSystem.Manager.StageManager>(UnityEngine.FindObjectsInactive.Include);
                if (_stageManager != null)
                {
                    GameLogger.LogInfo("[VictoryUI] StageManager 직접 찾기 완료 (FindFirstObjectByType)", GameLogger.LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[VictoryUI] StageManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

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
        /// 단, 승리 패널이 방금 표시된 경우(0.5초 이내)에는 닫지 않음
        /// </summary>
        private void HandleCombatStarted()
        {
            if (panel != null && panel.activeSelf)
            {
                // 승리 패널이 방금 표시된 경우(0.5초 이내)에는 닫지 않음
                // 이는 스테이지 완료 후 다음 스테이지로 자동 진행될 때 패널이 즉시 닫히는 것을 방지
                if (_justShown && Time.time - _showTime < 0.5f)
                {
                    GameLogger.LogInfo("[VictoryUI] 전투 시작 감지되었으나 승리 패널이 방금 표시되어 유지 (0.5초 이내)", GameLogger.LogCategory.UI);
                    return;
                }

                panel.SetActive(false);
                _justShown = false;
                GameLogger.LogInfo("[VictoryUI] 전투 시작 - 승리 패널 숨김", GameLogger.LogCategory.UI);
            }
        }

        private void HandleVictory()
        {
            Show();
        }

        /// <summary>
        /// 승리 패널을 표시합니다.
        /// </summary>
        public void Show()
        {
            // StageManager 주입 확인
            EnsureStageManagerInjected();

            if (panel != null)
            {
                panel.SetActive(true);
                _justShown = true;
                _showTime = Time.time;
            }

            bool isFinal = IsFinalVictory();
            
            // 현재 스테이지 정보 가져오기
            string stageInfo = "";
            if (_stageManager != null)
            {
                var currentStage = _stageManager.GetCurrentStage();
                if (currentStage != null)
                {
                    stageInfo = $"스테이지 {currentStage.stageNumber}";
                }
            }

            if (titleText != null)
            {
                if (isFinal)
                {
                    titleText.text = "게임 클리어";
                }
                else
                {
                    titleText.text = string.IsNullOrEmpty(stageInfo) ? "스테이지 클리어" : $"{stageInfo} 클리어";
                }
            }
            
            if (nextStageButton != null)
            {
                var label = nextStageButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = isFinal ? "메인 메뉴" : "다음 스테이지";
                }
            }

            GameLogger.LogInfo($"[VictoryUI] 승리 패널 표시 - {stageInfo} 클리어", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 승리 UI 패널을 숨깁니다.
        /// </summary>
        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
                _justShown = false;
            }
        }

        private void OnNextStageClicked()
        {
            GameLogger.LogInfo("[VictoryUI] 다음 스테이지 버튼 클릭", GameLogger.LogCategory.UI);
            
            // StageManager 주입 확인
            EnsureStageManagerInjected();
            
            bool isFinal = IsFinalVictory();
            
            // 최종 승리 시 처리
            if (isFinal)
            {
                // _sceneTransitionManager는 DI로 주입받음
                if (_sceneTransitionManager != null)
                {
                    _ = _sceneTransitionManager.TransitionToMainScene();
                }
                return;
            }
            
            // 스테이지 클리어 시: 다음 스테이지로 진행
            if (_stageManager != null)
            {
                // 다음 스테이지로 진행
                if (_stageManager.ProgressToNextStage())
                {
                    GameLogger.LogInfo("[VictoryUI] 다음 스테이지로 진행", GameLogger.LogCategory.UI);
                    
                    // 스테이지 시작
                    _stageManager.StartStage();
                    
                    // 패널 숨김
                    Hide();
                }
                else
                {
                    GameLogger.LogWarning("[VictoryUI] 다음 스테이지로 진행할 수 없습니다", GameLogger.LogCategory.UI);
                    // 다음 스테이지가 없어도 패널은 숨김
                    Hide();
                }
            }
            else
            {
                GameLogger.LogError("[VictoryUI] StageManager를 찾을 수 없습니다. 다음 스테이지로 진행할 수 없습니다.", GameLogger.LogCategory.Error);
            }
        }

        private bool IsFinalVictory()
        {
            // StageManager 주입 확인
            EnsureStageManagerInjected();
            
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


