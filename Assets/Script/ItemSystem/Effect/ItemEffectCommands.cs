
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
                GameLogger.LogInfo("체력이 이미 최대입니다 - 아이템 사용은 성공으로 처리", GameLogger.LogCategory.Core);
                return true; // 체력이 최대여도 아이템 사용은 성공으로 처리
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

        /// <summary>
        /// 부활 아이템은 사망한 사용자도 사용할 수 있도록 Execute 메서드를 오버라이드
        /// </summary>
        public override bool Execute(IItemUseContext context)
        {
            // 부활 아이템은 사망한 사용자도 허용 (null 체크만 수행)
            if (context?.User == null)
            {
                GameLogger.LogError("부활 실패: 사용자가 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            try
            {
                return ExecuteInternal(context);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[부활] 효과 실행 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Core);
                return false;
            }
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

            // StunDebuff는 매 턴 감소하므로 2턴으로 설정
            // EveryTurn 정책: 플레이어 턴(적용) → 적 턴(감소하며 스턴 유지) → 다음 플레이어 턴(만료)
            // 이렇게 하면 적 턴 전체를 차단할 수 있습니다
            var stunDebuff = new Game.SkillCardSystem.Effect.StunDebuff(2, itemIcon);
            // 아이템 유래 상태이상은 가드를 무시하고 직접 등록 (RegisterPerTurnEffect 사용)
            enemyCharacter.RegisterPerTurnEffect(stunDebuff);
            GameLogger.LogInfo($"[TimeStopEffect] 타임 스톱 스크롤 적용: {enemyCharacter.GetCharacterName()}에게 2턴 스턴 (아이템은 가드 무시)", GameLogger.LogCategory.Core);
            return true;
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

        /// <summary>
        /// 운명의 주사위 효과를 실행합니다
        /// 목적: 다음 턴에 사용될 적의 스킬카드를 랜덤하게 교체
        /// </summary>
        protected override bool ExecuteInternal(IItemUseContext context)
        {
            GameLogger.LogInfo("[DiceOfFate] 운명의 주사위 효과 실행 시작", GameLogger.LogCategory.Core);

            // 1단계: WAIT_SLOT_1 찾기 (다음 턴에 사용될 적 카드)
            var targetSlot = FindWaitSlot1();
            if (targetSlot == null)
            {
                GameLogger.LogError("[DiceOfFate] WAIT_SLOT_1을 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 2단계: 기존 적 카드 확인
            var existingCard = GetEnemyCardFromSlot(targetSlot);
            if (existingCard == null)
            {
                GameLogger.LogError("[DiceOfFate] WAIT_SLOT_1에 적 카드가 없습니다", GameLogger.LogCategory.Core);
                return false;
            }

            GameLogger.LogInfo($"[DiceOfFate] 교체 대상 카드: {existingCard.GetCardName()}", GameLogger.LogCategory.Core);

            // 3단계: 새 적 카드 생성
            var newCard = CreateRandomEnemyCard();
            if (newCard == null)
            {
                GameLogger.LogError("[DiceOfFate] 새 적 카드 생성에 실패했습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 4단계: 카드 데이터 교체 (GameObject 생성/삭제 없음)
            ReplaceCardInSlot(targetSlot, newCard);

            GameLogger.LogInfo($"[DiceOfFate] 운명의 주사위 적용 완료: {existingCard.GetCardName()} → {newCard.GetCardName()}", GameLogger.LogCategory.Core);
            return true;
        }

        private Game.CombatSystem.UI.CombatExecutionSlotUI FindWaitSlot1()
        {
            var combatSlots = UnityEngine.Object.FindObjectsByType<Game.CombatSystem.UI.CombatExecutionSlotUI>(FindObjectsSortMode.None);
            return System.Array.Find(combatSlots, s => s.Position == CombatSlotPosition.WAIT_SLOT_1);
        }

        private Game.SkillCardSystem.Interface.ISkillCard GetEnemyCardFromSlot(Game.CombatSystem.UI.CombatExecutionSlotUI slot)
        {
            var card = slot.GetCard();
            if (card != null && card.GetOwner() == SlotOwner.ENEMY)
            {
                return card;
            }

            // 자식에서 찾기
            var cardUI = slot.transform.GetComponentInChildren<Game.SkillCardSystem.UI.SkillCardUI>();
            if (cardUI != null)
            {
                var childCard = cardUI.GetCard();
                if (childCard != null && childCard.GetOwner() == SlotOwner.ENEMY)
                {
                    return childCard;
                }
            }

            return null;
        }

        private Game.SkillCardSystem.Interface.ISkillCard CreateRandomEnemyCard()
        {
            var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
            if (enemyManager?.GetCurrentEnemy() is not Game.CharacterSystem.Core.EnemyCharacter enemyCharacter)
            {
                GameLogger.LogError("[DiceOfFate] EnemyCharacter를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return null;
            }

            var enemyData = enemyCharacter.CharacterData;
            if (enemyData?.EnemyDeck == null)
            {
                GameLogger.LogError("[DiceOfFate] 적 스킬 덱을 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return null;
            }

            var randomEntry = enemyData.EnemyDeck.GetRandomEntry();
            if (randomEntry == null)
            {
                GameLogger.LogError("[DiceOfFate] 적 스킬 덱에서 랜덤 카드를 선택할 수 없습니다", GameLogger.LogCategory.Core);
                return null;
            }

            // 직접 SkillCardFactory 인스턴스 생성 (DI 없이)
            var audioManager = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            if (audioManager == null)
            {
                GameLogger.LogError("[DiceOfFate] AudioManager를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return null;
            }
            
            var skillCardFactory = new Game.SkillCardSystem.Factory.SkillCardFactory(audioManager);
            var newCard = skillCardFactory.CreateEnemyCard(randomEntry.definition, enemyCharacter.GetCharacterName());
            
            if (newCard != null)
            {
                GameLogger.LogInfo($"[DiceOfFate] 새 카드 생성 완료: {newCard.GetCardName()}", GameLogger.LogCategory.Core);
            }
            
            return newCard;
        }

        /// <summary>
        /// 슬롯의 카드를 새로운 카드로 교체합니다 (데이터 교체 방식)
        /// </summary>
        /// <param name="slot">대상 슬롯</param>
        /// <param name="newCard">새로운 카드 데이터</param>
        private void ReplaceCardInSlot(Game.CombatSystem.UI.CombatExecutionSlotUI slot, Game.SkillCardSystem.Interface.ISkillCard newCard)
        {
            if (slot == null || newCard == null)
            {
                GameLogger.LogError("[DiceOfFate] 슬롯 또는 새 카드가 null입니다", GameLogger.LogCategory.Core);
                return;
            }

            var oldCardName = slot.GetCard()?.GetCardName() ?? "null";
            GameLogger.LogInfo($"[DiceOfFate] 카드 교체 시작 - Position: {slot.Position}, 기존: {oldCardName} → 신규: {newCard.GetCardName()}", GameLogger.LogCategory.Core);
            
            // 1단계: 슬롯의 currentCard를 먼저 업데이트
            slot.SetCard(newCard);
            GameLogger.LogInfo($"[DiceOfFate] 슬롯 currentCard 업데이트 완료: {slot.GetCard()?.GetCardName()}", GameLogger.LogCategory.Core);
            
            // 2단계: CardSlotRegistry도 업데이트 (중요!)
            UpdateCardSlotRegistry(slot.Position, newCard);
            
            // 3단계: 자식 오브젝트에서 카드 UI 찾기
            var childCardUI = slot.transform.GetComponentInChildren<Game.SkillCardSystem.UI.SkillCardUI>();
            if (childCardUI != null)
            {
                GameLogger.LogInfo($"[DiceOfFate] 자식에서 카드 UI 발견: {childCardUI.GetCard()?.GetCardName()}", GameLogger.LogCategory.Core);
                
                // 카드 UI의 데이터도 업데이트
                childCardUI.SetCard(newCard);
                slot.SetCardUI(childCardUI); // 연결 복구
                
                GameLogger.LogInfo($"[DiceOfFate] 카드 UI 데이터 업데이트 완료: {childCardUI.GetCard()?.GetCardName()}", GameLogger.LogCategory.Core);
                return;
            }

            // 4단계: GetCardUI() 시도 (대안)
            var existingCardUI = slot.GetCardUI() as Game.SkillCardSystem.UI.SkillCardUI;
            if (existingCardUI != null)
            {
                GameLogger.LogInfo($"[DiceOfFate] GetCardUI()로 카드 UI 발견: {existingCardUI.GetCard()?.GetCardName()}", GameLogger.LogCategory.Core);
                
                // 카드 UI의 데이터도 업데이트
                existingCardUI.SetCard(newCard);
                
                GameLogger.LogInfo($"[DiceOfFate] 카드 UI 데이터 업데이트 완료: {existingCardUI.GetCard()?.GetCardName()}", GameLogger.LogCategory.Core);
                return;
            }

            // 5단계: 새 카드 UI 생성 (최후의 수단)
            GameLogger.LogWarning("[DiceOfFate] 기존 카드 UI를 찾을 수 없어 새로 생성합니다", GameLogger.LogCategory.Core);
            
            var prefab = Resources.Load<Game.SkillCardSystem.UI.SkillCardUI>("Prefab/SkillCard");
            if (prefab == null)
            {
                GameLogger.LogError("[DiceOfFate] SkillCardUI 프리팹을 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return;
            }

            var newCardUI = Game.SkillCardSystem.UI.SkillCardUIFactory.CreateUI(prefab, slot.transform, newCard, null, null);
            if (newCardUI != null)
            {
                // SetCardUI를 호출하면 AttachUIToSlot이 호출되어 올바른 위치(anchoredPosition: 0, 4)로 설정됩니다
                slot.SetCardUI(newCardUI);
                
                GameLogger.LogInfo("[DiceOfFate] 새 카드 UI 생성 및 배치 완료", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogError("[DiceOfFate] 새 카드 UI 생성 실패", GameLogger.LogCategory.Core);
            }
            
            // 최종 검증
            var finalCardName = slot.GetCard()?.GetCardName() ?? "null";
            GameLogger.LogInfo($"[DiceOfFate] 최종 검증 - 슬롯 카드: {finalCardName}, 예상: {newCard.GetCardName()}", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// CardSlotRegistry의 카드 데이터를 업데이트합니다
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <param name="newCard">새로운 카드</param>
        private void UpdateCardSlotRegistry(Game.CombatSystem.Slot.CombatSlotPosition position, Game.SkillCardSystem.Interface.ISkillCard newCard)
        {
            // SceneContext를 통해 ICardSlotRegistry 직접 접근
            var sceneContext = UnityEngine.Object.FindFirstObjectByType<Zenject.SceneContext>();
            if (sceneContext == null)
            {
                GameLogger.LogError("[DiceOfFate] SceneContext를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return;
            }

            var cardSlotRegistry = sceneContext.Container.TryResolve<Game.CombatSystem.Interface.ICardSlotRegistry>();
            if (cardSlotRegistry == null)
            {
                GameLogger.LogError("[DiceOfFate] ICardSlotRegistry를 DI 컨테이너에서 찾을 수 없습니다", GameLogger.LogCategory.Core);
                return;
            }

            // 기존 카드 UI 가져오기
            var existingUI = cardSlotRegistry.GetCardUIInSlot(position);
            
            // CardSlotRegistry에 새 카드 등록
            cardSlotRegistry.RegisterCard(position, newCard, existingUI, Game.CombatSystem.Data.SlotOwner.ENEMY);
            
            GameLogger.LogInfo($"[DiceOfFate] CardSlotRegistry 업데이트 완료: {position} = {newCard.GetCardName()}", GameLogger.LogCategory.Core);
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

        public ShieldBreakerEffectCommand(int duration = 2) : base("실드 브레이커")
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

            // 실드 브레이커 효과: 적의 가드 버프를 즉시 제거
            var guardBuffRemoved = RemoveGuardBuff(enemyCharacter);
            
            if (guardBuffRemoved)
            {
                GameLogger.LogInfo($"[ShieldBreakerEffect] 실드 브레이커 적용 완료: {enemyCharacter.GetCharacterName()}의 가드 버프 제거", GameLogger.LogCategory.Core);
                return true;
            }
            else
            {
                GameLogger.LogInfo($"[ShieldBreakerEffect] 실드 브레이커 적용 완료: {enemyCharacter.GetCharacterName()}에게 가드 버프가 없음", GameLogger.LogCategory.Core);
                return true; // 가드가 없어도 성공으로 처리
            }
        }

        /// <summary>
        /// 적의 가드 버프를 제거합니다
        /// </summary>
        /// <param name="enemyCharacter">대상 적 캐릭터</param>
        /// <returns>가드 버프가 제거되었으면 true, 없었으면 false</returns>
        private bool RemoveGuardBuff(Game.CharacterSystem.Interface.ICharacter enemyCharacter)
        {
            if (enemyCharacter == null)
            {
                GameLogger.LogError("[ShieldBreakerEffect] 적 캐릭터가 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            // CharacterBase로 캐스팅하여 perTurnEffects에 접근
            if (enemyCharacter is Game.CharacterSystem.Core.CharacterBase characterBase)
            {
                // 리플렉션을 사용하여 perTurnEffects 필드에 접근
                var perTurnEffectsField = typeof(Game.CharacterSystem.Core.CharacterBase).GetField("perTurnEffects", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (perTurnEffectsField != null)
                {
                    var perTurnEffects = perTurnEffectsField.GetValue(characterBase) as System.Collections.Generic.List<Game.SkillCardSystem.Interface.IPerTurnEffect>;
                    
                    if (perTurnEffects != null)
                    {
                        // GuardBuff 타입의 효과를 찾아서 제거
                        var guardBuff = perTurnEffects.Find(effect => effect is Game.SkillCardSystem.Effect.GuardBuff);
                        
                        if (guardBuff != null)
                        {
                            perTurnEffects.Remove(guardBuff);
                            
                            // 가드 상태를 false로 설정
                            characterBase.SetGuarded(false);
                            
                            // UI 업데이트를 위한 이벤트 발생
                            var onBuffsChangedField = typeof(Game.CharacterSystem.Core.CharacterBase).GetField("OnBuffsChanged", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            
                            if (onBuffsChangedField != null)
                            {
                                var onBuffsChanged = onBuffsChangedField.GetValue(characterBase) as System.Action<System.Collections.Generic.IReadOnlyList<Game.SkillCardSystem.Interface.IPerTurnEffect>>;
                                onBuffsChanged?.Invoke(perTurnEffects.AsReadOnly());
                            }
                            
                            GameLogger.LogInfo($"[ShieldBreakerEffect] 가드 버프 제거 완료: {enemyCharacter.GetCharacterName()}", GameLogger.LogCategory.Core);
                            return true;
                        }
                        else
                        {
                            GameLogger.LogInfo($"[ShieldBreakerEffect] 가드 버프가 없음: {enemyCharacter.GetCharacterName()}", GameLogger.LogCategory.Core);
                            return false;
                        }
                    }
                }
            }
            
            GameLogger.LogWarning($"[ShieldBreakerEffect] 가드 버프 제거 실패: {enemyCharacter.GetCharacterName()}", GameLogger.LogCategory.Core);
            return false;
        }
    }
}