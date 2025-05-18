using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.UI;
using Game.SkillCardSystem.Interface;

namespace Game.CharacterSystem.Core
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

        // HP UI를 갱신할 UI 참조 (연결 필수)
        protected CharacterUIController characterCardUI;


        public virtual string GetName() => gameObject.name;

        public virtual int GetHP() => currentHP;

        public int GetCurrentHP() => currentHP;

        public virtual int GetMaxHP() => maxHP;

        public virtual Sprite GetPortrait() => null;

        /// <summary>
        /// 체력을 감소시키고 UI를 갱신합니다.
        /// </summary>
        public virtual void TakeDamage(int amount)
        {
            currentHP = Mathf.Max(currentHP - amount, 0);
            characterCardUI?.SetHP(currentHP, maxHP);

            Debug.Log($"{GetName()} 피해: -{amount}, 남은 체력: {currentHP}");

            if (IsDead())
                Die();
        }

        /// <summary>
        /// 체력을 회복하고 UI를 갱신합니다.
        /// </summary>
        public virtual void Heal(int amount)
        {
            currentHP = Mathf.Min(currentHP + amount, maxHP);
            characterCardUI?.SetHP(currentHP, maxHP);

            Debug.Log($"{GetName()} 회복: +{amount}, 현재 체력: {currentHP}");
        }

        /// <summary>
        /// 최대 체력 설정 및 UI 초기화.
        /// </summary>
        public virtual void SetMaxHP(int value)
        {
            maxHP = value;
            currentHP = maxHP;
            characterCardUI?.SetHP(currentHP, maxHP);
        }

        /// <summary>
        /// UI 참조를 연결합니다.
        /// </summary>
        public virtual void SetCardUI(CharacterUIController ui)
        {
            characterCardUI = ui;
            characterCardUI?.Initialize(this);
        }


        public virtual void Die()
        {
            Debug.Log($"{GetName()} 사망 처리됨.");
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

        /// <summary>
        /// 캐릭터가 사망했는지 여부를 판단합니다. 이후 부활 처리 등 커스터마이징 시 활용.
        /// </summary>
        public virtual bool IsDead()
        {
            return currentHP <= 0;
        }
    }
}
