using Game.Domain.Combat.ValueObjects;

namespace Game.Domain.Combat.Interfaces
{
    /// <summary>
    /// 전투 실행(카드 효과 적용 등)을 담당하는 도메인 인터페이스입니다.
    /// </summary>
    public interface ICombatExecutor
    {
        /// <summary>
        /// 현재 전투가 실행 중인지 여부입니다.
        /// </summary>
        bool IsExecuting { get; }

        /// <summary>
        /// 전투 실행 큐를 초기화합니다.
        /// </summary>
        void ResetExecution();

        /// <summary>
        /// 슬롯과 소유자 정보를 기반으로 실행 요청을 큐에 추가합니다.
        /// 실제 카드/이펙트 정보는 상위 레이어에서 해석합니다.
        /// </summary>
        /// <param name="slot">실행할 슬롯 위치</param>
        /// <param name="owner">카드 소유자 턴 타입</param>
        void EnqueueExecution(SlotPosition slot, TurnType owner);

        /// <summary>
        /// 큐에 쌓인 실행 요청을 처리합니다.
        /// </summary>
        void ProcessExecutionQueue();
    }
}


