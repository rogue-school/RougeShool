using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using System.Collections;

namespace Game.CombatSystem.Initialization
{
    public class FlowCoordinatorInitializationStep : MonoBehaviour, ICombatInitializerStep
    {
        [Inject] private ICombatFlowCoordinator flowCoordinator;
        [Inject] private ICombatTurnManager turnManager; // 추가 필요

        public int Order => 50;

        public IEnumerator Initialize()
        {
            Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 흐름 초기화 시작");

            bool isComplete = false;

            flowCoordinator.RequestCombatPreparation(success =>
            {
                if (success)
                {
                    Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 성공");

                    //  여기에서 전투 턴 상태 진입
                    turnManager.ChangeState(turnManager.GetStateFactory().CreatePlayerInputState());
                }
                else
                {
                    Debug.LogError("[FlowCoordinatorInitializationStep] 전투 준비 실패");
                }

                isComplete = true;
            });

            // 콜백 완료까지 대기
            yield return new WaitUntil(() => isComplete);
        }
    }
}
