using UnityEngine;
using System.Collections.Generic;
using Game.Interface;
using Game.Effect;

namespace Game.Characters
{
    /// <summary>
    /// 플레이어 및 적 캐릭터의 공통 기반 클래스입니다.
    /// 체력, 이름, 상태이펙트 등의 공통 속성을 포함합니다.
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        protected int maxHP;
        protected int currentHP;

        protected List<IPerTurnEffect> perTurnEffects = new();

        public virtual string GetName() => gameObject.name;

        public virtual int GetHP() => currentHP;

        public int GetCurrentHP() => currentHP;

        public virtual int GetMaxHP() => maxHP;

        public virtual Sprite GetPortrait() => null; // 자식 클래스에서 override 가능

        public virtual void TakeDamage(int amount)
        {
            currentHP = Mathf.Max(currentHP - amount, 0);
            Debug.Log($"{GetName()} 피해: -{amount}, 남은 체력: {currentHP}");

            if (currentHP <= 0)
                Die();
        }

        public virtual void Heal(int amount)
        {
            currentHP = Mathf.Min(currentHP + amount, maxHP);
            Debug.Log($"{GetName()} 회복: +{amount}, 현재 체력: {currentHP}");
        }

        public virtual void Die()
        {
            Debug.Log($"{GetName()} 사망 처리됨.");
        }

        public virtual void SetMaxHP(int value)
        {
            maxHP = value;
            currentHP = maxHP;
        }

        public virtual void RegisterPerTurnEffect(IPerTurnEffect effect)
        {
            if (!perTurnEffects.Contains(effect))
            {
                perTurnEffects.Add(effect);
            }
        }

        public virtual void ProcessTurnEffects()
        {
            foreach (var effect in perTurnEffects.ToArray())
            {
                effect.OnTurnStart(this);

                if (effect.IsExpired)
                {
                    perTurnEffects.Remove(effect);
                }
            }
        }

        public bool IsDead()
        {
            return currentHP <= 0;
        }
    }
}
