using UnityEngine;
using Game.CombatSystem.Stage;
using Game.IManager;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Core;
using Game.StageSystem.Data;
using Game.StageSystem.Interface;
using Zenject;
using Game.CombatSystem.Utility;

namespace Game.StageSystem.Manager
{
    /// <summary>
    /// 현재 스테이지의 적 캐릭터를 관리하고 생성합니다.
    /// 핸드 등록은 EnemyHandInitializer 등 다른 시스템에서 처리합니다.
    /// 로그 스쿨 시스템: 준보스/보스 단계별 관리 지원
    /// </summary>
    public class StageManager : MonoBehaviour, IStageManager, IStagePhaseManager, IStageRewardManager
    {
        #region 인스펙터 필드

        [Header("스테이지 데이터")]
        [SerializeField] private StageData currentStage;
        
        [Header("로그 스쿨 시스템 - 단계별 스테이지")]
        [SerializeField] private StagePhaseData currentPhaseStage;

        #endregion

        #region 내부 상태

        private int currentEnemyIndex = 0;
        private bool isSpawning = false;
        
        // 로그 스쿨 시스템 상태
        private StagePhaseState currentPhase = StagePhaseState.None;
        private StageProgressState progressState = StageProgressState.NotStarted;
        private bool isSubBossDefeated = false;
        private bool isBossDefeated = false;
        private StageRewardData currentRewards;

        #endregion

        #region 의존성 주입

        [Inject] private IEnemySpawnerManager spawnerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private IEnemySpawnValidator spawnValidator;
        [Inject] private ICharacterDeathListener deathListener;

        #endregion

        #region 적 생성 흐름

        /// <summary>
        /// 다음 적을 생성하여 전투에 배치합니다. (코루틴 기반)
        /// </summary>
        public System.Collections.IEnumerator SpawnNextEnemyCoroutine()
        {
            if (isSpawning)
            {
                Debug.LogWarning("[StageManager] 중복 스폰 방지");
                yield break;
            }

            if (!spawnValidator.CanSpawnEnemy())
            {
                Debug.LogWarning("[StageManager] 스폰 조건 미충족");
                yield break;
            }

            if (enemyManager.GetEnemy() != null)
            {
                Debug.LogWarning("[StageManager] 이미 적이 존재합니다.");
                yield break;
            }

            if (!TryGetNextEnemyData(out var data))
            {
                Debug.LogWarning("[StageManager] 다음 적 데이터를 가져올 수 없습니다.");
                yield break;
            }

            isSpawning = true;
            EnemySpawnResult result = null;
            yield return spawnerManager.SpawnEnemyWithAnimation(data, r => { result = r; });
            if (result?.Enemy == null)
            {
                Debug.LogError("[StageManager] 적 생성 실패");
                isSpawning = false;
                yield break;
            }
            RegisterEnemy(result.Enemy);
            currentEnemyIndex++;
            isSpawning = false;
        }

        /// <summary>
        /// 기존 API 호환성을 위해 StartCoroutine으로 코루틴을 호출
        /// </summary>
        public void SpawnNextEnemy()
        {
            StartCoroutine(SpawnNextEnemyCoroutine());
        }

