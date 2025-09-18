using System.Threading.Tasks;

namespace Game.CoreSystem.Interface
{
    /// <summary>
    /// 씬 전환 관리 인터페이스
    /// </summary>
    public interface ISceneTransitionManager
    {
        /// <summary>
        /// 전환 중인지 여부
        /// </summary>
        bool IsTransitioning { get; }
        
        /// <summary>
        /// 씬 전환 시작 이벤트
        /// </summary>
        System.Action<string> OnSceneTransitionStart { get; set; }
        
        /// <summary>
        /// 씬 전환 완료 이벤트
        /// </summary>
        System.Action<string> OnSceneTransitionEnd { get; set; }
        
        /// <summary>
        /// 메인 씬으로 전환
        /// </summary>
        Task TransitionToMainScene();
        
        /// <summary>
        /// 전투 씬으로 전환
        /// </summary>
        Task TransitionToBattleScene();
        
        /// <summary>
        /// 스테이지 씬으로 전환
        /// </summary>
        Task TransitionToStageScene();
        
        /// <summary>
        /// 코어 씬으로 전환
        /// </summary>
        Task TransitionToCoreScene();
        
        /// <summary>
        /// 지정된 씬으로 전환
        /// </summary>
        Task TransitionToScene(string sceneName);
        
        /// <summary>
        /// 지정된 씬으로 전환 (전환 타입 지정)
        /// </summary>
        Task TransitionToScene(string sceneName, TransitionType transitionType);
    }
    
    /// <summary>
    /// 전환 타입 열거형
    /// </summary>
    public enum TransitionType
    {
        Fade,        // 페이드
        Slide,       // 슬라이드
        Instant      // 즉시 전환
    }
}
