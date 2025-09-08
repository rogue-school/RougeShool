using UnityEngine;

namespace Game.SaveSystem.Data
{
    /// <summary>
    /// 자동 저장 조건
    /// 슬레이 더 스파이어 방식: 특정 이벤트 발생 시 자동 저장
    /// </summary>
    [System.Serializable]
    public class AutoSaveCondition
    {
        #region 조건 정보

        /// <summary>
        /// 조건 이름
        /// </summary>
        [Header("조건 정보")]
        public string conditionName;

        /// <summary>
        /// 저장 트리거 타입
        /// </summary>
        public AutoSaveTrigger trigger;

        /// <summary>
        /// 활성화 여부
        /// </summary>
        public bool isEnabled;

        /// <summary>
        /// 조건 설명
        /// </summary>
        public string description;

        #endregion

        #region 생성자

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public AutoSaveCondition()
        {
            conditionName = "";
            trigger = AutoSaveTrigger.Manual;
            isEnabled = true;
            description = "";
        }

        /// <summary>
        /// 조건 정보로 생성
        /// </summary>
        /// <param name="conditionName">조건 이름</param>
        /// <param name="trigger">저장 트리거</param>
        /// <param name="description">조건 설명</param>
        public AutoSaveCondition(string conditionName, AutoSaveTrigger trigger, string description)
        {
            this.conditionName = conditionName;
            this.trigger = trigger;
            this.description = description;
            isEnabled = true;
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 자동 저장 조건이 유효한지 확인
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(conditionName) && isEnabled;
        }

        /// <summary>
        /// 조건 정보를 문자열로 반환
        /// </summary>
        public override string ToString()
        {
            return $"{conditionName} ({trigger}) - {description}";
        }

        #endregion
    }

    /// <summary>
    /// 자동 저장 트리거 타입
    /// </summary>
    public enum AutoSaveTrigger
    {
        /// <summary>
        /// 수동 저장
        /// </summary>
        Manual,

        /// <summary>
        /// 적 카드 배치 후
        /// </summary>
        EnemyCardPlaced,

        /// <summary>
        /// 턴 시작 버튼 누르기 전
        /// </summary>
        BeforeTurnStart,

        /// <summary>
        /// 턴 실행 중
        /// </summary>
        DuringTurnExecution,

        /// <summary>
        /// 턴 완료 후
        /// </summary>
        TurnCompleted,

        /// <summary>
        /// 스테이지 완료 후
        /// </summary>
        StageCompleted,

        /// <summary>
        /// 게임 종료 시
        /// </summary>
        GameExit,

        /// <summary>
        /// 씬 전환 시
        /// </summary>
        SceneTransition
    }
}
