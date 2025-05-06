using UnityEngine;
using System.Collections.Generic;
using Game.Interface;
using Game.Slots;
using Game.Battle;

namespace Game.Characters
{
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        [SerializeField] protected int currentHP;
        [SerializeField] protected int maxHP;

        protected List<IPerTurnEffect> perTurnEffects = new();

        public virtual int CurrentHP => currentHP;
        public virtual int MaxHP => maxHP;
        public virtual bool IsDead => currentHP <= 0;

        public abstract BattleSlotPosition BattleSlotPosition { get; }

        public abstract string characterName { get; }
        public abstract Sprite portrait { get; }

        public abstract string GetName();
        public abstract Sprite GetPortrait();

        public virtual int GetMaxHP() => maxHP;
        public virtual int GetCurrentHP() => currentHP;

        public virtual void Initialize(int hp)
        {
            maxHP = hp;
            currentHP = hp;
            perTurnEffects.Clear();
        }

        public virtual void TakeDamage(int amount)
        {
            currentHP -= amount;
            currentHP = Mathf.Max(0, currentHP);
            Debug.Log($"[{characterName}] 피해 {amount} → 현재 체력: {currentHP}");

            if (IsDead)
                OnDeath();
        }

        public virtual void Heal(int amount)
        {
            currentHP += amount;
            currentHP = Mathf.Min(currentHP, maxHP);
            Debug.Log($"[{characterName}] 회복 {amount} → 현재 체력: {currentHP}");
        }

        public virtual void RegisterPerTurnEffect(IPerTurnEffect effect)
        {
            perTurnEffects.Add(effect);
            Debug.Log($"[{characterName}] 지속 효과 등록됨: {effect.GetType().Name}");
        }

        public virtual void ExecutePerTurnEffects()
        {
            for (int i = perTurnEffects.Count - 1; i >= 0; i--)
            {
                var effect = perTurnEffects[i];
                effect.OnTurnStart(this);

                if (effect.IsExpired)
                {
                    Debug.Log($"[{characterName}] 지속 효과 종료: {effect.GetType().Name}");
                    perTurnEffects.RemoveAt(i);
                }
            }
        }

        protected virtual void OnDeath()
        {
            Debug.Log($"[{characterName}] 사망했습니다.");
        }
    }
}
