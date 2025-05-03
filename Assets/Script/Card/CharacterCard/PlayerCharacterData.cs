using System.Collections.Generic;
using UnityEngine;
using Game.Cards;
using Game.Player;

namespace Game.Cards
{
    /// <summary>
    /// 플레이어 캐릭터의 정보와 사용 가능한 스킬 카드 목록입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Assets/Characters/Player Character")]
    public class PlayerCharacterData : ScriptableObject
    {
        public string characterName;
        public Sprite portrait;
        public int maxHP;

        [Header("사용 가능한 스킬 카드 목록")]
        public List<PlayerSkillCard> skillCards;
    }
}
