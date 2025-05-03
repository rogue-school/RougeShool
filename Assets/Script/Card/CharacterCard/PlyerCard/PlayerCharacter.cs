using UnityEngine;
using Game.Cards;

namespace Game.Characters
{
    /// <summary>
    /// 선택된 플레이어 캐릭터를 초기화하고 전투 기능을 제공합니다.
    /// </summary>
    public class PlayerCharacter : CharacterBase
    {
        public PlayerCharacterData characterData;

        /// <summary>
        /// 외부에서 선택된 캐릭터 데이터를 주입받아 초기화합니다.
        /// </summary>
        public void Initialize(PlayerCharacterData data)
        {
            characterData = data;
            maxHP = characterData.maxHP;
            currentHP = maxHP;
        }

        public override string GetName() => characterData.characterName;
        public override Sprite GetPortrait() => characterData.portrait;
    }
}
