using UnityEngine;
using Zenject;
using Game.CoreSystem.Utility;
using Game.CombatSystem.Interface;
using Game.ItemSystem.Service;
using TMPro;
using System.Collections;

namespace Game.TutorialSystem
{
    /// <summary>
    /// 전투 튜토리얼을 제어하는 매니저입니다.
    /// - 플레이어/적 턴, 카드 드래그/드랍, 액티브 아이템 등 핵심 이벤트를 구독합니다.
    /// - PlayerPrefs의 게이트 키를 읽어 첫 진입 시에만 실행합니다.
    /// - 실제 코치마크/시퀀스는 후속 ScriptableObject와 View에서 구성합니다.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        #region 의존성 주입

        [Inject(Optional = true)] private ITurnController _turnController;
        [Inject(Optional = true)] private ItemService _itemService;

        [Header("오버레이")]
        [Tooltip("튜토리얼 오버레이 프리팹 (CanvasGroup 포함)")]
        [SerializeField] private TutorialOverlayView overlayPrefab;
        [Tooltip("(선택) 오버레이가 붙을 부모(미지정 시 활성 Canvas를 자동 탐색)")]
        [SerializeField] private RectTransform overlayParent;

        // 단순 페이지 시스템: 페이지 참조/하이라이트 제거 (프리팹 하나에 모든 페이지 작성)

        #endregion

        #region 상태

