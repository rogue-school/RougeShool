using UnityEngine;
using Game.Battle;
using Game.Cards;
using Game.Characters;
using Game.Slots;
using Game.Interface;
using Game.Managers;

namespace Game.UI
{
    /// <summary>
    /// 플레이어가 사용하는 전투 카드 슬롯 UI
    /// </summary>
    public class PlayerCardSlotUI : BaseCardSlotUI
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

                caster = PlayerManager.Instance.GetPlayer() as ICharacter;
                target = EnemyManager.Instance.GetRandomEnemy();

                Debug.Log($"[PlayerCardSlotUI] 위치 바인딩 완료 - {Position}");
            }
            else
            {
                Debug.LogWarning("[PlayerCardSlotUI] SlotAnchor가 누락되었습니다.");
            }
        }

        public override void ExecuteCardAutomatically()
        {
            if (card == null || caster == null || target == null)
            {
                Debug.LogWarning("[PlayerCardSlotUI] 카드, 캐스터 또는 타겟이 누락되었습니다.");
                return;
            }

            CardExecutor.Execute(card, caster, target);
            Debug.Log($"[PlayerCardSlotUI] 카드 실행됨 - {card.GetCardName()} → {target.GetName()}");
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
