using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effects;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Slot;

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
        /// 플레이어용 스킬 카드 ScriptableObject입니다.
        /// </summary>
        [field: SerializeField]
        public PlayerSkillCard Card { get; private set; }

        /// <summary>
        /// 카드 이름을 반환합니다.
        /// </summary>
        /// <returns>카드가 null이면 "Unknown" 반환</returns>
        public string GetCardName() => Card != null ? Card.name : "Unknown";

        /// <summary>
        /// 카드에 등록된 효과 목록을 생성합니다.
        /// </summary>
        /// <returns>SkillCardEffectSO 리스트 (없으면 null 또는 빈 리스트)</returns>
        public List<SkillCardEffectSO> CreateEffects() => Card?.CreateEffects();

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
