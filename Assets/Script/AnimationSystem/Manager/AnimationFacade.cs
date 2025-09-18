using UnityEngine;
using Zenject;
using System;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Data; // SlotOwner
using Game.CombatSystem.Slot; // SlotOwner
using Game.SkillCardSystem.Runtime; // RuntimeSkillCard
using System.Collections.Generic; // Added for List
using Game.SkillCardSystem.Slot;
using Game.CoreSystem.Animation;
using Game.AnimationSystem.Interface;
using Game.CoreSystem.Interface;

namespace Game.AnimationSystem.Manager
{
    public class AnimationFacade : MonoBehaviour, IAnimationFacade
    {
        #region Events
        public Action<ISkillCard, string> OnSkillCardAnimationPlayed { get; set; }
        public Action<string, string, bool> OnCharacterAnimationPlayed { get; set; }
        #endregion

        #region Private Fields
        private IAnimationDatabaseManager animationDatabaseManager;
        private Game.CombatSystem.Manager.EnemyHandManager enemyHandManager;
        private Game.CombatSystem.Slot.HandSlotRegistry handSlotRegistry;
        private Game.SkillCardSystem.Manager.PlayerHandManager playerHandManager;
        #endregion

        #region DI
        [Inject]
        public void Construct(
            IAnimationDatabaseManager animationDatabaseManager,
            Game.CombatSystem.Manager.EnemyHandManager enemyHandManager,
            Game.CombatSystem.Slot.HandSlotRegistry handSlotRegistry,
            Game.SkillCardSystem.Manager.PlayerHandManager playerHandManager)
        {
            this.animationDatabaseManager = animationDatabaseManager;
            this.enemyHandManager = enemyHandManager;
            this.handSlotRegistry = handSlotRegistry;
            this.playerHandManager = playerHandManager;
        }
        #endregion

        private void Awake()
        {
            // 캐릭터 생성/사망
            Game.CombatSystem.CombatEvents.OnPlayerCharacterDeath += HandlePlayerCharacterDeath;
            Game.CombatSystem.CombatEvents.OnEnemyCharacterDeath += HandleEnemyCharacterDeath;
            // 스킬카드 이벤트 구독은 제거됨 - ISkillCard 기반 메서드만 사용
        }

        // 캐릭터 사망 애니메이션
        private void HandlePlayerCharacterDeath(Game.CharacterSystem.Data.PlayerCharacterData data, GameObject obj)
        {
            PlayPlayerCharacterDeathAnimation(data.name, obj);
            VanishAllHandCardsOnCharacterDeath(data.name, false); // 플레이어 사망 시 핸드 카드 소멸 애니메이션 호출
        }
        private void HandleEnemyCharacterDeath(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj)
        {
            PlayEnemyCharacterDeathAnimation(data.name, obj);
            VanishAllHandCardsOnCharacterDeath(data.name, true); // 적 사망 시 핸드 카드 소멸 애니메이션 호출
        }
        // 스킬카드 생성 애니메이션 - string cardId 기반 메서드들은 제거됨
        // ISkillCard 기반 메서드만 사용
        // 전투슬롯 판별 (임시)
        private bool IsCombatSlot(Game.CombatSystem.Slot.CombatSlotPosition pos)
        {
            // 실제 전투슬롯 판별 로직 필요
            return true; // 임시로 항상 true
        }

        // 핸드 슬롯 스킬카드 소멸 애니메이션 이벤트 핸들러
        private void HandleHandSkillCardsVanishOnCharacterDeath(bool isPlayer)
        {
            VanishAllHandCardsOnCharacterDeath("Unknown", !isPlayer); // isPlayer가 true면 isEnemy는 false
        }

        // 데이터 로드
        public void LoadAllData() => animationDatabaseManager.ReloadDatabases();

