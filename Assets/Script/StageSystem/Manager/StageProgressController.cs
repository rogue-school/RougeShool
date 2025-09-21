using UnityEngine;
using Game.StageSystem.Data;
using Game.StageSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Interface;
using Zenject;

namespace Game.StageSystem.Manager
{
    /// <summary>
    /// 스테이지 진행 로직을 통합 관리하는 컨트롤러
    /// 로그 스쿨 시스템: 준보스 → 보스 순서로 진행
    /// </summary>
    public class StageProgressController : MonoBehaviour
    {
        #region 의존성 주입

        [Inject] private IStagePhaseManager phaseManager;
        [Inject] private IStageRewardManager rewardManager;
        [Inject] private EnemyManager enemyManager;
        // TODO: CharacterDeathListener 구현 필요

        #endregion

        #region 초기화

        private void Start()
        {
            // 적 사망 리스너 등록 (이벤트 구독이 아닌 직접 호출)
            // deathListener는 다른 시스템에서 적 사망 시 직접 호출됨
        }

        private void OnDestroy()
        {
            // 정리 작업 (필요시)
        }

        #endregion

        #region 스테이지 시작

        /// <summary>
        /// 스테이지 시작 (준보스부터 시작)
        /// </summary>
        public void StartStage()
        {
            if (phaseManager == null)
            {
                Debug.LogError("[StageProgressController] PhaseManager가 없습니다.");
                return;
            }

            Debug.Log("[StageProgressController] 스테이지 시작");
            phaseManager.StartSubBossPhase();
        }

        #endregion

        #region 적 사망 처리

        /// <summary>
        /// 적 사망 시 호출되는 메서드
        /// </summary>
        public void OnEnemyDeath(ICharacter enemy)
        {
            if (enemy == null) return;

            Debug.Log($"[StageProgressController] 적 사망: {enemy.CharacterName}");

            // 현재 단계에 따라 처리
            if (phaseManager.IsSubBossPhase())
            {
                OnSubBossDefeated();
            }
            else if (phaseManager.IsBossPhase())
            {
                OnBossDefeated();
            }
        }

        /// <summary>
        /// 준보스 처치 시 처리
        /// </summary>
        private void OnSubBossDefeated()
        {
            Debug.Log("[StageProgressController] 준보스 처치 완료");
            
            // 준보스 보상 지급
            rewardManager.GiveSubBossRewards();
            
            // 보스 단계로 진행
            phaseManager.StartBossPhase();
        }

        /// <summary>
        /// 보스 처치 시 처리
        /// </summary>
        private void OnBossDefeated()
        {
            Debug.Log("[StageProgressController] 보스 처치 완료");
            
            // 보스 보상 지급
            rewardManager.GiveBossRewards();
            
            // 스테이지 완료
            phaseManager.CompleteStage();
        }

        #endregion

        #region 스테이지 관리

        /// <summary>
        /// 스테이지 실패 처리
        /// </summary>
        public void FailStage()
        {
            Debug.Log("[StageProgressController] 스테이지 실패");
            phaseManager.FailStage();
        }

        /// <summary>
        /// 스테이지 리셋
        /// </summary>
        public void ResetStage()
        {
            Debug.Log("[StageProgressController] 스테이지 리셋");
            
            // 적 제거
            if (enemyManager != null)
            {
                var enemy = enemyManager.GetEnemy();
                if (enemy != null)
                {
                    enemyManager.UnregisterEnemy();
                }
            }
            
            // 상태 초기화는 PhaseManager에서 처리
            // 필요시 추가 초기화 로직 구현
        }

        #endregion

        #region 디버그 정보

        /// <summary>
        /// 현재 스테이지 상태 정보 출력
        /// </summary>
        [ContextMenu("현재 상태 출력")]
        public void PrintCurrentState()
        {
            if (phaseManager == null)
            {
                Debug.Log("[StageProgressController] PhaseManager가 없습니다.");
                return;
            }

            Debug.Log($"[StageProgressController] 현재 단계: {phaseManager.CurrentPhase}");
            Debug.Log($"[StageProgressController] 진행 상태: {phaseManager.ProgressState}");
            Debug.Log($"[StageProgressController] 준보스 처치: {phaseManager.IsSubBossDefeated}");
            Debug.Log($"[StageProgressController] 보스 처치: {phaseManager.IsBossDefeated}");
        }

        #endregion
    }
}
