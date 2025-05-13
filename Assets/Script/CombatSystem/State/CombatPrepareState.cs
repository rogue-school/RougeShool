using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Core;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 준비 단계 상태입니다. 적 캐릭터 소환 및 스킬카드 초기화를 담당합니다.
    /// </summary>
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ITurnStateController controller;

        public CombatPrepareState(ITurnStateController controller)
        {
            this.controller = controller;
        }

        public void EnterState()
        {
            Debug.Log("[CombatPrepareState] 전투 준비 상태 진입");

            // 1. 적 소환
            StageManager.Instance.SpawnNextEnemy();

            // 2. 적 카드 초기 핸드 구성
            EnemyHandManager.Instance.GenerateInitialHand();

            // 3. 적이 1번 슬롯 카드로 공격 슬롯 예약 (랜덤)
            ISkillCard enemyCard = EnemyHandManager.Instance.GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (enemyCard != null)
            {
                CombatTurnManager.Instance.ReserveEnemySlot(Random.value < 0.5f ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND);
            }

            // 4. 핸드 정렬 (앞으로 밀기), 3번 슬롯 새 카드 생성
            EnemyHandManager.Instance.AdvanceSlots();

            Debug.Log("[CombatPrepareState] 준비 완료, 플레이어 입력 대기 상태로 전환 예정");
            controller.RequestStateChange(new CombatPlayerInputState(controller));
        }

        public void ExecuteState()
        {
            // 플레이어가 카드를 전투 슬롯에 올리면 버튼이 활성화됨 (외부 처리)
        }

        public void ExitState()
        {
            Debug.Log("[CombatPrepareState] 종료");
        }
    }
}