        // 플레이어 캐릭터 애니메이션 실행
        public void PlayPlayerCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"[AnimationFacade] PlayPlayerCharacterAnimation 호출: characterId={characterId}, animationType={animationType}, target={target?.name}");
            animationDatabaseManager.PlayPlayerCharacterAnimation(characterId, animationType, target, onComplete);
        }

        // 적 캐릭터 애니메이션 실행
        public void PlayEnemyCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"[AnimationFacade] PlayEnemyCharacterAnimation 호출: characterId={characterId}, animationType={animationType}, target={target?.name}");
            animationDatabaseManager.PlayEnemyCharacterAnimation(characterId, animationType, target, onComplete);
        }

        // 캐릭터 사망 애니메이션 실행 (플레이어)
        public void PlayPlayerCharacterDeathAnimation(string characterId, GameObject target)
        {
            Debug.Log($"[AnimationFacade] PlayPlayerCharacterDeathAnimation 호출: characterId={characterId}, target={target?.name}");
            var entry = animationDatabaseManager.GetPlayerCharacterAnimationEntry(characterId);
            if (entry == null || entry.DeathAnimation.IsEmpty())
            {
                Debug.LogWarning($"[AnimationFacade] 캐릭터 {characterId}의 사망 애니메이션 타입이 설정되지 않음");
                return;
            }
            animationDatabaseManager.PlayPlayerCharacterAnimation(characterId, "death", target);
        }

        // 캐릭터 사망 애니메이션 실행 (적)
        public void PlayEnemyCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"[AnimationFacade] PlayEnemyCharacterDeathAnimation 호출: characterId={characterId}, target={target?.name}");
            var entry = animationDatabaseManager.GetEnemyCharacterAnimationEntry(characterId);
            if (entry == null || entry.DeathAnimation.IsEmpty())
            {
                Debug.LogWarning($"[AnimationFacade] 적 캐릭터 {characterId}의 사망 애니메이션 타입이 설정되지 않음");
                onComplete?.Invoke();
                return;
            }
            animationDatabaseManager.PlayEnemyCharacterAnimation(characterId, "death", target, onComplete);
        }

        // PlayCharacterAnimation, PlayCharacterDeathAnimation 파사드 메서드 추가
        public void PlayCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null, bool isEnemy = false)
        {
            // Debug.Log($"[AnimationFacade] PlayCharacterAnimation 호출: characterId={characterId}, animationType={animationType}, target={target?.name}, isEnemy={isEnemy}");
            if (isEnemy)
                PlayEnemyCharacterAnimation(characterId, animationType, target, onComplete);
            else
                PlayPlayerCharacterAnimation(characterId, animationType, target, onComplete);
        }
        public void PlayCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null, bool isEnemy = false)
        {
            Debug.Log($"[AnimationFacade] PlayCharacterDeathAnimation 호출: characterId={characterId}, target={target?.name}, isEnemy={isEnemy}");
            if (isEnemy)
                PlayEnemyCharacterDeathAnimation(characterId, target, onComplete);
            else
                PlayPlayerCharacterDeathAnimation(characterId, target);
        }

        // 스킬카드 애니메이션 실행 (통합) - string cardId 오버로드는 제거됨
        // ISkillCard 기반 메서드만 사용

        // ISkillCard 기반 오버로드 (통합 메서드 사용)
        public void PlaySkillCardAnimation(ISkillCard skillCard, GameObject target, string animationType, System.Action onComplete = null)
        {
            // Debug.Log($"[AnimationFacade] PlaySkillCardAnimation(ISkillCard) 호출: skillCard={skillCard?.GetCardName()}, target={target?.name}, animationType={animationType}");
            if (skillCard == null)
            {
                Debug.LogWarning("[AnimationFacade] skillCard가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            // 통합된 메서드 사용
            animationDatabaseManager.PlaySkillCardAnimation(skillCard, target, animationType, onComplete);
        }

        #region Drag Animation Methods
        /// <summary>
        /// 스킬카드 드래그 시작 애니메이션을 실행합니다.
        /// </summary>
        public void PlaySkillCardDragStartAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogWarning("[AnimationFacade] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            animationDatabaseManager.PlaySkillCardAnimation(card, target, "drag", onComplete);
        }

        /// <summary>
        /// 스킬카드 드래그 종료 애니메이션을 실행합니다.
        /// </summary>
        public void PlaySkillCardDragEndAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogWarning("[AnimationFacade] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            animationDatabaseManager.PlaySkillCardAnimation(card, target, "drag", onComplete);
        }

        // string cardId 기반 메서드들은 제거됨 - ISkillCard 기반 메서드만 사용
        #endregion

        #region Drop Animation Methods
        /// <summary>
        /// 스킬카드 드롭 애니메이션을 실행합니다.
        /// </summary>
        public void PlaySkillCardDropAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogWarning("[AnimationFacade] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            animationDatabaseManager.PlaySkillCardAnimation(card, target, "drop", onComplete);
        }

        // string cardId 기반 드롭 메서드들은 제거됨 - ISkillCard 기반 메서드만 사용
        #endregion

        // 상태 출력
        public void PrintStatus() => animationDatabaseManager.DebugDatabaseStatus();

        public bool IsHandVanishAnimationPlaying { get; private set; } = false;

        /// <summary>
        /// 캐릭터 사망 시 해당 캐릭터의 핸드 슬롯에 남아있는 모든 카드를 소멸 애니메이션으로 처리한다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="isEnemy">적 캐릭터 여부</param>
        public void VanishAllHandCardsOnCharacterDeath(string characterId, bool isEnemy = false)
        {
            IsHandVanishAnimationPlaying = true;
            Debug.Log($"[AnimationFacade] VanishAllHandCardsOnCharacterDeath 호출: characterId={characterId}, isEnemy={isEnemy}");

            // [수정] 적 캐릭터의 경우, EnemyHandManager를 통해 직접 소멸을 요청합니다.
            if (isEnemy)
            {
                // Use injected enemyHandManager
                if (enemyHandManager != null)
                {
                    Debug.Log($"[AnimationFacade] EnemyHandManager를 통해 모든 적 핸드 카드 소멸 시작");
                    enemyHandManager.VanishAllCardsForDeathAnimation(() => {
                        IsHandVanishAnimationPlaying = false;
                    });
                }
                else
                {
                    Debug.LogWarning("[AnimationFacade] EnemyHandManager를 찾을 수 없어 소멸 애니메이션을 실행할 수 없습니다.");
                    IsHandVanishAnimationPlaying = false;
                }
                return; // 새로운 로직 실행 후 반드시 종료
            }

            Debug.Log($"[AnimationFacade] (사망) 핸드 슬롯 전체 소멸 시작: 플레이어={!isEnemy}");
            // Use injected handSlotRegistry
            var ownerType = !isEnemy ? SlotOwner.PLAYER : SlotOwner.ENEMY;
            var handSlots = handSlotRegistry?.GetHandSlots(ownerType);
            var skillCards = new List<GameObject>();
            var slotPositions = new List<Game.SkillCardSystem.Slot.SkillCardSlotPosition>();
            if (handSlots != null)
            {
                foreach (var slot in handSlots)
                {
                    var cardUI = slot.GetCardUI();
                    var slotPos = slot.GetSlotPosition();
                    var card = slot.GetCard();
                    string cardUIName = (cardUI is UnityEngine.MonoBehaviour mb2 && mb2 != null) ? mb2.name : (cardUI != null ? cardUI.GetType().Name : "null");
                    string cardName = card != null ? card.GetCardName() : "null";
                    Debug.Log($"[VanishDebug] 슬롯: {slotPos}, 카드: {cardName}, 카드UI: {cardUIName}");
                    if (cardUI is UnityEngine.MonoBehaviour mb && mb != null)
                    {
                        skillCards.Add(mb.gameObject);
                        slotPositions.Add(slotPos);
                    }
                }
            }
            if (skillCards.Count == 0)
            {
                Debug.Log($"[AnimationFacade] 소멸할 스킬카드가 없습니다 (isEnemy={isEnemy})");
                IsHandVanishAnimationPlaying = false;
                return;
            }
            int finished = 0;
            for (int i = 0; i < skillCards.Count; i++)
            {
                var cardObj = skillCards[i];
                var slotPos = slotPositions[i];
                var vanishAnim = cardObj.GetComponent<AnimationSystem.Animator.SkillCardAnimation.VanishAnimation.DefaultSkillCardVanishAnimation>() ?? cardObj.AddComponent<AnimationSystem.Animator.SkillCardAnimation.VanishAnimation.DefaultSkillCardVanishAnimation>();
                vanishAnim.PlayVanishAnimation(() => {
                    if (!isEnemy)
                    {
                        // Use injected playerHandManager
                        playerHandManager?.RemoveCardUIAndReferences(slotPos);
                    }
                    else
                    {
                        // 이 경로는 더 이상 사용되지 않습니다.
                    }
                    finished++;
                    if (finished == skillCards.Count)
                    {
                        IsHandVanishAnimationPlaying = false;
                    }
                });
            }
        }
        
        /// <summary>
        /// 특정 캐릭터의 스킬카드들을 찾습니다.
        /// </summary>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="isPlayerCharacter">플레이어 캐릭터 여부</param>
        /// <returns>스킬카드 GameObject 리스트</returns>
        private List<GameObject> FindCharacterSkillCards(string characterName, bool isPlayerCharacter)
        {
            var skillCards = new List<GameObject>();
            
            // 플레이어/적 스킬카드 슬롯들에서 해당 캐릭터의 카드들을 찾기
            var cardSlots = FindObjectsByType<Game.SkillCardSystem.UI.SkillCardUI>(FindObjectsSortMode.None);
            
            foreach (var cardSlot in cardSlots)
            {
                if (cardSlot == null || cardSlot.GetCard() == null) continue;
                
                // 스킬카드가 특정 캐릭터에 속하는지 확인
                if (IsSkillCardBelongsToCharacter(cardSlot.GetCard(), characterName, isPlayerCharacter))
                {
                    skillCards.Add(cardSlot.gameObject);
                }
            }
            
            return skillCards;
        }
        
        /// <summary>
        /// 스킬카드가 특정 캐릭터에 속하는지 확인합니다.
        /// </summary>
        /// <param name="skillCard">스킬카드</param>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="isPlayerCharacter">플레이어 캐릭터 여부</param>
        /// <returns>캐릭터에 속하는지 여부</returns>
        private bool IsSkillCardBelongsToCharacter(Game.SkillCardSystem.Interface.ISkillCard skillCard, string characterName, bool isPlayerCharacter)
        {
            // 스킬카드의 소유자 정보를 확인
            // 실제 구현에서는 스킬카드에 캐릭터 정보가 포함되어 있어야 함
            // 현재는 간단히 플레이어/적 구분만으로 처리
            
            if (isPlayerCharacter)
            {
                // 플레이어 캐릭터의 스킬카드인지 확인
                return skillCard.GetCardName().Contains("Player") || 
                       skillCard.GetCardName().Contains(characterName);
            }
            else
            {
                // 적 캐릭터의 스킬카드인지 확인
                return skillCard.GetCardName().Contains("Enemy") || 
                       skillCard.GetCardName().Contains(characterName);
            }
        }
        
        #region 추가 인터페이스 메서드 구현
        
        // VanishAllHandCardsOnCharacterDeath 메서드는 위에서 이미 구현됨
        
        #endregion
    }
} 