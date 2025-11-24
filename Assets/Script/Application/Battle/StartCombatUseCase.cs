using System;
using Game.Domain.Combat.Interfaces;
using Game.Domain.Combat.ValueObjects;
using Game.Domain.Stage.Interfaces;

namespace Game.Application.Battle
{
    /// <summary>
    /// 전투를 시작하는 애플리케이션 유스케이스입니다.
    /// 
    /// 책임:
    /// - 현재 스테이지를 도메인 관점에서 시작 상태로 전환합니다.
    /// - 턴 매니저를 통해 전투를 활성화하고 첫 턴을 설정합니다.
    /// - UI, 이펙트, 씬 전환 등 프레젠테이션/인프라 의존성은 포함하지 않습니다.
    /// </summary>
    public sealed class StartCombatUseCase
    {
        private readonly ITurnManager _turnManager;

        /// <summary>
        /// 전투 시작 유스케이스를 생성합니다.
        /// </summary>
        /// <param name="turnManager">턴 관리 도메인 서비스</param>
        /// <exception cref="ArgumentNullException">turnManager가 null인 경우</exception>
        public StartCombatUseCase(ITurnManager turnManager)
        {
            _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager), "턴 매니저는 null일 수 없습니다.");
        }

        /// <summary>
        /// 전투를 시작하고 첫 턴을 설정합니다.
        /// </summary>
        /// <param name="stage">현재 진행할 스테이지 도메인 객체</param>
        /// <param name="firstTurnType">첫 턴의 주체 (플레이어/적)</param>
        /// <exception cref="ArgumentNullException">stage가 null인 경우</exception>
        /// <exception cref="InvalidOperationException">
        /// - 전투가 이미 활성화된 경우
        /// - 스테이지가 이미 시작되었거나 완료된 상태에서 다시 시작하려는 경우
        /// </exception>
        public void Execute(IStage stage, TurnType firstTurnType)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage), "스테이지는 null일 수 없습니다.");
            }

            if (_turnManager.IsGameActive)
            {
                throw new InvalidOperationException("전투가 이미 진행 중이므로 다시 시작할 수 없습니다.");
            }

            // 스테이지를 '진행 중' 상태로 전환
            stage.Start();

            // 턴 시스템 활성화 및 첫 턴 설정
            _turnManager.StartGame();
            _turnManager.SetTurn(1, firstTurnType);
        }
    }
}


