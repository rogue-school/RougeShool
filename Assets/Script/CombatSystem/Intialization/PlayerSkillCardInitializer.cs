using System.Collections;
using UnityEngine;
using Zenject;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Initialization
{
    public class PlayerSkillCardInitializer : MonoBehaviour, ICombatInitializerStep
    {
        [SerializeField] private int order = 20;
        public int Order => order;

        private IPlayerManager playerManager;
        private IPlayerHandManager handManager;

        [Inject]
        public void Construct(IPlayerManager playerManager, IPlayerHandManager handManager)
        {
            this.playerManager = playerManager;
            this.handManager = handManager;
        }

        public IEnumerator Initialize()
        {
            Debug.Log("[PlayerSkillCardInitializer] 플레이어 스킬카드 초기화 시작");

            var player = playerManager.GetPlayer();
            if (player == null)
            {
                Debug.LogError("[PlayerSkillCardInitializer] 플레이어가 존재하지 않습니다.");
                yield break;
            }

            handManager.SetPlayer(player);          // owner를 runtime에 명시적 설정
            handManager.GenerateInitialHand();
            handManager.LogPlayerHandSlotStates();
            player.InjectHandManager(handManager);  // 연결

            Debug.Log("[PlayerSkillCardInitializer] 플레이어 핸드 초기화 완료");
            yield return null;
        }
    }
}
