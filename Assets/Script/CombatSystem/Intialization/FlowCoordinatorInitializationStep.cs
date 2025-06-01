using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using System.Collections;

namespace Game.CombatSystem.Initialization
{
    public class FlowCoordinatorInitializationStep : MonoBehaviour, ICombatInitializerStep
    {
        [Inject] private ICombatFlowCoordinator flowCoordinator;

        public int Order => 50;

        public IEnumerator Initialize()
        {
            Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 흐름 초기화 시작");

            flowCoordinator.RequestCombatPreparation(success =>
            {
                if (success)
                    Debug.Log("[FlowCoordinatorInitializationStep] 전투 준비 성공");
                else
                    Debug.LogError("[FlowCoordinatorInitializationStep] 전투 준비 실패");
            });

            yield return null;
        }
    }
}
