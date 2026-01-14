using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 실행 관리자 인터페이스
    /// </summary>
    public interface ICombatExecutionManager
    {
        /// <summary>
        /// 카드 즉시 실행
        /// </summary>
        /// <param name="card">실행할 카드</param>
        /// <param name="slotPosition">슬롯 위치</param>
        void ExecuteCardImmediately(ISkillCard card, CombatSlotPosition slotPosition);

        /// <summary>
        /// 슬롯 이동 (새로운 5슬롯 시스템)
        /// </summary>
        void MoveSlotsForwardNew();

        /// <summary>
        /// 슬롯 이동 (레거시 4슬롯 시스템)
        /// </summary>
        void MoveSlotsForward();

        /// <summary>
        /// 실행 명령을 큐에 추가
        /// </summary>
        /// <param name="command">실행 명령</param>
        void QueueExecution(ExecutionCommand command);

        /// <summary>
        /// 큐의 모든 실행 명령 처리
        /// </summary>
        void ProcessExecutionQueue();

        /// <summary>
        /// 실행 리셋
        /// </summary>
        void ResetExecution();

        /// <summary>
        /// 실행 중 상태
        /// </summary>
        bool IsExecuting { get; }

        /// <summary>
        /// 초기화 상태
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 큐 크기
        /// </summary>
        int QueueCount { get; }

        /// <summary>
        /// 카드 실행 이벤트
        /// </summary>
        System.Action<ISkillCard, ICharacter, ICharacter> OnCardExecuted { get; set; }

        /// <summary>
        /// 실행 완료 이벤트
        /// </summary>
        System.Action<ExecutionResult> OnExecutionCompleted { get; set; }
    }
}
