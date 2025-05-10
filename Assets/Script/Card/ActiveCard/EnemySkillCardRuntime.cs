using UnityEngine;
using System.Collections.Generic;
using Game.Interface;
using Game.Slots;
using Game.Effect;
using Game.Enemy;

namespace Game.Cards
{
    /// <summary>
    /// 런타임에서 사용하는 적의 스킬 카드 인스턴스입니다.
    /// ScriptableObject에서 정보만 복사하며, 파워와 슬롯 정보는 독립적으로 유지합니다.
    /// </summary>
    public class EnemySkillCardRuntime : ISkillCard
    {
        private EnemySkillCard baseCard;
        private int power;
        private CombatSlotPosition? combatSlot;
        public SlotOwner GetOwner() => SlotOwner.ENEMY;

        public EnemySkillCardRuntime(EnemySkillCard baseCard, int power)
        {
            this.baseCard = baseCard;
            this.power = power;
        }

        public string GetCardName() => baseCard.GetCardName();
        public string GetDescription() => baseCard.GetDescription();
        public Sprite GetArtwork() => baseCard.GetArtwork();
        public int GetCoolTime() => 0;
        public int GetEffectPower(ICardEffect effect) => power;
        public List<ICardEffect> CreateEffects() => baseCard.CreateEffects();

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        public void SetHandSlot(SkillCardSlotPosition slot) { }
        public SkillCardSlotPosition? GetHandSlot() => null;
    }
}
