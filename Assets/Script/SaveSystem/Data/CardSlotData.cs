using UnityEngine;

namespace Game.SaveSystem.Data
{
    /// <summary>
    /// 카드 슬롯 데이터
    /// 개별 카드 슬롯의 상태를 저장하는 데이터 구조
    /// </summary>
    [System.Serializable]
    public class CardSlotData
    {
        #region 카드 정보

        /// <summary>
        /// 카드 ID
        /// </summary>
        [Header("카드 정보")]
        public string cardId;

        /// <summary>
        /// 카드 이름
        /// </summary>
        public string cardName;

        /// <summary>
        /// 카드 비용
        /// </summary>
        public int cardCost;

        /// <summary>
        /// 카드 설명
        /// </summary>
        public string cardDescription;

        /// <summary>
        /// 카드 타입
        /// </summary>
        public string cardType;

        #endregion

        #region 슬롯 정보

        /// <summary>
        /// 슬롯 위치 (enum 값)
        /// </summary>
        [Header("슬롯 정보")]
        public int slotPosition;

        /// <summary>
        /// 슬롯 소유자 (PLAYER/ENEMY)
        /// </summary>
        public string slotOwner;

        /// <summary>
        /// 슬롯 이름
        /// </summary>
        public string slotName;

        #endregion

        #region 카드 상태

        /// <summary>
        /// 사용 여부
        /// </summary>
        [Header("카드 상태")]
        public bool isUsed;

        /// <summary>
        /// 카드가 활성화되어 있는지 여부
        /// </summary>
        public bool isActive;

        #endregion

        #region 생성자

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public CardSlotData()
        {
            cardId = "";
            cardName = "";
            cardCost = 0;
            cardDescription = "";
            cardType = "";
            slotPosition = 0;
            slotOwner = "";
            slotName = "";
            isUsed = false;
            isActive = true;
        }

        /// <summary>
        /// 카드 정보로 생성
        /// </summary>
        /// <param name="cardId">카드 ID</param>
        /// <param name="cardName">카드 이름</param>
        /// <param name="cardCost">카드 비용</param>
        /// <param name="cardDescription">카드 설명</param>
        /// <param name="cardType">카드 타입</param>
        public CardSlotData(string cardId, string cardName, int cardCost, string cardDescription, string cardType)
        {
            this.cardId = cardId;
            this.cardName = cardName;
            this.cardCost = cardCost;
            this.cardDescription = cardDescription;
            this.cardType = cardType;
            slotPosition = 0;
            slotOwner = "";
            slotName = "";
            isUsed = false;
            isActive = true;
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 카드 슬롯 데이터가 유효한지 확인
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(cardId) && !string.IsNullOrEmpty(cardName);
        }

        /// <summary>
        /// 빈 슬롯인지 확인
        /// </summary>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(cardId);
        }

        /// <summary>
        /// 플레이어 슬롯인지 확인
        /// </summary>
        public bool IsPlayerSlot()
        {
            return slotOwner == "PLAYER";
        }

        /// <summary>
        /// 적 슬롯인지 확인
        /// </summary>
        public bool IsEnemySlot()
        {
            return slotOwner == "ENEMY";
        }

        /// <summary>
        /// 전투 슬롯인지 확인
        /// </summary>
        public bool IsCombatSlot()
        {
            return slotOwner == "COMBAT";
        }

        /// <summary>
        /// 카드 정보를 문자열로 반환
        /// </summary>
        public override string ToString()
        {
            if (IsEmpty())
                return "Empty Slot";
            
            return $"{cardName} (ID: {cardId}, Cost: {cardCost}, Owner: {slotOwner})";
        }

        #endregion
    }
}
