using UnityEngine;
using System.Threading.Tasks;
using System;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Interface;
using Game.StageSystem.Data;
using Game.StageSystem.Interface;
using Zenject;
using Game.CoreSystem.Utility;

namespace Game.StageSystem.Manager
{
    /// <summary>
    /// 스테이지 진행을 관리하는 매니저입니다.
    /// 적 캐릭터 2마리를 순차적으로 관리하며,
    /// 각 적 캐릭터 처치 시 개별 보상을 지급합니다.
    /// </summary>
    public class StageManager : MonoBehaviour, IStageManager, IStagePhaseManager, IStageRewardManager
    {
        #region 인스펙터 필드

        [System.Serializable]
        public class StageSettings
        {
            [Header("스테이지 데이터")]
            [Tooltip("현재 스테이지 데이터")]
            public StageData currentStage;

            [Space(5)]
            [Header("진행 설정")]
            [Tooltip("자동 스테이지 진행")]
            public bool autoProgress = true;

            [Tooltip("스테이지 전환 지연 시간 (초)")]
            [Range(0f, 5f)]
            public float transitionDelay = 1f;

            [Space(5)]
            [Header("난이도 설정")]
            [Tooltip("스테이지 난이도 배율")]
            [Range(0.5f, 3f)]
            public float difficultyMultiplier = 1f;
        }

        [System.Serializable]
        public class RewardSettings
        {
            [Header("보상 설정")]
            [Tooltip("적 처치 보상 활성화")]
            public bool enableEnemyDefeatRewards = true;

            [Tooltip("보상 지급 지연 시간 (초)")]
            [Range(0f, 3f)]
            public float rewardDelay = 0.5f;

            [Space(5)]
            [Header("보상 효과")]
            [Tooltip("보상 애니메이션 활성화")]
            public bool enableRewardAnimation = true;

            [Tooltip("보상 사운드 활성화")]
            public bool enableRewardSound = true;
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
        [Header("🎁 보상 설정")]
        [SerializeField] private RewardSettings rewardSettings = new RewardSettings();
        
        [Space(10)]
        [Header("🔧 디버그 설정")]
        [SerializeField] private DebugSettings debugSettings = new DebugSettings();

        #endregion

        #region 내부 상태

        private int currentEnemyIndex = 0;
        private bool isSpawning = false;
        
        // 스테이지 진행 상태
        private StagePhaseState currentPhase = StagePhaseState.None;
        private StageProgressState progressState = StageProgressState.NotStarted;
        private bool isSubBossDefeated = false;
        private bool isBossDefeated = false;
        private StageRewardData currentRewards;

        #endregion

        #region 이벤트

        /// <summary>적 처치 시 호출되는 이벤트</summary>
        public event Action<ICharacter> OnEnemyDefeated;
        
        /// <summary>스테이지 완료 시 호출되는 이벤트</summary>
        public event Action<StageData> OnStageCompleted;

        #endregion

        #region 의존성 주입 (최소화)

        // 핵심 의존성만 유지
        [Inject] private EnemyManager enemyManager;

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

            if (enemyManager.GetEnemy() != null)
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
                currentEnemyIndex++;
                
