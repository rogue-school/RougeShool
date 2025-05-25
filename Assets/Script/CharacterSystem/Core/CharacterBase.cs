using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.UI;
using Game.SkillCardSystem.Interface;

namespace Game.CharacterSystem.Core
{
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        protected int maxHP;
        protected int currentHP;
        protected int currentGuard;
        protected List<IPerTurnEffect> perTurnEffects = new();
        protected CharacterUIController characterCardUI;
        protected bool isGuarded = false;

        public virtual string GetCharacterName() => gameObject.name;
        public virtual int GetHP() => currentHP;
        public int GetCurrentHP() => currentHP;
        public virtual int GetMaxHP() => maxHP;
        public virtual Sprite GetPortrait() => null;

        public virtual void SetGuarded(bool value)
        {
            isGuarded = value;
            Debug.Log($"[{GetCharacterName()}] 가드 상태: {isGuarded}");
        }

        public virtual bool IsGuarded() => isGuarded;

        public virtual void GainGuard(int amount)
        {
            currentGuard += amount;
            Debug.Log($"[{GetCharacterName()}] 가드 +{amount} → 현재 가드: {currentGuard}");
        }

        public virtual void TakeDamage(int amount)
        {
            currentHP = Mathf.Max(currentHP - amount, 0);
            characterCardUI?.SetHP(currentHP, maxHP);
            Debug.Log($"[{GetCharacterName()}] 피해: -{amount}, 남은 체력: {currentHP}");

            if (IsDead())
                Die();
        }

        public virtual void Heal(int amount)
        {
            currentHP = Mathf.Min(currentHP + amount, maxHP);
            characterCardUI?.SetHP(currentHP, maxHP);
            Debug.Log($"[{GetCharacterName()}] 회복: +{amount}, 현재 체력: {currentHP}");
        }

        public virtual void SetMaxHP(int value)
        {
            maxHP = value;
            currentHP = maxHP;
            characterCardUI?.SetHP(currentHP, maxHP);
        }

        public virtual void SetCardUI(CharacterUIController ui)
        {
            characterCardUI = ui;
            characterCardUI?.Initialize(this);
        }

        public virtual void Die()
        {
            Debug.Log($"[{GetCharacterName()}] 사망 처리됨.");
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
                    perTurnEffects.Remove(effect);
            }
        }

        public virtual bool IsDead() => currentHP <= 0;
        public virtual bool IsAlive() => currentHP > 0;
    }
}
