using UnityEngine;
using Game.Slots;
using Game.Battle;
using Game.Cards;
using Game.Enemy;
using Game.Interface;

namespace Game.Characters
{
    /// <summary>
    /// 전투 중 등장하는 적 캐릭터. CharacterBase를 상속하고, ICharacter 인터페이스를 구현합니다.
    /// </summary>
    public class EnemyCharacter : CharacterBase, ICharacter
    {
        [SerializeField] private EnemyCharacterData characterData;

        public override BattleSlotPosition BattleSlotPosition => characterData.battleSlotPosition;
        public override string characterName => characterData.characterName;
        public override Sprite portrait => characterData.portrait;

        public override string GetName() => characterName;
        public override Sprite GetPortrait() => portrait;

        public override void Initialize(int hp)
        {
            base.Initialize(hp);
            maxHP = characterData.maxHP;
            currentHP = maxHP;
        }

        public int GetMaxHP() => maxHP;
        public int GetCurrentHP() => currentHP;
    }
}
