using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.IManager;

namespace Game.SkillCardSystem.Service
{
    /// <summary>
    /// 플레이어 턴 카드 관리 서비스입니다.
    /// 턴이 끝날 때마다 순환 시스템에서 새로운 카드를 가져와 핸드에 배치합니다.
    /// </summary>
    public class PlayerTurnCardService : MonoBehaviour
    {
        #region 의존성 필드

        private ICardCirculationSystem circulationSystem;
        private IPlayerHandManager handManager;
        private ICombatTurnManager turnManager;

        #endregion

        #region 의존성 주입

        [Inject]
        public void Construct(ICardCirculationSystem circulationSystem, IPlayerHandManager handManager, ICombatTurnManager turnManager)
        {
            this.circulationSystem = circulationSystem;
            this.handManager = handManager;
            this.turnManager = turnManager;
        }

        #endregion

        #region 턴 카드 관리

        /// <summary>
        /// 턴이 끝날 때 호출되어 새로운 카드를 순환 시스템에서 가져와 핸드에 배치합니다.
        /// </summary>
        public IEnumerator RefreshHandForNextTurn()
        {
            Debug.Log("<color=cyan>[PlayerTurnCardService] 다음 턴을 위한 핸드 새로고침 시작</color>");

            // 1. 현재 핸드의 카드들을 사용 보관함으로 이동
            var currentHandCards = handManager.GetAllHandCards();
            foreach (var (card, ui) in currentHandCards)
            {
                if (card != null)
                {
                    circulationSystem.MoveCardToUsedStorage(card);
                }
            }

            // 2. 핸드 클리어
            handManager.ClearAll();

            // 3. 순환 시스템에서 새로운 카드 드로우
            var newCards = circulationSystem.DrawCardsForTurn();
            Debug.Log($"[PlayerTurnCardService] 새로운 카드 드로우: {newCards.Count}장");

            // 4. 새로운 카드들을 핸드에 배치
            foreach (var card in newCards)
            {
                handManager.RestoreCardToHand(card);
            }

            // 5. 카드 등장 애니메이션
            yield return PlayCardSpawnAnimations(newCards);

            Debug.Log("<color=cyan>[PlayerTurnCardService] 핸드 새로고침 완료</color>");
        }

        /// <summary>
        /// 카드 등장 애니메이션을 재생합니다.
        /// </summary>
        private IEnumerator PlayCardSpawnAnimations(List<ISkillCard> cards)
        {
            var allCards = handManager.GetAllHandCards();
            int animCount = 0;
            
            foreach (var (card, ui) in allCards)
            {
                if (ui != null)
                {
                    var uiObj = ui as Game.SkillCardSystem.UI.SkillCardUI;
                    if (uiObj != null)
                    {
                        // AnimationFacade를 통해 애니메이션 실행
                        if (Game.AnimationSystem.Manager.AnimationFacade.Instance != null)
                        {
                            Game.AnimationSystem.Manager.AnimationFacade.Instance.PlaySkillCardAnimation(
                                card.CardData.Name, 
                                "spawn", 
                                uiObj.gameObject
                            );
                        }
                        animCount++;
                    }
                }
            }
            
            // 애니메이션 완료 대기
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log($"[PlayerTurnCardService] 카드 등장 애니메이션 완료: {animCount}개");
        }

        #endregion

        #region 이벤트 구독

        private void Start()
        {
            // 턴 매니저의 이벤트 구독 (필요시)
            // turnManager.OnTurnEnd += OnTurnEnd;
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            // turnManager.OnTurnEnd -= OnTurnEnd;
        }

        #endregion
    }
}
