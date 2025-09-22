using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Data;

namespace Game.StageSystem.Data
{
    [CreateAssetMenu(menuName = "Game/Stage/Stage Data")]
    public class StageData : ScriptableObject
    {
        [Header("스테이지 기본 정보")]
        [Tooltip("스테이지 번호 (1-4)")]
        [Range(1, 4)]
        public int stageNumber = 1;
        
        [Tooltip("스테이지 이름")]
        public string stageName = "스테이지 1";
        
        [Tooltip("스테이지 설명")]
        [TextArea(2, 4)]
        public string stageDescription = "스테이지 설명";
        
        [Space(10)]
        [Header("스테이지 설정")]
        [Tooltip("스테이지 난이도")]
        [Range(1, 5)]
        public int difficulty = 1;
        
        [Tooltip("스테이지 완료 시 다음 스테이지로 자동 진행")]
        public bool autoProgressToNext = true;
        
        [Space(10)]
        [Header("적 설정")]
        [Tooltip("이 스테이지에 등장할 적 목록")]
        public List<EnemyCharacterData> enemies;
        
        
        #region 프로퍼티
        
        /// <summary>
        /// 스테이지에 적이 있는지 확인
        /// </summary>
        public bool HasEnemies => enemies != null && enemies.Count > 0;
        
        /// <summary>
        /// 적의 총 수
        /// </summary>
        public int EnemyCount => enemies?.Count ?? 0;
        
        /// <summary>
        /// 다음 스테이지 번호
        /// </summary>
        public int NextStageNumber => stageNumber + 1;
        
        /// <summary>
        /// 마지막 스테이지인지 확인
        /// </summary>
        public bool IsLastStage => stageNumber >= 4;
        
        #endregion
        
        #region 유효성 검증
        
        /// <summary>
        /// 스테이지 데이터가 유효한지 확인
        /// </summary>
        public bool IsValid()
        {
            if (stageNumber < 1 || stageNumber > 4)
            {
                Debug.LogError($"[StageData] 잘못된 스테이지 번호: {stageNumber}");
                return false;
            }
            
            if (string.IsNullOrEmpty(stageName))
            {
                Debug.LogError($"[StageData] 스테이지 이름이 비어있습니다: {name}");
                return false;
            }
            
            if (!HasEnemies)
            {
                Debug.LogError($"[StageData] 스테이지에 적이 없습니다: {stageName}");
                return false;
            }
            
            return true;
        }
        
        #endregion
    }
}
