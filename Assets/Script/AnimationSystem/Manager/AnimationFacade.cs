using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using System.Collections.Generic; // Added for List

namespace AnimationSystem.Manager
{
    public class AnimationFacade : MonoBehaviour
    {
        public static AnimationFacade Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // 데이터 로드
        public void LoadAllData() => AnimationDatabaseManager.Instance.ReloadDatabases();

        // 플레이어 캐릭터 애니메이션 실행
        public void PlayPlayerCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
            => AnimationDatabaseManager.Instance.PlayPlayerCharacterAnimation(characterId, target, animationType, onComplete);

        // 적 캐릭터 애니메이션 실행
        public void PlayEnemyCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
            => AnimationDatabaseManager.Instance.PlayEnemyCharacterAnimation(characterId, target, animationType, onComplete);

        // 캐릭터 사망 애니메이션 실행 (플레이어)
        public void PlayPlayerCharacterDeathAnimation(string characterId, GameObject target)
        {
            var entry = AnimationDatabaseManager.Instance.GetPlayerCharacterAnimationEntry(characterId);
            if (entry == null || entry.DeathAnimation.IsEmpty())
            {
                Debug.LogWarning($"[AnimationFacade] 캐릭터 {characterId}의 사망 애니메이션 타입이 설정되지 않음");
                return;
            }
            AnimationDatabaseManager.Instance.PlayPlayerCharacterAnimation(characterId, target, "death");
        }

        // 캐릭터 사망 애니메이션 실행 (적)
        public void PlayEnemyCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null)
        {
            var entry = AnimationDatabaseManager.Instance.GetEnemyCharacterAnimationEntry(characterId);
            if (entry == null || entry.DeathAnimation.IsEmpty())
            {
                Debug.LogWarning($"[AnimationFacade] 적 캐릭터 {characterId}의 사망 애니메이션 타입이 설정되지 않음");
                onComplete?.Invoke();
                return;
            }
            AnimationDatabaseManager.Instance.PlayEnemyCharacterAnimation(characterId, target, "death", onComplete);
        }

