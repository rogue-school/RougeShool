using System;
using Game.Domain.Character.Interfaces;
using Game.Domain.Character.ValueObjects;

namespace Game.Domain.Character.Entities
{
    /// <summary>
    /// 캐릭터의 기본 도메인 엔티티입니다.
    /// Unity 컴포넌트와 분리된 순수 비즈니스 로직을 담습니다.
    /// </summary>
    public abstract class Character : ICharacter
    {
        private CharacterStats _stats;
        private bool _isGuarded;

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public CharacterStats Stats => _stats;

        /// <inheritdoc />
        public bool IsGuarded => _isGuarded;

        /// <inheritdoc />
        public bool IsDead => _stats.IsDead;

        /// <inheritdoc />
        public event Action<CharacterStats> HealthChanged;

        /// <inheritdoc />
        public event Action<bool> GuardStateChanged;

        /// <summary>
        /// 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="id">캐릭터 고유 ID</param>
        /// <param name="name">표시 이름</param>
        /// <param name="initialStats">초기 체력 정보</param>
        /// <exception cref="ArgumentException">ID 또는 이름이 유효하지 않은 경우</exception>
        /// <exception cref="ArgumentNullException">초기 체력 정보가 null인 경우</exception>
        protected Character(string id, string name, CharacterStats initialStats)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("캐릭터 ID는 null이거나 공백일 수 없습니다.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("캐릭터 이름은 null이거나 공백일 수 없습니다.", nameof(name));
            }

            _stats = initialStats ?? throw new ArgumentNullException(nameof(initialStats));

            Id = id;
            Name = name;
        }

        /// <inheritdoc />
        public void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "피해량은 1 이상이어야 합니다.");
            }

            if (_isGuarded)
            {
                // 가드 상태에서는 피해를 받지 않습니다.
                return;
            }

            ApplyDamageInternal(amount);
        }

        /// <inheritdoc />
        public void TakeDamageIgnoreGuard(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "피해량은 1 이상이어야 합니다.");
            }

            ApplyDamageInternal(amount);
        }

        /// <inheritdoc />
        public void Heal(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "회복량은 1 이상이어야 합니다.");
            }

            if (_stats.IsDead)
            {
                // 사망한 캐릭터는 회복하지 않습니다.
                return;
            }

            int newCurrent = Math.Min(_stats.CurrentHealth + amount, _stats.MaxHealth);
            UpdateStats(_stats.WithCurrentHealth(newCurrent));
        }

        /// <inheritdoc />
        public void SetGuarded(bool isGuarded)
        {
            if (_isGuarded == isGuarded)
            {
                return;
            }

            _isGuarded = isGuarded;
            GuardStateChanged?.Invoke(_isGuarded);
        }

        private void ApplyDamageInternal(int amount)
        {
            if (_stats.IsDead)
            {
                return;
            }

            int newCurrent = Math.Max(_stats.CurrentHealth - amount, 0);
            UpdateStats(_stats.WithCurrentHealth(newCurrent));
        }

        private void UpdateStats(CharacterStats newStats)
        {
            _stats = newStats ?? throw new ArgumentNullException(nameof(newStats));
            HealthChanged?.Invoke(_stats);
        }
    }
}


