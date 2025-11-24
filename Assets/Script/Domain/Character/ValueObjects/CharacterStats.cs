using System;

namespace Game.Domain.Character.ValueObjects
{
    /// <summary>
    /// 캐릭터의 체력 정보를 표현하는 값 객체입니다.
    /// </summary>
    public sealed class CharacterStats
    {
        /// <summary>
        /// 캐릭터의 최대 체력입니다.
        /// </summary>
        public int MaxHealth { get; }

        /// <summary>
        /// 캐릭터의 현재 체력입니다.
        /// </summary>
        public int CurrentHealth { get; }

        /// <summary>
        /// 캐릭터가 사망 상태인지 여부입니다.
        /// </summary>
        public bool IsDead => CurrentHealth <= 0;

        /// <summary>
        /// 캐릭터 체력 값을 생성합니다.
        /// </summary>
        /// <param name="maxHealth">최대 체력 (1 이상)</param>
        /// <param name="currentHealth">현재 체력 (0 이상, 최대 체력 이하)</param>
        /// <exception cref="ArgumentOutOfRangeException">입력 값이 허용 범위를 벗어난 경우</exception>
        public CharacterStats(int maxHealth, int currentHealth)
        {
            if (maxHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHealth),
                    maxHealth,
                    "최대 체력은 1 이상이어야 합니다.");
            }

            if (currentHealth < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentHealth),
                    currentHealth,
                    "현재 체력은 0 이상이어야 합니다.");
            }

            if (currentHealth > maxHealth)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentHealth),
                    currentHealth,
                    "현재 체력은 최대 체력을 초과할 수 없습니다.");
            }

            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
        }

        /// <summary>
        /// 현재 체력을 변경한 새로운 <see cref="CharacterStats"/> 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="newCurrentHealth">새로운 현재 체력 (0 이상, 최대 체력 이하)</param>
        /// <returns>변경된 체력 값을 가진 새 인스턴스</returns>
        public CharacterStats WithCurrentHealth(int newCurrentHealth)
        {
            return new CharacterStats(MaxHealth, newCurrentHealth);
        }

        /// <summary>
        /// 최대 체력과 현재 체력을 함께 변경한 새로운 <see cref="CharacterStats"/> 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="newMaxHealth">새로운 최대 체력 (1 이상)</param>
        /// <param name="newCurrentHealth">새로운 현재 체력 (0 이상, 새로운 최대 체력 이하)</param>
        /// <returns>변경된 체력 값을 가진 새 인스턴스</returns>
        public CharacterStats WithHealth(int newMaxHealth, int newCurrentHealth)
        {
            return new CharacterStats(newMaxHealth, newCurrentHealth);
        }
    }
}


