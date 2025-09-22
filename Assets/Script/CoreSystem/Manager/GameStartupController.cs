using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zenject;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Manager;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.StageSystem.Manager;
using Game.StageSystem.Interface;
using Game.CoreSystem.Interface;
using Game.CharacterSystem.Core;

namespace Game.CoreSystem.Manager
{
    /// <summary>
    /// 게임 시작 시퀀스를 통합 관리하는 컨트롤러입니다.
    /// 플레이어 캐릭터 > 적 캐릭터 > 전투/대기 슬롯 채우기 > 플레이어 스킬카드 생성 순서로 진행합니다.
    /// 플레이어는 한 번만 셋업하고, 적이 죽으면 적만 새로 셋업합니다.
    /// </summary>
    public class GameStartupController : MonoBehaviour
    {
        #region 설정

        [Header("게임 시작 설정")]
        [Tooltip("자동 게임 시작 활성화")]
        [SerializeField] private bool autoStartGame = true;

        [Tooltip("셋업 단계별 대기 시간")]
        [SerializeField] private float setupDelayBetweenSteps = 0.1f;

        #endregion

        #region 의존성 주입

        [Inject] private PlayerManager playerManager;
        [InjectOptional] private EnemyManager enemyManager;
        [Inject] private IPlayerHandManager playerHandManager;
        [Inject] private IStageManager stageManager;
        [InjectOptional] private CombatFlowManager combatFlowManager;
        [InjectOptional] private IGameStateManager gameStateManager;

        #endregion

        #region 내부 상태

        private bool isPlayerSetupComplete = false;
        private bool isGameStarted = false;
        private SetupPhase currentSetupPhase = SetupPhase.None;

        #endregion

        #region 이벤트

        /// <summary>게임 시작 완료 이벤트</summary>
        public System.Action OnGameStartupComplete;

        /// <summary>셋업 단계 완료 이벤트</summary>
        public System.Action<SetupPhase> OnSetupPhaseComplete;

        /// <summary>적 사망 시 셋업 재실행 이벤트</summary>
        public System.Action OnEnemyDeathSetupRequired;

        #endregion

        #region Unity 생명주기

        private void Start()
        {
            if (autoStartGame)
            {
                StartCoroutine(InitializeGameSequence());
            }
        }

        #endregion

        #region 게임 시작 시퀀스

