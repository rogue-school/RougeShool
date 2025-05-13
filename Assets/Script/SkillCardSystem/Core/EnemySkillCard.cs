using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

namespace Game.SkillCardSystem.Core
{
    /// <summary>
    /// 적 스킬 카드의 순수 데이터 ScriptableObject입니다.
    /// 이름, 설명, 아트워크, 기본 파워, 이펙트 정보를 포함합니다.
    /// 런타임 상태 정보는 포함하지 않습니다.
    /// </summary>
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
        public SlotOwner GetOwner() => SlotOwner.ENEMY;

        /// <summary>
        /// 적 카드는 쿨타임이 없습니다 (항상 0 반환)
        /// </summary>
        public int GetCoolTime() => 0;

        public int GetEffectPower(ICardEffect effect) => basePower;

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

        // 슬롯 및 파워 변경은 런타임 카드에서 관리하므로 제거
        public void SetHandSlot(SkillCardSlotPosition slot) { }
        public SkillCardSlotPosition? GetHandSlot() => null;
        public void SetCombatSlot(CombatSlotPosition slot) { }
        public CombatSlotPosition? GetCombatSlot() => null;
    }
}
