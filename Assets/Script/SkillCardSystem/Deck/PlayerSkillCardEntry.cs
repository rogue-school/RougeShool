using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effects;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Slot;

namespace Game.SkillCardSystem.Deck
{
    [System.Serializable]
    public class PlayerSkillCardEntry
    {
        [field: SerializeField] public SkillCardSlotPosition Slot { get; private set; }
        [field: SerializeField] public PlayerSkillCard Card { get; private set; }

        public string GetCardName() => Card != null ? Card.name : "Unknown";
        public List<SkillCardEffectSO> CreateEffects() => Card?.CreateEffects();

        /// <summary>
        /// 런타임 실행용 인스턴스를 생성합니다.
        /// </summary>
        public PlayerSkillCardInstance ToRuntimeInstance()
        {
            return new PlayerSkillCardInstance(Card.CardData, Card.CreateEffects(), SlotOwner.PLAYER);
        }
    }
}
