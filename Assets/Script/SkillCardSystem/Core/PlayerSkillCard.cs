using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.SkillCardSystem.Core
{
    /// <summary>
    /// ScriptableObject 기반의 플레이어 스킬 카드 데이터입니다.
    /// 런타임에서 실행되지는 않으며, 정보 제공용으로만 사용됩니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Card/Player Skill Card")]
    public class PlayerSkillCard : ScriptableObject, ISkillCard
    {
        [Header("기본 카드 데이터")]
        [SerializeField] private string cardName;
        [SerializeField] private string description;
        [SerializeField] private Sprite artwork;
        [SerializeField] private int coolTime;
        [SerializeField] private int power;

        [Header("카드 효과 (ICardEffect ScriptableObject 연결)")]
        [SerializeField] private List<ScriptableObject> effectObjects;

        private int currentCoolTime = 0;
        private SkillCardSlotPosition? handSlot = null;
        private CombatSlotPosition? combatSlot = null;

        public string GetCardName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => coolTime;
        public int GetCurrentCoolTime() => currentCoolTime;
        public int GetEffectPower(ICardEffect effect) => power;
        public SlotOwner GetOwner() => SlotOwner.PLAYER;

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

        public void SetCoolTime(int value) => coolTime = value;
        public void SetPower(int value) => power = value;

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        public void ActivateCoolTime() => currentCoolTime = coolTime;
        public void TickCoolDown() => currentCoolTime = Mathf.Max(0, currentCoolTime - 1);
        public bool IsOnCoolDown() => currentCoolTime > 0;

        /// <summary>
        /// ScriptableObject는 직접 실행되지 않도록 방지 로그 출력
        /// </summary>
        public void ExecuteCardAutomatically(ICardExecutionContext context)
        {
            Debug.LogWarning("[PlayerSkillCard] ScriptableObject는 직접 실행되지 않습니다.");
        }

        public ICharacter GetOwner(ICardExecutionContext context)
        {
            Debug.LogWarning("[PlayerSkillCard] GetOwner는 런타임 카드에서 호출되어야 합니다.");
            return null;
        }

        public ICharacter GetTarget(ICardExecutionContext context)
        {
            Debug.LogWarning("[PlayerSkillCard] GetTarget는 런타임 카드에서 호출되어야 합니다.");
            return null;
        }
    }
}
