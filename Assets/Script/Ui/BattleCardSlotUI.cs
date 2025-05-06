using UnityEngine;
using Game.Slots;
using Game.Cards;
using Game.Characters;
using Game.Battle;
using Game.Interface;
using Game.Managers;

namespace Game.UI
{
    /// <summary>
    /// 실제 전투에서 카드가 드롭되어 실행되는 슬롯 UI.
    /// SlotAnchor를 기반으로 자동 위치 바인딩 및 캐스터/타겟 설정을 담당합니다.
    /// </summary>
    public class BattleCardSlotUI : BaseCardSlotUI
    {
        [SerializeField] private SlotOwner owner;

        /// <summary>
        /// 이 슬롯의 전투상 포지션 (FIRST / SECOND)
        /// </summary>
        public BattleSlotPosition Position { get; private set; }

        /// <summary>
        /// 슬롯 초기화 시 SlotAnchor 정보 바인딩
        /// </summary>
        public override void AutoBind()
        {
            base.AutoBind();

            SlotAnchor anchor = GetComponent<SlotAnchor>();
            if (anchor != null)
            {
                Position = anchor.battleSlotPosition;
                owner = anchor.owner;

                InitializeCasterAndTarget();
                Debug.Log($"[BattleCardSlotUI] 슬롯 위치 자동 설정 완료: {Position}");
            }
            else
            {
                Debug.LogWarning($"[BattleCardSlotUI] SlotAnchor가 없습니다: {gameObject.name}");
            }
        }

        /// <summary>
        /// 이 슬롯에 드롭된 카드를 자동으로 실행
        /// </summary>
        public override void ExecuteCardAutomatically()
        {
            ISkillCard card = GetCard();
            if (card == null)
            {
                Debug.LogWarning("[BattleCardSlotUI] 실행할 카드가 없습니다.");
                return;
            }

            if (caster == null || target == null)
            {
                Debug.LogWarning("[BattleCardSlotUI] 캐스터 또는 타겟이 할당되지 않았습니다.");
                return;
            }

            CardExecutor.Execute(card, caster, target);
            Debug.Log($"[BattleCardSlotUI] {card.GetCardName()} 실행됨 → {caster.GetName()} → {target.GetName()}");
        }

        /// <summary>
        /// 캐스터와 타겟 자동 결정 (소유자 기준)
        /// </summary>
        private void InitializeCasterAndTarget()
        {
            if (owner == SlotOwner.Player)
            {
                caster = PlayerManager.Instance.GetPlayer() as ICharacter;
                target = EnemyManager.Instance.GetRandomEnemy();
            }
            else
            {
                caster = EnemyManager.Instance.GetEnemyBySlot(Position);
                target = PlayerManager.Instance.GetPlayer() as ICharacter;
            }
        }

        /// <summary>
        /// 외부에서 슬롯 위치를 수동 설정 가능하도록 제공
        /// </summary>
        public void SetSlotPosition(BattleSlotPosition position)
        {
            this.Position = position;
        }
    }
}
