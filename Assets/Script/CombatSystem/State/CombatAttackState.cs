using Game.CombatSystem.Interface;
using UnityEngine;
using Game.IManager;
using Game.CombatSystem.Context;
using Game.CharacterSystem.Interface;
using Game.AnimationSystem.Interface;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 공격 상태를 나타내며, 공격 처리 후 결과 상태로 전환합니다.
    /// </summary>
    public class CombatAttackState : ICombatTurnState
    {
        #region 필드

        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatSlotRegistry slotRegistry;
        private readonly IPlayerManager playerManager;
        private readonly TurnContext turnContext;
        private readonly IAnimationFacade animationFacade;

        #endregion

        #region 생성자

        /// <summary>
        /// 공격 상태를 초기화합니다.
        /// </summary>
        public CombatAttackState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICombatSlotRegistry slotRegistry,
            IPlayerManager playerManager,
            TurnContext turnContext,
            IAnimationFacade animationFacade
        )
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.slotRegistry = slotRegistry;
            this.playerManager = playerManager;
            this.turnContext = turnContext;
            this.animationFacade = animationFacade;
        }

        #endregion

        #region 상태 메서드

        /// <summary>
        /// 공격 상태 진입 시 호출되며, 공격 처리를 수행한 후 결과 상태로 전이합니다.
        /// </summary>
        public void EnterState()
        {
            Debug.Log("<color=cyan>[STATE] CombatAttackState 진입</color>");
            // 애니메이션 락이 걸려 있으면 대기 또는 return
            if (animationFacade.IsHandVanishAnimationPlaying)
            {
                Debug.Log("[CombatAttackState] 핸드 소멸 애니메이션 진행 중이므로 상태 전이 대기");
                return;
            }
            flowCoordinator.RequestFirstAttack(() =>
            {
                bool enemyDead = flowCoordinator.IsEnemyDead();
                bool playerDead = flowCoordinator.IsPlayerDead();

                // 1. 사망 시 소멸 애니메이션 먼저 실행 (중복 방지)
                if (enemyDead && !turnContext.WasHandCardsVanishedThisTurn)
                {
                    animationFacade.VanishAllHandCardsOnCharacterDeath("enemy", false);
                    turnContext.MarkHandCardsVanished();
                    Debug.Log("<color=cyan>[STATE] CombatAttackState → CombatResultState 전이 (적 사망 후 소멸 애니메이션 완료)</color>");
                    var next = turnManager.GetStateFactory().CreateResultState();
                    turnManager.RequestStateChange(next);
                    return;
                }
                if (playerDead && !turnContext.WasHandCardsVanishedThisTurn)
                {
                    animationFacade.VanishAllHandCardsOnCharacterDeath("player", false);
                    turnContext.MarkHandCardsVanished();
                    Debug.Log("<color=cyan>[STATE] CombatAttackState → CombatResultState 전이 (플레이어 사망 후 소멸 애니메이션 완료)</color>");
                    var next = turnManager.GetStateFactory().CreateResultState();
                    turnManager.RequestStateChange(next);
                    return;
                }

                // 2. 사망이 아니면 기존대로 바로 상태 전이
                Debug.Log("<color=cyan>[STATE] CombatAttackState → CombatResultState 전이</color>");
                var nextState = turnManager.GetStateFactory().CreateResultState();
                turnManager.RequestStateChange(nextState);
            });
        }

        /// <summary>
        /// 공격 상태 실행 중 반복적으로 호출됩니다. (현재는 비어 있음)
        /// </summary>
        public void ExecuteState() { }

        /// <summary>
        /// 상태 종료 시 호출됩니다. (현재는 비어 있음)
        /// </summary>
        public void ExitState() 
        { 
            Debug.Log("<color=cyan>[STATE] CombatAttackState 종료</color>"); 
        }

        #endregion
    }
}
