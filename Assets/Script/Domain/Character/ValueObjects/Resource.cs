using System;

namespace Game.Domain.Character.ValueObjects
{
    /// <summary>
    /// 캐릭터가 사용하는 리소스(마나, 화살 등)를 표현하는 값 객체입니다.
    /// </summary>
    public sealed class Resource
    {
        /// <summary>
        /// 리소스의 표시 이름입니다. (예: "마나", "화살")
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 리소스의 최대 보유량입니다.
        /// </summary>
        public int MaxAmount { get; }

        /// <summary>
        /// 현재 리소스 보유량입니다.
        /// </summary>
        public int CurrentAmount { get; }

        /// <summary>
        /// 리소스 정보를 생성합니다.
        /// </summary>
        /// <param name="name">리소스 이름 (null 또는 공백 불가)</param>
        /// <param name="maxAmount">최대 보유량 (0 이상)</param>
        /// <param name="currentAmount">현재 보유량 (0 이상, 최대 보유량 이하)</param>
        public Resource(string name, int maxAmount, int currentAmount)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("리소스 이름은 null이거나 공백일 수 없습니다.", nameof(name));
            }

            if (maxAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxAmount),
                    maxAmount,
                    "리소스 최대 보유량은 0 이상이어야 합니다.");
            }

            if (currentAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentAmount),
                    currentAmount,
                    "리소스 현재 보유량은 0 이상이어야 합니다.");
            }

            if (currentAmount > maxAmount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentAmount),
                    currentAmount,
                    "리소스 현재 보유량은 최대 보유량을 초과할 수 없습니다.");
            }

            Name = name;
            MaxAmount = maxAmount;
            CurrentAmount = currentAmount;
        }

        /// <summary>
        /// 현재 보유량을 변경한 새로운 <see cref="Resource"/> 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="newCurrentAmount">새로운 현재 보유량</param>
        public Resource WithCurrentAmount(int newCurrentAmount)
        {
            return new Resource(Name, MaxAmount, newCurrentAmount);
        }

        /// <summary>
        /// 최대 보유량과 현재 보유량을 함께 변경한 새로운 <see cref="Resource"/> 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="newMaxAmount">새로운 최대 보유량</param>
        /// <param name="newCurrentAmount">새로운 현재 보유량</param>
        public Resource WithAmounts(int newMaxAmount, int newCurrentAmount)
        {
            return new Resource(Name, newMaxAmount, newCurrentAmount);
        }
    }
}


