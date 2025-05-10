using System.Collections.Generic;
using UnityEngine;
using Game.Interface;
using Game.Slots;
using Game.Effect;

namespace Game.Cards
{
    /// <summary>
    /// 전투 중 생성되는 실시간 스킬 카드입니다.
    /// PlayerSkillCard 또는 EnemySkillCard를 기반으로 만들어지며,
    /// 개별 수치(파워, 쿨타임)와 슬롯 정보를 포함합니다.
    /// </summary>
    public class RuntimeSkillCard : ISkillCard
    {
        private string cardName;
        private string description;
        private Sprite artwork;
        private int power;
        private int coolTime;
        private List<ICardEffect> effects;
        private SlotOwner owner;

        private SkillCardSlotPosition? handSlot = null;
        private CombatSlotPosition? combatSlot = null;

        /// <summary>
        /// 런타임 스킬 카드 생성자
        /// </summary>
        public RuntimeSkillCard(
            string name,
            string desc,
            Sprite art,
            List<ICardEffect> effects,
            int power,
            int coolTime,
            SlotOwner owner
        )
        {
            this.cardName = name;
            this.description = desc;
            this.artwork = art;
            this.effects = effects;
            this.power = power;
            this.coolTime = coolTime;
            this.owner = owner;
        }

        public string GetCardName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => coolTime;
        public int GetEffectPower(ICardEffect effect) => power;
        public List<ICardEffect> CreateEffects() => effects;
        public SlotOwner GetOwner() => owner;

        public void SetPower(int value) => power = value;

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;
    }
}
