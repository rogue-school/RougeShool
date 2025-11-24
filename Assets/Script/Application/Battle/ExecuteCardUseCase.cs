using System;
using Game.Domain.Combat.Entities;
using Game.Domain.Combat.Interfaces;
using Game.Domain.Combat.ValueObjects;

namespace Game.Application.Battle
{
    /// <summary>
    /// 특정 슬롯에 있는 카드를 실행 큐에 등록하고 처리하는 애플리케이션 유스케이스입니다.
    /// 도메인 슬롯/턴 상태를 기반으로 전투 실행을 조정합니다.
    /// </summary>
    public sealed class ExecuteCardUseCase
    {
        private readonly ISlotRegistry _slotRegistry;
        private readonly ICombatExecutor _combatExecutor;

        /// <summary>
        /// 카드 실행 유스케이스를 생성합니다.
        /// </summary>
        /// <param name="slotRegistry">전투 슬롯 상태를 조회하기 위한 레지스트리</param>
        /// <param name="combatExecutor">전투 실행 도메인 서비스</param>
        /// <exception cref="ArgumentNullException">
        /// slotRegistry 또는 combatExecutor가 null인 경우
        /// </exception>
        public ExecuteCardUseCase(ISlotRegistry slotRegistry, ICombatExecutor combatExecutor)
        {
            _slotRegistry = slotRegistry ?? throw new ArgumentNullException(nameof(slotRegistry), "슬롯 레지스트리는 null일 수 없습니다.");
            _combatExecutor = combatExecutor ?? throw new ArgumentNullException(nameof(combatExecutor), "전투 실행기는 null일 수 없습니다.");
        }

        /// <summary>
        /// 지정된 슬롯에 있는 카드를 현재 턴 소유자 기준으로 실행합니다.
        /// </summary>
        /// <param name="slotPosition">실행할 슬롯 위치</param>
        /// <param name="owner">카드 소유자 턴 타입</param>
        /// <exception cref="InvalidOperationException">
        /// - 이미 전투 실행이 진행 중인 경우
        /// - 슬롯에 실행할 카드가 없는 경우
        /// </exception>
        public void Execute(SlotPosition slotPosition, TurnType owner)
        {
            if (_combatExecutor.IsExecuting)
            {
                throw new InvalidOperationException("전투 실행이 이미 진행 중입니다.");
            }

            CombatSlot slot = _slotRegistry.GetSlot(slotPosition);
            if (slot == null || slot.IsEmpty)
            {
                throw new InvalidOperationException("해당 슬롯에 실행할 카드가 없습니다.");
            }

            _combatExecutor.EnqueueExecution(slotPosition, owner);
            _combatExecutor.ProcessExecutionQueue();
        }
    }
}


