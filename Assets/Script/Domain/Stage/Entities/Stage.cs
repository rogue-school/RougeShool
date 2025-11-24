using System;
using Game.Domain.Stage.Interfaces;
using Game.Domain.Stage.ValueObjects;

namespace Game.Domain.Stage.Entities
{
    /// <summary>
    /// 스테이지 도메인 엔티티입니다.
    /// </summary>
    public sealed class Stage : IStage
    {
        private readonly StageDefinition _definition;
        private int _currentEnemyIndex;

        /// <inheritdoc />
        public StageDefinition Definition => _definition;

        /// <inheritdoc />
        public StageProgressState ProgressState { get; private set; }

        /// <summary>
        /// 스테이지를 생성합니다.
        /// </summary>
        /// <param name="definition">스테이지 정의</param>
        public Stage(StageDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _currentEnemyIndex = 0;
            ProgressState = StageProgressState.NotStarted;
        }

        /// <inheritdoc />
        public bool HasNextEnemy()
        {
            return _definition.EnemyIds != null && _currentEnemyIndex < _definition.EnemyIds.Count;
        }

        /// <inheritdoc />
        public string PeekNextEnemyId()
        {
            if (!HasNextEnemy())
            {
                return null;
            }

            return _definition.EnemyIds[_currentEnemyIndex];
        }

        /// <inheritdoc />
        public void AdvanceToNextEnemy()
        {
            if (!HasNextEnemy())
            {
                throw new InvalidOperationException("더 이상 진행할 적이 없습니다.");
            }

            if (ProgressState == StageProgressState.NotStarted)
            {
                ProgressState = StageProgressState.InProgress;
            }

            _currentEnemyIndex++;

            if (!HasNextEnemy())
            {
                ProgressState = StageProgressState.Completed;
            }
        }
    }
}


