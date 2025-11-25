using System;
using System.Collections.Generic;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Slot;
using Game.CoreSystem.Utility;
using Game.Domain.Combat.Interfaces;
using Game.Domain.Combat.ValueObjects;

namespace Game.Infrastructure.Combat
{
    /// <summary>
    /// 도메인 전투 실행 인터페이스를 Unity 전투 실행 매니저에 연결하는 어댑터입니다.
    /// </summary>
    public sealed class CombatExecutorAdapter : ICombatExecutor
    {
        private readonly CombatExecutionManager _executionManager;
        private readonly CombatSlotRegistry _combatSlotRegistry;
        private readonly Queue<(SlotPosition slot, TurnType owner)> _queue = new Queue<(SlotPosition slot, TurnType owner)>();

        private bool _isExecuting;

        /// <inheritdoc />
        public bool IsExecuting => _isExecuting;

        /// <summary>
        /// 전투 실행 어댑터를 생성합니다.
        /// </summary>
        /// <param name="executionManager">Unity 전투 실행 매니저</param>
        /// <param name="combatSlotRegistry">Unity 전투 슬롯 레지스트리</param>
        /// <exception cref="ArgumentNullException">필수 의존성이 null인 경우</exception>
        public CombatExecutorAdapter(
            CombatExecutionManager executionManager,
            CombatSlotRegistry combatSlotRegistry)
        {
            _executionManager = executionManager ?? throw new ArgumentNullException(nameof(executionManager), "CombatExecutionManager는 null일 수 없습니다.");
            _combatSlotRegistry = combatSlotRegistry ?? throw new ArgumentNullException(nameof(combatSlotRegistry), "CombatSlotRegistry는 null일 수 없습니다.");

            _executionManager.OnExecutionCompleted += OnExecutionCompleted;
        }

        /// <inheritdoc />
        public void ResetExecution()
        {
            _queue.Clear();
            _isExecuting = false;
        }

        /// <inheritdoc />
        public void EnqueueExecution(SlotPosition slot, TurnType owner)
        {
            _queue.Enqueue((slot, owner));
        }

        /// <inheritdoc />
        public void ProcessExecutionQueue()
        {
            if (_isExecuting)
            {
                return;
            }

            if (_queue.Count == 0)
            {
                return;
            }

            var (slot, _) = _queue.Dequeue();
            var combatPosition = ToCombatSlotPosition(slot);
            var uiSlot = _combatSlotRegistry.GetCombatSlot(combatPosition);

            if (uiSlot == null || uiSlot.IsEmpty())
            {
                GameLogger.LogWarning("[CombatExecutorAdapter] 실행할 카드가 없는 슬롯입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            var card = uiSlot.GetCard();
            if (card == null)
            {
                GameLogger.LogWarning("[CombatExecutorAdapter] 슬롯에 카드 인스턴스가 없습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            _isExecuting = true;
            _executionManager.ExecuteCardImmediately(card, combatPosition);
        }

        private void OnExecutionCompleted(Game.CombatSystem.Interface.ExecutionResult result)
        {
            _isExecuting = false;

            // 큐에 남은 요청이 있으면 이어서 처리
            if (_queue.Count > 0)
            {
                ProcessExecutionQueue();
            }
        }

        private static CombatSlotPosition ToCombatSlotPosition(SlotPosition position)
        {
            return position switch
            {
                SlotPosition.BattleSlot => CombatSlotPosition.BATTLE_SLOT,
                SlotPosition.WaitSlot1 => CombatSlotPosition.WAIT_SLOT_1,
                SlotPosition.WaitSlot2 => CombatSlotPosition.WAIT_SLOT_2,
                SlotPosition.WaitSlot3 => CombatSlotPosition.WAIT_SLOT_3,
                SlotPosition.WaitSlot4 => CombatSlotPosition.WAIT_SLOT_4,
                _ => CombatSlotPosition.NONE
            };
        }
    }
}


