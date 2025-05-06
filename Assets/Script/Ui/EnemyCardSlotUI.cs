using UnityEngine;
using Game.Cards;
using Game.Interface;
using Game.Managers;
using Game.Slots;

namespace Game.UI
{
    /// <summary>
    /// 적 캐릭터가 사용하는 스킬 카드 핸드 슬롯 UI
    /// </summary>
    public class EnemyCardSlotUI : BaseCardSlotUI
    {
        private ISkillCard card;

        /// <summary>
        /// 이 슬롯의 위치 정보 (예: ENEMY_SLOT_1, ENEMY_SLOT_2, ...)
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

                // 현재 설계상 적은 1명만 존재하므로 GetEnemyBySlot(Position) 대신 GetEnemy() 사용
                caster = EnemyManager.Instance.GetCurrentEnemy(); // 단일 적 기준
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
