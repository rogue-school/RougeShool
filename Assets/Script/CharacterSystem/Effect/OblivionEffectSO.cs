using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.SkillCardSystem.Data;
using Game.CoreSystem.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;

namespace Game.CharacterSystem.Effect
{
    /// <summary>
    /// 망각 패시브 효과입니다.
    /// 적의 스킬 카드가 대기 슬롯 1로 이동했을 때 시각적 정보를 랜덤한 다른 스킬로 교란합니다.
    /// 실제 카드 스킬은 원래 정보로 작동합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "OblivionEffect", menuName = "Game/Character/Effect/Oblivion Effect")]
    public class OblivionEffectSO : CharacterEffectSO
    {
        private ICharacter character;
        private ICardSlotRegistry slotRegistry;
        private List<SkillCardDefinition> availableCardDefinitions = new List<SkillCardDefinition>();
        private Dictionary<ISkillCard, SkillCardDefinition> obfuscatedCards = new Dictionary<ISkillCard, SkillCardDefinition>();

        public override void Initialize(ICharacter character)
        {
            this.character = character;
            
            // ICardSlotRegistry를 Zenject로 resolve
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
            
            // 사용 가능한 카드 정의 목록 수집 (적 덱에서)
            CollectAvailableCardDefinitions();
            
            // 이벤트 구독
            Game.CombatSystem.CombatEvents.OnEnemyCardMoved += OnEnemyCardMoved;
        }

        public override void OnHealthChanged(ICharacter character, int previousHP, int currentHP)
        {
            // 체력 변경 시 동작 없음
        }

        public override void OnDeath(ICharacter character)
        {
            // 사망 시 모든 교란 해제
            ClearAllObfuscations();
        }

