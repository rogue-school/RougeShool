using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Deck;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 플레이어 덱을 동적으로 관리하는 인터페이스입니다.
    /// 게임 중 덱 구성 변경, 카드 추가/제거, 보상 지급 등을 담당합니다.
    /// </summary>
    public interface IPlayerDeckManager
    {
        #region 덱 구성 관리

        /// <summary>
        /// 덱에 카드를 추가합니다.
        /// </summary>
        /// <param name="cardDefinition">추가할 카드 정의</param>
        /// <param name="quantity">추가할 수량 (기본값: 1)</param>
        /// <returns>추가 성공 여부</returns>
        bool AddCardToDeck(SkillCardDefinition cardDefinition, int quantity = 1);

        /// <summary>
        /// 덱에서 카드를 제거합니다.
        /// </summary>
        /// <param name="cardDefinition">제거할 카드 정의</param>
        /// <param name="quantity">제거할 수량 (기본값: 1)</param>
        /// <returns>제거 성공 여부</returns>
        bool RemoveCardFromDeck(SkillCardDefinition cardDefinition, int quantity = 1);

        /// <summary>
        /// 덱에서 특정 카드를 완전히 제거합니다.
        /// </summary>
        /// <param name="cardDefinition">완전히 제거할 카드 정의</param>
        /// <returns>제거 성공 여부</returns>
        bool RemoveAllCardsFromDeck(SkillCardDefinition cardDefinition);

        /// <summary>
        /// 덱의 특정 카드 수량을 설정합니다.
        /// </summary>
        /// <param name="cardDefinition">설정할 카드 정의</param>
        /// <param name="quantity">설정할 수량</param>
        /// <returns>설정 성공 여부</returns>
        bool SetCardQuantity(SkillCardDefinition cardDefinition, int quantity);

        #endregion

        #region 덱 조회

        /// <summary>
        /// 현재 덱 구성을 반환합니다.
        /// </summary>
        /// <returns>현재 덱 구성</returns>
        List<PlayerSkillDeck.CardEntry> GetCurrentDeck();

        /// <summary>
        /// 특정 카드의 현재 수량을 반환합니다.
        /// </summary>
        /// <param name="cardDefinition">조회할 카드 정의</param>
        /// <returns>카드 수량 (없으면 0)</returns>
        int GetCardQuantity(SkillCardDefinition cardDefinition);

        /// <summary>
        /// 덱에 특정 카드가 있는지 확인합니다.
        /// </summary>
        /// <param name="cardDefinition">확인할 카드 정의</param>
        /// <returns>카드 존재 여부</returns>
        bool HasCard(SkillCardDefinition cardDefinition);

        /// <summary>
        /// 덱의 총 카드 수를 반환합니다.
        /// </summary>
        /// <returns>총 카드 수</returns>
        int GetTotalCardCount();

        /// <summary>
        /// 덱의 고유 카드 종류 수를 반환합니다.
        /// </summary>
        /// <returns>고유 카드 종류 수</returns>
        int GetUniqueCardCount();

        #endregion

        #region 덱 검증

        /// <summary>
        /// 현재 덱이 유효한지 확인합니다.
        /// </summary>
        /// <returns>덱 유효성</returns>
        bool IsValidDeck();

        /// <summary>
        /// 덱의 최소/최대 카드 수 제한을 확인합니다.
        /// </summary>
        /// <param name="minCards">최소 카드 수</param>
        /// <param name="maxCards">최대 카드 수</param>
        /// <returns>제한 준수 여부</returns>
        bool IsWithinCardLimit(int minCards = 5, int maxCards = 30);

        #endregion

        #region 덱 저장/로드

        /// <summary>
        /// 현재 덱 구성을 저장합니다.
        /// </summary>
        void SaveDeckConfiguration();

        /// <summary>
        /// 저장된 덱 구성을 로드합니다.
        /// </summary>
        void LoadDeckConfiguration();

        /// <summary>
        /// 기본 덱으로 리셋합니다.
        /// </summary>
        void ResetToDefaultDeck();

        #endregion

        #region 이벤트

        /// <summary>
        /// 덱이 변경되었을 때 발생하는 이벤트
        /// </summary>
        System.Action<SkillCardDefinition, int> OnDeckChanged { get; set; }

        /// <summary>
        /// 카드가 추가되었을 때 발생하는 이벤트
        /// </summary>
        System.Action<SkillCardDefinition, int> OnCardAdded { get; set; }

        /// <summary>
        /// 카드가 제거되었을 때 발생하는 이벤트
        /// </summary>
        System.Action<SkillCardDefinition, int> OnCardRemoved { get; set; }

        #endregion
    }
}
