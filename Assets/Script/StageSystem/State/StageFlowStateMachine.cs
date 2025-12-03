using UnityEngine;
using Game.CoreSystem.Utility;
using Game.StageSystem.Manager;

namespace Game.StageSystem.State
{
    /// <summary>
    /// 스테이지 진행 상태를 관리하는 간단한 상태 머신(초기 뼈대).
    /// 현재는 이벤트에 반응하여 상태 전환만 수행하며,
    /// 실제 다음 적 소환/다음 스테이지 진행은 기존 StageManager 로직을 그대로 사용합니다.
    /// 추후 보상/스토리 연출 도입 시 본 상태에서 코루틴을 통해 흐름을 제어합니다.
    /// </summary>
    public class StageFlowStateMachine : MonoBehaviour
    {
        public enum StageFlowState
        {
            Combat,     // 전투 진행 중(기본)
            Reward,     // 보상 연출 대기(현재는 즉시 통과)
            Interlude,  // 스토리/컷씬(현재는 즉시 통과)
            Done        // 스테이지 종료
        }

        [SerializeField]
        private StageFlowState current = StageFlowState.Combat;

        private StageManager stageManager;

        public StageFlowState Current => current;

        /// <summary>
        /// 외부에서 StageManager를 연결합니다.
        /// </summary>
        public void Initialize(StageManager manager)
        {
            if (manager == null)
            {
                GameLogger.LogError("StageFlowStateMachine.Initialize: StageManager가 null 입니다.", GameLogger.LogCategory.Error);
                return;
            }

            // 기존 구독 해제 후 재구독(안전)
            if (stageManager != null)
            {
                stageManager.OnEnemyDefeated -= HandleEnemyDefeated;
                stageManager.OnStageCompleted -= HandleStageCompleted;
            }

            stageManager = manager;
            stageManager.OnEnemyDefeated += HandleEnemyDefeated;
            stageManager.OnStageCompleted += HandleStageCompleted;
            stageManager.OnRewardProcessCompleted += HandleRewardProcessCompleted;

            SetState(StageFlowState.Combat);
        }

        private void OnDestroy()
        {
            if (stageManager != null)
            {
                stageManager.OnEnemyDefeated -= HandleEnemyDefeated;
                stageManager.OnStageCompleted -= HandleStageCompleted;
                stageManager.OnRewardProcessCompleted -= HandleRewardProcessCompleted;
            }
        }

        #region 상태 전환

        private void SetState(StageFlowState next)
        {
            if (current == next) return;
            GameLogger.LogInfo($"[StageFlow] 상태 전환: {current} → {next}", GameLogger.LogCategory.Combat);
            current = next;
        }

        private void HandleEnemyDefeated(Game.CharacterSystem.Interface.ICharacter _)
        {
            // 보상 상태로 전환
            SetState(StageFlowState.Reward);
            // 보상 처리가 완료될 때까지 대기 (HandleRewardProcessCompleted에서 Combat으로 전환)
        }

        private void HandleRewardProcessCompleted()
        {
            // 보상 처리가 완료되었으므로 전투로 복귀
            SetState(StageFlowState.Combat);
        }

        private void HandleStageCompleted(Game.StageSystem.Data.StageData _)
        {
            // 스테이지 완료 시 인터루드로 전환 후 Done 처리(현재는 즉시 전환)
            SetState(StageFlowState.Interlude);
            // 추후: 스토리/컷씬 코루틴 완료 대기
            SetState(StageFlowState.Done);
        }

        #endregion
    }
}


