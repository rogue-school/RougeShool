using System.Collections.Generic;
using UnityEngine;
using Game.Interface;

namespace Game.Characters
{
    /// <summary>
    /// 캐릭터의 기본 속성 및 기능을 정의한 베이스 클래스입니다.
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour
    {
        [SerializeField] protected int currentHP;
        [SerializeField] protected int maxHP;

        // 턴마다 실행되는 지속 효과 리스트
        private readonly List<IPerTurnEffect> perTurnEffects = new();

        /// <summary>
        /// 현재 체력을 반환합니다.
        /// </summary>
        public int GetCurrentHP() => currentHP;

        /// <summary>
        /// 최대 체력을 반환합니다.
        /// </summary>
        public int GetMaxHP() => maxHP;

        /// <summary>
        /// 캐릭터가 피해를 받습니다.
        /// </summary>
        public virtual void TakeDamage(int value)
        {
            currentHP -= value;
            currentHP = Mathf.Max(currentHP, 0);
        }

        /// <summary>
        /// 캐릭터의 체력을 회복합니다.
        /// </summary>
        public virtual void Heal(int value)
        {
            currentHP += value;
            currentHP = Mathf.Min(currentHP, maxHP);
        }

        /// <summary>
        /// 캐릭터를 초기화합니다.
        /// </summary>
        public virtual void Initialize(int hp)
        {
            maxHP = hp;
            currentHP = maxHP;
        }

        /// <summary>
        /// 캐릭터가 사망했는지 여부를 반환합니다.
        /// </summary>
        public bool IsDead => currentHP <= 0;

        /// <summary>
        /// 지속 효과를 등록합니다.
        /// </summary>
        public void RegisterPerTurnEffect(IPerTurnEffect effect)
        {
            perTurnEffects.Add(effect);
        }

        /// <summary>
        /// 모든 지속 효과를 한 턴 실행합니다.
        /// </summary>
        public void TriggerPerTurnEffects()
        {
            foreach (var effect in perTurnEffects.ToArray())
            {
                effect.OnTurnStart(this);
                if (effect.IsExpired || IsDead)
                    perTurnEffects.Remove(effect);
            }
        }

        /// <summary>
        /// 이름을 반환합니다 (자식 클래스에서 구현).
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// 초상화 이미지를 반환합니다 (자식 클래스에서 구현).
        /// </summary>
        public abstract Sprite GetPortrait();
    }
}