        /// <summary>
        /// 게임 초기화 시퀀스를 실행합니다.
        /// </summary>
        private IEnumerator InitializeGameSequence()
        {
            if (isGameStarted)
            {
                GameLogger.LogWarning("게임이 이미 시작되었습니다.", GameLogger.LogCategory.Core);
                yield break;
            }

            GameLogger.LogInfo("게임 시작 시퀀스 시작", GameLogger.LogCategory.Core);

            // 1. 플레이어 캐릭터 셋업 (한 번만)
            yield return StartCoroutine(SetupPlayerCharacter());

            // 2. 적 캐릭터 셋업
            yield return StartCoroutine(SetupEnemyCharacter());

            // 3. 전투/대기 슬롯 채우기
            yield return StartCoroutine(SetupCombatSlots());

            // 4. 플레이어 스킬카드 생성
            yield return StartCoroutine(SetupPlayerSkillCards());

            // 5. 전투 시작
            StartCombat();

            isGameStarted = true;
            OnGameStartupComplete?.Invoke();

            GameLogger.LogInfo("게임 시작 시퀀스 완료", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// 적 사망 시 셋업을 재실행합니다. (플레이어는 그대로)
        /// </summary>
        public void OnEnemyDeath(ICharacter enemy)
        {
            GameLogger.LogInfo($"적 사망 감지: {enemy?.GetCharacterName()} - 적 셋업 재실행", GameLogger.LogCategory.Core);
            StartCoroutine(SetupAfterEnemyDeath());
        }

        /// <summary>
        /// 적 사망 후 셋업 시퀀스
        /// </summary>
        private IEnumerator SetupAfterEnemyDeath()
        {
            // 플레이어는 이미 셋업되어 있으므로 건너뛰고 적만 새로 셋업
            yield return StartCoroutine(SetupEnemyCharacter());
            yield return StartCoroutine(SetupCombatSlots());
            yield return StartCoroutine(SetupPlayerSkillCards());

            // 전투 재시작
            StartCombat();

            OnEnemyDeathSetupRequired?.Invoke();
            GameLogger.LogInfo("적 사망 후 셋업 완료", GameLogger.LogCategory.Core);
        }

        #endregion

        #region 셋업 단계별 구현

        /// <summary>
        /// 1단계: 플레이어 캐릭터 셋업
        /// </summary>
        private IEnumerator SetupPlayerCharacter()
        {
            currentSetupPhase = SetupPhase.PlayerCharacter;
            GameLogger.LogInfo("플레이어 캐릭터 셋업 시작", GameLogger.LogCategory.Core);

            // 플레이어 캐릭터가 이미 생성되어 있는지 확인
            var existingPlayer = playerManager.GetCharacter();
            if (existingPlayer != null)
            {
                GameLogger.LogInfo("플레이어 캐릭터가 이미 존재합니다.", GameLogger.LogCategory.Core);
                isPlayerSetupComplete = true;
                OnSetupPhaseComplete?.Invoke(currentSetupPhase);
                yield break;
            }

            // 플레이어 캐릭터 생성 (입장 연출 완료까지 대기하기 위해 이벤트 구독)
            bool playerReady = false;
            System.Action<Game.CharacterSystem.Interface.ICharacter> onReady = null;
            onReady = (ch) => { playerReady = true; };
            playerManager.OnPlayerCharacterReady += onReady;

            playerManager.CreateAndRegisterCharacter();

            // 캐릭터 생성 + 입장 애니메이션 완료까지 대기
            yield return new WaitUntil(() => playerManager.GetCharacter() != null && playerReady);
            playerManager.OnPlayerCharacterReady -= onReady;

            isPlayerSetupComplete = true;
            OnSetupPhaseComplete?.Invoke(currentSetupPhase);

            GameLogger.LogInfo("플레이어 캐릭터 셋업 완료", GameLogger.LogCategory.Core);
            yield return new WaitForSeconds(setupDelayBetweenSteps);
        }

        /// <summary>
        /// 2단계: 적 캐릭터 셋업
        /// </summary>
        private IEnumerator SetupEnemyCharacter()
        {
            currentSetupPhase = SetupPhase.EnemyCharacter;
            GameLogger.LogInfo("적 캐릭터 셋업 시작", GameLogger.LogCategory.Core);

            // EnemyManager가 없으면 런타임에 찾기
            if (enemyManager == null)
            {
                enemyManager = FindFirstObjectByType<EnemyManager>();
                if (enemyManager == null)
                {
                    GameLogger.LogError("EnemyManager를 찾을 수 없습니다.", GameLogger.LogCategory.Error);
                    yield break;
                }
                GameLogger.LogInfo("EnemyManager 런타임 발견", GameLogger.LogCategory.Core);
            }

            // 기존 적 제거
            var existingEnemy = enemyManager.GetCharacter();
            if (existingEnemy != null)
            {
                GameLogger.LogInfo("기존 적 제거 중...", GameLogger.LogCategory.Core);
                enemyManager.UnregisterCharacter();
                yield return new WaitForEndOfFrame();
            }

            // 다음 적 소환
            var spawnTask = stageManager.SpawnNextEnemyAsync();
            yield return new WaitUntil(() => spawnTask.IsCompleted);
            
            if (!spawnTask.Result)
            {
                GameLogger.LogError("적 소환 실패", GameLogger.LogCategory.Error);
                yield break;
            }

            // 적 생성 완료 + 입장 애니메이션 여유 시간 대기
            yield return new WaitUntil(() => enemyManager.GetCharacter() != null);
            yield return new WaitForSeconds(1.6f);

            OnSetupPhaseComplete?.Invoke(currentSetupPhase);

            GameLogger.LogInfo("적 캐릭터 셋업 완료", GameLogger.LogCategory.Core);
            yield return new WaitForSeconds(setupDelayBetweenSteps);
        }

        /// <summary>
        /// 3단계: 전투/대기 슬롯 채우기
        /// </summary>
        private IEnumerator SetupCombatSlots()
        {
            currentSetupPhase = SetupPhase.CombatSlots;
            GameLogger.LogInfo("전투/대기 슬롯 채우기 시작", GameLogger.LogCategory.Core);

            // 현재 적 정보로 초기 적 큐 세팅
            var enemy = enemyManager != null ? enemyManager.GetCharacter() : null;
            if (enemy is EnemyCharacter enemyCharacter)
            {
                var turnMgr = FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
                if (turnMgr != null)
                {
                    // 리플렉션 기반 호출로 컴파일 의존성 제거
                    try
                    {
                        var tmType = turnMgr.GetType();
                        var setup = tmType.GetMethod("SetupInitialEnemyQueue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        if (setup != null)
                        {
                            setup.Invoke(turnMgr, new object[] { enemyCharacter.Data, enemyCharacter.GetCharacterName() });
                        }
                        var force = tmType.GetMethod("ForceOneCycle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        if (force != null)
                        {
                            force.Invoke(turnMgr, null);
                        }
                        GameLogger.LogInfo("초기 적 카드 큐 세팅 진행", GameLogger.LogCategory.Core);
                    }
                    catch (System.Exception e)
                    {
                        GameLogger.LogWarning($"초기 적 큐 세팅 호출 실패: {e.Message}", GameLogger.LogCategory.Core);
                    }
                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    GameLogger.LogWarning("TurnManager를 찾을 수 없어 슬롯 세팅을 건너뜁니다.", GameLogger.LogCategory.Core);
                }
            }
            else
            {
                GameLogger.LogWarning("적 정보가 없어 슬롯 세팅을 건너뜁니다.", GameLogger.LogCategory.Core);
            }

            OnSetupPhaseComplete?.Invoke(currentSetupPhase);

            GameLogger.LogInfo("전투/대기 슬롯 채우기 완료", GameLogger.LogCategory.Core);
            yield return new WaitForSeconds(setupDelayBetweenSteps);
        }

        /// <summary>
        /// 4단계: 플레이어 스킬카드 생성
        /// </summary>
        private IEnumerator SetupPlayerSkillCards()
        {
            currentSetupPhase = SetupPhase.PlayerSkillCards;
            GameLogger.LogInfo("플레이어 스킬카드 생성 시작", GameLogger.LogCategory.Core);

            // 플레이어 핸드 매니저를 통한 스킬카드 생성
            if (playerHandManager != null)
            {
                var player = playerManager.GetCharacter();
                if (player != null)
                {
                    // 핸드 매니저에 플레이어 설정
                    playerHandManager.SetPlayer(player);

                    // 초기 핸드 생성 (UI 포함)
                    GameLogger.LogInfo("플레이어 초기 핸드 생성 중...", GameLogger.LogCategory.Core);
                    playerHandManager.GenerateInitialHand();
                    yield return new WaitForSeconds(setupDelayBetweenSteps);
                }
            }

            OnSetupPhaseComplete?.Invoke(currentSetupPhase);

            GameLogger.LogInfo("플레이어 스킬카드 생성 완료", GameLogger.LogCategory.Core);
            yield return new WaitForSeconds(setupDelayBetweenSteps);
        }

        #endregion

        #region 전투 시작

        /// <summary>
        /// 전투를 시작합니다.
        /// </summary>
        private void StartCombat()
        {
            GameLogger.LogInfo("전투 시작", GameLogger.LogCategory.Core);

            // CombatFlowManager가 없으면 런타임에 찾기
            if (combatFlowManager == null)
            {
                combatFlowManager = FindFirstObjectByType<CombatFlowManager>();
                if (combatFlowManager == null)
                {
                    GameLogger.LogError("CombatFlowManager를 찾을 수 없습니다.", GameLogger.LogCategory.Error);
                    return;
                }
                GameLogger.LogInfo("CombatFlowManager 런타임 발견", GameLogger.LogCategory.Core);
            }

            // CombatFlowManager의 적 사망 이벤트 구독
            combatFlowManager.OnEnemyDefeated += OnEnemyDeath;
            
            // CombatFlowManager 초기화 및 전투 시작
            StartCoroutine(InitializeAndStartCombat());
        }

        /// <summary>
        /// CombatFlowManager를 초기화하고 전투를 시작합니다.
        /// </summary>
        private IEnumerator InitializeAndStartCombat()
        {
            // CombatFlowManager 초기화 대기
            yield return StartCoroutine(combatFlowManager.InitializeCombat());
            
            // 전투 시작
            combatFlowManager.StartCombat();
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 수동으로 게임을 시작합니다.
        /// </summary>
        public void StartGame()
        {
            if (!isGameStarted)
            {
                StartCoroutine(InitializeGameSequence());
            }
        }

        /// <summary>
        /// 현재 셋업 단계를 반환합니다.
        /// </summary>
        public SetupPhase GetCurrentSetupPhase() => currentSetupPhase;

        /// <summary>
        /// 플레이어 셋업 완료 여부를 반환합니다.
        /// </summary>
        public bool IsPlayerSetupComplete() => isPlayerSetupComplete;

        /// <summary>
        /// 게임 시작 여부를 반환합니다.
        /// </summary>
        public bool IsGameStarted() => isGameStarted;

        #endregion
    }

    /// <summary>
    /// 셋업 단계 열거형
    /// </summary>
    public enum SetupPhase
    {
        None,
        PlayerCharacter,      // 1. 플레이어 캐릭터 생성
        EnemyCharacter,       // 2. 적 캐릭터 생성
        CombatSlots,          // 3. 전투/대기 슬롯 채우기
        PlayerSkillCards      // 4. 플레이어 스킬카드 생성
    }
}
