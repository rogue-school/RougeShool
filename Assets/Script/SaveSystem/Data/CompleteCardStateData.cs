using System.Collections.Generic;
using UnityEngine;

namespace Game.SaveSystem.Data
{
    /// <summary>
    /// 완전한 카드 상태 데이터
    /// 슬레이 더 스파이어 방식: 플레이어/적 핸드카드 + 전투 슬롯 + 카드 순환 상태
    /// </summary>
    [System.Serializable]
    public class CompleteCardStateData
    {
        #region 플레이어 핸드카드

        /// <summary>
        /// 플레이어 핸드카드 (슬롯별)
        /// </summary>
        [Header("플레이어 핸드카드")]
        public List<CardSlotData> playerHandSlots = new();

        #endregion

        // 적 핸드카드 관련 필드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

        #region 전투 슬롯에 배치된 카드

        /// <summary>
        /// 첫 번째 슬롯 카드 (FIRST)
        /// </summary>
        [Header("전투 슬롯 카드")]
        public CardSlotData firstSlotCard;

        /// <summary>
        /// 두 번째 슬롯 카드 (SECOND)
        /// </summary>
        public CardSlotData secondSlotCard;

        #endregion

        #region 카드 순환 시스템 상태

        /// <summary>
        /// 카드 순환 상태 (보관함 시스템 제거됨)
        /// </summary>
        [Header("카드 순환 상태 (보관함 시스템 제거됨)")]
        // unusedStorageCards와 usedStorageCards 필드가 제거되었습니다.

        #endregion

        #region 턴 상태

        /// <summary>
        /// 플레이어 선공 여부
        /// </summary>
        [Header("턴 상태")]
        public bool isPlayerFirst;

        /// <summary>
        /// 현재 턴 수
        /// </summary>
        public int currentTurn;

        /// <summary>
        /// 턴 단계
        /// </summary>
        public string turnPhase;

        #endregion

        #region 메타데이터

        /// <summary>
        /// 저장 시점
        /// </summary>
        [Header("메타데이터")]
        public string saveTrigger;

        /// <summary>
        /// 저장 시간
        /// </summary>
        public string saveTime;

        /// <summary>
        /// 씬 이름
        /// </summary>
        public string sceneName;

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 카드 상태 데이터가 유효한지 확인
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(saveTrigger) && !string.IsNullOrEmpty(sceneName);
        }

        /// <summary>
        /// 플레이어 핸드카드가 있는지 확인
        /// </summary>
        public bool HasPlayerHandCards()
        {
            return playerHandSlots != null && playerHandSlots.Count > 0;
        }

        // 적 핸드카드 확인 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

        /// <summary>
        /// 전투 슬롯에 카드가 배치되어 있는지 확인
        /// </summary>
        public bool HasCombatSlotCards()
        {
            return firstSlotCard != null || secondSlotCard != null;
        }

        /// <summary>
        /// 카드 순환 시스템 상태가 있는지 확인 (보관함 시스템 제거됨)
        /// </summary>
        public bool HasCardCirculationState()
        {
            // 보관함 시스템이 제거되어 항상 false 반환
            return false;
        }

        #endregion
    }
}
