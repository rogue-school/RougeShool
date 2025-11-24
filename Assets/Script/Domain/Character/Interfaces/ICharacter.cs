using System;
using Game.Domain.Character.ValueObjects;

namespace Game.Domain.Character.Interfaces
{
    /// <summary>
    /// 캐릭터의 핵심 비즈니스 로직을 정의하는 도메인 인터페이스입니다.
    /// Unity 컴포넌트나 UI에 의존하지 않습니다.
    /// </summary>
    public interface ICharacter
    {
        /// <summary>
        /// 캐릭터를 고유하게 식별하는 ID입니다.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 화면 등에 표시할 캐릭터 이름입니다.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 현재 캐릭터의 체력 정보입니다.
        /// </summary>
        CharacterStats Stats { get; }

        /// <summary>
        /// 현재 가드 상태인지 여부입니다.
        /// </summary>
        bool IsGuarded { get; }

        /// <summary>
        /// 캐릭터가 사망 상태인지 여부입니다.
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// 체력이 변경되었을 때 발생하는 이벤트입니다.
        /// </summary>
        event Action<CharacterStats> HealthChanged;

        /// <summary>
        /// 가드 상태가 변경되었을 때 발생하는 이벤트입니다.
        /// </summary>
        event Action<bool> GuardStateChanged;

        /// <summary>
        /// 가드를 고려하여 피해를 적용합니다.
        /// </summary>
        /// <param name="amount">피해량 (1 이상)</param>
        void TakeDamage(int amount);

        /// <summary>
        /// 가드를 무시하고 피해를 적용합니다.
        /// </summary>
        /// <param name="amount">피해량 (1 이상)</param>
        void TakeDamageIgnoreGuard(int amount);

        /// <summary>
        /// 캐릭터의 체력을 회복합니다.
        /// </summary>
        /// <param name="amount">회복량 (1 이상)</param>
        void Heal(int amount);

        /// <summary>
        /// 가드 상태를 설정합니다.
        /// </summary>
        /// <param name="isGuarded">true이면 가드 상태, false이면 비가드 상태입니다.</param>
        void SetGuarded(bool isGuarded);
    }
}


