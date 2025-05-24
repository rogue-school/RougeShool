using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.UI;
using Game.SkillCardSystem.Interface;

namespace Game.CharacterSystem.Core
{
    /// <summary>
    /// 플레이어 및 적 캐릭터의 공통 기반 클래스입니다.
    /// 체력, 이름, 상태 이펙트, UI 연동 등의 기능을 제공합니다.
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        /// <summary>
        /// 최대 체력 수치입니다.
        /// </summary>
        protected int maxHP;

        /// <summary>
        /// 현재 체력 수치입니다.
        /// </summary>
        protected int currentHP;

        /// <summary>
        /// 매 턴마다 처리되는 효과 목록입니다.
        /// </summary>
        protected List<IPerTurnEffect> perTurnEffects = new();

        /// <summary>
        /// 체력 UI를 제어하는 컴포넌트입니다.
        /// </summary>
        protected CharacterUIController characterCardUI;

        protected bool isGuarded = false;


        public virtual void SetGuarded(bool value)
        {
            isGuarded = value;
            Debug.Log($"[{GetCharacterName()}] 가드 상태: {isGuarded}");
        }
        public virtual bool IsGuarded() => isGuarded;

        /// <summary>
        /// 캐릭터의 이름을 반환합니다.
        /// </summary>
        public virtual string GetCharacterName() => gameObject.name;

        /// <summary>
        /// 현재 체력 수치를 반환합니다.
        /// </summary>
        public virtual int GetHP() => currentHP;

        /// <summary>
        /// 현재 체력을 반환합니다.
        /// </summary>
        public int GetCurrentHP() => currentHP;

        /// <summary>
        /// 최대 체력을 반환합니다.
        /// </summary>
        public virtual int GetMaxHP() => maxHP;

        /// <summary>
        /// 캐릭터의 초상화 스프라이트를 반환합니다 (하위 클래스에서 재정의 예상).
        /// </summary>
        public virtual Sprite GetPortrait() => null;

        /// <summary>
        /// 데미지를 입히고 체력 UI를 갱신합니다.
        /// </summary>
        public virtual void TakeDamage(int amount)
        {
            currentHP = Mathf.Max(currentHP - amount, 0);
            characterCardUI?.SetHP(currentHP, maxHP);

            Debug.Log($"[{GetCharacterName()}] 피해: -{amount}, 남은 체력: {currentHP}");

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

            Debug.Log($"[{GetCharacterName()}] 회복: +{amount}, 현재 체력: {currentHP}");
        }

        /// <summary>
        /// 최대 체력 설정 및 현재 체력 초기화, UI 동기화 처리.
        /// </summary>
        public virtual void SetMaxHP(int value)
        {
            maxHP = value;
            currentHP = maxHP;
            characterCardUI?.SetHP(currentHP, maxHP);
        }

        /// <summary>
        /// UI 컴포넌트를 외부에서 주입받아 연결합니다.
        /// </summary>
        public virtual void SetCardUI(CharacterUIController ui)
        {
            characterCardUI = ui;
            characterCardUI?.Initialize(this);
        }

        /// <summary>
        /// 캐릭터가 사망했을 때 호출됩니다. (오버라이딩 가능)
        /// </summary>
        public virtual void Die()
        {
            Debug.Log($"[{GetCharacterName()}] 사망 처리됨.");
        }

        /// <summary>
        /// 매 턴 적용되는 효과를 등록합니다.
        /// </summary>
        public virtual void RegisterPerTurnEffect(IPerTurnEffect effect)
        {
            if (!perTurnEffects.Contains(effect))
            {
                perTurnEffects.Add(effect);
            }
        }

        /// <summary>
        /// 모든 등록된 턴 효과를 처리하고, 만료된 효과를 제거합니다.
        /// </summary>
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
        /// 현재 캐릭터가 사망 상태인지 여부를 반환합니다.
        /// </summary>
        public virtual bool IsDead() => currentHP <= 0;
    }
}
