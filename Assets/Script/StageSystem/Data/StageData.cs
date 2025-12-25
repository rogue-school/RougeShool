using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Data;

namespace Game.StageSystem.Data
{
    /// <summary>
    /// 스테이지 데이터를 담는 ScriptableObject입니다
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Stage/Stage Data")]
    public class StageData : ScriptableObject
    {
        /// <summary>
        /// 스테이지 번호 (1-4)
        /// </summary>
        [Header("스테이지 기본 정보")]
        [Tooltip("스테이지 번호 (1-4)")]
        [Range(1, 4)]
        public int stageNumber = 1;
        
        /// <summary>
        /// 스테이지 이름
        /// </summary>
        [Tooltip("스테이지 이름")]
        public string stageName = "스테이지 1";
        
        /// <summary>
        /// 스테이지 설명
        /// </summary>
        [Tooltip("스테이지 설명")]
        [TextArea(2, 4)]
        public string stageDescription = "스테이지 설명";
        
        /// <summary>
        /// 스테이지 완료 시 다음 스테이지로 자동 진행 여부
        /// </summary>
        [Space(10)]
        [Header("스테이지 설정")]
        [Tooltip("스테이지 완료 시 다음 스테이지로 자동 진행")]
        public bool autoProgressToNext = true;

        [Space(10)]
        [Header("배경 설정")]
        [Tooltip("이 스테이지에서 사용할 배경 이미지")]
        [SerializeField] private Sprite _stageBackgroundSprite;
        
        /// <summary>
        /// 이 스테이지에 등장할 적 목록
        /// </summary>
        [Space(10)]
        [Header("적 설정")]
        [Tooltip("이 스테이지에 등장할 적 목록")]
        public List<EnemyCharacterData> enemies;
        
        
        #region 프로퍼티
        
        /// <summary>
        /// 스테이지에 적이 있는지 확인
        /// </summary>
        /// <returns>적이 있으면 true</returns>
        public bool HasEnemies => enemies != null && enemies.Count > 0;
        
        /// <summary>
        /// 적의 총 수
        /// </summary>
        /// <returns>적의 개수</returns>
        public int EnemyCount => enemies?.Count ?? 0;
        
        /// <summary>
        /// 다음 스테이지 번호
        /// </summary>
        /// <returns>다음 스테이지 번호</returns>
        public int NextStageNumber => stageNumber + 1;
        
        /// <summary>
        /// 마지막 스테이지인지 확인
        /// </summary>
        /// <returns>마지막 스테이지이면 true</returns>
        public bool IsLastStage => stageNumber >= 4;

        /// <summary>
        /// 이 스테이지에서 사용할 배경 스프라이트
        /// </summary>
        /// <returns>배경 스프라이트</returns>
        public Sprite StageBackgroundSprite => _stageBackgroundSprite;
        
        #endregion
        
        #region 유효성 검증
        
        /// <summary>
        /// 스테이지 데이터가 유효한지 확인
        /// </summary>
        /// <returns>유효하면 true</returns>
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
