using System.Collections.Generic;
using UnityEngine;
using Game.Interface;
using Game.Slots;
using Game.Effect;

namespace Game.Player
{
    /// <summary>
    /// 플레이어가 사용하는 스킬 카드의 데이터 ScriptableObject입니다.
    /// 기본 정보(이름, 설명, 쿨타임, 효과)를 포함하고, 런타임에서 슬롯 위치 및 쿨타임 상태를 관리합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Card/Player Skill Card")]
    public class PlayerSkillCard : ScriptableObject, ISkillCard
    {
        [Header("기본 카드 정보")]
        [SerializeField] private string cardName;
        [SerializeField] private string description;
        [SerializeField] private Sprite artwork;
        [SerializeField] private int coolTime;

        [Header("카드 효과")]
        [SerializeField] private List<ScriptableObject> effectObjects;

        private int power;
        private int currentCoolTime = 0;
        public SlotOwner GetOwner() => SlotOwner.PLAYER;

        private SkillCardSlotPosition? handSlot = null;
        private CombatSlotPosition? combatSlot = null;

        public string GetCardName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => coolTime;
        public int GetCurrentCoolTime() => currentCoolTime;
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

        public void SetCoolTime(int value) => coolTime = value;
        public void SetPower(int value) => power = value;

        public void SetHandSlot(SkillCardSlotPosition slot) => handSlot = slot;
        public SkillCardSlotPosition? GetHandSlot() => handSlot;

        public void SetCombatSlot(CombatSlotPosition slot) => combatSlot = slot;
        public CombatSlotPosition? GetCombatSlot() => combatSlot;

        /// <summary>
        /// 쿨타임 시작 (스킬 사용 시 호출)
        /// </summary>
        public void ActivateCoolTime()
        {
            currentCoolTime = coolTime;
        }

        /// <summary>
        /// 쿨타임 1 감소 (턴 종료 시 호출)
        /// </summary>
        public void TickCoolDown()
        {
            if (currentCoolTime > 0)
                currentCoolTime--;
        }

        /// <summary>
        /// 현재 쿨타임 중인지 확인
        /// </summary>
        public bool IsOnCoolDown() => currentCoolTime > 0;
    }
}
