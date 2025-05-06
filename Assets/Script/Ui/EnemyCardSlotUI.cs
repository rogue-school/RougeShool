using UnityEngine;
using Game.Battle;
using Game.Cards;
using Game.Characters;
using Game.Interface;
using Game.Managers;
using Game.Slots;

namespace Game.UI
{
    /// <summary>
    /// 적 캐릭터가 사용하는 전투 카드 슬롯 UI
    /// </summary>
    public class EnemyCardSlotUI : BaseCardSlotUI
    {
        private ISkillCard card;

        private void Awake()
        {
            AutoBind();
        }

        public override void AutoBind()
        {
            SlotAnchor anchor = GetComponent<SlotAnchor>();
            if (anchor != null)
            {
                Position = anchor.battleSlotPosition;
                caster = EnemyManager.Instance.GetEnemyBySlot(Position);
                target = PlayerManager.Instance.GetPlayer() as ICharacter;
            }
        }

        public override void ExecuteCardAutomatically()
        {
            if (card == null || caster == null || target == null)
            {
                Debug.LogWarning("[EnemyCardSlotUI] 카드 실행 불가: 카드 또는 캐릭터가 null");
                return;
            }

            CardExecutor.Execute(card, caster, target);
        }

        public void SetCard(ISkillCard card)
        {
            this.card = card;
        }

        public void Clear()
        {
            this.card = null;
        }

        public ISkillCard GetCard()
        {
            return this.card;
        }
    }
}
