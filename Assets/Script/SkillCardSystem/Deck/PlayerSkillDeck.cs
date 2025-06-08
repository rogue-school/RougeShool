using UnityEngine;
using System.Collections.Generic;

namespace Game.SkillCardSystem.Deck
{
    /// <summary>
    /// 플레이어의 스킬 카드 덱을 정의하는 ScriptableObject입니다.
    /// 각 카드와 해당 카드의 슬롯 위치 정보를 포함합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Decks/Player Skill Deck")]
    public class PlayerSkillDeck : ScriptableObject
    {
        [Header("플레이어 카드 덱 (순서 고정 및 슬롯 지정)")]
        [Tooltip("각 카드와 해당 카드가 배치될 슬롯 정보를 포함합니다.")]
        [SerializeField] private List<PlayerSkillCardEntry> cards = new();

        /// <summary>
        /// 카드 덱의 복사본을 반환합니다.
        /// 외부에서 리스트를 수정할 수 없습니다.
        /// </summary>
        public List<PlayerSkillCardEntry> Cards => new(cards);

        /// <summary>
        /// 카드 덱의 전체 개수를 반환합니다.
        /// </summary>
        public int Count => cards?.Count ?? 0;

        /// <summary>
        /// 지정한 인덱스에 위치한 카드 엔트리를 반환합니다.
        /// </summary>
        /// <param name="index">카드 인덱스</param>
        /// <returns>엔트리 객체 또는 범위 오류 시 null</returns>
        public PlayerSkillCardEntry GetEntryAt(int index)
        {
            if (index < 0 || index >= Count)
                return null;

            return cards[index];
        }

        /// <summary>
        /// 카드 덱 전체를 복사하여 반환합니다.
        /// </summary>
        /// <returns>카드 엔트리 리스트</returns>
        public List<PlayerSkillCardEntry> GetCards()
        {
            return Cards;
        }
    }
}
