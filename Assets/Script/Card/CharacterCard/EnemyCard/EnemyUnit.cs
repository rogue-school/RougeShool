using UnityEngine;
using Game.Cards;

namespace Game.Units
{
    public class EnemyUnit : Unit
    {
        public EnemyCharacterData characterData;

        private void Awake()
        {
            currentHP = characterData.maxHP;
        }

        public override string GetName() => characterData.characterName;
        public override Sprite GetPortrait() => characterData.portrait;
        public override int GetMaxHP() => characterData.maxHP;
    }
}

