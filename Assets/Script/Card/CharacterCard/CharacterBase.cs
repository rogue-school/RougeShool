using UnityEngine;
using Game.Interface;

namespace Game.Characters
{
    /// <summary>
    /// 플레이어와 적 캐릭터의 공통 전투 기능을 담당하는 추상 클래스입니다.
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        public int maxHP = 20;
        public int currentHP;

        public virtual void TakeDamage(int damage)
        {
            currentHP -= damage;
            Debug.Log($"{name}이(가) {damage}의 피해를 받았습니다. 남은 체력: {currentHP}");
        }

        public virtual void Heal(int amount)
        {
            currentHP += amount;
            if (currentHP > maxHP) currentHP = maxHP;
        }

        public virtual int GetCurrentHP() => currentHP;
        public virtual int GetMaxHP() => maxHP;

        public abstract string GetName();
        public abstract Sprite GetPortrait();
    }
}
