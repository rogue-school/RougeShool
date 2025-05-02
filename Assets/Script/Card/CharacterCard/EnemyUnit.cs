using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// 적 유닛 클래스. CharacterCardData를 통해 초기화됩니다.
    /// </summary>
    public class EnemyUnit : Unit
    {
        public CharacterCardData characterData;

        private void Start()
        {
            if (characterData != null)
            {
                maxHP = characterData.maxHP;
                currentHP = maxHP;
                Debug.Log($"[적 유닛 초기화] {characterData.characterName} HP: {maxHP}");
            }
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
        }
    }
}
