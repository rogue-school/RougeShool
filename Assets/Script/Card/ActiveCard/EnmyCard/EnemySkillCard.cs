using System.Collections.Generic;
using UnityEngine;
using Game.Interface;
using Game.Slots;
using Game.Effect;

namespace Game.Enemy
{
    /// <summary>
    /// 적이 사용하는 스킬 카드의 데이터 ScriptableObject입니다.
    /// 이름, 설명, 효과, 아트워크를 포함하며, 런타임에서 공격력과 슬롯 위치가 주입됩니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Card/Enemy Skill Card")]
    public class EnemySkillCard : ScriptableObject, ISkillCard
    {
        [Header("기본 카드 정보")]
        [SerializeField] private string cardName;
        [SerializeField] private string description;
        [SerializeField] private Sprite artwork;

        [Header("카드 효과")]
        [SerializeField] private List<ScriptableObject> effectObjects;

        // 런타임에서 주입되는 값
        private int power;

        // 슬롯 위치 정보
        private SkillCardSlotPosition? handSlot = null;
        private CombatSlotPosition? combatSlot = null;

        public string GetCardName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => 0; // 적은 쿨타임이 없거나 0 고정
        public int GetEffectPower(ICardEffect effect) => power;

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

        public void SetPower(int value) => power = value;

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;
    }
}
