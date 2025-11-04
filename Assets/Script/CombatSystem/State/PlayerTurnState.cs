using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Service;
using Game.ItemSystem.Data;
using Game.ItemSystem.Data.Reward;
using Game.ItemSystem.Service.Reward;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 플레이어 턴 상태
    /// - 플레이어가 카드를 드래그하여 배틀 슬롯에 배치할 수 있습니다
    /// - 카드 배치 시 CardExecutionState로 전환됩니다
    /// </summary>
    public class PlayerTurnState : BaseCombatState
    {
        public override string StateName => "PlayerTurn";

        // 플레이어 턴에서는 카드 드래그만 허용
        public override bool AllowPlayerCardDrag => true;
        public override bool AllowEnemyAutoExecution => false;
        public override bool AllowSlotMovement => false;
        public override bool AllowTurnSwitch => false;

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                return;
            }

            LogStateTransition("플레이어 턴 시작");

            // 소환 트리거 체크 (안전한 시점)
            context.StateMachine?.CheckSummonTriggerAtSafePoint();

            // 플레이어 턴 설정 (TurnController 사용)
            if (context.TurnController != null)
            {
                context.TurnController.SetTurnAndIncrement(Interface.TurnType.Player);
            }

            // 턴별 효과 처리 (가드, 출혈, 반격, 기절 등)
            ProcessTurnEffects(context);

            // 매 플레이어 턴마다 액티브 아이템 보상 지급 (첫 턴 제외)
            GiveActiveItemReward(context);

            // 소환 모드 확인 및 해제
            bool isSummonMode = context.SlotMovement?.IsSummonMode ?? false;

            if (isSummonMode)
            {
                LogStateTransition("소환 모드 감지 - 소환 모드 해제 후 핸드 생성");
                // 소환 모드 해제
                context.SlotMovement?.ClearSummonMode();
            }

            // 플레이어 손패 생성 (소환 모드 여부와 관계없이 항상 생성)
            if (context.HandManager != null)
            {
                context.HandManager.GenerateInitialHand();
                LogStateTransition("플레이어 손패 생성 완료");
            }
            else
            {
                LogWarning("HandManager가 null - 손패 생성 건너뜀");
            }

            LogStateTransition("플레이어 턴 시작 - 카드 드래그 대기 중");
        }

        public override void OnExit(CombatStateContext context)
        {
            base.OnExit(context);
            LogStateTransition("플레이어 턴 종료");
        }

        /// <summary>
        /// 턴별 효과를 처리합니다 (가드, 출혈, 반격, 기절 등)
        /// </summary>
        private void ProcessTurnEffects(CombatStateContext context)
        {
            if (context?.TurnController == null)
            {
                LogWarning("TurnController가 null - 턴별 효과 처리 건너뜀");
                return;
            }

            // TurnController의 ProcessAllCharacterTurnEffects 메서드 호출
            context.TurnController.ProcessAllCharacterTurnEffects();
            LogStateTransition("턴별 효과 처리 완료 (가드, 출혈, 반격, 기절 등)");
        }


        /// <summary>
        /// 매 플레이어 턴마다 액티브 아이템을 보상으로 지급합니다. (첫 턴 제외)
        /// </summary>
        private void GiveActiveItemReward(CombatStateContext context)
        {
            // 첫 플레이어 턴인지 확인 (턴 카운트가 1이면 첫 턴이므로 보상 지급 안 함)
            if (context?.TurnController == null)
            {
                LogWarning("TurnController가 null입니다 - 액티브 아이템 보상 지급 건너뜀");
                return;
            }

            // 첫 턴(턴 카운트 1)에는 보상 지급 안 함
            if (context.TurnController.TurnCount <= 1)
            {
                LogStateTransition("첫 플레이어 턴이므로 액티브 아이템 보상 지급 건너뜀");
                return;
            }

            // ItemService 찾기
            var itemService = UnityEngine.Object.FindFirstObjectByType<ItemService>();
            if (itemService == null)
            {
                LogWarning("ItemService를 찾을 수 없습니다 - 액티브 아이템 보상 지급 건너뜀");
                return;
            }

            // 인벤토리가 가득 찼는지 확인
            if (itemService.IsActiveInventoryFull())
            {
                LogStateTransition("인벤토리가 가득 찼습니다 - 액티브 아이템 보상 지급 건너뜀");
                return;
            }

            // 가중치가 적용된 보상 풀에서 액티브 아이템 1개 선택
            ActiveItemDefinition rewardItem = null;
            var pools = Resources.LoadAll<RewardPool>("Data/Reward");
            if (pools != null && pools.Length > 0)
            {
                var tempConfig = ScriptableObject.CreateInstance<EnemyRewardConfig>();
                tempConfig.activeCount = 1;
                tempConfig.activePools = pools;

                var generator = new RewardGenerator();
                var generated = generator.GenerateActive(tempConfig, player: null, stageIndex: 0, runSeed: 0);
                if (generated != null && generated.Length > 0)
                {
                    rewardItem = generated[0];
                }
            }

            // 풀을 찾지 못했거나 생성 실패 시 기본 랜덤 보상으로 폴백
            if (rewardItem == null)
            {
                var rewards = DefaultRewardService.GenerateDefaultActiveReward(count: 1);
                if (rewards == null || rewards.Length == 0)
                {
                    LogWarning("액티브 아이템 보상을 생성할 수 없습니다");
                    return;
                }
                rewardItem = rewards[0];
            }
            if (rewardItem == null)
            {
                LogWarning("생성된 액티브 아이템이 null입니다");
                return;
            }

            // 아이템 추가
            bool success = itemService.AddActiveItem(rewardItem);
            if (success)
            {
                LogStateTransition($"액티브 아이템 보상 지급 완료: {rewardItem.DisplayName} (턴 {context.TurnController.TurnCount})");
            }
            else
            {
                LogWarning($"액티브 아이템 보상 지급 실패: {rewardItem.DisplayName}");
            }
        }

        /// <summary>
        /// 플레이어가 카드를 배치했을 때 호출
        /// CardExecutionState로 전환합니다
        /// </summary>
        public void OnCardPlaced(CombatStateContext context,
            Game.SkillCardSystem.Interface.ISkillCard card,
            Game.CombatSystem.Slot.CombatSlotPosition slot)
        {
            if (context?.StateMachine == null)
            {
                LogError("StateMachine이 null입니다");
                return;
            }

            LogStateTransition($"카드 배치: {card?.GetCardName()} → {slot}");

            // 실행 컨텍스트 설정
            context.CurrentExecutingCard = card;
            context.CurrentExecutingSlot = slot;

            // 카드 실행 상태로 전환
            var executionState = new CardExecutionState();
            RequestTransition(context, executionState);
        }
    }
}
