using System;
using Game.Application.Services;
using Game.Domain.Combat.Interfaces;
using Game.Domain.Combat.ValueObjects;
using Game.Domain.Stage.Interfaces;

namespace Game.Application.Battle
{
    /// <summary>
    /// 전투를 시작하는 애플리케이션 유스케이스입니다.
    ///
    /// 책임:
    /// - 현재 스테이지가 전투를 시작할 수 있는 상태인지 검증합니다.
    /// - 턴 매니저를 통해 첫 턴과 페이즈를 설정합니다.
    /// - UI, 이펙트, 씬 전환 등 프레젠테이션/인프라 의존성은 포함하지 않습니다.
    /// </summary>
    public sealed class StartCombatUseCase
    {
        private readonly ITurnManager _turnManager;
        private readonly IEventBus _eventBus;

        /// <summary>
        /// 전투 시작 유스케이스를 생성합니다.
        /// </summary>
        /// <param name="turnManager">턴 관리 도메인 서비스</param>
        /// <param name="eventBus">전투 시작 이벤트를 발행할 이벤트 버스</param>
        /// <exception cref="ArgumentNullException">turnManager가 null인 경우</exception>
        public StartCombatUseCase(ITurnManager turnManager, IEventBus eventBus)
        {
            _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager), "턴 매니저는 null일 수 없습니다.");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus), "이벤트 버스는 null일 수 없습니다.");
        }

        /// <summary>
        /// 전투를 시작하고 첫 턴을 설정합니다.
        /// </summary>
        /// <param name="stage">현재 진행할 스테이지 도메인 객체</param>
        /// <param name="firstTurnType">첫 턴의 주체 (플레이어/적)</param>
        /// <exception cref="ArgumentNullException">stage가 null인 경우</exception>
        /// <exception cref="InvalidOperationException">
        /// - 전투가 이미 진행 중인 경우
        /// - 스테이지에 진행할 적이 없는 경우
        /// </exception>
        public void Execute(IStage stage, TurnType firstTurnType)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage), "스테이지는 null일 수 없습니다.");
            }

            // 이미 턴이 시작되었다면 전투가 진행 중인 것으로 간주합니다.
            if (_turnManager.CurrentTurnNumber > 0)
            {
                throw new InvalidOperationException("전투가 이미 진행 중이므로 다시 시작할 수 없습니다.");
            }

            if (!stage.HasNextEnemy())
            {
                throw new InvalidOperationException("스테이지에 진행할 적이 없습니다.");
            }

            // 첫 페이즈 및 첫 턴 설정
            var firstPhase = firstTurnType == TurnType.Player
                ? CombatPhase.PlayerTurn
                : CombatPhase.EnemyTurn;

            _turnManager.ChangePhase(firstPhase);
            var firstTurn = _turnManager.StartNextTurn(firstTurnType);

            // 전투 시작 이벤트 발행
            var startedEvent = new CombatStartedEvent(firstTurn.Number, firstTurn.TurnType);
            _eventBus.Publish(startedEvent);
        }
    }
}


