using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 턴 기반 카드 관리 구현체입니다.
    /// 카드 순환 시스템과 연동하여 턴별 카드 관리를 담당합니다.
    /// </summary>
    public class TurnBasedCardManager : ITurnBasedCardManager
    {
        #region 필드

        private readonly ICardCirculationSystem circulationSystem;
        private readonly List<ISkillCard> currentTurnCards = new();
        private bool isTurnStarted = false;
        private bool hasPlayedThisTurn = false;

        #endregion

        #region 프로퍼티

        public bool IsTurnStarted => isTurnStarted;
        public int AvailableCardsCount => currentTurnCards.Count;

        #endregion

        #region 생성자

        public TurnBasedCardManager(ICardCirculationSystem circulationSystem)
        {
            this.circulationSystem = circulationSystem;
        }

        #endregion

        #region 턴 관리

        public void StartTurn()
        {
            if (isTurnStarted)
            {
                Debug.LogWarning("[TurnBasedCardManager] 턴이 이미 시작되었습니다.");
                return;
            }

            // 카드 순환 시스템에서 카드 드로우
            currentTurnCards.Clear();
            currentTurnCards.AddRange(circulationSystem.DrawCardsForTurn());
            hasPlayedThisTurn = false;
            isTurnStarted = true;

            Debug.Log($"[TurnBasedCardManager] 턴 시작: {currentTurnCards.Count}장 드로우");
        }

        public void EndTurn()
        {
            if (!isTurnStarted)
            {
                Debug.LogWarning("[TurnBasedCardManager] 턴이 시작되지 않았습니다.");
                return;
            }

            // 턴 종료 처리 (보관함 시스템 제거됨)
            if (currentTurnCards.Count > 0)
            {
                currentTurnCards.Clear();
            }

            isTurnStarted = false;
            hasPlayedThisTurn = false;
            Debug.Log("[TurnBasedCardManager] 턴 종료 완료");
        }

        #endregion

        #region 카드 관리

        public List<ISkillCard> GetCurrentTurnCards()
        {
            return new List<ISkillCard>(currentTurnCards);
        }

        public void UseCard(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogWarning("[TurnBasedCardManager] null 카드를 사용할 수 없습니다.");
                return;
            }

            if (!currentTurnCards.Contains(card))
            {
                Debug.LogWarning("[TurnBasedCardManager] 현재 턴의 카드가 아닙니다.");
                return;
            }

            if (hasPlayedThisTurn)
            {
                Debug.LogWarning("[TurnBasedCardManager] 이 턴에는 이미 카드를 사용했습니다. 턴당 1장 제한.");
                return;
            }

            // 카드 사용 처리 (보관함 시스템 제거됨)
            currentTurnCards.Remove(card);
            hasPlayedThisTurn = true;

            Debug.Log($"[TurnBasedCardManager] 카드 사용: {card.CardDefinition?.CardName ?? "Unknown"} (남은 카드: {currentTurnCards.Count})");
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 턴 기반 카드 매니저를 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            currentTurnCards.Clear();
            isTurnStarted = false;
            Debug.Log("[TurnBasedCardManager] 초기화 완료");
        }

        /// <summary>
        /// 턴 기반 카드 매니저를 리셋합니다.
        /// </summary>
        public void Reset()
        {
            currentTurnCards.Clear();
            isTurnStarted = false;
            Debug.Log("[TurnBasedCardManager] 리셋 완료");
        }

        #endregion
    }
}
