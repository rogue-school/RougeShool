using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.SkillCardSystem.Core
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

        public string GetCardName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => 0;
        public int GetEffectPower(ICardEffect effect) => basePower;
        public SlotOwner GetOwner() => SlotOwner.ENEMY;

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

        public void SetHandSlot(SkillCardSlotPosition slot) { }
        public SkillCardSlotPosition? GetHandSlot() => null;
        public void SetCombatSlot(CombatSlotPosition slot) { }
        public CombatSlotPosition? GetCombatSlot() => null;

        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            Debug.LogWarning("[EnemySkillCard] ScriptableObject는 직접 실행되지 않습니다.");
        }

        public ICharacter GetOwner(ICardExecutionContext context)
        {
            Debug.LogWarning("[EnemySkillCard] GetOwner는 런타임 카드에서 호출되어야 합니다.");
            return null;
        }

        public ICharacter GetTarget(ICardExecutionContext context)
        {
            Debug.LogWarning("[EnemySkillCard] GetTarget는 런타임 카드에서 호출되어야 합니다.");
            return null;
        }
    }
}
