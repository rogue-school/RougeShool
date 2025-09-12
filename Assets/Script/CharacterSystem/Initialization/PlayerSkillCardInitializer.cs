using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Manager;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Deck;
using Game.CharacterSystem.Data;
using Game.AnimationSystem.Manager;
using Game.CharacterSystem.Interface;

namespace Game.CharacterSystem.Initialization
{
    /// <summary>
    /// 플레이어 캐릭터의 스킬카드를 초기화하고 핸드 슬롯에 배치합니다.
    /// </summary>
    public class PlayerSkillCardInitializer : MonoBehaviour, ICombatInitializerStep
    {
        [SerializeField] private int order = 20;
        public int Order => order;

        #region 의존성 필드

        private IPlayerManager playerManager;
        private IPlayerHandManager handManager;
        private ICardCirculationSystem circulationSystem;
        private ISkillCardFactory skillCardFactory;

        #endregion

        #region 의존성 주입

        [Inject]
        public void Construct(IPlayerManager playerManager, IPlayerHandManager handManager, ICardCirculationSystem circulationSystem, ISkillCardFactory skillCardFactory)
        {
            this.playerManager = playerManager;
            this.handManager = handManager;
            this.circulationSystem = circulationSystem;
            this.skillCardFactory = skillCardFactory;
        }

        #endregion

        #region 초기화 로직

        public IEnumerator Initialize()
        {
            Debug.Log("<color=cyan>[PlayerSkillCardInitializer] 플레이어 스킬카드 초기화 시작</color>");

            var player = playerManager.GetPlayer();
            if (player == null)
            {
                Debug.LogError("[PlayerSkillCardInitializer] 플레이어가 존재하지 않습니다.");
                yield break;
            }

            // 1. 플레이어 캐릭터 데이터에서 덱 정보 가져오기
            var characterData = player.CharacterData;
            if (characterData?.SkillDeck == null)
            {
                Debug.LogError("[PlayerSkillCardInitializer] 플레이어 캐릭터 데이터 또는 스킬 덱이 없습니다.");
                yield break;
            }

            // 2. 순환 시스템 초기화 (캐릭터별 덱으로 카드 생성)
            var initialCards = characterData.SkillDeck.CreateCardsForCirculation(skillCardFactory, characterData.DisplayName);
            circulationSystem.Initialize(initialCards);

            // 3. 첫 턴 카드 드로우 (순환 시스템에서 3장 가져오기)
            var firstTurnCards = circulationSystem.DrawCardsForTurn();
            Debug.Log($"[PlayerSkillCardInitializer] 첫 턴 카드 드로우: {firstTurnCards.Count}장");

            // 4. 핸드 매니저 설정 및 카드 배치
            handManager.SetPlayer(player);
            
            // 순환 시스템에서 가져온 카드들을 핸드에 배치
            foreach (var card in firstTurnCards)
            {
                handManager.RestoreCardToHand(card);
            }
            
            handManager.LogPlayerHandSlotStates();
            player.InjectHandManager(handManager);

            // 5. 카드 등장 애니메이션 병렬 실행 및 대기
            var allCards = handManager.GetAllHandCards();
            int total = 0;
            int animCount = 0;
            
            foreach (var (card, ui) in allCards)
            {
                if (ui != null)
                {
                    total++;
                    var uiObj = ui as Game.SkillCardSystem.UI.SkillCardUI;
                    if (uiObj != null)
                    {
                        // AnimationFacade를 통해 애니메이션 실행
                        if (AnimationFacade.Instance != null)
                        {
                            AnimationFacade.Instance.PlaySkillCardAnimation(card.CardData.Name, "spawn", uiObj.gameObject);
                        }
                        animCount++; // 애니메이션 호출 완료
                    }
                }
            }
            
            // 애니메이션 완료 대기 (간단한 지연)
            yield return new WaitForSeconds(0.5f);

            Debug.Log($"<color=cyan>[PlayerSkillCardInitializer] 플레이어 스킬카드 초기화 완료 - 총 {total}장 카드, {animCount}개 애니메이션</color>");
            yield return null;
        }

        #endregion
    }
}
