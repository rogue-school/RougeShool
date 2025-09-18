using UnityEngine;
using Zenject;
using System.Collections.Generic;
using System.Linq;
using Game.SkillCardSystem.Data;
using Game.CharacterSystem.Data;
using Game.AnimationSystem.Controllers;
using Game.AnimationSystem.Interface;
using Game.CoreSystem.Interface;

namespace Game.CoreSystem.Animation
{
    /// <summary>
    /// 애니메이션 시스템의 중앙 매니저
    /// 스킬카드와 캐릭터 애니메이션을 스크립트 기반으로 제어합니다.
    /// </summary>
    public class AnimationManager : MonoBehaviour
    {
        #region Private Fields
        private IAnimationDatabaseManager animationDatabaseManager;
        private IAnimationFacade animationFacade;
        #endregion

        #region DI
        [Inject]
        public void Construct(IAnimationDatabaseManager animationDatabaseManager, IAnimationFacade animationFacade)
        {
            this.animationDatabaseManager = animationDatabaseManager;
            this.animationFacade = animationFacade;
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
        public void LoadAllData() => animationDatabaseManager.ReloadDatabases();
        #endregion
        
        #region Public API - Data Access
        // AnimationDatabaseManager를 통한 통합 API만 제공
        // string cardId 기반 메서드는 제거됨 - ISkillCard 기반 메서드만 사용
        public void PlayCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null, bool isEnemy = false)
            => animationFacade.PlayCharacterAnimation(characterId, animationType, target, onComplete, isEnemy);
        public string GetCharacterDeathAnimationType(string characterId)
        {
            var entry = animationDatabaseManager.GetPlayerCharacterAnimationEntry(characterId);
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
        public void PrintStatus() => animationDatabaseManager.DebugDatabaseStatus();
        #endregion
    }
} 