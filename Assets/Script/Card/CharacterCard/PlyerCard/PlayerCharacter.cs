using UnityEngine;
using Game.Slots;
using Game.Battle;
using Game.Cards;
using Game.Player;

namespace Game.Characters
{
    public class PlayerCharacter : CharacterBase
    {
        [SerializeField] private PlayerCharacterData characterData;

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
    }
}
