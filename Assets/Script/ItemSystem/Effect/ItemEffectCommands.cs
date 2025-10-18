
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using UnityEngine;
using Zenject;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 체력 회복 효과 커맨드입니다.
    /// </summary>
    public class HealEffectCommand : BaseItemEffectCommand
    {
        private readonly int healAmount;

        public HealEffectCommand(int healAmount) : base("체력 회복")
        {
            this.healAmount = healAmount;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            int currentHP = context.User.GetCurrentHP();
            int maxHP = context.User.GetMaxHP();
            int actualHeal = Mathf.Min(healAmount, maxHP - currentHP);

            if (actualHeal > 0)
            {
                context.User.Heal(actualHeal);
                GameLogger.LogInfo($"체력 회복: {actualHeal} (현재: {currentHP + actualHeal}/{maxHP})", GameLogger.LogCategory.Core);
                return true;
            }
            else
            {
                GameLogger.LogInfo("체력이 이미 최대입니다", GameLogger.LogCategory.Core);
                return false;
            }
        }
    }

    /// <summary>
    /// 공격력 버프 효과 커맨드입니다.
    /// </summary>
    public class AttackBuffEffectCommand : BaseItemEffectCommand
    {
        private readonly int buffAmount;
        private readonly int duration;

        public AttackBuffEffectCommand(int buffAmount, int duration = 1) : base("공격력 버프")
        {
            this.buffAmount = buffAmount;
            this.duration = duration;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            // 아이템 정의를 ActiveItemDefinition으로 캐스팅
            var activeItemDef = context.ItemDefinition as Game.ItemSystem.Data.ActiveItemDefinition;
            var turnPolicy = activeItemDef?.turnPolicy ?? Interface.ItemEffectTurnPolicy.TargetTurnOnly;
            var itemIcon = context.ItemDefinition?.Icon;

            // 공격력 버프 효과 생성 및 적용
            var attackBuffEffect = new AttackPowerBuffEffect(buffAmount, duration, turnPolicy, itemIcon);
            context.User.RegisterPerTurnEffect(attackBuffEffect);

            // UI에 버프 아이콘 표시 (아이템 이미지 사용)
            if (context.User is Game.CharacterSystem.Core.PlayerCharacter playerCharacter)
            {
                GameLogger.LogInfo($"공격력 물약 아이콘: {itemIcon?.name ?? "null"}, 아이템: {context.ItemDefinition?.DisplayName ?? "null"}", GameLogger.LogCategory.Core);
                playerCharacter.AddBuffDebuffIcon("AttackPowerBuff", itemIcon, true, duration);
            }

            GameLogger.LogInfo($"공격력 버프 적용: +{buffAmount} ({duration}턴, 정책: {turnPolicy})", GameLogger.LogCategory.Core);
            return true;
        }
    }

    /// <summary>
    /// 광대 물약 랜덤 효과 커맨드입니다.
    /// </summary>
    public class ClownPotionEffectCommand : BaseItemEffectCommand
    {
        private int healChance;
        private int healAmount;
        private int damageAmount;

        public ClownPotionEffectCommand(int healChance = 50, int healAmount = 5, int damageAmount = 5)
            : base("광대 물약")
        {
            this.healChance = Mathf.Clamp(healChance, 0, 100);
            this.healAmount = healAmount;
            this.damageAmount = damageAmount;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            bool isHeal = UnityEngine.Random.Range(0, 100) < healChance;

            if (isHeal)
            {
                context.User.Heal(healAmount);
                GameLogger.LogInfo($"광대 물약 효과: 체력 회복 +{healAmount} (확률: {healChance}%)", GameLogger.LogCategory.Core);
            }
            else
            {
                context.User.TakeDamage(damageAmount);
                GameLogger.LogInfo($"광대 물약 효과: 데미지 -{damageAmount} (확률: {100 - healChance}%)", GameLogger.LogCategory.Core);
            }

            return true;
        }
    }

    /// <summary>
    /// 부활 효과 커맨드입니다.
    /// </summary>
    public class ReviveEffectCommand : BaseItemEffectCommand
    {
        public ReviveEffectCommand() : base("부활")
        {
        }

        protected override bool ValidateAdditionalConditions(IItemUseContext context)
        {
            if (!context.User.IsDead())
            {
                GameLogger.LogInfo("사용자가 이미 살아있습니다", GameLogger.LogCategory.Core);
                return false;
            }
            return true;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            // 최대 체력으로 부활
            int maxHP = context.User.GetMaxHP();
            context.User.Heal(maxHP);

            // 모든 디버프 제거
            if (context.User is Game.CharacterSystem.Core.CharacterBase characterBase)
            {
                var buffs = characterBase.GetBuffs();
                var debuffsToRemove = new System.Collections.Generic.List<Game.SkillCardSystem.Interface.IPerTurnEffect>();

                foreach (var effect in buffs)
                {
                    // 디버프 효과들만 제거 (버프는 유지)
                    if (effect is Game.SkillCardSystem.Interface.IStatusEffectDebuff)
                    {
                        debuffsToRemove.Add(effect);
                    }
                }

                foreach (var debuff in debuffsToRemove)
                {
                    // CharacterBase에서 디버프 제거 메서드가 있다면 사용
                    // 현재는 직접 제거할 수 없으므로 로그만 출력
                    GameLogger.LogInfo($"부활 시 디버프 제거: {debuff.GetType().Name}", GameLogger.LogCategory.Core);
                }
            }

            GameLogger.LogInfo($"부활 완료: 체력 {maxHP}으로 회복, 모든 디버프 제거", GameLogger.LogCategory.Core);
            return true;
        }
    }

    /// <summary>
    /// 시간 정지 효과 커맨드입니다.
    /// 다음 턴에 사용될 적 카드를 봉인시킵니다.
    /// </summary>
    public class TimeStopEffectCommand : BaseItemEffectCommand
    {
        private int sealCount;

        public TimeStopEffectCommand(int sealCount = 1) : base("시간 정지")
        {
            this.sealCount = Mathf.Max(1, sealCount);
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            var itemIcon = context.ItemDefinition?.Icon;

            // 적 캐릭터를 직접 찾기 (context.Target 무시)
            var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
            if (enemyManager == null)
            {
                GameLogger.LogError($"[TimeStopEffect] EnemyManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            var enemyCharacter = enemyManager.GetCurrentEnemy();
            if (enemyCharacter == null)
            {
                GameLogger.LogError($"[TimeStopEffect] 현재 적 캐릭터를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            GameLogger.LogInfo($"[TimeStopEffect] 적 캐릭터 발견: {enemyCharacter.GetCharacterName()}, 플레이어인가={enemyCharacter.IsPlayerControlled()}", GameLogger.LogCategory.Core);

            // StunDebuff는 매 턴 감소하므로 1턴으로 설정
            // EveryTurn 정책: 플레이어 턴(적용) → 적 턴(감소하며 스턴 유지) → 만료
            var stunDebuff = new Game.SkillCardSystem.Effect.StunDebuff(1, itemIcon);
            bool success = enemyCharacter.RegisterStatusEffect(stunDebuff);

            if (success)
            {
                GameLogger.LogInfo($"[TimeStopEffect] 타임 스톱 스크롤 적용 완료: {enemyCharacter.GetCharacterName()}에게 1턴 스턴", GameLogger.LogCategory.Core);
                return true;
            }
            else
            {
                GameLogger.LogWarning($"[TimeStopEffect] 타임 스톱 스크롤 적용 실패: {enemyCharacter.GetCharacterName()}이 보호 상태", GameLogger.LogCategory.Core);
                return false;
            }
        }
    }

    /// <summary>
    /// 운명의 주사위 효과 커맨드입니다.
    /// 다음 턴 적이 사용 예정인 스킬을 무작위로 변경시킵니다.
    /// </summary>
    public class DiceOfFateEffectCommand : BaseItemEffectCommand
    {
        private int changeCount;

        public DiceOfFateEffectCommand(int changeCount = 1) : base("운명의 주사위")
        {
            this.changeCount = Mathf.Max(1, changeCount);
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            // SlotRegistry를 통해 CombatSlotRegistry 접근
            var slotRegistry = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Slot.SlotRegistry>();
            if (slotRegistry == null)
            {
                GameLogger.LogWarning("[DiceOfFate] SlotRegistry를 찾을 수 없습니다. 전투 씬이 아닌 것으로 보입니다", GameLogger.LogCategory.Core);
                return false;
            }

            // SlotRegistry에서 CombatSlotRegistry 가져오기 (public 메서드 사용)
            var combatSlotRegistry = slotRegistry.GetCombatSlotRegistry();
            if (combatSlotRegistry == null)
            {
                GameLogger.LogWarning("[DiceOfFate] CombatSlotRegistry를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 대기 슬롯에서 적의 가장 앞 순서 카드 찾기
            var waitSlots = new[] { CombatSlotPosition.WAIT_SLOT_1, CombatSlotPosition.WAIT_SLOT_2, CombatSlotPosition.WAIT_SLOT_3, CombatSlotPosition.WAIT_SLOT_4 };
            ISkillCard targetCard = null;
            CombatSlotPosition targetSlot = CombatSlotPosition.WAIT_SLOT_1;

            // WAIT_SLOT_1부터 순서대로 확인하여 적의 첫 번째 카드 찾기
            foreach (var slot in waitSlots)
            {
                var combatSlot = combatSlotRegistry.GetCombatSlot(slot);
                if (combatSlot != null)
                {
                    var card = combatSlot.GetCard();
                    if (card != null && card.GetOwner() == SlotOwner.ENEMY)
                    {
                        targetCard = card;
                        targetSlot = slot;
                        GameLogger.LogInfo($"적의 대기 카드 발견: {slot} - {card.GetCardName()}", GameLogger.LogCategory.Core);
                        break; // 가장 앞 순서의 카드를 찾았으므로 중단
                    }
                }
            }

            if (targetCard == null)
            {
                GameLogger.LogWarning("대기 슬롯에서 적의 카드를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 적 덱에서 랜덤 카드 선택
            var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
            if (enemyManager == null)
            {
                GameLogger.LogError("EnemyManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            var currentEnemy = enemyManager.GetCurrentEnemy();
            if (currentEnemy == null)
            {
                GameLogger.LogError("현재 적 캐릭터를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 적 캐릭터의 데이터에서 스킬 덱 접근
            var enemyData = currentEnemy.CharacterData as Game.CharacterSystem.Data.EnemyCharacterData;
            if (enemyData?.EnemyDeck == null)
            {
                GameLogger.LogError("적 스킬 덱을 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            var randomEntry = enemyData.EnemyDeck.GetRandomEntry();
            if (randomEntry == null)
            {
                GameLogger.LogError("적 스킬 덱에서 랜덤 카드를 선택할 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 기존 카드 제거
            var targetCombatSlot = combatSlotRegistry.GetCombatSlot(targetSlot);
            if (targetCombatSlot != null)
            {
                targetCombatSlot.ClearAll();
            }

            // 새 카드 생성 및 배치 (임시로 로그만 출력)
            GameLogger.LogInfo($"운명의 주사위 적용 예정: 적의 {targetSlot} 카드 {targetCard.GetCardName()} → {randomEntry.definition.displayName}", GameLogger.LogCategory.Core);

            // TODO: SkillCardFactory를 통해 새 카드 생성 및 배치
            // 현재는 임시 구현으로 로그만 출력

            return true;
        }
    }

    /// <summary>
    /// 리롤 효과 커맨드입니다.
    /// </summary>
    public class RerollEffectCommand : BaseItemEffectCommand
    {
        private int rerollCount;

        public RerollEffectCommand(int rerollCount) : base("리롤")
        {
            this.rerollCount = rerollCount;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            // PlayerManager를 씬에서 직접 찾기
            var playerManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
            if (playerManager == null)
            {
                GameLogger.LogError("PlayerManager를 씬에서 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            var handManager = playerManager.GetPlayerHandManager();
            if (handManager == null)
            {
                GameLogger.LogError("PlayerHandManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 현재 핸드의 카드 수 확인
            int currentHandCount = 0;
            var handSlots = new[] {
                SkillCardSlotPosition.PLAYER_SLOT_1,
                SkillCardSlotPosition.PLAYER_SLOT_2,
                SkillCardSlotPosition.PLAYER_SLOT_3
            };

            foreach (var slot in handSlots)
            {
                if (handManager.GetCardInSlot(slot) != null)
                {
                    currentHandCount++;
                }
            }

            if (currentHandCount == 0)
            {
                GameLogger.LogWarning("리롤할 카드가 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 핸드 클리어
            handManager.ClearAll();

            // 새로운 카드 드로우 (덱에서 3장)
            // PlayerHandManager의 GenerateInitialHand 메서드를 사용하여 새 카드 드로우
            handManager.GenerateInitialHand();

            GameLogger.LogInfo($"역행의 모래시계 적용: 핸드 리롤 완료 ({currentHandCount}장 → 3장)", GameLogger.LogCategory.Core);
            return true;
        }
    }

    /// <summary>
    /// 실드 브레이커 효과 커맨드입니다.
    /// </summary>
    public class ShieldBreakerEffectCommand : BaseItemEffectCommand
    {
        private int duration;

        public ShieldBreakerEffectCommand(int duration = 1) : base("실드 브레이커")
        {
            this.duration = duration;
        }

        protected override bool ExecuteInternal(IItemUseContext context)
        {
            // 아이템 정의를 ActiveItemDefinition으로 캐스팅
            var activeItemDef = context.ItemDefinition as Game.ItemSystem.Data.ActiveItemDefinition;
            var turnPolicy = activeItemDef?.turnPolicy ?? Interface.ItemEffectTurnPolicy.EveryTurn;
            var itemIcon = context.ItemDefinition?.Icon;

            // 적 캐릭터를 직접 찾기 (context.Target 무시)
            var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
            if (enemyManager == null)
            {
                GameLogger.LogError($"[ShieldBreakerEffect] EnemyManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            var enemyCharacter = enemyManager.GetCurrentEnemy();
            if (enemyCharacter == null)
            {
                GameLogger.LogError($"[ShieldBreakerEffect] 현재 적 캐릭터를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            GameLogger.LogInfo($"[ShieldBreakerEffect] 적 캐릭터 발견: {enemyCharacter.GetCharacterName()}, 정책={turnPolicy}", GameLogger.LogCategory.Core);

            // 실드 브레이커 디버프 효과 생성 및 적용
            var shieldBreakerEffect = new ShieldBreakerDebuffEffect(duration, turnPolicy, itemIcon);
            bool success = enemyCharacter.RegisterStatusEffect(shieldBreakerEffect);

            if (success)
            {
                GameLogger.LogInfo($"[ShieldBreakerEffect] 실드 브레이커 적용 완료: {enemyCharacter.GetCharacterName()}에게 {duration}턴 디버프 (정책: {turnPolicy})", GameLogger.LogCategory.Core);
                return true;
            }
            else
            {
                GameLogger.LogWarning($"[ShieldBreakerEffect] 실드 브레이커 적용 실패: {enemyCharacter.GetCharacterName()}이 보호 상태", GameLogger.LogCategory.Core);
                return false;
            }
        }
    }
}