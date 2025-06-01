using UnityEngine;
using System.Collections;
using Zenject;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.Utility;

namespace Game.CombatSystem.State
{
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly IPlayerHandManager playerHandManager;
        private readonly ITurnCardRegistry cardRegistry;
        private readonly ICoroutineRunner coroutineRunner;

        private bool isStateEntered;
        private bool isButtonClicked;

        [Inject]
        public CombatPrepareState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            IEnemyHandManager enemyHandManager,
            IPlayerHandManager playerHandManager,
            ITurnCardRegistry cardRegistry,
            ICoroutineRunner coroutineRunner)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.enemyHandManager = enemyHandManager;
            this.playerHandManager = playerHandManager;
            this.cardRegistry = cardRegistry;
            this.coroutineRunner = coroutineRunner;
        }

        public void EnterState()
        {
            if (isStateEntered)
            {
                Debug.LogWarning("[CombatPrepareState] 이미 상태에 진입했습니다. 중복 호출 방지됨");
                return;
            }

            isStateEntered = true;
            isButtonClicked = false;

            Debug.Log("<color=yellow>[CombatPrepareState] 상태 진입</color>");

            flowCoordinator.DisablePlayerInput();
            cardRegistry.Reset();

            coroutineRunner.RunCoroutine(PrepareRoutine());
        }

        private IEnumerator PrepareRoutine()
        {
            yield return flowCoordinator.RegisterEnemyCard();

            flowCoordinator.ShowPlayerCardSelectionUI();
            flowCoordinator.DisableStartButton();
            flowCoordinator.RegisterStartButton(OnStartButtonClicked);

            // 플레이어가 카드 선택 완료할 때까지 대기
            yield return new WaitUntil(() => cardRegistry.HasPlayerCard());

            flowCoordinator.EnableStartButton();
            Debug.Log("<color=lime>[CombatPrepareState] 플레이어 카드 선택 완료 → 시작 버튼 활성화</color>");
        }

        private void OnStartButtonClicked()
        {
            if (isButtonClicked)
            {
                Debug.LogWarning("[CombatPrepareState] 시작 버튼이 이미 클릭됨. 중복 클릭 방지됨");
                return;
            }

            isButtonClicked = true;

            Debug.Log("<color=cyan>[CombatPrepareState] 전투 시작 버튼 클릭됨</color>");

            flowCoordinator.HidePlayerCardSelectionUI();
            flowCoordinator.UnregisterStartButton();

            var nextState = turnManager.GetStateFactory().CreateFirstAttackState();
            turnManager.RequestStateChange(nextState);
        }

        public void ExecuteState()
        {
            // 이 상태에서는 실시간 로직 없음
        }

        public void ExitState()
        {
            Debug.Log("<color=grey>[CombatPrepareState] 상태 종료</color>");

            flowCoordinator.UnregisterStartButton();
            flowCoordinator.HidePlayerCardSelectionUI();

            isStateEntered = false;
            isButtonClicked = false;
        }
    }
}
