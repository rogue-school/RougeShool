using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;

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

        // 상태 출력
        public void PrintStatus() => AnimationDatabaseManager.Instance.DebugDatabaseStatus();
    }
} 