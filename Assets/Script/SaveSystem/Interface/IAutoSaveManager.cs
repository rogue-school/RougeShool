using System.Threading.Tasks;
using System.Collections.Generic;

namespace Game.SaveSystem.Interface
{
    /// <summary>
    /// 자동 저장 매니저 인터페이스
    /// </summary>
    public interface IAutoSaveManager
    {
        /// <summary>
        /// 자동 저장 활성화 여부
        /// </summary>
        bool IsAutoSaveEnabled { get; }
        
        /// <summary>
        /// 자동 저장 간격 (초)
        /// </summary>
        float AutoSaveInterval { get; }
        
        /// <summary>
        /// 자동 저장 트리거
        /// </summary>
        /// <param name="triggerName">트리거 이름</param>
        void TriggerAutoSave(string triggerName);
        
        /// <summary>
        /// 게임 상태 저장
        /// </summary>
        /// <param name="saveName">저장 이름</param>
        Task SaveGameState(string saveName);
        
        /// <summary>
        /// 게임 상태 로드
        /// </summary>
        /// <param name="saveName">저장 이름</param>
        void LoadGameState(string saveName);
        
        /// <summary>
        /// 자동 저장 조건 추가
        /// </summary>
        /// <param name="conditionName">조건 이름</param>
        /// <param name="trigger">트리거 타입</param>
        /// <param name="description">설명</param>
        void AddAutoSaveCondition(string conditionName, AutoSaveTrigger trigger, string description);
        
        /// <summary>
        /// 자동 저장 조건 제거
        /// </summary>
        /// <param name="conditionName">조건 이름</param>
        void RemoveAutoSaveCondition(string conditionName);
        
        /// <summary>
        /// 자동 저장 조건 업데이트
        /// </summary>
        /// <param name="conditionName">조건 이름</param>
        /// <param name="isEnabled">활성화 여부</param>
        void UpdateAutoSaveCondition(string conditionName, bool isEnabled);
    }
    
    /// <summary>
    /// 자동 저장 트리거 타입
    /// </summary>
    public enum AutoSaveTrigger
    {
        Manual,         // 수동
        TurnComplete,   // 턴 완료
        EnemyDefeat,    // 적 처치
        StageComplete,  // 스테이지 완료
        TimeInterval    // 시간 간격
    }
}
