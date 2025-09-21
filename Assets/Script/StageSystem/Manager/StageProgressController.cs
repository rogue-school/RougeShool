using UnityEngine;
using Game.StageSystem.Data;
using Game.CoreSystem.Utility;
using Game.StageSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Interface;
using Zenject;

namespace Game.StageSystem.Manager
{
    /// <summary>
    /// 스테이지 진행 로직을 통합 관리하는 컨트롤러
    /// 모든 적 처치 시 스테이지 완료(승리) 처리
    /// </summary>
    public class StageProgressController : MonoBehaviour
    {
        #region 의존성 주입

        [Inject] private IStageManager stageManager;
        [Inject] private IStageRewardManager rewardManager;
        [Inject] private EnemyManager enemyManager;

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
        /// 스테이지 시작
        /// </summary>
        public void StartStage()
        {
            if (stageManager == null)
            {
                GameLogger.LogError("[StageProgressController] StageManager가 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            GameLogger.LogInfo("[StageProgressController] 스테이지 시작", GameLogger.LogCategory.Core);
            stageManager.StartStage();
        }

        #endregion

        #region 적 사망 처리

        /// <summary>
        /// 적 사망 시 호출되는 메서드
        /// </summary>
        public void OnEnemyDeath(ICharacter enemy)
        {
            if (enemy == null) return;

            GameLogger.LogInfo($"[StageProgressController] 적 사망: {enemy.CharacterName}", GameLogger.LogCategory.Core);

            // 적 처치 보상 지급
            rewardManager.GiveEnemyDefeatRewards();
            
            // StageManager에서 다음 적 생성 또는 스테이지 완료 처리
            // (StageManager.UpdateStageProgress에서 자동으로 처리됨)
        }

        #endregion

        #region 스테이지 관리

        /// <summary>
        /// 스테이지 실패 처리
        /// </summary>
        public void FailStage()
        {
            GameLogger.LogInfo("[StageProgressController] 스테이지 실패", GameLogger.LogCategory.Core);
            stageManager.FailStage();
        }

        /// <summary>
        /// 스테이지 리셋
        /// </summary>
        public void ResetStage()
        {
            GameLogger.LogInfo("[StageProgressController] 스테이지 리셋", GameLogger.LogCategory.Core);
            
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
            if (stageManager == null)
            {
                GameLogger.LogInfo("[StageProgressController] StageManager가 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            GameLogger.LogInfo($"[StageProgressController] 진행 상태: {stageManager.ProgressState}", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"[StageProgressController] 스테이지 완료: {stageManager.IsStageCompleted}", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"[StageProgressController] 다음 적 있음: {stageManager.HasNextEnemy()}", GameLogger.LogCategory.Core);
        }

        #endregion
    }
}
