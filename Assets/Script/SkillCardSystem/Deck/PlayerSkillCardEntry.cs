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
    /// 플레이어의 스킬 카드와 해당 카드가 위치할 슬롯 정보를 담는 클래스입니다.
    /// 런타임 시 실행 가능한 인스턴스로 변환될 수 있습니다.
    /// </summary>
    [System.Serializable]
    public class PlayerSkillCardEntry
    {
        /// <summary>
        /// 이 카드가 위치할 핸드 슬롯 위치입니다.
        /// </summary>
        [field: SerializeField]
        public SkillCardSlotPosition Slot { get; private set; }

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
                Debug.LogWarning($"[PlayerSkillCardEntry] 런타임 변환 실패: Card 또는 CardData가 null입니다. Slot: {Slot}");
                return null;
            }

            return new PlayerSkillCardInstance(
                Card.CardData,
                Card.CreateEffects(),
                SlotOwner.PLAYER
            );
        }
    }
}
