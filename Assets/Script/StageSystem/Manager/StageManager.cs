using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Interface;
using Game.StageSystem.Data;
using Game.StageSystem.Interface;
using Zenject;
using Game.CoreSystem.Utility;
using DG.Tweening;
using Game.CoreSystem.Audio;
using Game.ItemSystem.Runtime;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.StageSystem.Manager
{
    /// <summary>
    /// 스테이지 진행을 관리하는 매니저입니다.
    /// 스테이지의 모든 적을 순차적으로 생성하고,
    /// 모든 적 처치 시 스테이지 완료(승리)를 처리합니다.
    /// </summary>
    public class StageManager : MonoBehaviour, IStageManager
    {
        #region 인스펙터 필드

        [System.Serializable]
        public class StageSettings
        {
            [Header("스테이지 데이터")]
            [Tooltip("모든 스테이지 데이터 (1-4번 스테이지)")]
            public List<StageData> allStages = new List<StageData>();

        }


        [System.Serializable]
        public class DebugSettings
        {
            [Header("디버그 옵션")]
            [Tooltip("스테이지 정보 로깅")]
            public bool enableStageLogging = true;

            [Tooltip("적 상태 시각화")]
            public bool showEnemyStatus = false;

            [Tooltip("보상 정보 표시")]
            public bool showRewardInfo = false;

            [Tooltip("스테이지 진행 상태 표시")]
            public bool showProgressStatus = false;
        }

        [Header("🏰 스테이지 설정")]
        [SerializeField] private StageSettings stageSettings = new StageSettings();
        
        [Space(10)]
        [Header("🔧 디버그 설정")]
        [SerializeField] private DebugSettings debugSettings = new DebugSettings();

        #endregion

        #region 내부 상태

        private int currentEnemyIndex = 0;
        private bool isSpawning = false;
        private bool isStageCompleted = false;
        private bool isSummonInProgress = false;
        private bool isDestroyed = false;

        // 스테이지 진행 상태
        private StageProgressState progressState = StageProgressState.NotStarted;

        // 다중 스테이지 관리
        private StageData currentStage;
        private int totalStagesCompleted = 0;
        private bool isGameCompleted = false;

        [Zenject.Inject] private EnemyManager enemyManager;
        [Zenject.Inject(Optional = true)] private Game.CoreSystem.Interface.IAudioManager audioManager;
        [Zenject.Inject(Optional = true)] private Game.SkillCardSystem.Interface.IPlayerHandManager playerHandManager;
        [Zenject.Inject(Optional = true)] private Game.CombatSystem.Slot.CombatSlotRegistry combatSlotRegistry;
        [Zenject.Inject(Optional = true)] private Game.CombatSystem.Interface.ICombatTurnManager turnManager;
        [Zenject.Inject(Optional = true)] private Game.CharacterSystem.Manager.PlayerManager playerManager;
        [Zenject.Inject(Optional = true)] private Game.CombatSystem.State.CombatStateMachine combatStateMachine;
        [Zenject.Inject(Optional = true)] private Game.SkillCardSystem.Manager.PlayerHandManager playerHandManagerConcrete;
        [Zenject.Inject(Optional = true)] private ICardCirculationSystem cardCirculationSystem;
        [Zenject.Inject(Optional = true)] private Game.ItemSystem.Service.ItemService itemService;
        [Zenject.Inject(Optional = true)] private Game.CombatSystem.UI.VictoryUI victoryUI;
        [Zenject.Inject(Optional = true)] private Game.CharacterSystem.UI.EnemyCharacterUIController enemyCharacterUIController;

        private bool isWaitingForPlayer = false;

        [Header("🎁 보상 UI 브리지 (선택)")]
        [SerializeField] private RewardOnEnemyDeath rewardBridge;

        #endregion

        #region 이벤트

        /// <summary>적 처치 시 호출되는 이벤트</summary>
        public event Action<ICharacter> OnEnemyDefeated;
        
        /// <summary>스테이지 완료 시 호출되는 이벤트</summary>
        public event Action<StageData> OnStageCompleted;
        
        /// <summary>게임 완료 시 호출되는 이벤트 (모든 스테이지 완료)</summary>
        public event Action OnGameCompleted;
        
        /// <summary>스테이지 전환 시 호출되는 이벤트</summary>
        public event Action<StageData, StageData> OnStageTransition;
        
        /// <summary>보상 처리가 완료되었을 때 호출되는 이벤트</summary>
        public event Action OnRewardProcessCompleted;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 스테이지 매니저 초기화
        /// </summary>
        private void Start()
        {
            // 씬 재로드 시 상태 초기화
            isDestroyed = false;
            
            // 튜토리얼 실행 여부 결정 및 저장 (메인 메뉴 설정/최초 완료 상태 반영)
            try
            {
                bool skip = PlayerPrefs.GetInt("TUTORIAL_SKIP", 0) == 1;
                bool done = PlayerPrefs.GetInt("TUTORIAL_DONE", 0) == 1;
                int shouldRun = (!skip && !done) ? 1 : 0;
                PlayerPrefs.SetInt("TUTORIAL_SHOULD_RUN", shouldRun);
                PlayerPrefs.Save();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[StageManager] 튜토리얼 실행 플래그 설정 실패: {ex.Message}", GameLogger.LogCategory.UI);
            }

            // 새게임 요청 플래그 확인 및 초기화
            if (PlayerPrefs.GetInt("NEW_GAME_REQUESTED", 0) == 1)
            {
                InitializeGameStateForNewGame();
                PlayerPrefs.SetInt("NEW_GAME_REQUESTED", 0);
                PlayerPrefs.Save();
                
                // 통계 세션 시작
                // Statistics 제거됨
                
                // 새게임인 경우 기본 스테이지 로드
                LoadDefaultStage();
            }
            else
            {
                // 저장된 진행 상황이 있으면 자동 로드
                StartCoroutine(AutoLoadSavedProgress());
            }

            // PlayerManager의 플레이어 준비 완료 이벤트 구독
            if (playerManager != null)
            {
                playerManager.OnPlayerCharacterReady += OnPlayerReady;
            }
            else
            {
                GameLogger.LogWarning("[StageManager] PlayerManager를 찾을 수 없습니다 - 플레이어 준비 대기 건너뜀", GameLogger.LogCategory.Combat);
            }
        }

        private void OnDisable()
        {
            // DOTween 애니메이션 정리
            transform.DOKill();
        }

        private void OnDestroy()
        {
            // DOTween 애니메이션 정리
            transform.DOKill();
            
            // 씬 전환/파괴 상태 표시
            isDestroyed = true;
            
            // 이벤트 구독 해제
            if (playerManager != null)
            {
                playerManager.OnPlayerCharacterReady -= OnPlayerReady;
            }
        }

        /// <summary>
        /// 새게임을 위한 게임 상태 초기화
        /// </summary>
        private void InitializeGameStateForNewGame()
        {
            // ItemService Fallback 주입 시도
            EnsureItemServiceInjected();
            
            // 인벤토리 초기화 (스킬카드 스택은 캐릭터 생성 시 초기화됨)
            if (itemService != null)
            {
                itemService.ResetInventoryForNewGame();
                GameLogger.LogInfo("[StageManager] 인벤토리 초기화 완료", GameLogger.LogCategory.Save);
            }
            else
            {
                GameLogger.LogWarning("[StageManager] ItemService를 찾을 수 없습니다 - 인벤토리 초기화 건너뜀", GameLogger.LogCategory.Save);
            }
        }

        /// <summary>
        /// ItemService가 주입되지 않았으면 Fallback 주입을 시도합니다.
        /// </summary>
        private void EnsureItemServiceInjected()
        {
            if (itemService != null) return;

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
                            var resolvedItemService = sceneContainer.TryResolve<Game.ItemSystem.Service.ItemService>();
                            if (resolvedItemService != null)
                            {
                                itemService = resolvedItemService;
                                GameLogger.LogInfo("[StageManager] ItemService를 SceneContext에서 찾아서 주입했습니다.", GameLogger.LogCategory.Save);
                                return;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[StageManager] SceneContext 주입 시도 실패: {ex.Message}", GameLogger.LogCategory.Save);
                    }
                }

                // 2. FindFirstObjectByType을 사용한 폴백
                itemService = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Service.ItemService>(UnityEngine.FindObjectsInactive.Include);
                if (itemService != null)
                {
                    GameLogger.LogInfo("[StageManager] ItemService 직접 찾기 완료 (FindFirstObjectByType)", GameLogger.LogCategory.Save);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[StageManager] ItemService 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 플레이어 캐릭터 준비 완료 시 호출
        /// </summary>
        private void OnPlayerReady(ICharacter player)
        {
            // Statistics 제거됨

            // 대기 중이었다면 스테이지 시작
            if (isWaitingForPlayer)
            {
                isWaitingForPlayer = false;
                StartStage();
            }
        }
        
        /// <summary>
        /// 저장된 진행 상황을 자동으로 로드합니다.
        /// </summary>
        private System.Collections.IEnumerator AutoLoadSavedProgress()
        {
            // SaveSystem 제거됨 - 항상 기본 스테이지 로드
            LoadDefaultStage();
            yield break;
        }
        
        /// <summary>
        /// 기본 스테이지를 로드합니다.
        /// </summary>
        private void LoadDefaultStage()
        {
            if (LoadStage(1))
            {
                // 플레이어 준비 완료 대기 플래그 설정
                isWaitingForPlayer = true;
                
                // 플레이어가 이미 준비되었는지 확인
                if (playerManager != null && playerManager.GetCharacter() != null)
                {
                    isWaitingForPlayer = false;
                    StartStage();
                }
            }
            else
            {
                GameLogger.LogError("기본 스테이지 로드 실패", GameLogger.LogCategory.Combat);
            }
        }
        
        /// <summary>
        /// 다른 씬으로 전환하기 전에 현재 진행 상황을 저장합니다.
        /// 메인 씬으로 전환되는 경우 통계도 저장합니다.
        /// </summary>
        public Task SaveProgressBeforeSceneTransition()
        {
            // 씬 전환 상태 표시
            isDestroyed = true;
            
            // SaveSystem 및 Statistics 제거됨 - 저장 로직 없음
            return Task.CompletedTask;
        }

        #endregion

        #region 의존성 주입 (최소화)

        // 핵심 의존성만 유지
        // EnemyManager는 런타임에 찾아서 사용

        #endregion

        #region 적 생성 흐름

        /// <summary>
        /// 다음 적을 생성하여 전투에 배치합니다 (async/await 기반)
        /// </summary>
        /// <returns>적 생성 성공 여부</returns>
        public async Task<bool> SpawnNextEnemyAsync()
        {
            // 씬 전환/파괴 상태 확인
            if (isDestroyed || this == null)
            {
                GameLogger.LogDebug("StageManager가 파괴되었습니다 - 적 생성 취소", GameLogger.LogCategory.Combat);
                return false;
            }
            
            if (isSpawning)
            {
                GameLogger.LogWarning("중복 스폰 방지", GameLogger.LogCategory.Combat);
                return false;
            }

            if (enemyManager?.GetEnemy() != null)
            {
                GameLogger.LogWarning("이미 적이 존재합니다", GameLogger.LogCategory.Combat);
                return false;
            }

            if (!TryGetNextEnemyData(out var data))
            {
                GameLogger.LogWarning("다음 적 데이터를 가져올 수 없습니다", GameLogger.LogCategory.Combat);
                return false;
            }

            isSpawning = true;
            
            try
            {
                // 적 생성 (단순화된 로직)
                var enemy = await CreateEnemyAsync(data);
                
                // 씬 전환/파괴 상태 재확인 (생성 후에도 확인)
                if (isDestroyed || this == null || enemy == null)
                {
                    if (isDestroyed || this == null)
                    {
                        GameLogger.LogWarning("StageManager가 파괴되었습니다 - 적 생성 취소 (생성 후)", GameLogger.LogCategory.Combat);
                    }
                    else
                    {
                        GameLogger.LogError("적 생성 실패", GameLogger.LogCategory.Combat);
                    }
                    isSpawning = false;
                    return false;
                }

                RegisterEnemy(enemy);

                // 적별 BGM 재생 (AudioManager에 위임)
                if (audioManager != null)
                {
                    audioManager.PlayEnemyBGM(data);
                }
                else
                {
                    GameLogger.LogWarning("AudioManager가 null입니다 - BGM 재생 건너뜀", GameLogger.LogCategory.Audio);
                }

                currentEnemyIndex++;

                // CombatStateMachine에 적 생성 완료 알림 (DI 주입)
                if (combatStateMachine != null)
                {
                    if (currentEnemyIndex == 1)
                    {
                        // 첫 번째 적이 생성되면 CombatStateMachine 시작
                        // 적 데이터를 가져와서 StartCombat에 전달
                        if (enemy is Game.CharacterSystem.Core.EnemyCharacter enemyChar)
                        {
                            var enemyData = enemyChar.CharacterData;
                            if (enemyData != null)
                            {
                                combatStateMachine.StartCombat(enemyData, enemy.GetCharacterName());
                            }
                            else
                            {
                                GameLogger.LogWarning($"[StageManager] 적 데이터를 가져올 수 없습니다: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                                combatStateMachine.StartCombat();
                            }
                        }
                        else
                        {
                            GameLogger.LogWarning($"[StageManager] 적 캐릭터 타입을 확인할 수 없습니다: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                            combatStateMachine.StartCombat();
                        }
                    }
                    else
                    {
                        // 다음 적이 생성되면 CombatStateMachine에 알림
                        combatStateMachine.OnNextEnemySpawned();
                    }
                }
                else
                {
                    GameLogger.LogWarning("[StageManager] CombatStateMachine을 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"적 생성 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
                return false;
            }
            finally
            {
                isSpawning = false;
            }
        }

        /// <summary>
        /// 다음 적을 생성합니다 (기존 API 호환성을 위한 동기 메서드)
        /// </summary>
        public void SpawnNextEnemy()
        {
            _ = SpawnNextEnemyAsync();
        }



        /// <summary>
        /// 스테이지 종료 시 BGM 정리 (씬 전환 전 호출)
        /// </summary>
        public void CleanupStageBGM()
        {
            if (audioManager != null)
            {
                audioManager.StopBGM();
            }
        }

        /// <summary>
        /// 적 캐릭터를 시스템에 등록합니다
        /// </summary>
        /// <param name="enemy">등록할 적 캐릭터</param>
        public void RegisterEnemy(ICharacter enemy)
        {
            enemyManager?.RegisterEnemy(enemy);

            if (enemy is EnemyCharacter concreteEnemy)
            {
                // SetDeathListener는 TODO 상태이므로 SetDeathCallback 사용
                concreteEnemy.SetDeathCallback(OnEnemyDeath);
                // OnSummonRequested 이벤트는 더 이상 사용하지 않음 (상태 패턴으로 처리)
            }
        }

        /// <summary>
        /// 소환된 적 캐릭터를 시스템에 등록합니다
        /// 일반 적과 달리 사망 콜백을 덮어쓰지 않습니다
        /// </summary>
        /// <param name="enemy">등록할 소환된 적 캐릭터</param>
        public void RegisterSummonedEnemy(ICharacter enemy)
        {
            GameLogger.LogInfo($"[StageManager] RegisterSummonedEnemy 호출: {enemy?.GetCharacterName() ?? "null"}", GameLogger.LogCategory.Combat);
            
            enemyManager?.RegisterEnemy(enemy);
            GameLogger.LogInfo($"[StageManager] EnemyManager에 소환된 적 등록 완료", GameLogger.LogCategory.Combat);

            // 소환된 적은 소환 사망 콜백을 설정합니다
            if (enemy is EnemyCharacter concreteEnemy)
            {
                concreteEnemy.SetDeathCallback(OnSummonedEnemyDeath);
                GameLogger.LogInfo($"[StageManager] 소환된 적 사망 콜백 설정 완료: {concreteEnemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                // OnSummonRequested 이벤트는 더 이상 사용하지 않음 (상태 패턴으로 처리)
            }
            else
            {
                GameLogger.LogWarning($"[StageManager] RegisterSummonedEnemy: EnemyCharacter로 캐스팅 실패", GameLogger.LogCategory.Combat);
            }
        }

		/// <summary>
		/// 적 처치 시 호출되는 메서드
		/// </summary>
		public void OnEnemyDeath(ICharacter enemy)
		{
			GameLogger.LogInfo($"[StageManager] OnEnemyDeath 호출: {enemy?.GetCharacterName() ?? "null"}, 소환스택={originalEnemyStack.Count}", GameLogger.LogCategory.Combat);
			
			// 소환된 적인지 확인 (소환 컨텍스트 스택이 비어 있지 않으면 소환 체인 진행 중)
			if (originalEnemyStack.Count > 0)
			{
				GameLogger.LogInfo($"[StageManager] 소환된 적 사망 감지 - OnSummonedEnemyDeath 호출 (스택: {originalEnemyStack.Count})", GameLogger.LogCategory.Combat);
				
				// 소환된 적 사망 콜백 호출
				OnSummonedEnemyDeath(enemy);
				return; // 소환된 적은 일반적인 적 처치 로직을 건너뜀
			}
			
			GameLogger.LogInfo($"[StageManager] 원본 적 사망 감지 - 일반 적 처치 로직 진행", GameLogger.LogCategory.Combat);

			// 일반 적 처치 로직
			// CombatStateMachine에 적 사망 알림 (적 제거 전에 알려야 함, DI 주입)
			if (combatStateMachine != null)
			{
				combatStateMachine.OnEnemyDeathDetected();
			}

			// 적 처치 이벤트 발생
			OnEnemyDefeated?.Invoke(enemy);

			// 적을 enemyManager에서 제거
            if (enemyManager != null)
            {
                enemyManager.UnregisterEnemy();
            }

			// 적 GameObject 파괴
            if (enemy is EnemyCharacter enemyCharacter)
            {
                Destroy(enemyCharacter.gameObject);
            }

            // 보상창 열기는 EnemyDefeatedState 완료 후로 이동
            // (EnemyDefeatedState에서 OnEnemyDefeatedCleanupCompleted 이벤트 발생 시 처리)
		}

		/// <summary>
		/// 보상 처리가 완료되었을 때 호출되는 콜백
		/// </summary>
		private void HandleRewardProcessCompleted()
		{
			// 콜백 해제
			if (rewardBridge != null)
			{
				rewardBridge.OnRewardProcessCompleted -= HandleRewardProcessCompleted;
			}

			// 보상 완료 이벤트 발생 (StageFlowStateMachine 등이 구독)
			OnRewardProcessCompleted?.Invoke();

			// 스테이지 진행 상태 업데이트
			UpdateStageProgress(null); // enemy는 이미 제거되었으므로 null 전달
		}

		/// <summary>
		/// 적 캐릭터 처치 후 스테이지 진행 상태를 업데이트합니다.
		/// 모든 적 처치 시 스테이지 완료(승리)를 처리합니다.
		/// </summary>
		private void UpdateStageProgress(ICharacter enemy)
		{
			// 다음 적이 있는지 확인
			if (HasMoreEnemies())
			{
				// 적 카드 슬롯 정리 후 다음 적 생성
				_ = ClearEnemySlotsAndSpawnNext();
			}
			else
			{
				// 모든 적 처치 완료 - 스테이지 승리!
				CompleteStage();
			}
		}

		/// <summary>
		/// EnemyDefeatedState의 정리 작업이 완료되었을 때 호출되는 메서드
		/// </summary>
        public void OnEnemyDefeatedCleanupCompleted()
        {
			// 스테이지 1의 마지막 적 처치 시 스킬카드 보상 지급 시도
			TryGiveStage1FinalEnemyCardReward();
			
			// rewardBridge가 null이면 씬에서 찾기
			if (rewardBridge == null)
			{
				rewardBridge = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Runtime.RewardOnEnemyDeath>(UnityEngine.FindObjectsInactive.Include);
				if (rewardBridge != null)
				{
					GameLogger.LogInfo("[StageManager] RewardOnEnemyDeath를 씬에서 찾아서 연결했습니다.", GameLogger.LogCategory.Combat);
				}
				else
				{
					GameLogger.LogWarning("[StageManager] RewardOnEnemyDeath를 찾을 수 없습니다. 적 처치 보상이 작동하지 않을 수 있습니다. CombatScene에 RewardOnEnemyDeath 컴포넌트가 있는지 확인하세요.", GameLogger.LogCategory.Combat);
				}
			}
			
			// 보상 UI 열기 및 완료 대기 (설정된 경우)
			if (rewardBridge != null)
			{
				// 보상 완료 콜백 연결
				rewardBridge.OnRewardProcessCompleted += HandleRewardProcessCompleted;
				
                rewardBridge.OnEnemyKilled();
			}
            else
            {
                // 보상 브리지가 없으면 바로 다음 진행
                GameLogger.LogWarning("[StageManager] rewardBridge가 null입니다. 보상 처리를 건너뜁니다.", GameLogger.LogCategory.Combat);
                UpdateStageProgress(null);
            }
		}

		/// <summary>
		/// 스테이지 1의 마지막 적을 처치했을 때 스킬카드 보상을 지급합니다.
		/// 보상은 덱에 추가되며, 다음 스테이지부터 전투에 등장합니다.
		/// </summary>
		private void TryGiveStage1FinalEnemyCardReward()
		{
			try
			{
				// 스테이지 정보 또는 보상 시스템이 준비되지 않은 경우 건너뜀
				if (currentStage == null)
				{
					return;
				}

				// 스테이지 1이 아니면 처리하지 않음
				if (currentStage.stageNumber != 1)
				{
					return;
				}

				// 아직 남은 적이 있다면 마지막 적이 아니므로 처리하지 않음
				if (HasMoreEnemies())
				{
					return;
				}

				// 플레이어/캐릭터 데이터 확인
				if (playerManager == null)
				{
					GameLogger.LogWarning("[StageManager] PlayerManager가 주입되지 않았습니다. 스킬카드 보상을 지급할 수 없습니다.", GameLogger.LogCategory.SkillCard);
					return;
				}

				var player = playerManager.GetPlayer();
				if (player == null)
				{
					GameLogger.LogWarning("[StageManager] 플레이어 캐릭터가 생성되지 않았습니다. 스킬카드 보상을 지급할 수 없습니다.", GameLogger.LogCategory.SkillCard);
					return;
				}

				if (player.CharacterData is not PlayerCharacterData playerData)
				{
					GameLogger.LogWarning("[StageManager] 플레이어 캐릭터 데이터가 PlayerCharacterData가 아닙니다. 스킬카드 보상을 지급할 수 없습니다.", GameLogger.LogCategory.SkillCard);
					return;
				}

				// 우선 캐릭터에 설정된 고유 스킬카드를 보상으로 사용
				SkillCardDefinition rewardCard = playerData.UniqueSkillCard;

				// 고유 스킬카드가 설정되지 않은 경우, 스킬 덱에서 플레이어/공용 카드 중 하나를 선택
				if (rewardCard == null)
				{
					if (playerData.SkillDeck == null)
					{
						GameLogger.LogWarning($"[StageManager] 플레이어 캐릭터 '{playerData.DisplayName}'의 스킬 덱이 설정되지 않았습니다. 스킬카드 보상을 지급할 수 없습니다.", GameLogger.LogCategory.SkillCard);
						return;
					}

					var cardEntries = playerData.SkillDeck.CardEntries;
					if (cardEntries != null)
					{
						foreach (var entry in cardEntries)
						{
							if (entry == null || entry.cardDefinition == null)
							{
								continue;
							}

							var definition = entry.cardDefinition;
							if (definition.configuration.ownerPolicy == OwnerPolicy.Enemy)
							{
								continue;
							}

							rewardCard = definition;
							break;
						}
					}
				}

				if (rewardCard == null)
				{
					GameLogger.LogWarning("[StageManager] 스킬 덱에서 보상으로 줄 수 있는 스킬카드를 찾지 못했습니다.", GameLogger.LogCategory.SkillCard);
					return;
				}

				// 보상 UI 브리지가 있으면: 보상창에서 선택/나가기 시 지급되도록 후보만 설정
				if (rewardBridge != null)
				{
					rewardBridge.SetPendingSkillCardReward(rewardCard);

					if (debugSettings != null && debugSettings.showRewardInfo)
					{
						GameLogger.LogInfo($"[StageManager] 스테이지 1 마지막 적 처치 보상으로 스킬카드 후보 설정: {rewardCard.displayName}", GameLogger.LogCategory.SkillCard);
					}
				}
				else
				{
					// 보상 UI가 없으면 즉시 덱에 추가 (자동 지급)
					if (cardCirculationSystem == null)
					{
						GameLogger.LogWarning("[StageManager] 카드 순환 시스템이 주입되지 않았습니다. 스킬카드 보상을 자동 지급할 수 없습니다.", GameLogger.LogCategory.SkillCard);
						return;
					}

					bool success = cardCirculationSystem.GiveCardReward(rewardCard, 1);
					if (!success)
					{
						GameLogger.LogWarning($"[StageManager] 스킬카드 보상 자동 지급에 실패했습니다: {rewardCard.displayName}", GameLogger.LogCategory.SkillCard);
						return;
					}

					if (debugSettings != null && debugSettings.showRewardInfo)
					{
						GameLogger.LogInfo($"[StageManager] 스테이지 1 마지막 적 처치 보상으로 스킬카드 자동 지급: {rewardCard.displayName}", GameLogger.LogCategory.SkillCard);
					}
				}
			}
			catch (Exception ex)
			{
				GameLogger.LogError($"[StageManager] 스킬카드 보상 처리 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
			}
		}

        /// <summary>
        /// 적 카드 슬롯을 정리하고 다음 적을 생성합니다.
        /// </summary>
        private async Task ClearEnemySlotsAndSpawnNext()
        {
            // 적 카드 슬롯 정리
            await ClearEnemyCardsFromSlots();

            // 다음 적 생성
            await SpawnNextEnemyAsync();
        }

        /// <summary>
        /// 전투/대기 슬롯에서 모든 카드를 제거합니다 (플레이어 턴 마커 + 적 카드).
        /// 새로운 로직: 적 처치 시 모든 슬롯을 완전히 정리
        /// </summary>
        private async Task ClearEnemyCardsFromSlots()
        {
            // TurnManager를 통해 모든 카드 제거 (데이터 + UI)
            if (turnManager != null)
            {
                if (turnManager is Game.CombatSystem.Manager.TurnManager tm)
                {
                    // 적 캐시 초기화
                    tm.ClearEnemyCache();

                    // 모든 슬롯 정리 (플레이어 턴 마커 + 적 카드 모두 제거)
                    var allSlots = new System.Collections.Generic.List<Game.CombatSystem.Slot.CombatSlotPosition>
                    {
                        Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT,
                        Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_1,
                        Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_2,
                        Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_3,
                        Game.CombatSystem.Slot.CombatSlotPosition.WAIT_SLOT_4
                    };

                    foreach (var slot in allSlots)
                    {
                        tm.ClearSlot(slot);
                    }
                }
            }
            else
            {
                GameLogger.LogWarning($"[StageManager] TurnManager를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
            }

            await Task.Yield();
        }

        /// <summary>
        /// 적 캐릭터를 생성합니다.
        /// </summary>
        private async Task<ICharacter> CreateEnemyAsync(EnemyCharacterData data)
        {
            return await CreateEnemyInternalAsync(data);
        }

        /// <summary>
        /// 소환 시스템용 적 캐릭터 생성 (public 접근)
        /// </summary>
        public async Task<ICharacter> CreateEnemyForSummonAsync(EnemyCharacterData data)
        {
            return await CreateEnemyInternalAsync(data);
        }

        /// <summary>
        /// 적 캐릭터를 생성합니다 (내부 구현)
        /// </summary>
        private async Task<ICharacter> CreateEnemyInternalAsync(EnemyCharacterData data)
        {
            // 씬 전환/파괴 상태 확인
            if (isDestroyed || this == null)
            {
                GameLogger.LogDebug("StageManager가 파괴되었습니다 - 적 생성 취소", GameLogger.LogCategory.Combat);
                return null;
            }
            
            if (data?.Prefab == null)
            {
                GameLogger.LogError("적 데이터 또는 프리팹이 null입니다", GameLogger.LogCategory.Error);
                return null;
            }

            // 비동기 처리 시뮬레이션
            await Task.Delay(100);

            // 씬 전환/파괴 상태 재확인 (비동기 작업 중간에 씬이 전환될 수 있음)
            if (isDestroyed || this == null)
            {
                GameLogger.LogWarning("StageManager가 파괴되었습니다 - 적 생성 취소 (비동기 작업 중)", GameLogger.LogCategory.Combat);
                return null;
            }

            if (enemyManager == null)
            {
                GameLogger.LogError("EnemyManager를 찾을 수 없습니다", GameLogger.LogCategory.Error);
                return null;
            }

            var characterSlot = enemyManager.GetCharacterSlot();
            if (characterSlot == null)
            {
                GameLogger.LogError("EnemyManager의 characterSlot을 찾을 수 없습니다", GameLogger.LogCategory.Error);
                return null;
            }

            // 적 프리팹 인스턴스 생성 (characterSlot에 배치)
            var enemyInstance = Instantiate(data.Prefab, characterSlot);
            enemyInstance.name = data.name; // ScriptableObject의 이름 사용
            
            // ICharacter 컴포넌트 확인
            if (!enemyInstance.TryGetComponent(out ICharacter enemy))
            {
                GameLogger.LogError($"적 프리팹에 ICharacter 컴포넌트가 없습니다: {data.CharacterName}", GameLogger.LogCategory.Error);
                Destroy(enemyInstance);
                return null;
            }
            
            // Zenject 의존성 주입 (Instantiate로 생성된 객체는 자동 주입되지 않음)
            try
            {
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    // SceneContext에서 먼저 시도
                    Zenject.DiContainer sceneContainer = null;
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        sceneContainer = sceneContextRegistry.TryGetContainerForScene(enemyInstance.scene);
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogDebug($"SceneContextRegistry를 찾을 수 없거나 씬 컨테이너 획득 중 오류: {ex.Message}", GameLogger.LogCategory.Combat);
                    }

                    // SceneContext에서 주입 시도
                    if (sceneContainer != null)
                    {
                        sceneContainer.InjectGameObject(enemyInstance);
                        GameLogger.LogDebug($"적 캐릭터 Zenject 주입 완료 (SceneContext): {data.CharacterName}", GameLogger.LogCategory.Combat);
                    }
                    else
                    {
                        // ProjectContext에서 주입 시도
                        projectContext.Container.InjectGameObject(enemyInstance);
                        GameLogger.LogDebug($"적 캐릭터 Zenject 주입 완료 (ProjectContext): {data.CharacterName}", GameLogger.LogCategory.Combat);
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"적 캐릭터 Zenject 주입 중 오류 (계속 진행): {ex.Message}", GameLogger.LogCategory.Combat);
            }
            
            // 적 데이터 설정
            enemy.SetCharacterData(data);

            // 등장 연출 (오른쪽 바깥에서 자리로) - 플레이어와 동일한 방식으로 처리
            // 씬 전환/파괴 상태 확인
            if (isDestroyed || this == null || enemyInstance == null)
            {
                GameLogger.LogWarning("StageManager가 파괴되었습니다 - 애니메이션 취소", GameLogger.LogCategory.Combat);
                return null;
            }
            
            var entranceTween = TryPlayEntranceAnimation(enemyInstance.transform, fromLeft: false);

            // 애니메이션 완료 대기 (플레이어와 동일한 방식: TaskCompletionSource 사용)
            if (entranceTween != null && !isDestroyed && this != null && enemyInstance != null)
            {
                try
                {
                    GameLogger.LogDebug($"적 등장 애니메이션 시작: {data.CharacterName}", GameLogger.LogCategory.Combat);
                    
                    // TaskCompletionSource를 사용하여 애니메이션 완료를 대기
                    var tcs = new TaskCompletionSource<bool>();
                    bool animationCompleted = false;
                    
                    entranceTween.OnComplete(() =>
                    {
                        if (!animationCompleted)
                        {
                            animationCompleted = true;
                            tcs.TrySetResult(true);
                            GameLogger.LogDebug($"적 등장 애니메이션 완료: {data.CharacterName}", GameLogger.LogCategory.Combat);
                        }
                    });
                    
                    // 타임아웃 설정 (최대 2초)
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(2.0));
                    var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);
                    
                    // 씬 전환/파괴 상태 재확인
                    if (isDestroyed || this == null || enemyInstance == null || characterSlot == null)
                    {
                        GameLogger.LogWarning("StageManager가 파괴되었습니다 - 애니메이션 완료 후 취소", GameLogger.LogCategory.Combat);
                        // 애니메이션 취소
                        if (entranceTween != null && entranceTween.IsActive())
                        {
                            entranceTween.Kill();
                        }
                        return null;
                    }
                    
                    if (completedTask == timeoutTask && !animationCompleted)
                    {
                        GameLogger.LogDebug($"적 등장 애니메이션 타임아웃: {data.CharacterName}", GameLogger.LogCategory.Combat);
                        // 타임아웃 시 애니메이션 취소
                        if (entranceTween != null && entranceTween.IsActive())
                        {
                            entranceTween.Kill();
                        }
                    }
                }
                catch (Exception ex)
                {
                    GameLogger.LogWarning($"애니메이션 대기 중 오류 발생 (씬 전환 가능성): {ex.Message}", GameLogger.LogCategory.Combat);
                    // 애니메이션 취소
                    if (entranceTween != null && entranceTween.IsActive())
                    {
                        entranceTween.Kill();
                    }
                    
                    // 씬 전환/파괴 상태 확인
                    if (isDestroyed || this == null || enemyInstance == null || characterSlot == null)
                    {
                        return null;
                    }
                }
            }

            // 최종 상태 확인
            if (isDestroyed || this == null || enemyInstance == null || characterSlot == null)
            {
                GameLogger.LogWarning("StageManager가 파괴되었습니다 - 적 생성 최종 취소", GameLogger.LogCategory.Combat);
                return null;
            }

            GameLogger.LogInfo($"적 캐릭터 생성 및 배치 완료: {data.CharacterName} (슬롯: {characterSlot.name})", GameLogger.LogCategory.Combat);
            return enemy;
        }

        /// <summary>
        /// 캐릭터가 화면 밖에서 슬라이드 인 되는 연출을 수행합니다.
        /// RectTransform이 있으면 DOAnchorPos, 아니면 DOMove를 사용합니다.
        /// </summary>
        private Tween TryPlayEntranceAnimation(Transform target, bool fromLeft)
        {
            if (target == null || isDestroyed || this == null) return null;
            
            const float duration = 1.5f;
            var ease = Ease.InOutCubic;

            try
            {
                if (target is RectTransform rt)
                {
                    // 객체가 유효한지 확인
                    if (rt == null || rt.gameObject == null) return null;
                    
                    Vector2 end = rt.anchoredPosition;
                    Vector2 start = new Vector2(fromLeft ? -1100f : 1100f, end.y);
                    rt.anchoredPosition = start;
                    
                    var tween = rt.DOAnchorPos(end, duration)
                        .SetEase(ease)
                        .SetAutoKill(true); // 자동 정리 설정
                    
                    return tween;
                }
                else
                {
                    // 객체가 유효한지 확인
                    if (target == null || target.gameObject == null) return null;
                    
                    Vector3 end = target.position;
                    Vector3 start = new Vector3(fromLeft ? -1100f : 1100f, end.y, end.z);
                    target.position = start;
                    
                    var tween = target.DOMove(end, duration)
                        .SetEase(ease)
                        .SetAutoKill(true); // 자동 정리 설정
                    
                    return tween;
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogWarning($"애니메이션 생성 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Combat);
                return null;
            }
        }

        /// <summary>
        /// 다음 적 데이터를 조회합니다.
        /// </summary>
        private bool TryGetNextEnemyData(out EnemyCharacterData data)
        {
            data = null;

            if (currentStage == null ||
                currentStage.enemies == null ||
                currentEnemyIndex >= currentStage.enemies.Count)
                return false;

            data = currentStage.enemies[currentEnemyIndex];
            return data != null && data.Prefab != null;
        }

        #endregion

        #region 스테이지 정보

        /// <summary>
        /// 현재 스테이지 데이터를 반환합니다
        /// </summary>
        /// <returns>현재 스테이지 데이터, 없으면 null</returns>
        public StageData GetCurrentStage() => currentStage;

        /// <summary>
        /// 다음 적이 있는지 확인합니다
        /// </summary>
        /// <returns>다음 적이 있으면 true</returns>
        public bool HasNextEnemy() =>
            currentStage != null && currentEnemyIndex < currentStage.enemies.Count;

        /// <summary>
        /// 아직 처치하지 않은 적이 더 있는지 확인합니다.
        /// </summary>
        private bool HasMoreEnemies()
        {
            return HasNextEnemy();
        }

        /// <summary>
        /// 다음 적 데이터를 미리 확인합니다 (제거하지 않음)
        /// </summary>
        /// <returns>다음 적 데이터, 없으면 null</returns>
        public EnemyCharacterData PeekNextEnemyData() =>
            HasNextEnemy() ? currentStage.enemies[currentEnemyIndex] : null;

        /// <summary>
        /// 현재 스테이지 번호를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="stageNumber">스테이지 번호</param>
        public void SetCurrentStageNumber(int stageNumber)
        {
            if (stageNumber < 1 || stageNumber > 4)
            {
                GameLogger.LogError($"잘못된 스테이지 번호: {stageNumber}", GameLogger.LogCategory.Combat);
                return;
            }
            
            LoadStage(stageNumber);
            GameLogger.LogInfo($"스테이지 번호 설정: {stageNumber}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 현재 스테이지 번호를 가져옵니다. (저장 시스템용)
        /// </summary>
        /// <returns>스테이지 번호</returns>
        public int GetCurrentStageNumber()
        {
            return currentStage?.stageNumber ?? 1;
        }
        
        /// <summary>
        /// 스테이지 진행 상태를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="state">설정할 진행 상태</param>
        public void SetProgressState(StageProgressState state)
        {
            progressState = state;
            OnProgressChanged?.Invoke(progressState);
            GameLogger.LogInfo($"스테이지 진행 상태 설정: {state}", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 현재 적 인덱스를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="index">설정할 적 인덱스</param>
        public void SetCurrentEnemyIndex(int index)
        {
            if (index < 0)
            {
                GameLogger.LogError($"잘못된 적 인덱스: {index}", GameLogger.LogCategory.Combat);
                return;
            }
            
            currentEnemyIndex = index;
            GameLogger.LogInfo($"현재 적 인덱스 설정: {index}", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 현재 적 인덱스를 가져옵니다. (저장 시스템용)
        /// </summary>
        /// <returns>현재 적 인덱스</returns>
        public int GetCurrentEnemyIndex()
        {
            return currentEnemyIndex;
        }
        
        /// <summary>
        /// 다음 스테이지가 있는지 확인합니다
        /// </summary>
        /// <returns>다음 스테이지가 있으면 true</returns>
        public bool HasNextStage()
        {
            // 다음 스테이지 번호 계산 후 실제 데이터 존재 여부로 판단
            int currentStageNum = currentStage?.stageNumber ?? 1;
            int nextStageNumber = currentStageNum + 1;
            
            // 디버깅: 현재 스테이지와 다음 스테이지 번호 로그
            GameLogger.LogDebug($"[StageManager] HasNextStage 체크 - 현재 스테이지: {currentStageNum}, 다음 스테이지 번호: {nextStageNumber}", GameLogger.LogCategory.Combat);
            
            // 디버깅: 등록된 모든 스테이지 정보 로그
            if (stageSettings.allStages != null && stageSettings.allStages.Count > 0)
            {
                var stageNumbers = new System.Text.StringBuilder("등록된 스테이지: ");
                foreach (var stage in stageSettings.allStages)
                {
                    if (stage != null)
                    {
                        stageNumbers.Append($"스테이지 {stage.stageNumber} ({stage.stageName}), ");
                    }
                }
                GameLogger.LogDebug($"[StageManager] {stageNumbers}", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogWarning("[StageManager] 등록된 스테이지가 없습니다", GameLogger.LogCategory.Combat);
            }
            
            var nextStage = GetStageData(nextStageNumber);
            bool hasNext = nextStage != null;
            
            if (!hasNext)
            {
                GameLogger.LogWarning($"[StageManager] 스테이지 {nextStageNumber} 데이터를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogDebug($"[StageManager] 다음 스테이지 발견: 스테이지 {nextStageNumber} ({nextStage.stageName})", GameLogger.LogCategory.Combat);
            }
            
            return hasNext;
        }
        
        /// <summary>
        /// 다음 스테이지로 진행합니다
        /// </summary>
        /// <returns>진행 성공 여부</returns>
        public bool ProgressToNextStage()
        {
            if (!HasNextStage())
            {
                GameLogger.LogWarning("더 이상 진행할 스테이지가 없습니다", GameLogger.LogCategory.Combat);
                return false;
            }
            
            int nextStageNumber = (currentStage?.stageNumber ?? 1) + 1;
            return LoadStage(nextStageNumber);
        }
        
        /// <summary>
        /// 특정 스테이지를 로드합니다.
        /// </summary>
        /// <param name="stageNumber">로드할 스테이지 번호</param>
        public bool LoadStage(int stageNumber)
        {
            if (stageNumber < 1 || stageNumber > 4)
            {
                GameLogger.LogError($"잘못된 스테이지 번호: {stageNumber}", GameLogger.LogCategory.Combat);
                return false;
            }
            
            var stageData = GetStageData(stageNumber);
            if (stageData == null)
            {
                GameLogger.LogWarning($"스테이지 {stageNumber} 데이터를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                return false;
            }
            
            if (!stageData.IsValid())
            {
                GameLogger.LogError($"스테이지 {stageNumber} 데이터가 유효하지 않습니다", GameLogger.LogCategory.Combat);
                return false;
            }
            
            // 이전 스테이지 저장
            var previousStage = currentStage;
            
            // 새 스테이지 설정
            currentStage = stageData;
            currentEnemyIndex = 0;
            isStageCompleted = false;
            progressState = StageProgressState.NotStarted;
            
            // 소환 데이터 초기화 (새 스테이지 시작 시 모든 소환 상태 리셋)
            ClearSummonData();
            isSummonInProgress = false;
            
            // 스테이지 전환 이벤트 발생
            if (previousStage != null)
            {
                OnStageTransition?.Invoke(previousStage, currentStage);
            }
            
            GameLogger.LogInfo($"스테이지 {stageNumber} 로드 완료: {currentStage.stageName}", GameLogger.LogCategory.Combat);
            return true;
        }
        
        /// <summary>
        /// 특정 번호의 스테이지 데이터를 가져옵니다.
        /// </summary>
        /// <param name="stageNumber">스테이지 번호</param>
        /// <returns>스테이지 데이터</returns>
        private StageData GetStageData(int stageNumber)
        {
            if (stageSettings.allStages == null || stageSettings.allStages.Count == 0)
            {
                GameLogger.LogError("스테이지 데이터가 설정되지 않았습니다", GameLogger.LogCategory.Combat);
                return null;
            }
            
            GameLogger.LogDebug($"[StageManager] GetStageData 호출 - 찾는 스테이지 번호: {stageNumber}", GameLogger.LogCategory.Combat);
            
            foreach (var stage in stageSettings.allStages)
            {
                if (stage != null)
                {
                    GameLogger.LogDebug($"[StageManager] 스테이지 확인 - 번호: {stage.stageNumber}, 이름: {stage.stageName}, 일치: {stage.stageNumber == stageNumber}", GameLogger.LogCategory.Combat);
                    if (stage.stageNumber == stageNumber)
                    {
                        GameLogger.LogDebug($"[StageManager] 스테이지 {stageNumber} 데이터 찾음: {stage.stageName}", GameLogger.LogCategory.Combat);
                    return stage;
                    }
                }
            }
            
            GameLogger.LogWarning($"[StageManager] 스테이지 {stageNumber} 데이터를 찾을 수 없습니다. 등록된 스테이지 수: {stageSettings.allStages.Count}", GameLogger.LogCategory.Combat);
            return null;
        }

        #endregion

        #region 로그 스쿨 시스템 - 단계별 관리

        #region 스테이지 진행 관리

        /// <summary>
        /// 현재 스테이지 진행 상태
        /// </summary>
        public StageProgressState ProgressState => progressState;
        
        /// <summary>
        /// 스테이지 완료 여부
        /// </summary>
        public bool IsStageCompleted => isStageCompleted;

        /// <summary>
        /// 스테이지를 시작합니다
        /// 첫 번째 적을 생성합니다
        /// </summary>
        public void StartStage()
        {
            if (currentStage == null || currentStage.enemies.Count == 0)
            {
                GameLogger.LogWarning("스테이지 데이터가 유효하지 않습니다", GameLogger.LogCategory.Combat);
                return;
            }

            // 이전 적이 있으면 정리 (GameObject 포함)
            if (enemyManager != null)
            {
                var currentEnemy = enemyManager.GetEnemy();
                if (currentEnemy != null)
                {
                    GameLogger.LogDebug($"[StageManager] 이전 적 정리 중: {currentEnemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                    
                    // EnemyManager에서 참조 제거
                    enemyManager.UnregisterEnemy();
                    
                    // 적 GameObject 파괴
                    if (currentEnemy is EnemyCharacter enemyCharacter)
                    {
                        Destroy(enemyCharacter.gameObject);
                        GameLogger.LogInfo("[StageManager] 이전 적 GameObject 파괴 완료", GameLogger.LogCategory.Combat);
                    }
                }
            }
            
            // 적 슬롯의 모든 자식 오브젝트 파괴 (안전장치)
            var enemySlotGameObject = GameObject.Find("EnemyCharacterSlot");
            if (enemySlotGameObject != null)
            {
                int childCount = enemySlotGameObject.transform.childCount;
                if (childCount > 0)
                {
                    GameLogger.LogInfo($"[StageManager] 적 슬롯의 자식 오브젝트 {childCount}개 파괴 중", GameLogger.LogCategory.Combat);
                    for (int i = enemySlotGameObject.transform.childCount - 1; i >= 0; i--)
                    {
                        var child = enemySlotGameObject.transform.GetChild(i);
                        if (child != null)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                    GameLogger.LogInfo("[StageManager] 적 슬롯 정리 완료", GameLogger.LogCategory.Combat);
                }
            }

            // 승리 UI 숨기기
            if (victoryUI != null)
            {
                victoryUI.Hide();
                GameLogger.LogInfo("[StageManager] 승리 UI 숨김", GameLogger.LogCategory.UI);
            }

            // 전투 상태 머신 리셋 (새 스테이지 시작 전)
            // 주의: 스테이지 1 처음 시작 시에는 _currentState가 이미 null이므로 리셋 불필요
            // 스테이지 전환 시에만 리셋 필요
            if (combatStateMachine != null)
            {
                var currentState = combatStateMachine.GetCurrentState();
                if (currentState != null)
                {
                    GameLogger.LogInfo($"[StageManager] 전투 상태 리셋: {currentState.StateName} → None", GameLogger.LogCategory.Combat);
                    combatStateMachine.ResetCombatState();
                    
                    // 상태 리셋 후 짧은 대기 (코루틴 정리 시간)
                    StartCoroutine(WaitForStateResetAndContinue());
                    return; // 코루틴에서 나머지 작업 계속
                }
                else
                {
                    GameLogger.LogInfo("[StageManager] 전투 상태가 이미 None - 리셋 불필요", GameLogger.LogCategory.Combat);
                }
            }

            // 상태 리셋이 필요 없거나 완료된 경우 바로 계속
            ContinueStartStageAfterReset();
        }

        /// <summary>
        /// 전투 상태 리셋 후 나머지 작업을 계속합니다.
        /// </summary>
        private System.Collections.IEnumerator WaitForStateResetAndContinue()
        {
            // 상태 리셋 후 짧은 대기 (코루틴 정리 시간)
            yield return new WaitForSeconds(0.1f);
            
            // 나머지 StartStage 작업 계속
            ContinueStartStageAfterReset();
        }

        /// <summary>
        /// 상태 리셋 후 StartStage의 나머지 작업을 계속합니다.
        /// </summary>
        private void ContinueStartStageAfterReset()
        {
            // 플레이어 체력을 최대 체력으로 회복
            if (playerManager != null)
            {
                var player = playerManager.GetCharacter();
                if (player != null)
                {
                    int currentHP = player.GetCurrentHP();
                    int maxHP = player.GetMaxHP();
                    if (currentHP < maxHP)
                    {
                        int healAmount = maxHP - currentHP;
                        player.Heal(healAmount);
                        GameLogger.LogInfo($"[StageManager] 플레이어 체력 회복: {currentHP} → {maxHP}", GameLogger.LogCategory.Character);
                    }
                }
            }

            // 적 카드 슬롯 정리
            _ = ClearEnemyCardsFromSlots();

            progressState = StageProgressState.InProgress;
            currentEnemyIndex = 0;
            isStageCompleted = false;
            
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogInfo($"스테이지 시작: {currentStage.stageName} (스테이지 {currentStage.stageNumber})", GameLogger.LogCategory.Combat);
            
            // 첫 번째 적의 BGM 즉시 재생 (스테이지 시작 시)
            if (audioManager != null && currentStage.enemies != null && currentStage.enemies.Count > 0)
            {
                var firstEnemyData = currentStage.enemies[0];
                GameLogger.LogDebug($"첫 번째 적 BGM 재생 시작: {firstEnemyData.DisplayName}", GameLogger.LogCategory.Audio);
                audioManager.PlayEnemyBGM(firstEnemyData);
            }
            else if (audioManager == null)
            {
                GameLogger.LogDebug("AudioManager가 null입니다 - 첫 적 BGM 재생 건너뜀", GameLogger.LogCategory.Audio);
            }
            
            // 첫 번째 적 생성
            _ = SpawnNextEnemyAsync();
        }

        /// <summary>
        /// 스테이지를 완료합니다. 모든 적 처치 시 호출됩니다.
        /// </summary>
        public void CompleteStage()
        {
            progressState = StageProgressState.Completed;
            isStageCompleted = true;
            totalStagesCompleted++;
            
            OnProgressChanged?.Invoke(progressState);
            
            // 스테이지 완료 이벤트 발생
            OnStageCompleted?.Invoke(currentStage);
            // 전투 UI 브리지: 보상 종료 후 최종적으로 스테이지가 완료되면 승리 이벤트 발행
            Game.CombatSystem.CombatEvents.RaiseVictory();
            
            GameLogger.LogInfo($"스테이지 완료 (승리!): {currentStage.stageName} (스테이지 {currentStage.stageNumber})", GameLogger.LogCategory.Combat);
            
            // 다음 스테이지로 진행 또는 게임 완료 처리
            if (currentStage.IsLastStage)
            {
                // 마지막 스테이지 완료 - 게임 완료!
                GameLogger.LogDebug("[StageManager] 마지막 스테이지 완료 - 게임 완료 처리", GameLogger.LogCategory.Combat);
                CompleteGame();
            }
            else
            {
                // 승리 패널이 먼저 표시되도록 자동 진행을 하지 않음
                // 승리 패널에서 "다음 스테이지" 버튼을 누르면 그때 진행됨
                GameLogger.LogDebug(
                    $"[StageManager] 승리 패널 표시 대기 - 승리 패널에서 다음 스테이지 버튼을 눌러야 진행됩니다 (현재 스테이지: {currentStage.stageNumber})",
                    GameLogger.LogCategory.Combat);
            }
        }
        
        /// <summary>
        /// 게임을 완료합니다. (모든 스테이지 완료)
        /// </summary>
        private void CompleteGame()
        {
            isGameCompleted = true;
            OnGameCompleted?.Invoke();
            GameLogger.LogInfo("🎉 게임 완료! 모든 스테이지를 클리어했습니다!", GameLogger.LogCategory.Combat);
            
            // Statistics 제거됨
        }
        

        public void FailStage()
        {
            progressState = StageProgressState.Failed;
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogWarning($"스테이지 실패: {currentStage?.stageName ?? "Unknown"} (스테이지 {currentStage?.stageNumber ?? 1})", GameLogger.LogCategory.Combat);
            
            // Statistics 제거됨
        }

        // Statistics 제거됨 - StartStatisticsSession, EndStatisticsSession, SaveStatisticsSession 메서드 제거

        public event System.Action<StageProgressState> OnProgressChanged;

        #endregion

        #region 게임 상태 정보

        /// <summary>
        /// 게임이 완료되었는지 확인합니다.
        /// </summary>
        public bool IsGameCompleted => isGameCompleted;

        /// <summary>
        /// 완료된 스테이지 수를 가져옵니다.
        /// </summary>
        public int TotalStagesCompleted => totalStagesCompleted;

        /// <summary>
        /// 전체 스테이지 수를 가져옵니다.
        /// </summary>
        public int TotalStages => 4;

        /// <summary>
        /// 게임 진행률을 가져옵니다. (0.0 ~ 1.0)
        /// </summary>
        public float GameProgress => (float)totalStagesCompleted / TotalStages;

        /// <summary>
        /// 특정 스테이지 데이터를 가져옵니다. (public 버전)
        /// </summary>
        /// <param name="stageNumber">스테이지 번호</param>
        /// <returns>스테이지 데이터</returns>
        public StageData GetStageDataPublic(int stageNumber)
        {
            return GetStageData(stageNumber);
        }

        /// <summary>
        /// 모든 스테이지 데이터를 가져옵니다.
        /// </summary>
        /// <returns>모든 스테이지 데이터</returns>
        public List<StageData> GetAllStages()
        {
            return stageSettings.allStages ?? new List<StageData>();
        }

        /// <summary>
        /// 현재 활성화된 적이 소환된 적인지 확인
        /// </summary>
        public bool IsSummonedEnemyActive()
        {
            return isSummonedEnemyActive;
        }
        
        /// <summary>
        /// 원본 적 스택 개수를 반환합니다 (디버깅용)
        /// </summary>
        public int GetOriginalEnemyStackCount()
        {
            return originalEnemyStack.Count;
        }

        #endregion

        #region 소환 시스템

        /// <summary>
        /// 소환/복귀용 원본 적 정보를 관리하는 컨텍스트입니다.
        /// 다단계 소환을 지원하기 위해 스택으로 관리됩니다.
        /// </summary>
        private struct OriginalEnemyContext
        {
            public EnemyCharacterData EnemyData;
            public int EnemyHP;
        }

        // 기존 필드는 현재 소환 체인의 최상단 컨텍스트 스냅샷으로 유지합니다.
        private EnemyCharacterData originalEnemyData;
        private int originalEnemyHP;
        private EnemyCharacterData summonTargetData;
        private bool isSummonedEnemyActive = false;

        /// <summary>
        /// 다단계 소환을 위한 원본 적 컨텍스트 스택입니다.
        /// </summary>
        private readonly Stack<OriginalEnemyContext> originalEnemyStack = new Stack<OriginalEnemyContext>();

        private void HandleSummonRequest(EnemyCharacterData summonTarget, int currentHP)
        {
            // 이 메서드는 더 이상 사용되지 않습니다 (상태 패턴으로 처리됨)
            GameLogger.LogWarning("[StageManager] HandleSummonRequest는 더 이상 사용되지 않습니다", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 소환/복귀 전환 처리
        /// </summary>
        private async Task TransitionToSummonState(EnemyCharacterData targetEnemy, bool isRestore, int restoreHP = -1)
        {
            GameLogger.LogInfo($"[소환] TransitionToSummonState 시작: 대상={targetEnemy?.DisplayName ?? "null"}, 복귀모드={isRestore}", GameLogger.LogCategory.Combat);
            
            // 씬 전환/파괴 상태 확인
            if (isDestroyed || this == null)
            {
                GameLogger.LogWarning("StageManager가 파괴되었습니다 - 소환 처리 취소", GameLogger.LogCategory.Combat);
                return;
            }
            
            // CombatStateMachine 확인 (DI 주입)
            if (combatStateMachine == null)
            {
                GameLogger.LogError("[소환] CombatStateMachine이 주입되지 않았습니다 - 소환 중단", GameLogger.LogCategory.Combat);
                return;
            }
            
            GameLogger.LogInfo($"[소환] CombatStateMachine 확인 완료, 현재 상태: {combatStateMachine.GetCurrentState()?.StateName ?? "null"}", GameLogger.LogCategory.Combat);

            try
            {
                GameLogger.LogInfo($"[소환] 1단계 시작: 기존 적 제거 및 슬롯 정리", GameLogger.LogCategory.Combat);
                
                // 1단계: 기존 적 제거 및 슬롯 정리
                await CleanupCurrentEnemy();
                
                // 씬 전환/파괴 상태 재확인
                if (isDestroyed || this == null)
                {
                    GameLogger.LogWarning("StageManager가 파괴되었습니다 - 소환 처리 취소 (정리 후)", GameLogger.LogCategory.Combat);
                    return;
                }
                
                ICharacter newEnemy = null;
                
                if (isRestore)
                {
                    // 복귀 모드: 기존 GameObject를 찾아서 재활성화 (Initialize 호출 방지)
                    GameLogger.LogInfo($"[소환] 2단계 시작: 기존 적 재활성화 - 대상: {targetEnemy?.DisplayName ?? "null"}", GameLogger.LogCategory.Combat);
                    
                    // restoreHP가 명시적으로 전달된 경우 사용, 아니면 필드 값 사용
                    int hpToRestore = restoreHP >= 0 ? restoreHP : originalEnemyHP;
                    newEnemy = FindAndReactivateOriginalEnemy(targetEnemy, hpToRestore);
                    if (newEnemy == null)
                    {
                        GameLogger.LogError("[소환] 기존 적 재활성화 실패", GameLogger.LogCategory.Combat);
                        return;
                    }
                    
                    GameLogger.LogInfo($"[소환] 기존 적 재활성화 완료: {newEnemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                    
                    GameLogger.LogInfo($"[소환] 3단계 시작: 적 등록 - 복귀모드=true, 원본HP={hpToRestore}", GameLogger.LogCategory.Combat);
                    
                    RegisterEnemy(newEnemy);
                    GameLogger.LogInfo($"[소환] RegisterEnemy 호출 완료: {newEnemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                    GameLogger.LogInfo($"[소환] 복귀 완료: {targetEnemy.DisplayName} (HP 복원: {hpToRestore}/{newEnemy.GetMaxHP()})", GameLogger.LogCategory.Combat);
                }
                else
                {
                    // 소환 모드: 새로운 적 생성
                    GameLogger.LogInfo($"[소환] 2단계 시작: 새로운 적 생성 - 대상: {targetEnemy?.DisplayName ?? "null"}", GameLogger.LogCategory.Combat);
                    
                    newEnemy = await CreateEnemyForSummonAsync(targetEnemy);
                    if (newEnemy == null)
                    {
                        GameLogger.LogError("[소환] 적 생성 실패", GameLogger.LogCategory.Combat);
                        return;
                    }
                    
                    GameLogger.LogInfo($"[소환] 적 생성 완료: {newEnemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                    
                    GameLogger.LogInfo($"[소환] 3단계 시작: 적 등록 - 소환모드", GameLogger.LogCategory.Combat);
                    
                    RegisterSummonedEnemy(newEnemy);
                    GameLogger.LogInfo($"[소환] RegisterSummonedEnemy 호출 완료: {newEnemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                    GameLogger.LogInfo($"[소환] 소환 완료: {targetEnemy.DisplayName}", GameLogger.LogCategory.Combat);
                }

                // 4단계: 소환/복귀 완료 처리
                if (!isRestore)
                {
                    GameLogger.LogInfo("[소환] 소환 완료 - CombatInitState가 자동으로 슬롯 설정을 처리합니다", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogInfo("[소환] 복귀 완료 - CombatInitState로 전환하여 슬롯 설정", GameLogger.LogCategory.Combat);
                    
                    // 복귀 모드: CombatInitState로 직접 전환하여 슬롯 설정
                    if (combatStateMachine != null && targetEnemy != null)
                    {
                        var combatInitState = new Game.CombatSystem.State.CombatInitState();
                        combatInitState.SetEnemyData(targetEnemy, targetEnemy.DisplayName);
                        combatInitState.SetSummonMode(true);
                        combatStateMachine.ChangeState(combatInitState);
                        GameLogger.LogInfo($"[소환] CombatInitState로 전환 완료: {targetEnemy.DisplayName}", GameLogger.LogCategory.Combat);
                    }
                }
                
                // 소환 진행 완료 플래그 해제
                isSummonInProgress = false;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[소환] 처리 중 오류: {ex.Message}", GameLogger.LogCategory.Combat);
                // 예외 발생 시에도 소환 진행 플래그 해제
                isSummonInProgress = false;
            }
        }

        /// <summary>
        /// 현재 소환이 진행 중인지 확인합니다.
        /// </summary>
        public bool IsSummonInProgress => isSummonInProgress;

        /// <summary>
        /// 비활성화된 원본 적을 찾아서 재활성화합니다 (복귀 모드 전용)
        /// </summary>
        /// <param name="targetData">재활성화할 적 데이터</param>
        /// <param name="restoreHP">복원할 HP 값</param>
        /// <returns>재활성화된 적 캐릭터</returns>
        private ICharacter FindAndReactivateOriginalEnemy(EnemyCharacterData targetData, int restoreHP)
        {
            if (targetData == null)
            {
                GameLogger.LogError("[StageManager] FindAndReactivateOriginalEnemy: targetData가 null입니다", GameLogger.LogCategory.Combat);
                return null;
            }

            if (enemyManager == null)
            {
                GameLogger.LogError("[StageManager] FindAndReactivateOriginalEnemy: EnemyManager를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                return null;
            }

            var characterSlot = enemyManager.GetCharacterSlot();
            if (characterSlot == null)
            {
                GameLogger.LogError("[StageManager] FindAndReactivateOriginalEnemy: CharacterSlot을 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                return null;
            }

            // CharacterSlot의 모든 자식 중 비활성화된 적 찾기
            foreach (Transform child in characterSlot)
            {
                if (!child.gameObject.activeSelf)
                {
                    if (child.TryGetComponent<EnemyCharacter>(out var enemyChar))
                    {
                        // 데이터 일치 확인
                        if (enemyChar.CharacterData == targetData)
                        {
                            GameLogger.LogInfo($"[StageManager] 원본 적 발견: {enemyChar.GetCharacterName()}", GameLogger.LogCategory.Combat);

                            // GameObject 재활성화
                            child.gameObject.SetActive(true);
                            GameLogger.LogInfo($"[StageManager] 원본 적 재활성화 완료: {enemyChar.GetCharacterName()}", GameLogger.LogCategory.Combat);
                            
                            // 데미지 텍스트 정리
                            enemyChar.ClearDamageTexts();

                            // HP 복원 (Initialize가 호출되지 않으므로 HP가 유지됨)
                            // restoreHP가 -1이 아니면 복원 (명시적으로 전달된 경우)
                            if (restoreHP >= 0)
                            {
                                enemyChar.SetCurrentHP(restoreHP);
                                GameLogger.LogInfo($"[StageManager] 원본 적 HP 복원: {restoreHP}/{enemyChar.GetMaxHP()}", GameLogger.LogCategory.Combat);
                            }
                            else
                            {
                                GameLogger.LogWarning($"[StageManager] HP 복원 값이 유효하지 않음: {restoreHP}", GameLogger.LogCategory.Combat);
                            }

                            // HP 바 컨트롤러 재초기화
                            enemyChar.ReinitializeHPBarController();

                            // EnemyCharacterUIController 재연결
                            if (enemyCharacterUIController != null)
                            {
                                enemyCharacterUIController.SetTarget(enemyChar);
                            }

                            // UI 업데이트
                            enemyChar.RefreshUI();

                            // 버프/이펙트 UI 업데이트
                            enemyChar.NotifyBuffsChanged();

                            // Idle 시각 효과 재시작
                            enemyChar.StartIdleVisualLoop();

                            return enemyChar;
                        }
                    }
                }
            }

            GameLogger.LogError($"[StageManager] 비활성화된 원본 적을 찾을 수 없습니다: {targetData?.DisplayName ?? "null"}", GameLogger.LogCategory.Combat);
            return null;
        }

        /// <summary>
        /// 기존 적 제거 및 슬롯 정리
        /// </summary>
        private async System.Threading.Tasks.Task CleanupCurrentEnemy()
        {
            GameLogger.LogInfo("[소환] 기존 적 및 슬롯 정리 시작", GameLogger.LogCategory.Combat);

            // 기존 적 제거
            var currentEnemy = enemyManager?.GetEnemy();
            if (currentEnemy != null)
            {
                enemyManager.UnregisterEnemy();
                if (currentEnemy is EnemyCharacter enemyChar)
                {
                    UnityEngine.Object.Destroy(enemyChar.gameObject);
                }
                GameLogger.LogInfo($"[소환] 기존 적 제거 완료: {currentEnemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
            }

            // 플레이어 핸드 카드 제거 (DI 주입)
            if (playerHandManagerConcrete != null)
            {
                playerHandManagerConcrete.ClearAll();
                GameLogger.LogInfo("[소환] 플레이어 핸드 카드 제거 완료", GameLogger.LogCategory.Combat);
            }

            // 모든 슬롯 정리 (DI 주입)
            if (turnManager != null)
            {
                turnManager.ClearAllSlots();
                GameLogger.LogInfo("[소환] 전투/대기 슬롯 정리 완료", GameLogger.LogCategory.Combat);

                // 적 캐시 정리 및 슬롯 상태 리셋
                turnManager.ClearEnemyCache();
                turnManager.ResetSlotStates();
                GameLogger.LogInfo("[소환] 적 캐시 정리 및 슬롯 상태 리셋 완료", GameLogger.LogCategory.Combat);
            }

            // 정리 완료 대기
            await System.Threading.Tasks.Task.Delay(300);
        }

        private void OnSummonedEnemyDeath(ICharacter summonedEnemy)
        {
            // 현재 소환 체인의 최상단 컨텍스트를 사용하여 복귀를 처리합니다.
            if (originalEnemyStack.Count > 0)
            {
                var context = originalEnemyStack.Pop();
                var restoreEnemyData = context.EnemyData;
                var restoreHP = context.EnemyHP; // 로컬 변수에 저장 (필드 초기화 전에)

                GameLogger.LogInfo(
                    $"[소환] {summonedEnemy.GetCharacterName()} 사망 → {restoreEnemyData?.DisplayName} 복귀 (HP: {restoreHP}, 남은 스택: {originalEnemyStack.Count})",
                    GameLogger.LogCategory.Combat);

                // 스택이 비어 있으면 최상위 소환 체인이 종료된 것이므로 필드를 초기화합니다.
                if (originalEnemyStack.Count == 0)
                {
                    originalEnemyData = null;
                    originalEnemyHP = 0;
                }
                else
                {
                    // 남은 상위 컨텍스트를 스냅샷으로 유지
                    var parent = originalEnemyStack.Peek();
                    originalEnemyData = parent.EnemyData;
                    originalEnemyHP = parent.EnemyHP;
                }

                // 복귀 전환 상태로 이동 (HP 값을 직접 전달하여 필드 초기화 영향 방지)
                _ = TransitionToSummonState(restoreEnemyData, true, restoreHP);
            }
            else
            {
                GameLogger.LogWarning("[소환] 원본 적 컨텍스트 스택이 비어 있어 복귀할 수 없습니다.", GameLogger.LogCategory.Combat);
                // 데이터가 없어도 상태는 초기화
                isSummonedEnemyActive = false;
            }
        }

        // 더 이상 사용하지 않음 - SummonTransitionState가 처리
        /*
        private async Task RestoreOriginalEnemy()
        {
            ...
        }

        private async Task ClearPlayerHandsAndSlots()
        {
            ...
        }

        private async Task ClearSummonedEnemyCards()
        {
            ...
        }
        */

        /// <summary>
        /// 소환된 적 활성화 상태를 설정합니다 (상태 패턴에서 사용)
        /// </summary>
        public void SetSummonedEnemyActive(bool active)
        {
            isSummonedEnemyActive = active;
            GameLogger.LogInfo($"[StageManager] 소환된 적 활성화 상태 설정: {active}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 원본 적 데이터를 반환합니다 (상태 패턴에서 사용)
        /// 항상 현재 소환 체인의 최상단 컨텍스트를 반환합니다.
        /// </summary>
        public EnemyCharacterData GetOriginalEnemyData()
        {
            return originalEnemyData;
        }
        
        /// <summary>
        /// 원본 적 HP를 반환합니다 (상태 패턴에서 사용)
        /// 항상 현재 소환 체인의 최상단 컨텍스트를 반환합니다.
        /// </summary>
        public int GetOriginalEnemyHP()
        {
            return originalEnemyHP;
        }
        
        /// <summary>
        /// 새로운 소환 컨텍스트의 원본 적 데이터를 설정합니다.
        /// 다단계 소환을 위해 스택에 푸시합니다.
        /// </summary>
        public void SetOriginalEnemyData(EnemyCharacterData data)
        {
            if (data == null)
            {
                GameLogger.LogWarning("[StageManager] 원본 적 데이터 설정 시 null이 전달되었습니다", GameLogger.LogCategory.Combat);
                return;
            }

            var context = new OriginalEnemyContext
            {
                EnemyData = data,
                EnemyHP = originalEnemyHP // HP는 이후 SetOriginalEnemyHP에서 갱신
            };

            originalEnemyStack.Push(context);
            originalEnemyData = data;

            GameLogger.LogInfo($"[StageManager] 원본 적 컨텍스트 푸시: {data.DisplayName} (스택 깊이: {originalEnemyStack.Count})", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 현재 소환 컨텍스트의 원본 적 HP를 설정합니다.
        /// 가장 최근에 푸시된 컨텍스트의 HP를 갱신합니다.
        /// </summary>
        public void SetOriginalEnemyHP(int hp)
        {
            GameLogger.LogInfo($"[StageManager] SetOriginalEnemyHP 호출: HP={hp}, 현재 스택={originalEnemyStack.Count}", GameLogger.LogCategory.Combat);
            
            if (originalEnemyStack.Count == 0)
            {
                // 소환 컨텍스트가 없는데 HP만 설정되는 경우는 예외적인 상황이므로 경고를 남깁니다.
                originalEnemyHP = hp;
                GameLogger.LogWarning($"[StageManager] 소환 컨텍스트 없이 원본 적 HP가 설정되었습니다: {hp}", GameLogger.LogCategory.Combat);
                return;
            }

            var context = originalEnemyStack.Pop();
            int previousHP = context.EnemyHP;
            context.EnemyHP = hp;
            originalEnemyStack.Push(context);

            originalEnemyHP = hp;
            originalEnemyData = context.EnemyData;
            
            GameLogger.LogInfo($"[StageManager] 원본 적 HP 갱신: {previousHP} → {hp} (대상: {context.EnemyData?.DisplayName}, 스택 깊이: {originalEnemyStack.Count})", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 소환 대상을 설정합니다 (상태 패턴에서 사용)
        /// </summary>
        public void SetSummonTarget(EnemyCharacterData target)
        {
            summonTargetData = target;
            GameLogger.LogInfo($"[StageManager] 소환 대상 설정: {target?.DisplayName}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 소환 대상 데이터를 반환합니다
        /// </summary>
        public EnemyCharacterData GetSummonTarget()
        {
            return summonTargetData;
        }

        /// <summary>
        /// 소환 관련 데이터 초기화
        /// </summary>
        public void ClearSummonData()
        {
            originalEnemyData = null;
            originalEnemyHP = 0;
            summonTargetData = null;
            isSummonedEnemyActive = false;
            originalEnemyStack.Clear();
            GameLogger.LogInfo("[StageManager] 소환 데이터 초기화 완료", GameLogger.LogCategory.Combat);
        }

        #endregion

        #endregion
    }
}

