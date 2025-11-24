using System;

namespace Game.Domain.Card.ValueObjects
{
    /// <summary>
    /// 카드의 전투 관련 수치를 나타내는 값 객체입니다.
    /// </summary>
    public sealed class CardStats
    {
        /// <summary>
        /// 기본 데미지입니다.
        /// </summary>
        public int BaseDamage { get; }

        /// <summary>
        /// 공격 횟수입니다.
        /// </summary>
        public int Hits { get; }

        /// <summary>
        /// 가드를 무시하는지 여부입니다.
        /// </summary>
        public bool IgnoreGuard { get; }

        /// <summary>
        /// 반격 버프를 무시하는지 여부입니다.
        /// </summary>
        public bool IgnoreCounter { get; }

        /// <summary>
        /// 카드 사용 시 소모되는 리소스 양입니다.
        /// </summary>
        public int ResourceCost { get; }

        /// <summary>
        /// 카드 전투 수치를 생성합니다.
        /// </summary>
        /// <param name="baseDamage">기본 데미지 (0 이상)</param>
        /// <param name="hits">공격 횟수 (1 이상)</param>
        /// <param name="ignoreGuard">가드 무시 여부</param>
        /// <param name="ignoreCounter">반격 무시 여부</param>
        /// <param name="resourceCost">리소스 소모량 (0 이상)</param>
        public CardStats(
            int baseDamage,
            int hits,
            bool ignoreGuard,
            bool ignoreCounter,
            int resourceCost)
        {
            if (baseDamage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(baseDamage),
                    baseDamage,
                    "기본 데미지는 0 이상이어야 합니다.");
            }

            if (hits <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hits),
                    hits,
                    "공격 횟수는 1 이상이어야 합니다.");
            }

            if (resourceCost < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resourceCost),
                    resourceCost,
                    "리소스 소모량은 0 이상이어야 합니다.");
            }

            BaseDamage = baseDamage;
            Hits = hits;
            IgnoreGuard = ignoreGuard;
            IgnoreCounter = ignoreCounter;
            ResourceCost = resourceCost;
        }

        /// <summary>
        /// 리소스 소모량만 변경한 새 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="newCost">새 리소스 소모량</param>
        public CardStats WithResourceCost(int newCost)
        {
            return new CardStats(BaseDamage, Hits, IgnoreGuard, IgnoreCounter, newCost);
        }
    }
}