        public override void Cleanup(ICharacter character)
        {
            // 이벤트 구독 해제
            Game.CombatSystem.CombatEvents.OnEnemyCardMoved -= OnEnemyCardMoved;
            
            // 모든 교란 해제
            ClearAllObfuscations();
            
            GameLogger.LogInfo($"[OblivionEffectSO] {character.GetCharacterName()} 망각 패시브 정리 완료", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 적 덱에서 사용 가능한 카드 정의 목록을 수집합니다.
        /// </summary>
        private void CollectAvailableCardDefinitions()
        {
            availableCardDefinitions.Clear();
            
            if (character is Game.CharacterSystem.Core.EnemyCharacter enemyCharacter)
            {
                var enemyData = enemyCharacter.CharacterData as Game.CharacterSystem.Data.EnemyCharacterData;
                if (enemyData?.EnemyDeck != null)
                {
                    var allCards = enemyData.EnemyDeck.GetAllCards();
                    foreach (var entry in allCards)
                    {
                        if (entry.definition != null && !availableCardDefinitions.Contains(entry.definition))
                        {
                            availableCardDefinitions.Add(entry.definition);
                        }
                    }
                }
            }
            
            // 사용 가능한 카드가 없으면 경고
            if (availableCardDefinitions.Count == 0)
            {
                GameLogger.LogWarning("[OblivionEffectSO] 사용 가능한 카드 정의를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 적 카드가 이동했을 때 호출됩니다.
        /// 대기 슬롯 1로 이동한 적 카드의 시각적 정보를 교란합니다.
        /// </summary>
        private void OnEnemyCardMoved(string cardId, GameObject cardObj, CombatSlotPosition position)
        {
            // 대기 슬롯 1에서만 작동
            if (position != CombatSlotPosition.WAIT_SLOT_1)
            {
                return;
            }

            if (slotRegistry == null)
            {
                GameLogger.LogWarning("[OblivionEffectSO] CardSlotRegistry를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                return;
            }

            // 대기 슬롯 1의 카드 가져오기
            var card = slotRegistry.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_1);
            if (card == null || card.IsFromPlayer())
            {
                return; // 적 카드가 아니면 무시
            }

            // 이미 교란된 카드인지 확인
            if (obfuscatedCards.ContainsKey(card))
            {
                return; // 이미 교란됨
            }

            // 카드 UI 가져오기
            var cardUI = slotRegistry.GetCardUIInSlot(CombatSlotPosition.WAIT_SLOT_1);
            if (cardUI == null)
            {
                GameLogger.LogWarning("[OblivionEffectSO] 카드 UI를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                return;
            }

            // 랜덤한 다른 카드 정의 선택 (원본 카드 제외)
            var originalDefinition = card.CardDefinition;
            var randomDefinition = GetRandomCardDefinition(originalDefinition);
            
            if (randomDefinition != null)
            {
                // 시각적 정보 교란
                ObfuscateCardVisual(cardUI, randomDefinition);
                obfuscatedCards[card] = randomDefinition;
                
                GameLogger.LogInfo($"[OblivionEffectSO] 카드 시각적 정보 교란: {originalDefinition?.displayName} → {randomDefinition.displayName}", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 원본 카드를 제외한 랜덤한 카드 정의를 반환합니다.
        /// </summary>
        private SkillCardDefinition GetRandomCardDefinition(SkillCardDefinition exclude)
        {
            if (availableCardDefinitions.Count == 0)
            {
                return null;
            }

            // 원본 카드를 제외한 목록 생성
            var candidates = availableCardDefinitions.Where(def => def != exclude).ToList();
            
            if (candidates.Count == 0)
            {
                // 제외할 카드가 없으면 전체 목록에서 선택
                candidates = availableCardDefinitions;
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            return candidates[Random.Range(0, candidates.Count)];
        }

        /// <summary>
        /// 카드 UI의 시각적 정보를 교란합니다.
        /// 리플렉션을 사용하여 private 필드에 접근합니다.
        /// </summary>
        private void ObfuscateCardVisual(SkillCardUI cardUI, SkillCardDefinition fakeDefinition)
        {
            if (cardUI == null || fakeDefinition == null)
            {
                return;
            }

            // 리플렉션을 사용하여 private 필드에 접근
            var cardNameTextField = typeof(SkillCardUI).GetField("cardNameText", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var descriptionTextField = typeof(SkillCardUI).GetField("descriptionText", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var damageTextField = typeof(SkillCardUI).GetField("damageText", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var cardArtImageField = typeof(SkillCardUI).GetField("cardArtImage", 
                BindingFlags.NonPublic | BindingFlags.Instance);

            // 카드명 교란
            if (cardNameTextField != null)
            {
                var cardNameText = cardNameTextField.GetValue(cardUI) as TextMeshProUGUI;
                if (cardNameText != null)
                {
                    string fakeName = !string.IsNullOrEmpty(fakeDefinition.displayNameKO)
                        ? fakeDefinition.displayNameKO
                        : fakeDefinition.displayName;
                    cardNameText.text = fakeName;
                }
            }

            // 설명 교란
            if (descriptionTextField != null)
            {
                var descriptionText = descriptionTextField.GetValue(cardUI) as TextMeshProUGUI;
                if (descriptionText != null)
                {
                    descriptionText.text = fakeDefinition.description ?? "";
                }
            }

            // 데미지 교란 (데미지가 있는 경우)
            if (damageTextField != null)
            {
                var damageText = damageTextField.GetValue(cardUI) as TextMeshProUGUI;
                if (damageText != null)
                {
                    if (fakeDefinition.configuration?.hasDamage == true)
                    {
                        int fakeDamage = fakeDefinition.configuration.damageConfig.baseDamage;
                        damageText.text = fakeDamage.ToString();
                    }
                    else
                    {
                        damageText.text = "";
                    }
                }
            }

            // 아트워크 교란
            if (cardArtImageField != null)
            {
                var cardArtImage = cardArtImageField.GetValue(cardUI) as UnityEngine.UI.Image;
                if (cardArtImage != null && fakeDefinition.artwork != null)
                {
                    cardArtImage.sprite = fakeDefinition.artwork;
                }
            }
        }

        /// <summary>
        /// 모든 교란을 해제합니다.
        /// </summary>
        private void ClearAllObfuscations()
        {
            if (slotRegistry == null)
            {
                return;
            }

            // 모든 슬롯에서 교란된 카드 찾아서 복원
            var allSlots = new[]
            {
                CombatSlotPosition.BATTLE_SLOT,
                CombatSlotPosition.WAIT_SLOT_1,
                CombatSlotPosition.WAIT_SLOT_2,
                CombatSlotPosition.WAIT_SLOT_3,
                CombatSlotPosition.WAIT_SLOT_4
            };

            foreach (var slot in allSlots)
            {
                var card = slotRegistry.GetCardInSlot(slot);
                if (card != null && obfuscatedCards.ContainsKey(card))
                {
                    var cardUI = slotRegistry.GetCardUIInSlot(slot);
                    if (cardUI != null)
                    {
                        // 원래 정보로 복원
                        cardUI.SetCard(card);
                        obfuscatedCards.Remove(card);
                    }
                }
            }

            obfuscatedCards.Clear();
        }
    }
}