        private bool _shouldRun;
        private bool _isRunning;
        private TutorialOverlayView _overlay;
        private Step _step = Step.None;
        private bool _sawEnemyTurn;
        // 동일 세션에서 중복 시작 방지(플레이어 턴 진입이 여러 번 호출되는 경우 대비)
        private bool _startedOnce;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 정책 변경: 한 번만 보는 튜토리얼이 아님. 메인 메뉴의 스킵 토글만 반영
            bool skip = PlayerPrefs.GetInt("TUTORIAL_SKIP", 0) == 1;
            _shouldRun = !skip;
            GameLogger.LogInfo($"[TutorialManager] Gate: SHOULD_RUN={_shouldRun} (by SKIP only), SKIP={PlayerPrefs.GetInt("TUTORIAL_SKIP",0)}", GameLogger.LogCategory.UI);
        }

        private void Start()
        {
            // 항상 이벤트는 구독하여, 런타임에 게이트 변경/강제 실행 시 반응하도록 함
            TrySubscribe();

            if (!_shouldRun)
            {
                GameLogger.LogInfo("[TutorialManager] 튜토리얼 미실행 – 게이트에 의해 대기 상태", GameLogger.LogCategory.UI);
                // 게이트가 열리면(ForceShow 등) Player 턴 이벤트에서 시작됨
                return;
            }

            // 시작 시 즉시 표시하지 않습니다. 전투 상태머신이 PlayerTurn으로 진입하며
            // TurnCount가 증가한 이벤트(최초 1 이상)에서 시작합니다.
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        #endregion

        #region 구독

        private void TrySubscribe()
        {
            if (_turnController != null)
            {
                _turnController.OnTurnChanged += HandleTurnChanged;
                _turnController.OnTurnCountChanged += HandleTurnCountChanged;
                GameLogger.LogInfo("[TutorialManager] ITurnController 이벤트 구독 완료", GameLogger.LogCategory.UI);
            }

            if (_itemService != null)
            {
                _itemService.OnActiveItemUsed += HandleActiveItemUsed;
            }

            // Fallback: TurnManager 어댑터에서도 구독 시도 (DI 미주입 대비)
            if (_turnController == null)
            {
                var tm = FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
                if (tm != null)
                {
                    tm.OnTurnChanged += (t) => HandleTurnChanged(ConvertTurnType(t));
                    tm.OnTurnCountChanged += (c) => HandleTurnCountChanged(c);
                    GameLogger.LogWarning("[TutorialManager] ITurnController 미주입 → TurnManager에 직접 구독", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogWarning("[TutorialManager] Turn 이벤트 소스를 찾지 못했습니다(디아보스?)", GameLogger.LogCategory.UI);
                }
            }
        }

        private void Unsubscribe()
        {
            if (_turnController != null)
            {
                _turnController.OnTurnChanged -= HandleTurnChanged;
                _turnController.OnTurnCountChanged -= HandleTurnCountChanged;
            }

            if (_itemService != null)
            {
                _itemService.OnActiveItemUsed -= HandleActiveItemUsed;
            }
        }

        #endregion

        #region 이벤트 핸들러 (시퀀스 연결 지점)

        private void HandleActiveItemUsed(Game.ItemSystem.Data.ActiveItemDefinition def, int slotIndex)
        {
            if (!_isRunning) return;
        }

        #endregion

        #region 실행 제어

        private void StartTutorialIfReady()
        {
            if (!_shouldRun) return; // 게이트 닫힘
            if (_isRunning || _startedOnce) return;
            if (_overlay == null) PrepareOverlay();
            _isRunning = true;
            _startedOnce = true;
            GameLogger.LogInfo("[TutorialManager] 튜토리얼 실행 시작", GameLogger.LogCategory.UI);
            // 1~2단계: 페이지(수동 Next)
            if (_overlay != null)
            {
                _overlay.Completed += OnOverlayCompleted;
                _overlay.ShowFirstPage();
                _step = Step.PagesIntroAndTooltip; // 내부 페이지 구성은 프리팹 순서로 처리
            }
        }

        /// <summary>
        /// 튜토리얼 완료 처리
        /// </summary>
        public void CompleteTutorial()
        {
            if (!_isRunning) return;
            _isRunning = false;
            // 한 번만 보는 정책이 아니므로 완료 후 플래그 저장하지 않음
            GameLogger.LogInfo("[TutorialManager] 튜토리얼 완료", GameLogger.LogCategory.UI);
        }

        private void OnOverlayCompleted()
        {
            if (_overlay != null)
            {
                _overlay.Completed -= OnOverlayCompleted;
                // no op
            }
            // 페이지 완료 → 드래그/드랍 안내 후 실제 플레이 대기 단계로 전환
            if (_step == Step.PagesIntroAndTooltip)
            {
                _step = Step.WaitCardUseEnemyThenPlayer;
                _sawEnemyTurn = false;
                GameLogger.LogInfo("[TutorialManager] 페이지 완료 → 카드 사용 및 턴 복귀 대기", GameLogger.LogCategory.UI);
                // 안내를 닫고 실제 플레이를 유도
                if (_overlay != null) _overlay.Hide();
                return;
            }

            if (_step == Step.ActiveItem)
            {
                CompleteTutorial();
            }
        }

        private enum Step
        {
            None,
            PagesIntroAndTooltip,          // 1) 턴 순서, 2) 카드 툴팁 (수동 next)
            WaitCardUseEnemyThenPlayer,    // 3) 드래그/드랍 → 적 턴 → 플레이어 복귀 감지
            ActiveItem,                    // 4) 액티브 아이템 설명 (수동 next로 완료)
            Done
        }

        private void PrepareOverlay()
        {
            if (_overlay != null) return;

            // 우선: 씬에 이미 존재하면 재사용
            _overlay = FindFirstObjectByType<TutorialOverlayView>();
            if (_overlay != null) return;

            if (overlayPrefab == null)
            {
                GameLogger.LogWarning("[TutorialManager] overlayPrefab이 지정되지 않았습니다. 오버레이 없이 진행됩니다", GameLogger.LogCategory.UI);
                return;
            }

            // 부모 결정: 지정된 부모 > 최고 우선 Canvas > 자체 Transform
            Transform parent = overlayParent != null ? overlayParent : (Transform)FindBestCanvasTransform();
            // overlayParent가 지정되었지만 Canvas 계층이 아니면 보이는 캔버스로 대체
            if (parent != null && parent.GetComponentInParent<Canvas>() == null)
            {
                GameLogger.LogWarning("[TutorialManager] 지정한 Overlay Parent에 Canvas가 없습니다. 활성 Canvas로 대체합니다", GameLogger.LogCategory.UI);
                var fallback = FindBestCanvasTransform();
                if (fallback != null) parent = fallback;
            }
            if (parent == null) parent = this.transform;
            _overlay = Instantiate(overlayPrefab, parent);
            // Stretch anchors to full screen if possible
            var rt = _overlay.transform as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;
                rt.SetAsLastSibling();
            }
            GameLogger.LogInfo($"[TutorialManager] 오버레이 생성 위치: {(parent != null ? parent.name : "(self)")}", GameLogger.LogCategory.UI);
        }

        private Transform FindBestCanvasTransform()
        {
            Canvas best = null;
            var canvases = GameObject.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int bestScore = int.MinValue;
            foreach (var c in canvases)
            {
                if (!c.isActiveAndEnabled) continue;
                int score = 0;
                // ScreenSpaceOverlay 우선
                if (c.renderMode == RenderMode.ScreenSpaceOverlay) score += 1000;
                // 가장 높은 sortingOrder 우선
                score += c.sortingOrder;
                // 이름 힌트(게임 UI) 보너스
                var n = c.name.ToLowerInvariant();
                if (n.Contains("ui")) score += 50;
                if (n.Contains("hud")) score += 40;
                if (n.Contains("stage") || n.Contains("environment")) score += 20;
                if (n.Contains("canvas")) score += 10;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = c;
                }
                GameLogger.LogInfo($"[TutorialManager] Canvas 후보: {c.name}, renderMode={c.renderMode}, sortingOrder={c.sortingOrder}, score={score}", GameLogger.LogCategory.UI);
            }
            return best != null ? best.transform : null;
        }

        // 디버그: 강제 표시/플래그 초기화
        [ContextMenu("Tutorial/Force Show Now")]
        private void ForceShowNow()
        {
            PlayerPrefs.SetInt("TUTORIAL_SKIP", 0);
            PlayerPrefs.Save();
            _shouldRun = true;
            _startedOnce = false;
            _isRunning = false;
            StartTutorialIfReady();
        }

        [ContextMenu("Tutorial/Clear Flags (Done/Skip)")]
        private void ClearFlags()
        {
            PlayerPrefs.SetInt("TUTORIAL_SKIP", 0);
            PlayerPrefs.Save();
            GameLogger.LogInfo("[TutorialManager] 튜토리얼 플래그 초기화 (SKIP=0)", GameLogger.LogCategory.UI);
            _shouldRun = true;
        }

        private Game.CombatSystem.Interface.TurnType ConvertTurnType(Game.CombatSystem.Manager.TurnManager.TurnType t)
        {
            // 이름이 Player/Enemy로 동일하다고 가정하여 안전 매핑
            switch (t)
            {
                case Game.CombatSystem.Manager.TurnManager.TurnType.Player:
                    return Game.CombatSystem.Interface.TurnType.Player;
                case Game.CombatSystem.Manager.TurnManager.TurnType.Enemy:
                    return Game.CombatSystem.Interface.TurnType.Enemy;
                default:
                    return Game.CombatSystem.Interface.TurnType.Player;
            }
        }

        private void TryStartIfAlreadyPlayerTurn()
        {
            if (!_shouldRun || _isRunning || _startedOnce) return;
            bool isPlayer = _turnController != null ? _turnController.IsPlayerTurn() : false;
            if (!isPlayer)
            {
                var tm = FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
                if (tm != null) isPlayer = tm.IsPlayerTurn();
            }
            GameLogger.LogInfo($"[TutorialManager] 초기 상태 검사: isPlayerTurn={isPlayer}", GameLogger.LogCategory.UI);
            if (isPlayer) StartCoroutine(ShowNextFrame());
        }

        // ==== 이벤트 기반 진행 ====
        private void HandleTurnChanged(TurnType turn)
        {
            // 1) 아직 시작 전이면, 플레이어 턴 진입 시 첫 페이지를 표시합니다.
            if (!_isRunning)
            {
                if (_shouldRun && turn == TurnType.Player)
                {
                    // 초기 TurnController 설정 단계의 Player 이벤트(턴 카운트 0)를 건너뛰고,
                    // 실제 상태머신 진입 후 SetTurnAndIncrement로 1 이상이 된 뒤 시작합니다.
                    int count = _turnController?.TurnCount ?? 0;
                    if (count >= 1)
                    {
                        StartCoroutine(ShowNextFrame());
                    }
                }
                return;
            }
            // 2) 진행 중인 경우: 카드 사용→적→플레이어 복귀 감지 단계일 때만 처리
            if (_step != Step.WaitCardUseEnemyThenPlayer) return;

            if (turn == TurnType.Enemy)
            {
                _sawEnemyTurn = true;
                return;
            }

            if (turn == TurnType.Player && _sawEnemyTurn)
            {
                // 플레이어 턴 복귀. 3번째 턴 이상에서 아이템 설명 표시
                int count = _turnController?.TurnCount ?? 0;
                if (count >= 3)
                {
                    _step = Step.ActiveItem;
                    if (_overlay != null) { _overlay.ShowFirstPage(); _overlay.Completed += OnOverlayCompleted; }
                }
            }
        }

        private void HandleTurnCountChanged(int count)
        {
            // 사용 안 함: HandleTurnChanged에서 처리
        }

        #region 편의 메서드
        /// <summary>
        /// 인스펙터에서 강제로 다시 시작하고 싶을 때 호출합니다.
        /// </summary>
        [ContextMenu("Restart Tutorial (Editor)")]
        public void RestartTutorial()
        {
            // 스킵만 해제하면 다음 Player 턴에서 다시 실행됨
            PlayerPrefs.SetInt("TUTORIAL_SKIP", 0);
            PlayerPrefs.Save();
            _isRunning = false;
            StartTutorialIfReady();
        }

        // PageChanged 훅 제거됨(사중 레이어 미사용 단순 페이지 모드)
        #endregion

        private IEnumerator ShowNextFrame()
        {
            yield return null; // 한 프레임 지연 (UI 생성 직후 보장)
            StartTutorialIfReady();
        }

        #endregion
    }
}


