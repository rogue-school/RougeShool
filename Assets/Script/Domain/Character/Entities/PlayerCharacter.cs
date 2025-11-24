using Game.Domain.Character.Interfaces;
using Game.Domain.Character.ValueObjects;

namespace Game.Domain.Character.Entities
{
    /// <summary>
    /// 플레이어 조작 캐릭터의 도메인 엔티티입니다.
    /// </summary>
    public sealed class PlayerCharacter : Character, IPlayerCharacter
    {
        /// <summary>
        /// 플레이어가 사용하는 리소스 정보입니다. (마나, 화살 등)
        /// </summary>
        public Resource Resource { get; private set; }

        /// <summary>
        /// 플레이어 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="id">캐릭터 ID</param>
        /// <param name="name">표시 이름</param>
        /// <param name="initialStats">초기 체력 정보</param>
        /// <param name="initialResource">초기 리소스 정보</param>
        public PlayerCharacter(
            string id,
            string name,
            CharacterStats initialStats,
            Resource initialResource)
            : base(id, name, initialStats)
        {
            Resource = initialResource;
        }

        /// <summary>
        /// 리소스를 소비합니다.
        /// </summary>
        /// <param name="amount">소비량 (1 이상)</param>
        public void ConsumeResource(int amount)
        {
            if (Resource == null)
            {
                return;
            }

            if (amount <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "리소스 소비량은 1 이상이어야 합니다.");
            }

            int newCurrent = System.Math.Max(Resource.CurrentAmount - amount, 0);
            Resource = Resource.WithCurrentAmount(newCurrent);
        }

        /// <summary>
        /// 리소스를 회복합니다.
        /// </summary>
        /// <param name="amount">회복량 (1 이상)</param>
        public void RestoreResource(int amount)
        {
            if (Resource == null)
            {
                return;
            }

            if (amount <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "리소스 회복량은 1 이상이어야 합니다.");
            }

            int newCurrent = System.Math.Min(Resource.CurrentAmount + amount, Resource.MaxAmount);
            Resource = Resource.WithCurrentAmount(newCurrent);
        }
    }
}


