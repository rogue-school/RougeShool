using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using UnityEngine;
using System.Collections;
using System.Linq;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 운명의 실 효과를 적용하는 커맨드.
    /// 플레이어에게 운명의 실 디버프를 적용하고, 핸드에서 3장을 뽑아 2개를 제거한 후 1개를 전투 슬롯으로 이동시킵니다.
    /// </summary>
    public class ThreadOfFateEffectCommand : ICardEffectCommand
    {
        private readonly int duration;
        private readonly Sprite icon;

        public ThreadOfFateEffectCommand(int duration = 1, Sprite icon = null)
        {
            this.duration = duration;
            this.icon = icon;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Target is not ICharacter target)
            {
                GameLogger.LogWarning("[ThreadOfFateEffectCommand] 대상이 캐릭터가 아니거나 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 플레이어에게만 적용
            if (!target.IsPlayerControlled())
            {
                GameLogger.LogWarning("[ThreadOfFateEffectCommand] 플레이어가 아닌 대상에게 운명의 실을 적용하려고 시도했습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // SkillCardDefinition의 커스텀 설정이 있으면 우선 사용
            int finalDuration = duration;
            Sprite finalIcon = icon;
            
            if (context.Card?.CardDefinition != null)
            {
                var cfg = context.Card.CardDefinition.configuration;
                if (cfg != null && cfg.hasEffects)
                {
                    // 해당 카드의 EffectConfiguration 중 ThreadOfFateEffectSO 항목의 커스텀 설정을 확인
                    foreach (var eff in cfg.effects)
                    {
                        if (eff?.effectSO is ThreadOfFateEffectSO && eff.useCustomSettings)
                        {
                            // 커스텀 설정에서 duration 읽기
                            finalDuration = eff.customSettings.threadOfFateDuration;
                            GameLogger.LogInfo($"[ThreadOfFateEffectCommand] 커스텀 설정 사용: duration={finalDuration}", GameLogger.LogCategory.SkillCard);
                            break;
                        }
                    }
                }
            }

            // 운명의 실 디버프 적용
            var debuff = new ThreadOfFateDebuff(finalDuration, finalIcon);
            target.RegisterPerTurnEffect(debuff);
            
            GameLogger.LogInfo($"[ThreadOfFateEffectCommand] {target.GetCharacterName()}에게 운명의 실 디버프 적용 ({finalDuration}턴). 플레이어 턴 시작 시 효과가 실행됩니다.", GameLogger.LogCategory.SkillCard);

            // 핸드 카드 처리는 플레이어 턴 시작 시 ThreadOfFateDebuff의 OnTurnStart에서 처리됨
            // (PlayerCharacter.ProcessTurnEffectsCoroutine에서 코루틴으로 실행)
        }

        /// <summary>
        /// 핸드에서 3장을 뽑고 2개를 제거한 후 1개를 전투 슬롯으로 이동시키는 코루틴입니다.
        /// </summary>
        private IEnumerator ProcessHandCardsCoroutine(ICharacter player)
        {
            // PlayerHandManager 찾기
            var handManager = UnityEngine.Object.FindFirstObjectByType<Game.SkillCardSystem.Manager.PlayerHandManager>();
            
            // ICardSlotRegistry를 Zenject로 resolve
            ICardSlotRegistry slotRegistry = null;
            var sceneContext = UnityEngine.Object.FindFirstObjectByType<Zenject.SceneContext>();
            if (sceneContext != null && sceneContext.Container != null)
            {
                slotRegistry = sceneContext.Container.TryResolve<ICardSlotRegistry>();
            }
            
            if (slotRegistry == null)
            {
                // ProjectContext에서 시도
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    slotRegistry = projectContext.Container.TryResolve<ICardSlotRegistry>();
                }
            }
            
            if (handManager == null || slotRegistry == null)
            {
                GameLogger.LogWarning("[ThreadOfFateEffectCommand] 필요한 매니저를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                yield break;
            }

            // 리플렉션을 사용하여 circulationSystem 접근
            var circulationSystemField = typeof(Game.SkillCardSystem.Manager.PlayerHandManager).GetField("circulationSystem",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (circulationSystemField == null)
            {
                GameLogger.LogWarning("[ThreadOfFateEffectCommand] circulationSystem 필드를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                yield break;
            }

            var circulationSystem = circulationSystemField.GetValue(handManager) as Game.SkillCardSystem.Interface.ICardCirculationSystem;
            if (circulationSystem == null)
            {
                GameLogger.LogWarning("[ThreadOfFateEffectCommand] CardCirculationSystem을 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                yield break;
            }

            // 3장 드로우
            var drawnCards = circulationSystem.DrawCardsForTurn();
            if (drawnCards == null || drawnCards.Count < 3)
            {
                GameLogger.LogWarning("[ThreadOfFateEffectCommand] 3장을 드로우할 수 없습니다. (드로우된 카드 수: {drawnCards?.Count ?? 0})", GameLogger.LogCategory.SkillCard);
                yield break;
            }

            // 3장 중 랜덤으로 1장 선택 (나머지 2장은 제거)
            var selectedCard = drawnCards[Random.Range(0, Mathf.Min(3, drawnCards.Count))];
            var cardsToRemove = drawnCards.Where(c => c != selectedCard).Take(2).ToList();

            // 2장 제거 (핸드에서 제거만 하고 순환 시스템에는 그대로 유지)
            foreach (var card in cardsToRemove)
            {
                handManager.RemoveCard(card);
                GameLogger.LogInfo($"[ThreadOfFateEffectCommand] 카드 제거: {card.GetCardName()}", GameLogger.LogCategory.SkillCard);
            }

            // 애니메이션 대기
            yield return new WaitForSeconds(0.5f);

            // 선택된 카드를 전투 슬롯으로 이동
            if (selectedCard != null)
            {
                // SlotRegistry를 통해 전투 슬롯 찾기
                var slotRegistryMono = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Slot.SlotRegistry>();
                ICombatCardSlot battleSlot = null;
                
                if (slotRegistryMono != null)
                {
                    battleSlot = slotRegistryMono.GetCombatSlot(CombatSlotPosition.BATTLE_SLOT);
                }

                if (battleSlot != null)
                {
                    // 카드 UI 프리팹 가져오기 (리플렉션 사용)
                    var cardUIPrefabField = typeof(Game.SkillCardSystem.Manager.PlayerHandManager).GetField("cardUIPrefab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var cardUIPrefab = cardUIPrefabField?.GetValue(handManager) as Game.SkillCardSystem.UI.SkillCardUI;
                    
                    if (cardUIPrefab == null)
                    {
                        // 캐시된 프리팹 확인
                        var cachedPrefabField = typeof(Game.SkillCardSystem.Manager.PlayerHandManager).GetField("_cachedCardUIPrefab",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        cardUIPrefab = cachedPrefabField?.GetValue(null) as Game.SkillCardSystem.UI.SkillCardUI;
                    }
                    
                    if (cardUIPrefab != null)
                    {
                        // battleSlot이 MonoBehaviour인지 확인
                        Transform slotTransform = null;
                        if (battleSlot is MonoBehaviour slotMono)
                        {
                            slotTransform = slotMono.transform;
                        }
                        else
                        {
                            GameLogger.LogWarning("[ThreadOfFateEffectCommand] 전투 슬롯이 MonoBehaviour가 아닙니다.", GameLogger.LogCategory.SkillCard);
                            slotRegistry.RegisterCard(CombatSlotPosition.BATTLE_SLOT, selectedCard, null, SlotOwner.PLAYER);
                            yield break;
                        }
                        
                        // SkillCardUIFactory를 사용하여 UI 생성
                        var cardUI = Game.SkillCardSystem.UI.SkillCardUIFactory.CreateUI(
                            cardUIPrefab, 
                            slotTransform, 
                            selectedCard, 
                            null, 
                            null);
                        
                        if (cardUI != null)
                        {
                            slotRegistry.RegisterCard(CombatSlotPosition.BATTLE_SLOT, selectedCard, cardUI, SlotOwner.PLAYER);
                            GameLogger.LogInfo($"[ThreadOfFateEffectCommand] 카드를 전투 슬롯으로 이동: {selectedCard.GetCardName()}", GameLogger.LogCategory.SkillCard);
                        }
                        else
                        {
                            // 폴백: UI 없이 데이터만 등록
                            slotRegistry.RegisterCard(CombatSlotPosition.BATTLE_SLOT, selectedCard, null, SlotOwner.PLAYER);
                            GameLogger.LogWarning("[ThreadOfFateEffectCommand] 카드 UI 생성 실패. UI 없이 등록합니다.", GameLogger.LogCategory.SkillCard);
                        }
                    }
                    else
                    {
                        // 폴백: UI 없이 데이터만 등록
                        slotRegistry.RegisterCard(CombatSlotPosition.BATTLE_SLOT, selectedCard, null, SlotOwner.PLAYER);
                        GameLogger.LogWarning("[ThreadOfFateEffectCommand] SkillCardUI 프리팹을 찾을 수 없습니다. UI 없이 등록합니다.", GameLogger.LogCategory.SkillCard);
                    }
                }
                else
                {
                    // 폴백: UI 없이 데이터만 등록
                    slotRegistry.RegisterCard(CombatSlotPosition.BATTLE_SLOT, selectedCard, null, SlotOwner.PLAYER);
                    GameLogger.LogWarning("[ThreadOfFateEffectCommand] 전투 슬롯을 찾을 수 없습니다. UI 없이 등록합니다.", GameLogger.LogCategory.SkillCard);
                }
            }
        }
    }
}

