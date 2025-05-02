using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// 플레이어 유닛 클래스. CharacterCardData를 통해 초기화됩니다.
    /// 후공 디버프 등의 상태도 이곳에 포함됩니다.
    /// </summary>
    public class PlayerUnit : Unit
    {
        public CharacterCardData characterData;

        [Header("후공 디버프")]
        public bool isForcedRearNextTurn = false;

        private void Start()
        {
            if (characterData != null)
            {
                maxHP = characterData.maxHP;
                currentHP = maxHP;
                Debug.Log($"[플레이어 유닛 초기화] {characterData.characterName} HP: {maxHP}");
            }
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
        }
    }
}
