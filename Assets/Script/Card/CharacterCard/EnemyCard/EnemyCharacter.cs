using UnityEngine;
using Game.Cards;

namespace Game.Characters
{
    /// <summary>
    /// 적 캐릭터 클래스입니다.
    /// </summary>
    public class EnemyCharacter : CharacterBase
    {
        public EnemyCharacterData characterData;

        private void Awake()
        {
            maxHP = characterData.maxHP;
            currentHP = maxHP;
        }

        public override string GetName() => characterData.characterName;
        public override Sprite GetPortrait() => characterData.portrait;
    }
}
