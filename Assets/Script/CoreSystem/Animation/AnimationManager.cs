using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Core;
using Game.CharacterSystem.Data;
using Game.AnimationSystem.Controllers;

namespace Game.CoreSystem.Animation
{
    /// <summary>
    /// 애니메이션 시스템의 중앙 매니저
    /// 스킬카드와 캐릭터 애니메이션을 스크립트 기반으로 제어합니다.
    /// </summary>
    public class AnimationManager : MonoBehaviour
    {
        #region Singleton
        public static AnimationManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // InitializeManager(); // 데이터 로드 및 컨트롤러 생성은 AnimationDatabaseManager에서 처리
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion
        
        #region Data Collections
        // 데이터 및 컨트롤러는 AnimationDatabaseManager에서 관리
        #endregion
        
        #region Animation Controllers
        // 데이터 및 컨트롤러는 AnimationDatabaseManager에서 관리
        #endregion
        
        #region Events
        public static System.Action OnDataLoaded;
        public static System.Action<string> OnAnimationStarted;
        public static System.Action<string> OnAnimationCompleted;
        public static System.Action<string> OnAnimationFailed;
        #endregion
        
        #region Initialization
        // AnimationDatabaseManager를 통한 통합 API만 제공
        public void LoadAllData() => AnimationDatabaseManager.Instance.ReloadDatabases();
        #endregion
        
        #region Public API - Data Access
        // AnimationDatabaseManager를 통한 통합 API만 제공
        public void PlaySkillCardAnimation(string cardId, string animationType, GameObject target)
            => AnimationDatabaseManager.Instance.PlayPlayerSkillCardAnimation(cardId, target, animationType, null);
        public void PlayCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null, bool isEnemy = false)
            => AnimationSystem.Manager.AnimationFacade.Instance.PlayCharacterAnimation(characterId, animationType, target, onComplete, isEnemy);
        public string GetCharacterDeathAnimationType(string characterId)
        {
            var entry = AnimationDatabaseManager.Instance.GetPlayerCharacterAnimationEntry(characterId);
            if (entry != null && !entry.DeathAnimation.IsEmpty())
                return entry.DeathAnimation.AnimationScriptType;
            return null;
        }
        #endregion
        
        #region Public API - Animation Control
        // AnimationDatabaseManager를 통한 통합 API만 제공
        #endregion
        
        #region Utility Methods
        // LogMessage, LogError 등 디버그 시스템 잔재 메서드와 관련 주석, 호출부 모두 삭제
        // Debug.Log/Debug.LogError만 남김 (주석)
        // 실제 치명적 에러 상황만 로그로 남기고, 나머지는 제거
        // AnimationDatabaseManager를 통한 통합 API만 제공
        public void PrintStatus() => AnimationDatabaseManager.Instance.DebugDatabaseStatus();
        #endregion
    }
} 