using UnityEngine;
using Game.CombatSystem.Manager;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 상태의 기본 인터페이스
    /// 모든 전투 상태는 이 인터페이스를 구현해야 합니다.
    /// </summary>
    public interface ICombatState
    {
        /// <summary>
        /// 상태의 고유 이름
        /// </summary>
        string StateName { get; }

        /// <summary>
        /// 상태 진입 시 호출
        /// </summary>
        /// <param name="context">전투 컨텍스트</param>
        void OnEnter(CombatStateContext context);

        /// <summary>
        /// 상태 실행 중 매 프레임 호출 (필요시 사용)
        /// </summary>
        /// <param name="context">전투 컨텍스트</param>
        void OnUpdate(CombatStateContext context);

        /// <summary>
        /// 상태 종료 시 호출
        /// </summary>
        /// <param name="context">전투 컨텍스트</param>
        void OnExit(CombatStateContext context);

        /// <summary>
        /// 플레이어 카드 드래그 가능 여부
        /// </summary>
        bool AllowPlayerCardDrag { get; }

        /// <summary>
        /// 적 카드 자동 실행 가능 여부
        /// </summary>
        bool AllowEnemyAutoExecution { get; }

        /// <summary>
        /// 슬롯 이동 가능 여부
        /// </summary>
        bool AllowSlotMovement { get; }

        /// <summary>
        /// 턴 전환 가능 여부
        /// </summary>
        bool AllowTurnSwitch { get; }
    }
}
