using UnityEngine;
using UnityEngine.UI;
using Game.Interface;
using Game.Cards;
using Game.Battle;
using Game.Managers;
using Game.Characters;

namespace Game.UI
{
    /// <summary>
    /// 전투 중 슬롯에 배치된 스킬 카드 UI를 관리하는 클래스입니다.
    /// 자동 슬롯 위치 추론 및 카드 실행 시 캐스터/타겟 자동 설정이 포함됩니다.
    /// </summary>
    public class BattleCardSlotUI : BaseCardSlotUI
    {
        [SerializeField] private Image highlightImage;

        /// <summary>
        /// 현재 슬롯에 할당된 카드의 위치를 나타냅니다.
        /// 자동 추론 시스템과 연동됩니다.
        /// </summary>
        public SlotPosition Position { get; private set; }

        private CharacterBase caster;
        private CharacterBase target;

        /// <summary>
        /// 슬롯 초기화 시 자동으로 위치를 추론하고, 캐스터/타겟도 설정합니다.
        /// </summary>
        public override void AutoBind()
        {
            base.AutoBind();

            // 이름 기반으로 슬롯 위치를 자동 추론
            Position = InferSlotPositionFromName(name);

            // 자동 캐릭터 설정
            InitializeCasterAndTarget();
        }

        /// <summary>
        /// 슬롯에서 카드가 제거될 때 호출됩니다.
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            // 강조 이미지가 있다면 비활성화
            if (highlightImage != null)
                highlightImage.enabled = false;
        }

        /// <summary>
        /// 이름 기반으로 슬롯 위치를 추론합니다.
        /// </summary>
        /// <param name="slotName">슬롯의 이름</param>
        /// <returns>추론된 슬롯 포지션</returns>
        private SlotPosition InferSlotPositionFromName(string slotName)
        {
            string lower = slotName.ToLower();
            if (lower.Contains("front")) return SlotPosition.FRONT;
            if (lower.Contains("back")) return SlotPosition.BACK;
            if (lower.Contains("support")) return SlotPosition.SUPPORT;

            return SlotPosition.UNKNOWN;
        }

        /// <summary>
        /// 슬롯 이름 또는 위치 정보를 기반으로 캐스터와 타겟을 자동 설정합니다.
        /// </summary>
        private void InitializeCasterAndTarget()
        {
            string lowerName = name.ToLower();
            if (lowerName.Contains("enemy"))
            {
                caster = EnemySpawnerManager.Instance.GetEnemyBySlot(Position);
                target = PlayerManager.Instance.GetPlayer();
            }
            else // 기본적으로 플레이어 슬롯
            {
                caster = PlayerManager.Instance.GetPlayer();
                target = EnemySpawnerManager.Instance.GetEnemyBySlot(Position);
            }
        }

        /// <summary>
        /// 슬롯에 있는 카드를 자동으로 실행합니다.
        /// 내부적으로 캐스터와 타겟을 자동 추론합니다.
        /// </summary>
        public void ExecuteCardAutomatically()
        {
            var card = GetCard();
            if (card == null)
            {
                Debug.LogWarning($"[BattleCardSlotUI] 슬롯 {Position}에 카드가 없습니다.");
                return;
            }

            if (caster == null || target == null)
            {
                Debug.LogError($"[BattleCardSlotUI] 캐스터 또는 타겟이 설정되지 않았습니다.");
                return;
            }

            CardExecutor.Instance.ExecuteCard(card, caster, target);
        }
    }
}
