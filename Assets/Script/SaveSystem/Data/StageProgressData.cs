using System.Collections.Generic;
using UnityEngine;
using Game.StageSystem.Data;

namespace Game.SaveSystem.Data
{
    /// <summary>
    /// 스테이지 진행 상황 저장 데이터
    /// </summary>
    [System.Serializable]
    public class StageProgressData
    {
        #region 스테이지 정보
        
        [Header("스테이지 정보")]
        [Tooltip("현재 스테이지 번호")]
        public int currentStageNumber;
        
        [Tooltip("현재 스테이지 이름")]
        public string currentStageName;
        
        [Tooltip("스테이지 진행 상태")]
        public StageProgressState progressState;
        
        [Tooltip("현재 적 인덱스")]
        public int currentEnemyIndex;
        
        #endregion
        
        #region 전투 상태
        
        [Header("전투 상태")]
        [Tooltip("전투 활성화 여부")]
        public bool isCombatActive;
        
        [Tooltip("전투 플로우 상태")]
        public string combatFlowState;
        
        #endregion
        
        #region 턴 정보
        
        [Header("턴 정보")]
        [Tooltip("현재 턴 수")]
        public int turnCount;
        
        [Tooltip("현재 턴 타입")]
        public string currentTurn; // "Player" or "Enemy"
        
        #endregion
        
        #region 캐릭터 상태
        
        [Header("캐릭터 상태")]
        [Tooltip("플레이어 상태")]
        public CharacterStateData playerState;
        
        [Tooltip("적 상태")]
        public CharacterStateData enemyState;
        
        #endregion
        
        #region 카드 상태
        
        [Header("카드 상태")]
        [Tooltip("플레이어 핸드 카드 ID 목록")]
        public List<string> playerHandCardIds = new List<string>();
        
        [Tooltip("전투 슬롯 상태(슬롯/소유자/카드ID)")]
        public List<SlotCardState> combatSlots = new List<SlotCardState>();
        
        #endregion
        
        #region 메타데이터
        
        [Header("메타데이터")]
        [Tooltip("저장 트리거")]
        public string saveTrigger;
        
        [Tooltip("저장 시간")]
        public string saveTime;
        
        [Tooltip("씬 이름")]
        public string sceneName;
        
        #endregion
        
        #region 유틸리티 메서드
        
        /// <summary>
        /// 데이터 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            return currentStageNumber > 0 && 
                   !string.IsNullOrEmpty(currentStageName) &&
                   !string.IsNullOrEmpty(saveTime) &&
                   !string.IsNullOrEmpty(sceneName);
        }
        
        /// <summary>
        /// 저장 정보를 문자열로 반환
        /// </summary>
        public string GetSaveInfo()
        {
            return $"스테이지 {currentStageNumber} - {currentStageName} (턴 {turnCount}) - {saveTime}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// 전투 슬롯에 배치된 카드 상태
    /// </summary>
    [System.Serializable]
    public class SlotCardState
    {
        [Tooltip("슬롯 위치 (예: BATTLE_SLOT, WAIT_SLOT_1..4)")]
        public string position;
        
        [Tooltip("소유자 (Player/Enemy)")]
        public string owner;
        
        [Tooltip("카드 ID")]
        public string cardId;
    }
    
    /// <summary>
    /// 캐릭터 상태 데이터
    /// </summary>
    [System.Serializable]
    public class CharacterStateData
    {
        [Header("캐릭터 기본 정보")]
        [Tooltip("캐릭터 ID")]
        public string characterId;
        
        [Tooltip("현재 HP")]
        public int currentHP;
        
        [Tooltip("최대 HP")]
        public int maxHP;
        
        [Tooltip("가드 상태")]
        public bool isGuarded;
        
        [Header("버프/디버프")]
        [Tooltip("버프 목록")]
        public List<BuffData> buffs = new List<BuffData>();
        
        /// <summary>
        /// 캐릭터 상태 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(characterId) && 
                   maxHP > 0 && 
                   currentHP >= 0 && 
                   currentHP <= maxHP;
        }
    }
    
    /// <summary>
    /// 버프/디버프 데이터
    /// </summary>
    [System.Serializable]
    public class BuffData
    {
        [Tooltip("효과 ID")]
        public string effectId;
        
        [Tooltip("남은 턴 수")]
        public int remainingTurns;
        
        [Tooltip("디버프 여부")]
        public bool isDebuff;
        
        /// <summary>
        /// 버프 데이터 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(effectId) && remainingTurns > 0;
        }
    }
}
