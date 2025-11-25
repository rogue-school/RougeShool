using System;
using Game.Application.Services;
using Game.Domain.Combat.Entities;
using Game.Domain.Combat.Interfaces;
using Game.Domain.Combat.ValueObjects;

namespace Game.Application.Battle
{
    /// <summary>
    /// 현재 턴을 종료하고 다음 턴으로 진행하는 애플리케이션 유스케이스입니다.
    /// 도메인 턴/페이즈 정보를 기반으로 턴 전환을 수행합니다.
    /// </summary>
    public sealed class EndTurnUseCase
    {
        private readonly ITurnManager _turnManager;
        private readonly IEventBus _eventBus;

        /// <summary>
        /// 턴 종료 유스케이스를 생성합니다.
        /// </summary>
        /// <param name="turnManager">턴 관리 도메인 서비스</param>
        /// <param name="eventBus">턴 변경 이벤트를 발행할 이벤트 버스</param>
        /// <exception cref="ArgumentNullException">turnManager가 null인 경우</exception>
        public EndTurnUseCase(ITurnManager turnManager, IEventBus eventBus)
        {
            _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager), "턴 매니저는 null일 수 없습니다.");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus), "이벤트 버스는 null일 수 없습니다.");
        }

        /// <summary>
        /// 현재 턴을 종료하고 다음 턴을 시작합니다.
        /// </summary>
        /// <returns>시작된 다음 턴 정보</returns>
        /// <exception cref="InvalidOperationException">
        /// - 아직 시작된 턴이 없는 경우
        /// </exception>
        public Turn Execute()
        {
            if (_turnManager.CurrentTurnNumber <= 0)
            {
                throw new InvalidOperationException("현재 진행 중인 턴이 없어 종료할 수 없습니다.");
            }

            TurnType currentTurnType = _turnManager.CurrentTurnType;
            CombatPhase currentPhase = _turnManager.Phase;

            // 현재 턴을 완료 상태로 표시
            _turnManager.CompleteCurrentTurn();

            // 다음 턴 타입 계산
            TurnType nextTurnType = currentTurnType == TurnType.Player
                ? TurnType.Enemy
                : TurnType.Player;

            // 페이즈 전환 (플레이어 ↔ 적 턴 간 전환만 처리)
            CombatPhase nextPhase = currentPhase;
            if (currentPhase == CombatPhase.PlayerTurn && nextTurnType == TurnType.Enemy)
            {
                nextPhase = CombatPhase.EnemyTurn;
            }
            else if (currentPhase == CombatPhase.EnemyTurn && nextTurnType == TurnType.Player)
            {
                nextPhase = CombatPhase.PlayerTurn;
            }

            if (nextPhase != currentPhase)
            {
                _turnManager.ChangePhase(nextPhase);
            }

            // 다음 턴 시작
            Turn nextTurn = _turnManager.StartNextTurn(nextTurnType);

            // 턴 변경 이벤트 발행
            var changedEvent = new TurnChangedEvent(nextTurn.Number, nextTurn.TurnType, nextTurn.Phase);
            _eventBus.Publish(changedEvent);

            return nextTurn;
        }
    }
}


