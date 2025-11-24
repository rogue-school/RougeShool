using System;
using System.Collections.Generic;
using Game.Domain.Character.ValueObjects;

namespace Game.Domain.Stage.ValueObjects
{
    /// <summary>
    /// 스테이지의 정적 정의 정보를 나타내는 값 객체입니다.
    /// </summary>
    public sealed class StageDefinition
    {
        /// <summary>
        /// 스테이지 ID입니다.
        /// </summary>
        public StageId Id { get; }

        /// <summary>
        /// 스테이지 번호입니다.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// 스테이지 이름입니다.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 스테이지 설명입니다.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 스테이지 완료 시 자동으로 다음 스테이지로 진행할지 여부입니다.
        /// </summary>
        public bool AutoProgressToNext { get; }

        /// <summary>
        /// 이 스테이지에 등장하는 적들의 ID 목록입니다.
        /// </summary>
        public IReadOnlyList<string> EnemyIds { get; }

        /// <summary>
        /// 스테이지 정의를 생성합니다.
        /// </summary>
        /// <param name="id">스테이지 ID</param>
        /// <param name="number">스테이지 번호 (1 이상)</param>
        /// <param name="name">스테이지 이름</param>
        /// <param name="description">스테이지 설명</param>
        /// <param name="autoProgressToNext">자동 진행 여부</param>
        /// <param name="enemyIds">적 ID 목록</param>
        public StageDefinition(
            StageId id,
            int number,
            string name,
            string description,
            bool autoProgressToNext,
            IReadOnlyList<string> enemyIds)
        {
            if (number <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(number),
                    number,
                    "스테이지 번호는 1 이상이어야 합니다.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("스테이지 이름은 null이거나 공백일 수 없습니다.", nameof(name));
            }

            if (enemyIds == null || enemyIds.Count == 0)
            {
                throw new ArgumentException("스테이지에는 최소 1개 이상의 적이 필요합니다.", nameof(enemyIds));
            }

            Id = id;
            Number = number;
            Name = name;
            Description = description ?? string.Empty;
            AutoProgressToNext = autoProgressToNext;
            EnemyIds = enemyIds;
        }
    }
}


