using System;
using Game.Domain.Combat.Entities;
using Game.Domain.Combat.Interfaces;
using Game.Domain.Combat.ValueObjects;

namespace Game.Domain.Combat.Services
{
    /// <summary>
    /// 도메인 전투 턴과 페이즈 상태를 관리하는 기본 구현체입니다.
    /// </summary>
    public sealed class DomainTurnManager : ITurnManager
    {
        private int _currentTurnNumber;
        private TurnType _currentTurnType;
        private CombatPhase _currentPhase;
        private bool _isTurnActive;

        /// <inheritdoc />
        public int CurrentTurnNumber => _currentTurnNumber;

        /// <inheritdoc />
        public TurnType CurrentTurnType => _currentTurnType;

        /// <inheritdoc />
        public CombatPhase Phase => _currentPhase;

        /// <summary>
        /// 전투 턴 매니저를 생성합니다.
        /// </summary>
        public DomainTurnManager()
        {
            _currentTurnNumber = 0;
            _currentTurnType = TurnType.Player;
            _currentPhase = CombatPhase.None;
            _isTurnActive = false;
        }

        /// <inheritdoc />
        public Turn StartNextTurn(TurnType turnType)
        {
            if (!Enum.IsDefined(typeof(TurnType), turnType))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(turnType),
                    turnType,
                    "유효하지 않은 턴 타입입니다.");
            }

            if (_isTurnActive)
            {
                throw new InvalidOperationException("이전 턴이 아직 완료되지 않았습니다.");
            }

            _currentTurnNumber++;
            _currentTurnType = turnType;
            _isTurnActive = true;

            return new Turn(_currentTurnNumber, _currentTurnType, _currentPhase, false);
        }

        /// <inheritdoc />
        public void CompleteCurrentTurn()
        {
            if (_currentTurnNumber <= 0)
            {
                throw new InvalidOperationException("시작된 턴이 없어 완료할 수 없습니다.");
            }

            if (!_isTurnActive)
            {
                throw new InvalidOperationException("현재 진행 중인 턴이 없어 완료할 수 없습니다.");
            }

            _isTurnActive = false;
        }

        /// <inheritdoc />
        public void ChangePhase(CombatPhase nextPhase)
        {
            if (!Enum.IsDefined(typeof(CombatPhase), nextPhase))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(nextPhase),
                    nextPhase,
                    "유효하지 않은 전투 페이즈입니다.");
            }

            _currentPhase = nextPhase;
        }
    }
}


