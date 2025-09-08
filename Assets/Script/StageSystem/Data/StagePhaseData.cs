using UnityEngine;
using Game.CharacterSystem.Data;

namespace Game.StageSystem.Data
{
    /// <summary>
    /// 스테이지 단계별 데이터 (준보스/보스 구성)
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Stage/Stage Phase Data")]
    public class StagePhaseData : ScriptableObject
    {
        #region 스테이지 구성

        [Header("준보스 데이터")]
        [SerializeField] private EnemyCharacterData subBoss;
        
        [Header("보스 데이터")]
        [SerializeField] private EnemyCharacterData boss;

        #endregion

        #region 스테이지 정보

        [Header("스테이지 정보")]
        [SerializeField] private string stageName;
        [SerializeField] private int stageNumber;
        [SerializeField] private string stageDescription;

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 준보스 데이터
        /// </summary>
        public EnemyCharacterData SubBoss => subBoss;

        /// <summary>
        /// 보스 데이터
        /// </summary>
        public EnemyCharacterData Boss => boss;

        /// <summary>
        /// 스테이지 이름
        /// </summary>
        public string StageName => stageName;

        /// <summary>
        /// 스테이지 번호
        /// </summary>
        public int StageNumber => stageNumber;

        /// <summary>
        /// 스테이지 설명
        /// </summary>
        public string StageDescription => stageDescription;

        #endregion

        #region 유효성 검증

        /// <summary>
        /// 스테이지 데이터가 유효한지 확인
        /// </summary>
        public bool IsValid()
        {
            return subBoss != null && boss != null && !string.IsNullOrEmpty(stageName);
        }

        /// <summary>
        /// 준보스 데이터가 유효한지 확인
        /// </summary>
        public bool HasValidSubBoss()
        {
            return subBoss != null && subBoss.Prefab != null;
        }

        /// <summary>
        /// 보스 데이터가 유효한지 확인
        /// </summary>
        public bool HasValidBoss()
        {
            return boss != null && boss.Prefab != null;
        }

        #endregion
    }
}
