using Game.Interface;
using Game.Slots;
using Game.Effect;
using Game.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// 런타임에서 사용되는 플레이어 스킬 카드 인스턴스입니다.
    /// </summary>
    public class PlayerSkillCardRuntime : ISkillCard
    {
        private PlayerSkillCard baseCard;
        private int power;
        private int coolTime;
        public SlotOwner GetOwner() => SlotOwner.PLAYER;

        private SkillCardSlotPosition? handSlot;
        private CombatSlotPosition? combatSlot;

        public PlayerSkillCardRuntime(PlayerSkillCard card, int power, int coolTime)
        {
            this.baseCard = card;
            this.power = power;
            this.coolTime = coolTime;
        }

        public string GetCardName() => baseCard.GetCardName();
        public string GetDescription() => baseCard.GetDescription();
        public Sprite GetArtwork() => baseCard.GetArtwork();
        public int GetCoolTime() => coolTime;
        public int GetEffectPower(ICardEffect effect) => power;
        public List<ICardEffect> CreateEffects() => baseCard.CreateEffects();

        public void TickCoolTime() => coolTime = Mathf.Max(0, coolTime - 1);

        /// <summary>
        /// 카드가 사용되었을 때 쿨타임을 시작합니다.
        /// </summary>
        public void ActivateCoolTime()
        {
            coolTime = baseCard.GetCoolTime();
        }

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;
    }
}
