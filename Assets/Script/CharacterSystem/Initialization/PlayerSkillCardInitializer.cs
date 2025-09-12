using System.Collections;
using UnityEngine;
using Zenject;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
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

        #endregion

        #region 의존성 주입

        [Inject]
        public void Construct(IPlayerManager playerManager, IPlayerHandManager handManager)
        {
            this.playerManager = playerManager;
            this.handManager = handManager;
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

            handManager.SetPlayer(player);          // owner를 runtime에 명시적으로 설정
            handManager.GenerateInitialHand();
            handManager.LogPlayerHandSlotStates();
            player.InjectHandManager(handManager);  // 연결

            // 카드 등장 애니메이션 병렬 실행 및 대기
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

            Debug.Log("<color=cyan>[PlayerSkillCardInitializer] 플레이어 핸드 카드 등장 애니메이션 완료</color>");
            yield return null;
        }

        #endregion
    }
}
