using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effect;
using Game.SkillCardSystem.Core;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Deck
{
    /// <summary>
    /// 플레이어의 스킬 카드와 해당 카드의 개수 정보를 담는 클래스입니다.
    /// 순환 시스템에서 자동으로 슬롯에 배치되며, 런타임 시 실행 가능한 인스턴스로 변환될 수 있습니다.
    /// </summary>
    [System.Serializable]
    public class PlayerSkillCardEntry
    {
        /// <summary>
        /// 이 카드의 개수입니다. 순환 시스템에서 이 개수만큼 미사용 보관함에 추가됩니다.
        /// </summary>
        [field: SerializeField]
        [Tooltip("이 카드의 개수 (순환 시스템에서 미사용 보관함에 추가될 개수)")]
        public int Count { get; private set; } = 1;

        /// <summary>
        /// (기존) 플레이어용 스킬 카드 ScriptableObject입니다.
        /// </summary>
        [field: SerializeField]
        public PlayerSkillCard Card { get; private set; }

        /// <summary>
        /// (신규) 공용 카드 정의를 직접 참조합니다. 설정 시 Card 대신 본 정의를 우선 사용합니다.
        /// </summary>
        [field: SerializeField]
        public SkillCardDefinition Definition { get; private set; }

        /// <summary>
        /// 카드 이름을 반환합니다.
        /// </summary>
        /// <returns>카드가 null이면 "Unknown" 반환</returns>
        public string GetCardName() => Definition != null ? Definition.displayNameKO : (Card != null ? Card.name : "Unknown");

        /// <summary>
        /// 카드에 등록된 효과 목록을 생성합니다.
        /// </summary>
        /// <returns>SkillCardEffectSO 리스트 (없으면 null 또는 빈 리스트)</returns>
        public List<SkillCardEffectSO> CreateEffects() => Card?.CreateEffects();

        /// <summary>
        /// 공용 정의 기반으로 카드를 생성합니다. 정의가 없으면 null 반환.
        /// </summary>
        public ISkillCard CreateFromDefinition(ISkillCardFactory factory, string ownerCharacterName)
        {
            if (Definition == null || factory == null) return null;
            return factory.CreateFromDefinition(Definition, Game.SkillCardSystem.Data.Owner.Player, ownerCharacterName);
        }

        /// <summary>
        /// 이 카드 엔트리를 실행 가능한 런타임 인스턴스로 변환합니다.
        /// </summary>
        /// <returns>PlayerSkillCardInstance 객체</returns>
        public PlayerSkillCardInstance ToRuntimeInstance()
        {
            if (Card == null || Card.CardData == null)
            {
                Debug.LogWarning($"[PlayerSkillCardEntry] 런타임 변환 실패: Card 또는 CardData가 null입니다.");
                return null;
            }

            return new PlayerSkillCardInstance(
                Card.CardData,
                Card.CreateEffects(),
                SlotOwner.PLAYER
            );
        }

        /// <summary>
        /// 카드 개수만큼 순환 시스템용 카드 리스트를 생성합니다.
        /// </summary>
        /// <param name="factory">카드 팩토리</param>
        /// <param name="ownerCharacterName">소유자 캐릭터 이름</param>
        /// <returns>생성된 카드 리스트</returns>
        public List<ISkillCard> CreateCardsForCirculation(ISkillCardFactory factory, string ownerCharacterName)
        {
            var cards = new List<ISkillCard>();
            
            if (Definition != null && factory != null)
            {
                // Definition 기반으로 카드 생성
                for (int i = 0; i < Count; i++)
                {
                    var card = factory.CreateFromDefinition(Definition, Game.SkillCardSystem.Data.Owner.Player, ownerCharacterName);
                    if (card != null)
                    {
                        cards.Add(card);
                    }
                }
            }
            else if (Card != null)
            {
                // 기존 Card 기반으로 카드 생성
                for (int i = 0; i < Count; i++)
                {
                    var runtimeInstance = ToRuntimeInstance();
                    if (runtimeInstance != null)
                    {
                        cards.Add(runtimeInstance);
                    }
                }
            }
            
            return cards;
        }
    }
}
