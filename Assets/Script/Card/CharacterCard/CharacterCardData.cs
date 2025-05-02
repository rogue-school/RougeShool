using UnityEngine;
using System.Collections.Generic;
using Game.Cards;

namespace Game.Units
{
    /// <summary>
    /// 유닛(플레이어 또는 몬스터)을 구성하는 캐릭터 카드 데이터입니다.
    /// 캐릭터 이름, HP, 덱, 초상화 등의 정보를 담습니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Character/Character Card")]
    public class CharacterCardData : ScriptableObject
    {
        public string characterName;                     // 캐릭터 이름 (예: 아케인, 슬라임)
        public int maxHP = 10;                           // 최대 체력
        public Sprite portrait;                          // 캐릭터 이미지 (선택)
        public List<PlayerCardData> initialDeck;         // 초기 핸드 또는 덱
    }
}
