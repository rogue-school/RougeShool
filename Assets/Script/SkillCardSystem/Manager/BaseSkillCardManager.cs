using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 단순화된 SkillCardSystem 베이스 매니저
    /// 최소한의 공통 기능만 제공하는 경량 베이스 클래스
    /// </summary>
    public abstract class BaseSkillCardManager<T> : MonoBehaviour 
        where T : class
    {
        #region 최소한의 인스펙터 설정
        
        [Header("기본 설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] protected bool enableDebugLogging = true;
        
        [Tooltip("씬 전환 시 유지 여부")]
        [SerializeField] protected bool persistAcrossScenes = false;
        
        #endregion
        
        #region 초기화 상태
        
        public bool IsInitialized { get; protected set; } = false;
        
        #endregion
        
        #region Unity 생명주기
        
        protected virtual void Awake()
        {
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"{GetType().Name} 초기화", GameLogger.LogCategory.SkillCard);
            }
        }
        
        #endregion
        
        #region 추상 메서드
        
        /// <summary>
        /// 매니저 초기화를 수행합니다.
        /// </summary>
        protected abstract void Initialize();
        
        /// <summary>
        /// 매니저 리셋을 수행합니다.
        /// </summary>
        public abstract void Reset();
        
        #endregion
        
        #region 공통 유틸리티
        
        /// <summary>
        /// 디버그 로그를 출력합니다.
        /// </summary>
        protected void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"[{GetType().Name}] {message}", GameLogger.LogCategory.SkillCard);
            }
        }
        
        /// <summary>
        /// 경고 로그를 출력합니다.
        /// </summary>
        protected void LogWarning(string message)
        {
            GameLogger.LogWarning($"[{GetType().Name}] {message}", GameLogger.LogCategory.SkillCard);
        }
        
        /// <summary>
        /// 에러 로그를 출력합니다.
        /// </summary>
        protected void LogError(string message)
        {
            GameLogger.LogError($"[{GetType().Name}] {message}", GameLogger.LogCategory.SkillCard);
        }
        
        #endregion
    }
}