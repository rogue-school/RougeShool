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

        // 스테이지 진행 상태
        private StageProgressState progressState = StageProgressState.NotStarted;

        // 다중 스테이지 관리
        private StageData currentStage;
        private int totalStagesCompleted = 0;
        private bool isGameCompleted = false;

        [Zenject.Inject(Optional = true)] private Game.CoreSystem.Save.SaveManager saveManager;
        [Zenject.Inject] private EnemyManager enemyManager;
        [Zenject.Inject(Optional = true)] private Game.CoreSystem.Interface.IAudioManager audioManager;
        [Zenject.Inject(Optional = true)] private Game.SkillCardSystem.Interface.IPlayerHandManager playerHandManager;
        [Zenject.Inject(Optional = true)] private Game.CombatSystem.Slot.CombatSlotRegistry combatSlotRegistry;
        [Zenject.Inject(Optional = true)] private Game.CombatSystem.Interface.ICombatTurnManager turnManager;
        [Zenject.Inject(Optional = true)] private Game.CharacterSystem.Manager.PlayerManager playerManager;
        [Zenject.Inject(Optional = true)] private Game.CombatSystem.State.CombatStateMachine combatStateMachine;
        [Zenject.Inject(Optional = true)] private Game.SkillCardSystem.Manager.PlayerHandManager playerHandManagerConcrete;

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

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 스테이지 매니저 초기화
        /// </summary>
        private void Start()
        {
            // 새게임 요청 플래그 확인 및 초기화
            if (PlayerPrefs.GetInt("NEW_GAME_REQUESTED", 0) == 1)
            {
                GameLogger.LogInfo("[StageManager] 새게임 요청 감지 - 게임 상태 초기화 시작", GameLogger.LogCategory.Save);
                InitializeGameStateForNewGame();
                PlayerPrefs.SetInt("NEW_GAME_REQUESTED", 0);
                PlayerPrefs.Save();
                
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
                GameLogger.LogInfo("[StageManager] PlayerManager 이벤트 구독 완료", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogWarning("[StageManager] PlayerManager를 찾을 수 없습니다 - 플레이어 준비 대기 건너뜀", GameLogger.LogCategory.Combat);
            }
        }

        private void OnDestroy()
        {
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
            GameLogger.LogInfo("[StageManager] 새게임 상태 초기화 시작", GameLogger.LogCategory.Save);
            
            // 인벤토리 초기화 (스킬카드 스택은 캐릭터 생성 시 초기화됨)
            var itemService = FindFirstObjectByType<Game.ItemSystem.Service.ItemService>();
            if (itemService != null)
            {
                itemService.ResetInventoryForNewGame();
                GameLogger.LogInfo("[StageManager] 인벤토리 초기화 완료", GameLogger.LogCategory.Save);
            }
            else
            {
                GameLogger.LogWarning("[StageManager] ItemService를 찾을 수 없습니다 - 인벤토리 초기화 건너뜀", GameLogger.LogCategory.Save);
            }
            
            GameLogger.LogInfo("[StageManager] 새게임 상태 초기화 완료", GameLogger.LogCategory.Save);
        }

        /// <summary>
        /// 플레이어 캐릭터 준비 완료 시 호출
        /// </summary>
        private void OnPlayerReady(ICharacter player)
        {
            GameLogger.LogInfo($"[StageManager] 플레이어 준비 완료: {player.GetCharacterName()}", GameLogger.LogCategory.Combat);

            // 대기 중이었다면 스테이지 시작
            if (isWaitingForPlayer)
            {
                isWaitingForPlayer = false;
                GameLogger.LogInfo("[StageManager] 플레이어 준비 완료 → 스테이지 시작", GameLogger.LogCategory.Combat);
                StartStage();
            }
        }
        
        /// <summary>
        /// 저장된 진행 상황을 자동으로 로드합니다.
        /// </summary>
        private System.Collections.IEnumerator AutoLoadSavedProgress()
        {
            if (saveManager == null)
            {
                GameLogger.LogWarning("[StageManager] SaveManager를 찾을 수 없습니다 - 기본 스테이지 로드로 진행", GameLogger.LogCategory.Save);
                LoadDefaultStage();
                yield break;
            }
            
            // 새 게임인지 확인
            if (saveManager.IsNewGame())
            {
                GameLogger.LogInfo("[StageManager] 새 게임 시작 - 저장된 데이터 로드 건너뛰기", GameLogger.LogCategory.Save);
                
                // 새 게임 플래그 해제
                saveManager.ClearNewGameFlag();
                
                // 기본 스테이지 로드
                LoadDefaultStage();
                yield break;
            }
            
            // 저장된 진행 상황이 있는지 확인
            if (saveManager.HasStageProgressSave())
            {
                GameLogger.LogInfo("[StageManager] 저장된 진행 상황 발견, 자동 로드 시작", GameLogger.LogCategory.Save);
                
                // 비동기 로드를 코루틴으로 변환
                var loadTask = saveManager.LoadStageProgress();
                yield return new WaitUntil(() => loadTask.IsCompleted);
                
                if (loadTask.Result)
                {
                    GameLogger.LogInfo("[StageManager] 저장된 진행 상황 자동 로드 완료", GameLogger.LogCategory.Save);
                }
                else
                {
                    GameLogger.LogWarning("[StageManager] 저장된 진행 상황 로드 실패", GameLogger.LogCategory.Save);
                    // 로드 실패 시 기본 스테이지 로드
                    LoadDefaultStage();
                }
            }
            else
            {
                GameLogger.LogInfo("[StageManager] 저장된 진행 상황이 없습니다. 기본 스테이지를 시작합니다", GameLogger.LogCategory.Save);
                // 저장된 데이터가 없으면 기본 스테이지 로드
                LoadDefaultStage();
            }
        }
        
        /// <summary>
        /// 기본 스테이지를 로드합니다.
        /// </summary>
        private void LoadDefaultStage()
        {
            if (LoadStage(1))
            {
                GameLogger.LogInfo("기본 스테이지 로드 완료 - 플레이어 준비 대기 중", GameLogger.LogCategory.Combat);

                // 플레이어 준비 완료 대기 플래그 설정
                isWaitingForPlayer = true;

                // 플레이어가 이미 준비되었는지 확인
                if (playerManager != null && playerManager.GetCharacter() != null)
                {
                    GameLogger.LogInfo("[StageManager] 플레이어가 이미 준비됨 - 즉시 스테이지 시작", GameLogger.LogCategory.Combat);
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
        /// </summary>
        public async Task SaveProgressBeforeSceneTransition()
        {
            try
            {
                if (saveManager != null)
                {
                    await saveManager.SaveCurrentProgress("SceneTransition");
                    GameLogger.LogInfo("[StageManager] 씬 전환 전 진행 상황 저장 완료", GameLogger.LogCategory.Save);
                }
                else
                {
                    GameLogger.LogWarning("[StageManager] SaveManager를 찾을 수 없습니다", GameLogger.LogCategory.Save);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[StageManager] 씬 전환 전 저장 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        #endregion

        #region 의존성 주입 (최소화)

        // 핵심 의존성만 유지
        // EnemyManager는 런타임에 찾아서 사용

        #endregion

        #region 적 생성 흐름

        /// <summary>
        /// 다음 적을 생성하여 전투에 배치합니다. (async/await 기반)
        /// </summary>
        public async Task<bool> SpawnNextEnemyAsync()
        {
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
                if (enemy == null)
                {
                    GameLogger.LogError("적 생성 실패", GameLogger.LogCategory.Combat);
                    return false;
                }

                RegisterEnemy(enemy);

                // 적별 BGM 재생 (AudioManager에 위임)
                if (audioManager != null)
                {
                    GameLogger.LogInfo($"AudioManager 존재 - PlayEnemyBGM 호출: {data.DisplayName}", GameLogger.LogCategory.Audio);
                    audioManager.PlayEnemyBGM(data);
                }
                else
                {
                    GameLogger.LogWarning("AudioManager가 null입니다 - BGM 재생 건너뜀", GameLogger.LogCategory.Audio);
                }

                currentEnemyIndex++;

                GameLogger.LogInfo($"[StageManager] 적 생성 완료: {enemy.GetCharacterName()} (인덱스 증가: {currentEnemyIndex - 1} → {currentEnemyIndex})", GameLogger.LogCategory.Combat);

                // CombatStateMachine에 적 생성 완료 알림 (DI 주입)
                if (combatStateMachine != null)
                {
                    if (currentEnemyIndex == 1)
                    {
                        // 첫 번째 적이 생성되면 CombatStateMachine 시작
                        GameLogger.LogInfo($"[StageManager] 첫 번째 적 생성 완료 - CombatStateMachine 시작: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                        
                        // 적 데이터를 가져와서 StartCombat에 전달
                        if (enemy is Game.CharacterSystem.Core.EnemyCharacter enemyChar)
                        {
                            var enemyData = enemyChar.CharacterData;
                            if (enemyData != null)
                            {
                                GameLogger.LogInfo($"[StageManager] 적 데이터로 CombatStateMachine 시작: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
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
                        GameLogger.LogInfo($"[StageManager] 다음 적 생성 완료 - CombatStateMachine에 알림: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
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
        /// 기존 API 호환성을 위한 동기 메서드
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
                GameLogger.LogInfo("스테이지 BGM 정리 완료", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 적 캐릭터를 시스템에 등록합니다.
        /// </summary>
        public void RegisterEnemy(ICharacter enemy)
        {
            enemyManager?.RegisterEnemy(enemy);

            if (enemy is EnemyCharacter concreteEnemy)
            {
                // SetDeathListener는 TODO 상태이므로 SetDeathCallback 사용
                concreteEnemy.SetDeathCallback(OnEnemyDeath);
                // OnSummonRequested 이벤트는 더 이상 사용하지 않음 (상태 패턴으로 처리)
            }

            GameLogger.LogInfo($"적 등록 완료: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 소환된 적 캐릭터를 시스템에 등록합니다.
        /// 일반 적과 달리 사망 콜백을 덮어쓰지 않습니다.
        /// </summary>
        public void RegisterSummonedEnemy(ICharacter enemy)
        {
            enemyManager?.RegisterEnemy(enemy);

            // 소환된 적은 소환 사망 콜백을 설정합니다
            if (enemy is EnemyCharacter concreteEnemy)
            {
                concreteEnemy.SetDeathCallback(OnSummonedEnemyDeath);
                // OnSummonRequested 이벤트는 더 이상 사용하지 않음 (상태 패턴으로 처리)
            }

            GameLogger.LogInfo($"소환된 적 등록 완료: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
        }

		/// <summary>
		/// 적 처치 시 호출되는 메서드
		/// </summary>
		public void OnEnemyDeath(ICharacter enemy)
		{
			GameLogger.LogInfo($"[StageManager] 적 처치: {enemy.GetCharacterName()} (현재 인덱스: {currentEnemyIndex})", GameLogger.LogCategory.Combat);

			// 소환된 적인지 확인 (원본 적 데이터가 저장되어 있는지로 판단)
			if (originalEnemyData != null)
			{
				GameLogger.LogInfo($"[StageManager] 소환된 적 사망 감지 - 원본 복귀 시작: {originalEnemyData.DisplayName}", GameLogger.LogCategory.Combat);
				// 소환된 적 사망 콜백 호출
				OnSummonedEnemyDeath(enemy);
				return; // 소환된 적은 일반적인 적 처치 로직을 건너뜀
			}

			// 일반 적 처치 로직
			// CombatStateMachine에 적 사망 알림 (적 제거 전에 알려야 함, DI 주입)
			if (combatStateMachine != null)
			{
				GameLogger.LogInfo($"[StageManager] CombatStateMachine에 적 사망 알림", GameLogger.LogCategory.Combat);
				combatStateMachine.OnEnemyDeathDetected();
			}

			// 적 처치 이벤트 발생
			OnEnemyDefeated?.Invoke(enemy);

			// 적을 enemyManager에서 제거
			if (enemyManager != null)
			{
				enemyManager.UnregisterEnemy();
				GameLogger.LogInfo($"[StageManager] 적 제거 완료: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
			}

			// 적 GameObject 파괴
			if (enemy is EnemyCharacter enemyCharacter)
			{
				Destroy(enemyCharacter.gameObject);
				GameLogger.LogInfo($"[StageManager] 적 오브젝트 파괴: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
			}

			// 보상창 열기는 EnemyDefeatedState 완료 후로 이동
			// (EnemyDefeatedState에서 OnEnemyDefeatedCleanupCompleted 이벤트 발생 시 처리)
			
			GameLogger.LogInfo("[StageManager] 적 처치 완료 - 전투 정리 대기 중", GameLogger.LogCategory.Combat);
		}

		/// <summary>
		/// 보상 처리가 완료되었을 때 호출되는 콜백
		/// </summary>
		private void OnRewardProcessCompleted()
		{
			// 콜백 해제
			if (rewardBridge != null)
			{
				rewardBridge.OnRewardProcessCompleted -= OnRewardProcessCompleted;
			}

			GameLogger.LogInfo("[StageManager] 보상 처리 완료 - 다음 진행 시작", GameLogger.LogCategory.UI);
			
			// EnemyDefeatedState에 보상 완료 알림
			// CombatStateMachine의 현재 상태를 직접 접근할 수 없으므로
			// 다른 방식으로 처리 (예: 이벤트 시스템 사용)
			GameLogger.LogInfo("[StageManager] 보상 완료 - EnemyDefeatedState에 알림 전송", GameLogger.LogCategory.UI);
			
			// 스테이지 진행 상태 업데이트
			UpdateStageProgress(null); // enemy는 이미 제거되었으므로 null 전달
		}

		/// <summary>
		/// 적 캐릭터 처치 후 스테이지 진행 상태를 업데이트합니다.
		/// 모든 적 처치 시 스테이지 완료(승리)를 처리합니다.
		/// </summary>
		private void UpdateStageProgress(ICharacter enemy)
		{
			GameLogger.LogInfo($"[StageManager] UpdateStageProgress - 현재 인덱스: {currentEnemyIndex}, 총 적 수: {currentStage?.enemies.Count ?? 0}", GameLogger.LogCategory.Combat);

			// 다음 적이 있는지 확인
			if (HasMoreEnemies())
			{
				GameLogger.LogInfo($"[StageManager] 다음 적이 존재함 - 생성 시작", GameLogger.LogCategory.Combat);

				// 적 카드 슬롯 정리 후 다음 적 생성
				_ = ClearEnemySlotsAndSpawnNext();
			}
			else
			{
				GameLogger.LogInfo($"[StageManager] 모든 적 처치 완료 - 스테이지 승리", GameLogger.LogCategory.Combat);
				// 모든 적 처치 완료 - 스테이지 승리!
				CompleteStage();
			}
		}

		/// <summary>
		/// EnemyDefeatedState의 정리 작업이 완료되었을 때 호출되는 메서드
		/// </summary>
		public void OnEnemyDefeatedCleanupCompleted()
		{
			GameLogger.LogInfo("[StageManager] 전투 정리 완료 - 보상창 열기 시작", GameLogger.LogCategory.UI);
			
			// 보상 UI 열기 및 완료 대기 (설정된 경우)
			if (rewardBridge != null)
			{
				// 보상 완료 콜백 연결
				rewardBridge.OnRewardProcessCompleted += OnRewardProcessCompleted;
				
				rewardBridge.OnEnemyKilled();
				if (debugSettings != null && debugSettings.showRewardInfo)
				{
					GameLogger.LogInfo("[StageManager] 전투 정리 완료 → 보상 UI 오픈 요청 (완료 대기)", GameLogger.LogCategory.UI);
				}
			}
			else
			{
				// 보상 브리지가 없으면 바로 다음 진행
				GameLogger.LogInfo("[StageManager] 보상 브리지가 없음 - 바로 다음 진행", GameLogger.LogCategory.UI);
				UpdateStageProgress(null);
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
            GameLogger.LogInfo($"[StageManager] 모든 슬롯 정리 시작 (플레이어 턴 마커 + 적 카드)", GameLogger.LogCategory.Combat);

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

                    GameLogger.LogInfo($"[StageManager] 모든 슬롯 정리 완료", GameLogger.LogCategory.Combat);
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
            if (data?.Prefab == null)
            {
                GameLogger.LogError("적 데이터 또는 프리팹이 null입니다", GameLogger.LogCategory.Error);
                return null;
            }

            // 비동기 처리 시뮬레이션
            await Task.Delay(100);

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
            
            // 적 데이터 설정
            enemy.SetCharacterData(data);

            // 등장 연출 (오른쪽 바깥에서 자리로) - Ease.InOutCubic 그래프
            var entranceTween = TryPlayEntranceAnimation(enemyInstance.transform, fromLeft: false);

            // 애니메이션 완료 대기
            if (entranceTween != null)
            {
                GameLogger.LogInfo($"적 등장 애니메이션 시작: {data.CharacterName}", GameLogger.LogCategory.Combat);
                await entranceTween.AsyncWaitForCompletion();
                GameLogger.LogInfo($"적 등장 애니메이션 완료: {data.CharacterName}", GameLogger.LogCategory.Combat);
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
            if (target == null) return null;
            const float duration = 1.5f;
            var ease = Ease.InOutCubic;

            if (target is RectTransform rt)
            {
                Vector2 end = rt.anchoredPosition;
                Vector2 start = new Vector2(fromLeft ? -1100f : 1100f, end.y);
                rt.anchoredPosition = start;
                return rt.DOAnchorPos(end, duration).SetEase(ease);
            }
            else
            {
                Vector3 end = target.position;
                Vector3 start = new Vector3(fromLeft ? -1100f : 1100f, end.y, end.z);
                target.position = start;
                return target.DOMove(end, duration).SetEase(ease);
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

        /// <inheritdoc />
        public StageData GetCurrentStage() => currentStage;

        /// <inheritdoc />
        public bool HasNextEnemy() =>
            currentStage != null && currentEnemyIndex < currentStage.enemies.Count;

        /// <summary>
        /// 아직 처치하지 않은 적이 더 있는지 확인합니다.
        /// </summary>
        private bool HasMoreEnemies()
        {
            return HasNextEnemy();
        }

        /// <inheritdoc />
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
        /// 다음 스테이지가 있는지 확인합니다.
        /// </summary>
        public bool HasNextStage()
        {
            return currentStage?.stageNumber < 4;
        }
        
        /// <summary>
        /// 다음 스테이지로 진행합니다.
        /// </summary>
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
                GameLogger.LogError($"스테이지 {stageNumber} 데이터를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
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
            
            foreach (var stage in stageSettings.allStages)
            {
                if (stage != null && stage.stageNumber == stageNumber)
                {
                    return stage;
                }
            }
            
            return null;
        }

        #endregion

        #region 로그 스쿨 시스템 - 단계별 관리

        #region 스테이지 진행 관리

        public StageProgressState ProgressState => progressState;
        public bool IsStageCompleted => isStageCompleted;

        /// <summary>
        /// 스테이지를 시작합니다. 첫 번째 적을 생성합니다.
        /// </summary>
        public void StartStage()
        {
            if (currentStage == null || currentStage.enemies.Count == 0)
            {
                GameLogger.LogWarning("스테이지 데이터가 유효하지 않습니다", GameLogger.LogCategory.Combat);
                return;
            }

            progressState = StageProgressState.InProgress;
            currentEnemyIndex = 0;
            isStageCompleted = false;
            
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogInfo($"스테이지 시작: {currentStage.stageName} (스테이지 {currentStage.stageNumber})", GameLogger.LogCategory.Combat);
            
            // 첫 번째 적의 BGM 즉시 재생 (스테이지 시작 시)
            if (audioManager != null && currentStage.enemies != null && currentStage.enemies.Count > 0)
            {
                var firstEnemyData = currentStage.enemies[0];
                GameLogger.LogInfo($"첫 번째 적 BGM 재생 시작: {firstEnemyData.DisplayName}", GameLogger.LogCategory.Audio);
                audioManager.PlayEnemyBGM(firstEnemyData);
            }
            else if (audioManager == null)
            {
                GameLogger.LogWarning("AudioManager가 null입니다 - 첫 적 BGM 재생 건너뜀", GameLogger.LogCategory.Audio);
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
            
            GameLogger.LogInfo($"스테이지 완료 (승리!): {currentStage.stageName} (스테이지 {currentStage.stageNumber})", GameLogger.LogCategory.Combat);
            
            // 다음 스테이지로 진행 또는 게임 완료 처리
            if (currentStage.IsLastStage)
            {
                // 마지막 스테이지 완료 - 게임 완료!
                CompleteGame();
            }
            else if (currentStage.autoProgressToNext)
            {
                // 다음 스테이지로 자동 진행 (즉시)
                if (ProgressToNextStage())
                {
                    GameLogger.LogInfo($"다음 스테이지로 진행: {currentStage.stageName}", GameLogger.LogCategory.Combat);
                    StartStage();
                }
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
        }
        

        public void FailStage()
        {
            progressState = StageProgressState.Failed;
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogWarning($"스테이지 실패: {currentStage?.stageName ?? "Unknown"} (스테이지 {currentStage?.stageNumber ?? 1})", GameLogger.LogCategory.Combat);
        }

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

        #endregion

        #region 소환 시스템

        private EnemyCharacterData originalEnemyData;
        private int originalEnemyHP;
        private EnemyCharacterData summonTargetData;
        private bool isSummonedEnemyActive = false;

        private void HandleSummonRequest(EnemyCharacterData summonTarget, int currentHP)
        {
            // 이 메서드는 더 이상 사용되지 않습니다 (상태 패턴으로 처리됨)
            GameLogger.LogWarning("[StageManager] HandleSummonRequest는 더 이상 사용되지 않습니다", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 소환/복귀 전환 처리
        /// </summary>
        private async Task TransitionToSummonState(EnemyCharacterData targetEnemy, bool isRestore)
        {
            // CombatStateMachine 확인 (DI 주입)
            if (combatStateMachine == null)
            {
                GameLogger.LogError("[소환] CombatStateMachine이 주입되지 않았습니다 - 소환 중단", GameLogger.LogCategory.Combat);
                return;
            }

            try
            {
                // 1단계: 기존 적 제거 및 슬롯 정리
                await CleanupCurrentEnemy();
                
                // 2단계: 새로운 적 생성
                var newEnemy = await CreateEnemyForSummonAsync(targetEnemy);
                if (newEnemy == null)
                {
                    GameLogger.LogError("[소환] 적 생성 실패", GameLogger.LogCategory.Combat);
                    return;
                }

                // 3단계: 적 등록
                if (isRestore)
                {
                    RegisterEnemy(newEnemy);
                    // 복귀 시 원래 HP 복원
                    if (newEnemy is EnemyCharacter enemyChar && originalEnemyHP > 0)
                    {
                        enemyChar.SetCurrentHP(originalEnemyHP);
                        GameLogger.LogInfo($"[소환] 복귀 완료: {targetEnemy.DisplayName} (HP 복원: {originalEnemyHP})", GameLogger.LogCategory.Combat);
                    }
                    else
                    {
                        GameLogger.LogInfo($"[소환] 복귀 완료: {targetEnemy.DisplayName}", GameLogger.LogCategory.Combat);
                    }
                }
                else
                {
                    RegisterSummonedEnemy(newEnemy);
                    GameLogger.LogInfo($"[소환] 소환 완료: {targetEnemy.DisplayName}", GameLogger.LogCategory.Combat);
                }

                // 4단계: 소환 완료 - CombatInitState가 자동으로 감지하여 처리
                if (!isRestore)
                {
                    GameLogger.LogInfo("[소환] 소환 완료 - CombatInitState가 자동으로 슬롯 설정을 처리합니다", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogInfo("[소환] 복귀 완료 - CombatInitState가 자동으로 슬롯 설정을 처리합니다", GameLogger.LogCategory.Combat);
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
            GameLogger.LogInfo($"[소환] {summonedEnemy.GetCharacterName()} 사망 → {originalEnemyData?.DisplayName} 복귀 (HP: {originalEnemyHP})", GameLogger.LogCategory.Combat);
            
            if (originalEnemyData != null)
            {
                // 복귀 전환 상태로 이동
                _ = TransitionToSummonState(originalEnemyData, true);
                
                // 원본 적 복귀 완료 후 소환 변수 초기화
                originalEnemyData = null;
                originalEnemyHP = 0;
                // isSummonedEnemyActive는 TransitionToSummonState에서 false로 설정됨
            }
            else
            {
                GameLogger.LogWarning("[소환] 원본 적 데이터가 없어서 복귀할 수 없습니다.", GameLogger.LogCategory.Combat);
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
        /// </summary>
        public EnemyCharacterData GetOriginalEnemyData()
        {
            return originalEnemyData;
        }

        /// <summary>
        /// 원본 적 HP를 반환합니다 (상태 패턴에서 사용)
        /// </summary>
        public int GetOriginalEnemyHP()
        {
            return originalEnemyHP;
        }

        /// <summary>
        /// 원본 적 데이터를 설정합니다 (상태 패턴에서 사용)
        /// </summary>
        public void SetOriginalEnemyData(EnemyCharacterData data)
        {
            originalEnemyData = data;
            GameLogger.LogInfo($"[StageManager] 원본 적 데이터 설정: {data?.DisplayName}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 원본 적 HP를 설정합니다 (상태 패턴에서 사용)
        /// </summary>
        public void SetOriginalEnemyHP(int hp)
        {
            originalEnemyHP = hp;
            GameLogger.LogInfo($"[StageManager] 원본 적 HP 설정: {hp}", GameLogger.LogCategory.Combat);
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
            GameLogger.LogInfo("[StageManager] 소환 데이터 초기화 완료", GameLogger.LogCategory.Combat);
        }

        #endregion

        #endregion
    }
}

