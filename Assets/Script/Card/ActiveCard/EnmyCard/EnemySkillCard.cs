using System.Collections.Generic;
using UnityEngine;
using Game.Interface;
using Game.Slots;
using Game.Effect;

namespace Game.Enemy
{
    [CreateAssetMenu(menuName = "Game/Card/Enemy Skill Card")]
    public class EnemySkillCard : ScriptableObject, ISkillCard
    {
        [Header("기본 카드 정보")]
        [SerializeField] private string cardName;
        [SerializeField] private string description;
        [SerializeField] private Sprite artwork;
        [SerializeField] private int basePower;

        [Header("카드 효과")]
        [SerializeField] private List<ScriptableObject> effectObjects;

        private SkillCardSlotPosition? handSlot = null;
        private CombatSlotPosition? combatSlot = null;

        public string GetCardName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => 0;
        public int GetEffectPower(ICardEffect effect) => basePower;
        public void SetPower(int value) => basePower = value;

        public List<ICardEffect> CreateEffects()
        {
            var list = new List<ICardEffect>();
            foreach (var obj in effectObjects)
            {
                if (obj is ICardEffect effect)
                    list.Add(effect);
            }
            return list;
        }

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;
    }
}
