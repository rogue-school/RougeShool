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
    /// 적의 스킬 카드가 대기 슬롯 4, 3, 2에 배치될 때 시각적 정보를 망각 이미지로 교체합니다.
    /// 실제 카드 스킬은 원래 정보로 작동합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "OblivionEffect", menuName = "Game/Character/Effect/Oblivion Effect")]
    public class OblivionEffectSO : CharacterEffectSO
    {
        private ICharacter character;
        private ICardSlotRegistry slotRegistry;
        private List<SkillCardDefinition> availableCardDefinitions = new List<SkillCardDefinition>();
        private Dictionary<ISkillCard, CombatSlotPosition> obfuscatedCards = new Dictionary<ISkillCard, CombatSlotPosition>();
        private Sprite oblivionSprite;
        private const string OBLIVION_IMAGE_PATH = "Image/스킬 아이콘-1차 1/2. 기믹 스킬/Skill_204_망각";
        private const string OBLIVION_DESCRIPTION = "망각: 이 카드의 정보는 망각되어 보이지 않습니다.";

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
            
            // 망각 이미지 로드
            LoadOblivionSprite();
            
            // 사용 가능한 카드 정의 목록 수집 (적 덱에서)
            CollectAvailableCardDefinitions();
            
            // 이벤트 구독
            Game.CombatSystem.CombatEvents.OnEnemyCardMoved += OnEnemyCardMoved;
            Game.CombatSystem.CombatEvents.OnEnemyCardSpawn += OnEnemyCardSpawn;
            
            // 초기 슬롯 상태 확인
            CheckInitialSlots();
        }
        
        /// <summary>
        /// 망각 이미지를 로드합니다.
        /// </summary>
        private void LoadOblivionSprite()
        {
            oblivionSprite = Resources.Load<Sprite>(OBLIVION_IMAGE_PATH);
            if (oblivionSprite == null)
            {
                GameLogger.LogWarning($"[OblivionEffectSO] 망각 이미지를 로드할 수 없습니다: {OBLIVION_IMAGE_PATH}", GameLogger.LogCategory.Character);
            }
            else
            {
                GameLogger.LogInfo("[OblivionEffectSO] 망각 이미지 로드 완료", GameLogger.LogCategory.Character);
            }
        }
        
        /// <summary>
        /// 초기 슬롯 상태를 확인하고 망각 효과를 적용합니다.
        /// 대기 슬롯 4, 3, 2에 있는 적 카드에 망각 효과를 적용합니다.
        /// </summary>
        private void CheckInitialSlots()
        {
            if (slotRegistry == null) return;
            
            var waitSlots = new[]
            {
                CombatSlotPosition.WAIT_SLOT_4,
                CombatSlotPosition.WAIT_SLOT_3,
                CombatSlotPosition.WAIT_SLOT_2
            };
            
            foreach (var slot in waitSlots)
            {
                var card = slotRegistry.GetCardInSlot(slot);
                if (card != null && !card.IsFromPlayer() && !obfuscatedCards.ContainsKey(card))
                {
                    // 약간의 지연 후 적용 (UI 생성 완료 대기)
                    UnityEngine.Object.FindFirstObjectByType<MonoBehaviour>()?.StartCoroutine(DelayedApplyOblivion(card, slot));
                }
            }
        }
        
        private System.Collections.IEnumerator DelayedApplyOblivion(ISkillCard card, CombatSlotPosition slot)
        {
            yield return new WaitForEndOfFrame();
            ApplyOblivionToCard(card, slot);
        }
        
        /// <summary>
        /// 적 카드가 생성되었을 때 호출됩니다.
        /// 생성 시점부터 망각 효과를 적용합니다.
        /// </summary>
        private void OnEnemyCardSpawn(string cardId, GameObject cardObj)
        {
            if (slotRegistry == null) return;
            
            // 약간의 지연 후 적용 (UI 생성 완료 대기)
            UnityEngine.Object.FindFirstObjectByType<MonoBehaviour>()?.StartCoroutine(DelayedCheckAndApplyOblivion());
        }
        
        private System.Collections.IEnumerator DelayedCheckAndApplyOblivion()
        {
            yield return new WaitForEndOfFrame();
            
            if (slotRegistry == null) yield break;
            
            // 모든 대기 슬롯에서 새로 생성된 적 카드 확인 (대기 슬롯 1 제외)
            var waitSlots = new[]
            {
                CombatSlotPosition.WAIT_SLOT_4,
                CombatSlotPosition.WAIT_SLOT_3,
                CombatSlotPosition.WAIT_SLOT_2
            };
            
            foreach (var slot in waitSlots)
            {
                var card = slotRegistry.GetCardInSlot(slot);
                if (card != null && !card.IsFromPlayer() && !obfuscatedCards.ContainsKey(card))
                {
                    ApplyOblivionToCard(card, slot);
                }
            }
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
            Game.CombatSystem.CombatEvents.OnEnemyCardSpawn -= OnEnemyCardSpawn;
            
            // 모든 망각 효과 해제
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
        /// 대기 슬롯 1에 도착하면 망각 효과를 해제합니다.
        /// </summary>
        private void OnEnemyCardMoved(string cardId, GameObject cardObj, CombatSlotPosition position)
        {
            if (slotRegistry == null)
            {
                GameLogger.LogWarning("[OblivionEffectSO] CardSlotRegistry를 찾을 수 없습니다.", GameLogger.LogCategory.Character);
                return;
            }

            // 대기 슬롯 1에 도착하면 망각 해제
            if (position == CombatSlotPosition.WAIT_SLOT_1)
            {
                var card = slotRegistry.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_1);
                if (card != null && !card.IsFromPlayer() && obfuscatedCards.ContainsKey(card))
                {
                    // 약간의 지연 후 해제 (UI 이동 완료 대기)
                    UnityEngine.Object.FindFirstObjectByType<MonoBehaviour>()?.StartCoroutine(DelayedRemoveOblivion(card));
                }
            }
            // 다른 슬롯(4, 3, 2)으로 이동한 경우, 망각 효과 유지 또는 적용
            else if (position == CombatSlotPosition.WAIT_SLOT_4 ||
                     position == CombatSlotPosition.WAIT_SLOT_3 ||
                     position == CombatSlotPosition.WAIT_SLOT_2)
            {
                var card = slotRegistry.GetCardInSlot(position);
                if (card != null && !card.IsFromPlayer() && !obfuscatedCards.ContainsKey(card))
                {
                    // 망각 효과 적용
                    ApplyOblivionToCard(card, position);
                }
            }
        }
        
        private System.Collections.IEnumerator DelayedRemoveOblivion(ISkillCard card)
        {
            yield return new WaitForEndOfFrame();
            RemoveOblivionFromCard(card);
        }
        
        /// <summary>
        /// 카드에 망각 효과를 적용합니다.
        /// </summary>
        private void ApplyOblivionToCard(ISkillCard card, CombatSlotPosition slot)
        {
            if (card == null || slotRegistry == null) return;
            
            var cardUI = slotRegistry.GetCardUIInSlot(slot);
            if (cardUI == null)
            {
                GameLogger.LogWarning($"[OblivionEffectSO] 카드 UI를 찾을 수 없습니다. 슬롯: {slot}", GameLogger.LogCategory.Character);
                return;
            }

            // 망각 효과 적용
            ObfuscateCardVisual(cardUI);
            obfuscatedCards[card] = slot;
            
            GameLogger.LogInfo($"[OblivionEffectSO] 망각 효과 적용: {card.GetCardName()} (슬롯: {slot})", GameLogger.LogCategory.Character);
        }
        
        /// <summary>
        /// 카드에서 망각 효과를 제거합니다.
        /// 대기 슬롯 1에 있을 때만 해제합니다.
        /// </summary>
        private void RemoveOblivionFromCard(ISkillCard card)
        {
            if (card == null || slotRegistry == null) return;
            
            if (!obfuscatedCards.TryGetValue(card, out var previousSlot))
            {
                return; // 망각되지 않은 카드
            }
            
            // slotRegistry에서 현재 슬롯 확인 (대기 슬롯 1에 있을 때만 해제)
            var cardInSlot1 = slotRegistry.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_1);
            if (cardInSlot1 != card)
            {
                // 대기 슬롯 1에 이 카드가 없으면 망각 유지
                return;
            }
            
            var cardUI = slotRegistry.GetCardUIInSlot(CombatSlotPosition.WAIT_SLOT_1);
            if (cardUI != null)
            {
                // 원래 정보로 복원
                cardUI.SetCard(card);
            }
            
            obfuscatedCards.Remove(card);
            GameLogger.LogInfo($"[OblivionEffectSO] 망각 효과 제거: {card.GetCardName()} (대기 슬롯 1 도착)", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 카드 UI의 시각적 정보를 망각 이미지로 교체합니다.
        /// 리플렉션을 사용하여 private 필드에 접근합니다.
        /// </summary>
        private void ObfuscateCardVisual(SkillCardUI cardUI)
        {
            if (cardUI == null)
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

            // 카드명 숨김
            if (cardNameTextField != null)
            {
                var cardNameText = cardNameTextField.GetValue(cardUI) as TextMeshProUGUI;
                if (cardNameText != null)
                {
                    cardNameText.text = "";
                }
            }

            // 설명 숨김
            if (descriptionTextField != null)
            {
                var descriptionText = descriptionTextField.GetValue(cardUI) as TextMeshProUGUI;
                if (descriptionText != null)
                {
                    descriptionText.text = "";
                }
            }

            // 데미지 숨김
            if (damageTextField != null)
            {
                var damageText = damageTextField.GetValue(cardUI) as TextMeshProUGUI;
                if (damageText != null)
                {
                    damageText.text = "";
                }
            }

            // 아트워크를 망각 이미지로 교체
            if (cardArtImageField != null)
            {
                var cardArtImage = cardArtImageField.GetValue(cardUI) as UnityEngine.UI.Image;
                if (cardArtImage != null && oblivionSprite != null)
                {
                    cardArtImage.sprite = oblivionSprite;
                }
            }
        }
        
        /// <summary>
        /// 카드가 망각 상태인지 확인합니다.
        /// </summary>
        public bool IsCardObfuscated(ISkillCard card)
        {
            return card != null && obfuscatedCards.ContainsKey(card);
        }

        /// <summary>
        /// 모든 망각 효과를 해제합니다.
        /// </summary>
        private void ClearAllObfuscations()
        {
            if (slotRegistry == null)
            {
                return;
            }

            // 모든 슬롯에서 망각된 카드 찾아서 복원
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
                    RemoveOblivionFromCard(card);
                }
            }

            obfuscatedCards.Clear();
        }
    }
}


