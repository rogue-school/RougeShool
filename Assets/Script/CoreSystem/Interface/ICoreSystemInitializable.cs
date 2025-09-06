using System.Collections;

namespace Game.CoreSystem.Interface
{
    /// <summary>
    /// CoreSystem 초기화를 위한 인터페이스
    /// </summary>
    public interface ICoreSystemInitializable
    {
        /// <summary>
        /// 시스템 초기화 수행
        /// </summary>
        /// <returns>초기화 완료까지 대기</returns>
        IEnumerator Initialize();
        
        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 초기화 실패 시 호출
        /// </summary>
        void OnInitializationFailed();
    }
}