                GameLogger.LogInfo($"적 생성 완료: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
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
        /// 적 캐릭터를 시스템에 등록합니다.
        /// </summary>
        private void RegisterEnemy(ICharacter enemy)
        {
            enemyManager.RegisterEnemy(enemy);
            
            // 적 캐릭터에 사망 리스너 설정
            if (enemy is EnemyCharacter concreteEnemy)
            {
                concreteEnemy.SetDeathListener(new EnemyDeathHandler(this));
            }
            
            GameLogger.LogInfo($"적 등록 완료: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 적 처치 시 호출되는 메서드
        /// </summary>
        private void OnEnemyDeath(ICharacter enemy)
        {
            GameLogger.LogInfo($"적 처치: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
            
            // 적 처치 이벤트 발생
            OnEnemyDefeated?.Invoke(enemy);
            
            // 적 처치 시 보상 지급
            GiveEnemyDefeatReward(enemy);
            
            // 스테이지 진행 상태 업데이트
            UpdateStageProgress(enemy);
        }

        /// <summary>
        /// 적 사망 처리를 위한 내부 클래스
        /// </summary>
        private class EnemyDeathHandler
        {
            private readonly StageManager stageManager;

            public EnemyDeathHandler(StageManager stageManager)
            {
                this.stageManager = stageManager;
            }

            public void OnCharacterDied(ICharacter character)
            {
                if (!character.IsPlayerControlled())
                {
                    stageManager.OnEnemyDeath(character);
                }
            }

            public void OnEnemyDeath(ICharacter enemy)
            {
                stageManager.OnEnemyDeath(enemy);
            }
        }

        /// <summary>
        /// 적 캐릭터 처치 시 보상을 지급합니다.
        /// </summary>
        private void GiveEnemyDefeatReward(ICharacter enemy)
        {
            if (currentRewards == null)
            {
                GameLogger.LogWarning("보상 데이터가 설정되지 않았습니다", GameLogger.LogCategory.Combat);
                return;
            }

            // 현재 단계에 따른 보상 지급
            if (currentPhase == StagePhaseState.SubBoss)
            {
                GiveEnemyRewards(StagePhaseState.SubBoss);
            }
            else if (currentPhase == StagePhaseState.Boss)
            {
                GiveEnemyRewards(StagePhaseState.Boss);
            }
        }

        /// <summary>
        /// 적 캐릭터 처치 후 스테이지 진행 상태를 업데이트합니다.
        /// </summary>
        private void UpdateStageProgress(ICharacter enemy)
        {
            if (currentPhase == StagePhaseState.SubBoss)
            {
                isSubBossDefeated = true;
                StartBossPhase();
            }
            else if (currentPhase == StagePhaseState.Boss)
            {
                isBossDefeated = true;
                CompleteStage();
            }
        }

        /// <summary>
        /// 적 캐릭터를 생성합니다. (단순화된 로직)
        /// </summary>
        private async Task<ICharacter> CreateEnemyAsync(EnemyCharacterData data)
        {
            // 실제 적 생성 로직은 다른 시스템에 위임
            // 여기서는 단순히 데이터 검증만 수행
            if (data?.Prefab == null)
            {
                GameLogger.LogError("적 데이터 또는 프리팹이 null입니다", GameLogger.LogCategory.Error);
                return null;
            }

            // 비동기 처리 시뮬레이션
            await Task.Delay(100);
            
            // 실제 구현에서는 적 생성 로직을 호출
            // var enemy = Instantiate(data.Prefab).GetComponent<ICharacter>();
            // enemy.Initialize(data);
            // return enemy;
            
            // 임시로 null 반환 (실제 구현 시 수정 필요)
            return null;
        }

        /// <summary>
        /// 다음 적 데이터를 조회합니다.
        /// </summary>
        private bool TryGetNextEnemyData(out EnemyCharacterData data)
        {
            data = null;

            if (stageSettings.currentStage == null ||
                stageSettings.currentStage.enemies == null ||
                currentEnemyIndex >= stageSettings.currentStage.enemies.Count)
                return false;

            data = stageSettings.currentStage.enemies[currentEnemyIndex];
            return data != null && data.Prefab != null;
        }

        #endregion

        #region 스테이지 정보

        /// <inheritdoc />
        public StageData GetCurrentStage() => stageSettings.currentStage;

        /// <inheritdoc />
        public bool HasNextEnemy() =>
            stageSettings.currentStage != null && currentEnemyIndex < stageSettings.currentStage.enemies.Count;

        /// <inheritdoc />
        public EnemyCharacterData PeekNextEnemyData() =>
            HasNextEnemy() ? stageSettings.currentStage.enemies[currentEnemyIndex] : null;

        /// <summary>
        /// 현재 스테이지 번호를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="stageNumber">스테이지 번호</param>
        public void SetCurrentStageNumber(int stageNumber)
        {
            // TODO: 실제 스테이지 번호 관리 로직 구현 필요
            // 현재는 StageData의 name이나 다른 식별자를 사용할 수 있음
            GameLogger.LogInfo($"스테이지 번호 설정: {stageNumber}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 현재 스테이지 번호를 가져옵니다. (저장 시스템용)
        /// </summary>
        /// <returns>스테이지 번호</returns>
        public int GetCurrentStageNumber()
        {
            // TODO: 실제 스테이지 번호 반환 로직 구현 필요
            return 0; // 임시값
        }

        #endregion

        #region 로그 스쿨 시스템 - 단계별 관리

        #region IStagePhaseManager 구현

        public StagePhaseState CurrentPhase => currentPhase;
        public StageProgressState ProgressState => progressState;
        public bool IsSubBossDefeated => isSubBossDefeated;
        public bool IsBossDefeated => isBossDefeated;

        public void StartSubBossPhase()
        {
            // StagePhaseData가 없어도 StageData만으로 진행 가능
            if (stageSettings.currentStage == null || stageSettings.currentStage.enemies.Count == 0)
            {
                GameLogger.LogWarning("스테이지 데이터가 유효하지 않습니다", GameLogger.LogCategory.Combat);
                return;
            }

            currentPhase = StagePhaseState.SubBoss;
            progressState = StageProgressState.SubBossBattle;
            isSubBossDefeated = false;
            
            OnPhaseChanged?.Invoke(currentPhase);
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogInfo($"준보스 단계 시작: {stageSettings.currentStage.name}", GameLogger.LogCategory.Combat);
        }

        public void StartBossPhase()
        {
            // StagePhaseData가 없어도 StageData만으로 진행 가능
            if (stageSettings.currentStage == null || stageSettings.currentStage.enemies.Count == 0)
            {
                GameLogger.LogWarning("스테이지 데이터가 유효하지 않습니다", GameLogger.LogCategory.Combat);
                return;
            }

            currentPhase = StagePhaseState.Boss;
            progressState = StageProgressState.BossBattle;
            isBossDefeated = false;
            
            OnPhaseChanged?.Invoke(currentPhase);
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogInfo($"보스 단계 시작: {stageSettings.currentStage.name}", GameLogger.LogCategory.Combat);
        }

        public void CompleteStage()
        {
            currentPhase = StagePhaseState.Completed;
            progressState = StageProgressState.Completed;
            
            OnPhaseChanged?.Invoke(currentPhase);
            OnProgressChanged?.Invoke(progressState);
            
            // 스테이지 완료 이벤트 발생
            OnStageCompleted?.Invoke(stageSettings.currentStage);
            
            // 스테이지 완료 보상 지급 (선택적)
            GiveStageCompletionRewards();
            
            GameLogger.LogInfo($"스테이지 완료: {stageSettings.currentStage.name}", GameLogger.LogCategory.Combat);
        }

        public void FailStage()
        {
            progressState = StageProgressState.Failed;
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogWarning($"스테이지 실패: {stageSettings.currentStage.name}", GameLogger.LogCategory.Combat);
        }

        public bool IsSubBossPhase() => currentPhase == StagePhaseState.SubBoss;
        public bool IsBossPhase() => currentPhase == StagePhaseState.Boss;
        public bool IsStageCompleted() => currentPhase == StagePhaseState.Completed;

        /// <summary>
        /// 현재 단계를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="phase">설정할 단계</param>
        public void SetCurrentPhase(StagePhaseState phase)
        {
            currentPhase = phase;
            OnPhaseChanged?.Invoke(currentPhase);
            GameLogger.LogInfo($"현재 단계 설정: {phase}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 준보스 처치 상태를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="defeated">처치 여부</param>
        public void SetSubBossDefeated(bool defeated)
        {
            isSubBossDefeated = defeated;
            GameLogger.LogInfo($"준보스 처치 상태 설정: {defeated}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 보스 처치 상태를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="defeated">처치 여부</param>
        public void SetBossDefeated(bool defeated)
        {
            isBossDefeated = defeated;
            GameLogger.LogInfo($"보스 처치 상태 설정: {defeated}", GameLogger.LogCategory.Combat);
        }

        public event System.Action<StagePhaseState> OnPhaseChanged;
        public event System.Action<StageProgressState> OnProgressChanged;

        #endregion

        #region IStageRewardManager 구현

        /// <summary>
        /// 적 캐릭터 처치 시 보상을 지급합니다. (통합 메서드)
        /// </summary>
        /// <param name="phase">현재 스테이지 단계</param>
        public void GiveEnemyRewards(StagePhaseState phase)
        {
            if (currentRewards == null)
            {
                GameLogger.LogWarning("보상 데이터가 설정되지 않았습니다", GameLogger.LogCategory.Combat);
                return;
            }

            bool hasRewards = false;
            string phaseName = GetPhaseDisplayName(phase);

            // 적 처치 보상 지급 (통합된 보상 시스템)
            if (currentRewards.HasEnemyDefeatRewards())
            {
                GiveRewardsByType(currentRewards.EnemyDefeatRewards, currentRewards.EnemyDefeatCurrency, phaseName);
                hasRewards = true;
            }

            if (!hasRewards)
            {
                GameLogger.LogWarning($"{phaseName} 보상이 없습니다", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 보상 타입별로 보상을 지급합니다.
        /// </summary>
        private void GiveRewardsByType(
            System.Collections.Generic.List<StageRewardData.RewardItem> items,
            System.Collections.Generic.List<StageRewardData.RewardCurrency> currencies,
            string phaseName)
        {
            GameLogger.LogInfo($"{phaseName} 보상 지급 시작", GameLogger.LogCategory.Combat);

            // 아이템 보상 지급
            foreach (var item in items)
            {
                OnItemRewardGiven?.Invoke(item);
                GameLogger.LogInfo($"{phaseName} 아이템 보상: {item.itemName} x{item.quantity}", GameLogger.LogCategory.Combat);
            }

            // 화폐 보상 지급
            foreach (var currency in currencies)
            {
                OnCurrencyRewardGiven?.Invoke(currency);
                GameLogger.LogInfo($"{phaseName} 화폐 보상: {currency.currencyType} {currency.amount}", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 단계별 표시 이름을 반환합니다.
        /// </summary>
        private string GetPhaseDisplayName(StagePhaseState phase)
        {
            return phase switch
            {
                StagePhaseState.SubBoss => "첫 번째 적",
                StagePhaseState.Boss => "두 번째 적",
                _ => "적"
            };
        }

        // 기존 API 호환성을 위한 메서드들
        public void GiveSubBossRewards() => GiveEnemyRewards(StagePhaseState.SubBoss);
        public void GiveBossRewards() => GiveEnemyRewards(StagePhaseState.Boss);

        public void GiveStageCompletionRewards()
        {
            if (currentRewards == null || !currentRewards.HasEnemyDefeatRewards())
            {
                GameLogger.LogWarning("적 처치 보상이 없습니다", GameLogger.LogCategory.Combat);
                return;
            }

            GameLogger.LogInfo("적 처치 보상 지급 시작", GameLogger.LogCategory.Combat);

            // 아이템 보상 지급
            foreach (var item in currentRewards.EnemyDefeatRewards)
            {
                OnItemRewardGiven?.Invoke(item);
                GameLogger.LogInfo($"적 처치 아이템 보상: {item.itemName} x{item.quantity}", GameLogger.LogCategory.Combat);
            }

            // 화폐 보상 지급
            foreach (var currency in currentRewards.EnemyDefeatCurrency)
            {
                OnCurrencyRewardGiven?.Invoke(currency);
                GameLogger.LogInfo($"적 처치 화폐 보상: {currency.currencyType} {currency.amount}", GameLogger.LogCategory.Combat);
            }
        }

        public void GiveRewards(StageRewardData rewards)
        {
            if (rewards == null)
            {
                GameLogger.LogWarning("보상 데이터가 null입니다", GameLogger.LogCategory.Combat);
                return;
            }

            SetCurrentRewards(rewards);
            
            // 모든 보상 지급
            GiveSubBossRewards();
            GiveBossRewards();
            GiveStageCompletionRewards();
        }

        public bool HasEnemyDefeatRewards() => currentRewards?.HasEnemyDefeatRewards() ?? false;

        public void SetCurrentRewards(StageRewardData rewards)
        {
            currentRewards = rewards;
            GameLogger.LogInfo($"보상 데이터 설정: {rewards?.name ?? "null"}", GameLogger.LogCategory.Combat);
        }

        public StageRewardData GetCurrentRewards() => currentRewards;

        public event System.Action<StageRewardData.RewardItem> OnItemRewardGiven;
        public event System.Action<StageRewardData.RewardCurrency> OnCurrencyRewardGiven;

        #endregion

        #endregion
    }
}