        /// <summary>
        /// 적 캐릭터를 시스템에 등록하고, 사망 리스너를 연결합니다.
        /// </summary>
        private void RegisterEnemy(IEnemyCharacter enemy)
        {
            enemyManager.RegisterEnemy(enemy);
            if (enemy is EnemyCharacter concrete && deathListener != null)
                concrete.SetDeathListener(deathListener);
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

        /// <inheritdoc />
        public EnemyCharacterData PeekNextEnemyData() =>
            HasNextEnemy() ? currentStage.enemies[currentEnemyIndex] : null;

        /// <summary>
        /// 현재 스테이지 번호를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="stageNumber">스테이지 번호</param>
        public void SetCurrentStageNumber(int stageNumber)
        {
            // TODO: 실제 스테이지 번호 관리 로직 구현 필요
            // 현재는 StageData의 name이나 다른 식별자를 사용할 수 있음
            Debug.Log($"[StageManager] 스테이지 번호 설정: {stageNumber}");
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
            if (currentPhaseStage == null || !currentPhaseStage.HasValidSubBoss())
            {
                Debug.LogWarning("[StageManager] 준보스 데이터가 유효하지 않습니다.");
                return;
            }

            currentPhase = StagePhaseState.SubBoss;
            progressState = StageProgressState.SubBossBattle;
            isSubBossDefeated = false;
            
            OnPhaseChanged?.Invoke(currentPhase);
            OnProgressChanged?.Invoke(progressState);
            
            Debug.Log($"[StageManager] 준보스 단계 시작: {currentPhaseStage.StageName}");
        }

        public void StartBossPhase()
        {
            if (currentPhaseStage == null || !currentPhaseStage.HasValidBoss())
            {
                Debug.LogWarning("[StageManager] 보스 데이터가 유효하지 않습니다.");
                return;
            }

            currentPhase = StagePhaseState.Boss;
            progressState = StageProgressState.BossBattle;
            isBossDefeated = false;
            
            OnPhaseChanged?.Invoke(currentPhase);
            OnProgressChanged?.Invoke(progressState);
            
            Debug.Log($"[StageManager] 보스 단계 시작: {currentPhaseStage.StageName}");
        }

        public void CompleteStage()
        {
            currentPhase = StagePhaseState.Completed;
            progressState = StageProgressState.Completed;
            
            OnPhaseChanged?.Invoke(currentPhase);
            OnProgressChanged?.Invoke(progressState);
            
            // 스테이지 완료 보상 지급
            GiveStageCompletionRewards();
            
            Debug.Log($"[StageManager] 스테이지 완료: {currentPhaseStage.StageName}");
        }

        public void FailStage()
        {
            progressState = StageProgressState.Failed;
            OnProgressChanged?.Invoke(progressState);
            
            Debug.Log($"[StageManager] 스테이지 실패: {currentPhaseStage.StageName}");
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
            Debug.Log($"[StageManager] 현재 단계 설정: {phase}");
        }

        /// <summary>
        /// 준보스 처치 상태를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="defeated">처치 여부</param>
        public void SetSubBossDefeated(bool defeated)
        {
            isSubBossDefeated = defeated;
            Debug.Log($"[StageManager] 준보스 처치 상태 설정: {defeated}");
        }

        /// <summary>
        /// 보스 처치 상태를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="defeated">처치 여부</param>
        public void SetBossDefeated(bool defeated)
        {
            isBossDefeated = defeated;
            Debug.Log($"[StageManager] 보스 처치 상태 설정: {defeated}");
        }

        public event System.Action<StagePhaseState> OnPhaseChanged;
        public event System.Action<StageProgressState> OnProgressChanged;

        #endregion

        #region IStageRewardManager 구현

        public void GiveSubBossRewards()
        {
            if (currentRewards == null || !currentRewards.HasSubBossRewards())
            {
                Debug.LogWarning("[StageManager] 준보스 보상이 없습니다.");
                return;
            }

            // 아이템 보상 지급
            foreach (var item in currentRewards.SubBossRewards)
            {
                OnItemRewardGiven?.Invoke(item);
                Debug.Log($"[StageManager] 준보스 아이템 보상: {item.itemName} x{item.quantity}");
            }

            // 화폐 보상 지급
            foreach (var currency in currentRewards.SubBossCurrency)
            {
                OnCurrencyRewardGiven?.Invoke(currency);
                Debug.Log($"[StageManager] 준보스 화폐 보상: {currency.currencyType} {currency.amount}");
            }
        }

        public void GiveBossRewards()
        {
            if (currentRewards == null || !currentRewards.HasBossRewards())
            {
                Debug.LogWarning("[StageManager] 보스 보상이 없습니다.");
                return;
            }

            // 아이템 보상 지급
            foreach (var item in currentRewards.BossRewards)
            {
                OnItemRewardGiven?.Invoke(item);
                Debug.Log($"[StageManager] 보스 아이템 보상: {item.itemName} x{item.quantity}");
            }

            // 화폐 보상 지급
            foreach (var currency in currentRewards.BossCurrency)
            {
                OnCurrencyRewardGiven?.Invoke(currency);
                Debug.Log($"[StageManager] 보스 화폐 보상: {currency.currencyType} {currency.amount}");
            }
        }

        public void GiveStageCompletionRewards()
        {
            if (currentRewards == null || !currentRewards.HasStageCompletionRewards())
            {
                Debug.LogWarning("[StageManager] 스테이지 완료 보상이 없습니다.");
                return;
            }

            // 아이템 보상 지급
            foreach (var item in currentRewards.StageCompletionRewards)
            {
                OnItemRewardGiven?.Invoke(item);
                Debug.Log($"[StageManager] 스테이지 완료 아이템 보상: {item.itemName} x{item.quantity}");
            }

            // 화폐 보상 지급
            foreach (var currency in currentRewards.StageCompletionCurrency)
            {
                OnCurrencyRewardGiven?.Invoke(currency);
                Debug.Log($"[StageManager] 스테이지 완료 화폐 보상: {currency.currencyType} {currency.amount}");
            }
        }

        public void GiveRewards(StageRewardData rewards)
        {
            if (rewards == null)
            {
                Debug.LogWarning("[StageManager] 보상 데이터가 null입니다.");
                return;
            }

            SetCurrentRewards(rewards);
            
            // 모든 보상 지급
            GiveSubBossRewards();
            GiveBossRewards();
            GiveStageCompletionRewards();
        }

        public bool HasSubBossRewards() => currentRewards?.HasSubBossRewards() ?? false;
        public bool HasBossRewards() => currentRewards?.HasBossRewards() ?? false;
        public bool HasStageCompletionRewards() => currentRewards?.HasStageCompletionRewards() ?? false;

        public void SetCurrentRewards(StageRewardData rewards)
        {
            currentRewards = rewards;
            Debug.Log($"[StageManager] 보상 데이터 설정: {rewards?.name ?? "null"}");
        }

        public StageRewardData GetCurrentRewards() => currentRewards;

        public event System.Action<StageRewardData.RewardItem> OnItemRewardGiven;
        public event System.Action<StageRewardData.RewardCurrency> OnCurrencyRewardGiven;

        #endregion

        #endregion
    }
}