        // PlayCharacterAnimation, PlayCharacterDeathAnimation 파사드 메서드 추가
        public void PlayCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null, bool isEnemy = false)
        {
            if (isEnemy)
                PlayEnemyCharacterAnimation(characterId, animationType, target, onComplete);
            else
                PlayPlayerCharacterAnimation(characterId, animationType, target, onComplete);
        }
        public void PlayCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null, bool isEnemy = false)
        {
            if (isEnemy)
                PlayEnemyCharacterDeathAnimation(characterId, target, onComplete);
            else
                PlayPlayerCharacterDeathAnimation(characterId, target);
        }

        // 스킬카드 애니메이션 실행
        public void PlaySkillCardAnimation(string cardId, string animationType, GameObject target)
            => AnimationDatabaseManager.Instance.PlayPlayerSkillCardAnimation(cardId, target, animationType);
        public void PlaySkillCardAnimation(string cardId, string animationType, GameObject target, System.Action onComplete)
            => AnimationDatabaseManager.Instance.PlayPlayerSkillCardAnimation(cardId, target, animationType, onComplete);

        // ISkillCard 기반 오버로드 추가
        public void PlaySkillCardAnimation(ISkillCard card, string animationType, GameObject target, System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogWarning("[AnimationFacade] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            var owner = card.GetOwner();
            if (owner == SlotOwner.ENEMY)
                AnimationDatabaseManager.Instance.PlayEnemySkillCardAnimation(card.CardData.Name, target, animationType, onComplete);
            else
                AnimationDatabaseManager.Instance.PlayPlayerSkillCardAnimation(card.CardData.Name, target, animationType, onComplete);
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
            AnimationDatabaseManager.Instance.PlaySkillCardDragStartAnimation(card, target, onComplete);
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
            AnimationDatabaseManager.Instance.PlaySkillCardDragEndAnimation(card, target, onComplete);
        }

        /// <summary>
        /// 플레이어 스킬카드 드래그 시작 애니메이션을 실행합니다.
        /// </summary>
        public void PlayPlayerSkillCardDragStartAnimation(string cardId, GameObject target, System.Action onComplete = null)
        {
            AnimationDatabaseManager.Instance.PlayPlayerSkillCardDragStartAnimation(cardId, target, onComplete);
        }

        /// <summary>
        /// 플레이어 스킬카드 드래그 종료 애니메이션을 실행합니다.
        /// </summary>
        public void PlayPlayerSkillCardDragEndAnimation(string cardId, GameObject target, System.Action onComplete = null)
        {
            AnimationDatabaseManager.Instance.PlayPlayerSkillCardDragEndAnimation(cardId, target, onComplete);
        }

        /// <summary>
        /// 적 스킬카드 드래그 시작 애니메이션을 실행합니다.
        /// </summary>
        public void PlayEnemySkillCardDragStartAnimation(string cardId, GameObject target, System.Action onComplete = null)
        {
            AnimationDatabaseManager.Instance.PlayEnemySkillCardDragStartAnimation(cardId, target, onComplete);
        }

        /// <summary>
        /// 적 스킬카드 드래그 종료 애니메이션을 실행합니다.
        /// </summary>
        public void PlayEnemySkillCardDragEndAnimation(string cardId, GameObject target, System.Action onComplete = null)
        {
            AnimationDatabaseManager.Instance.PlayEnemySkillCardDragEndAnimation(cardId, target, onComplete);
        }
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
            AnimationDatabaseManager.Instance.PlaySkillCardDropAnimation(card, target, onComplete);
        }

        /// <summary>
        /// 플레이어 스킬카드 드롭 애니메이션을 실행합니다.
        /// </summary>
        public void PlayPlayerSkillCardDropAnimation(string cardId, GameObject target, System.Action onComplete = null)
        {
            AnimationDatabaseManager.Instance.PlayPlayerSkillCardDropAnimation(cardId, target, onComplete);
        }

        /// <summary>
        /// 적 스킬카드 드롭 애니메이션을 실행합니다.
        /// </summary>
        public void PlayEnemySkillCardDropAnimation(string cardId, GameObject target, System.Action onComplete = null)
        {
            AnimationDatabaseManager.Instance.PlayEnemySkillCardDropAnimation(cardId, target, onComplete);
        }
        #endregion

        // 상태 출력
        public void PrintStatus() => AnimationDatabaseManager.Instance.DebugDatabaseStatus();

        /// <summary>
        /// 캐릭터 사망 시 해당 캐릭터의 스킬카드들을 소멸시킵니다.
        /// </summary>
        /// <param name="characterName">사망한 캐릭터 이름</param>
        /// <param name="isPlayerCharacter">플레이어 캐릭터 여부</param>
        /// <param name="onComplete">완료 콜백</param>
        public void VanishCharacterSkillCards(string characterName, bool isPlayerCharacter, System.Action onComplete = null)
        {
            Debug.Log($"[AnimationFacade] 캐릭터 사망으로 인한 스킬카드 소멸 시작: {characterName}, 플레이어: {isPlayerCharacter}");
            
            // 해당 캐릭터의 스킬카드들을 찾아서 소멸 애니메이션 적용
            var skillCards = FindCharacterSkillCards(characterName, isPlayerCharacter);
            
            if (skillCards.Count == 0)
            {
                Debug.Log($"[AnimationFacade] 소멸할 스킬카드가 없습니다: {characterName}");
                onComplete?.Invoke();
                return;
            }
            
            Debug.Log($"[AnimationFacade] 소멸할 스킬카드 수: {skillCards.Count}");
            
            // 모든 스킬카드에 소멸 애니메이션 적용
            int completedCount = 0;
            int totalCount = skillCards.Count;
            
            foreach (var skillCard in skillCards)
            {
                if (skillCard == null) continue;
                
                // 스킬카드에 VanishAnimation 컴포넌트 추가
                var vanishAnim = skillCard.GetComponent<AnimationSystem.Animator.SkillCardAnimation.VanishAnimation.DefaultSkillCardVanishAnimation>();
                if (vanishAnim == null)
                {
                    vanishAnim = skillCard.AddComponent<AnimationSystem.Animator.SkillCardAnimation.VanishAnimation.DefaultSkillCardVanishAnimation>();
                }
                
                // 소멸 애니메이션 실행
                vanishAnim.PlayAnimation("vanish", () => {
                    completedCount++;
                    Debug.Log($"[AnimationFacade] 스킬카드 소멸 완료: {completedCount}/{totalCount}");
                    
                    // 모든 스킬카드 소멸 완료 시
                    if (completedCount >= totalCount)
                    {
                        Debug.Log($"[AnimationFacade] 모든 스킬카드 소멸 완료: {characterName}");
                        onComplete?.Invoke();
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
    }
} 