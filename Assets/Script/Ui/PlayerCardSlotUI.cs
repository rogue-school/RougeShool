using UnityEngine;
using Game.Cards;
using Game.Slots;
using Game.Interface;
using Game.Managers;

namespace Game.UI
{
    /// <summary>
    /// 플레이어가 사용하는 스킬 카드 핸드 슬롯 UI
    /// </summary>
    public class PlayerCardSlotUI : BaseCardSlotUI
    {
        private ISkillCard card;

        /// <summary>
        /// 핸드 슬롯 위치 (PLAYER_SLOT_1, PLAYER_SLOT_2, ...)
        /// </summary>
        public SkillCardSlotPosition Position { get; private set; }

        private void Awake()
        {
            AutoBind();
        }

        public override void AutoBind()
        {
            SlotAnchor anchor = GetComponent<SlotAnchor>();
            if (anchor != null)
            {
                Position = anchor.skillCardSlotPosition;

                caster = PlayerManager.Instance.GetPlayer() as ICharacter;
                target = EnemyManager.Instance.GetCurrentEnemy(); // 구조상 단일 적만 존재

                Debug.Log($"[PlayerCardSlotUI] 슬롯 자동 바인딩 완료 - {Position}");
            }
            else
            {
                Debug.LogWarning("[PlayerCardSlotUI] SlotAnchor가 없습니다.");
            }
        }

        public override void ExecuteCardAutomatically()
        {
            if (card == null || caster == null || target == null)
            {
                Debug.LogWarning("[PlayerCardSlotUI] 카드 실행 불가: 카드 또는 캐릭터가 null");
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
