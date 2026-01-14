using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;

namespace Game.SkillCardSystem.Deck
{
    /// <summary>
    /// 플레이어의 스킬 카드 덱을 정의하는 ScriptableObject입니다.
    /// 새로운 덱 기반 시스템: 미사용 보관함에서 카드를 드로우하여 핸드에 배치합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Decks/Player Skill Deck")]
    public class PlayerSkillDeck : ScriptableObject
    {
        [Header("플레이어 카드 덱 구성")]
        [Tooltip("플레이어가 사용할 스킬카드 정의와 수량을 정의합니다.")]
        [SerializeField] private List<CardEntry> cardEntries = new();

        /// <summary>
        /// 카드 엔트리 클래스 - 카드 정의와 수량을 포함합니다.
        /// </summary>
        [System.Serializable]
        public class CardEntry
        {
            [Header("카드 정의")]
            [Tooltip("스킬카드 정의")]
            public SkillCardDefinition cardDefinition;

            [Header("수량 설정")]
            [Tooltip("덱에 포함될 카드 수량 (개발자가 원하는 만큼 설정 가능)")]
            [Min(1)]
            public int quantity = 1;

            /// <summary>
            /// 카드 엔트리가 유효한지 확인합니다.
            /// </summary>
            public bool IsValid()
            {
                return cardDefinition != null && quantity > 0;
            }

            /// <summary>
            /// 카드 엔트리의 문자열 표현을 반환합니다.
            /// </summary>
            public override string ToString()
            {
                return cardDefinition != null ? $"{cardDefinition.displayName} x{quantity}" : "Invalid Entry";
            }
        }

        /// <summary>
        /// 모든 카드 정의를 수량만큼 반복하여 반환합니다.
        /// 미사용 보관함 초기화에 사용됩니다.
        /// </summary>
        public List<SkillCardDefinition> GetAllCards()
        {
            var allCards = new List<SkillCardDefinition>();
            
            foreach (var entry in cardEntries)
            {
                if (entry.IsValid())
                {
                    for (int i = 0; i < entry.quantity; i++)
                    {
                        allCards.Add(entry.cardDefinition);
                    }
                }
            }
            
            return allCards;
        }

        /// <summary>
        /// 덱의 총 카드 수를 반환합니다.
        /// </summary>
        public int TotalCardCount
        {
            get
            {
                int total = 0;
                foreach (var entry in cardEntries)
                {
                    if (entry.IsValid())
                    {
                        total += entry.quantity;
                    }
                }
                return total;
            }
        }

        /// <summary>
        /// 카드 엔트리 목록을 반환합니다.
        /// </summary>
        public List<CardEntry> CardEntries => new(cardEntries);

        /// <summary>
        /// 모든 카드 엔트리를 반환합니다. (동적 덱 관리용)
        /// </summary>
        public List<CardEntry> GetAllCardEntries()
        {
            return new List<CardEntry>(cardEntries);
        }

        /// <summary>
        /// 지정한 카드 정의의 수량을 반환합니다.
        /// </summary>
        /// <param name="cardDefinition">찾을 카드 정의</param>
        /// <returns>해당 카드의 수량</returns>
        public int GetCardQuantity(SkillCardDefinition cardDefinition)
        {
            foreach (var entry in cardEntries)
            {
                if (entry.cardDefinition == cardDefinition)
                {
                    return entry.quantity;
                }
            }
            return 0;
        }

        /// <summary>
        /// 덱 구성의 유효성을 검사합니다.
        /// </summary>
        /// <returns>유효한 덱인지 여부</returns>
        public bool IsValidDeck()
        {
            if (cardEntries == null || cardEntries.Count == 0)
            {
                Debug.LogWarning("[PlayerSkillDeck] 덱이 비어있습니다.");
                return false;
            }

            int validEntries = 0;
            foreach (var entry in cardEntries)
            {
                if (entry.IsValid())
                {
                    validEntries++;
                }
                else
                {
                    Debug.LogWarning($"[PlayerSkillDeck] 유효하지 않은 카드 엔트리: {entry}");
                }
            }

            if (validEntries == 0)
            {
                Debug.LogError("[PlayerSkillDeck] 유효한 카드 엔트리가 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 덱 정보를 로그로 출력합니다.
        /// </summary>
        [ContextMenu("덱 정보 출력")]
        public void LogDeckInfo()
        {
            Debug.Log($"[PlayerSkillDeck] 덱 이름: {name}");
            Debug.Log($"[PlayerSkillDeck] 총 카드 수: {TotalCardCount}");
            Debug.Log($"[PlayerSkillDeck] 카드 엔트리 수: {cardEntries.Count}");
            
            foreach (var entry in cardEntries)
            {
                if (entry.IsValid())
                {
                    Debug.Log($"  - {entry}");
                }
            }
        }
    }
}
