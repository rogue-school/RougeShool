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

            // 턴별 효과 처리 및 출혈 이펙트 완료 대기 후 다음 동작 진행
            if (context.StateMachine != null && context.StateMachine is MonoBehaviour stateMachineMono)
            {
                stateMachineMono.StartCoroutine(ProcessTurnEffectsAndContinue(context));
            }
            else
            {
                // Fallback: 코루틴을 시작할 수 없으면 즉시 처리
                ProcessTurnEffects(context);
                ContinueAfterTurnEffects(context);
            }
        }

        /// <summary>
        /// 턴별 효과 처리 및 출혈 이펙트 완료 후 다음 동작 진행
        /// </summary>
        private System.Collections.IEnumerator ProcessTurnEffectsAndContinue(CombatStateContext context)
        {
            // 턴별 효과 처리 (출혈 이펙트 완료 대기)
            yield return ProcessTurnEffectsCoroutine(context);

            // 출혈 이펙트 완료 후 다음 동작 진행
            ContinueAfterTurnEffects(context);
        }

        /// <summary>
        /// 턴별 효과 처리 후 다음 동작 진행
        /// </summary>
        private void ContinueAfterTurnEffects(CombatStateContext context)
        {
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
        /// 출혈 이펙트 완료를 기다립니다.
        /// </summary>
        private void ProcessTurnEffects(CombatStateContext context)
        {
            if (context?.TurnController == null)
            {
                LogWarning("TurnController가 null - 턴별 효과 처리 건너뜀");
                return;
            }

            // 즉시 처리 (코루틴 없이)
            context.TurnController.ProcessAllCharacterTurnEffects();
            LogStateTransition("턴별 효과 처리 완료 (가드, 출혈, 반격, 기절 등)");
        }

        /// <summary>
        /// 턴별 효과를 처리하는 코루틴 (출혈 이펙트 완료 대기)
        /// 플레이어와 적의 출혈 효과를 동시에 처리합니다.
        /// </summary>
        private System.Collections.IEnumerator ProcessTurnEffectsCoroutine(CombatStateContext context)
        {
            if (context?.TurnController == null || context.PlayerManager == null || context.EnemyManager == null)
            {
                LogWarning("필수 매니저가 null - 턴별 효과 처리 건너뜀");
                yield break;
            }

            // 플레이어와 적 캐릭터 가져오기
            var player = context.PlayerManager.GetCharacter();
            var enemy = context.EnemyManager.GetCharacter();

            // 모든 캐릭터의 출혈 효과 개수 카운트
            int totalBleedEffectCount = 0;
            if (player is Game.CharacterSystem.Core.CharacterBase playerBase)
            {
                var playerBuffs = playerBase.GetBuffs();
                foreach (var buff in playerBuffs)
                {
                    if (buff is Game.SkillCardSystem.Effect.BleedEffect)
                    {
                        totalBleedEffectCount++;
                    }
                }
            }

            if (enemy is Game.CharacterSystem.Core.CharacterBase enemyBase)
            {
                var enemyBuffs = enemyBase.GetBuffs();
                foreach (var buff in enemyBuffs)
                {
                    if (buff is Game.SkillCardSystem.Effect.BleedEffect)
                    {
                        totalBleedEffectCount++;
                    }
                }
            }

            // 출혈 효과가 없으면 즉시 처리
            if (totalBleedEffectCount == 0)
            {
                context.TurnController.ProcessAllCharacterTurnEffects();
                LogStateTransition("출혈 효과 없음 - 즉시 처리 완료");
                yield break;
            }

            // 출혈 효과가 있으면 동시에 처리
            int completedBleedEffects = 0;
            System.Action onBleedComplete = () => completedBleedEffects++;

            // 이벤트 구독 (ProcessTurnEffects 호출 전에 구독해야 함)
            Game.CombatSystem.CombatEvents.OnBleedTurnStartEffectComplete += onBleedComplete;

            // 모든 캐릭터의 턴 효과를 동시에 처리 (출혈 이펙트 재생 시작)
            context.TurnController.ProcessAllCharacterTurnEffects();

            // 모든 출혈 이펙트 완료 대기 (타임아웃: 최대 1.5초)
            float timeout = 1.5f;
            float elapsed = 0f;
            
            while (completedBleedEffects < totalBleedEffectCount && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 이벤트 구독 해제
            Game.CombatSystem.CombatEvents.OnBleedTurnStartEffectComplete -= onBleedComplete;

            if (completedBleedEffects >= totalBleedEffectCount)
            {
                LogStateTransition($"모든 출혈 이펙트 완료 ({completedBleedEffects}/{totalBleedEffectCount})");
            }
            else
            {
                LogWarning($"출혈 이펙트 완료 타임아웃 ({completedBleedEffects}/{totalBleedEffectCount}, {elapsed:F2}초 경과)");
            }
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

            // 소환/복귀 진행 중에는 보상 지급 차단 (턴 카운트 유지 요청에 따라 재생성 구간 보상 억제)
            bool isSummonMode = context.SlotMovement != null && context.SlotMovement.IsSummonMode;
            if (isSummonMode)
            {
                LogStateTransition($"소환/복귀 모드이므로 액티브 아이템 보상 지급 건너뜀 (턴: {context.TurnController.TurnCount})");
                return;
            }

            // 첫 턴(턴 카운트 1)에는 보상 지급 안 함
            if (context.TurnController.TurnCount <= 1)
            {
                LogStateTransition("첫 플레이어 턴이므로 액티브 아이템 보상 지급 건너뜀");
                return;
            }

            // ItemService 조회
            var itemService = Game.ItemSystem.Service.ItemService.Instance;
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
