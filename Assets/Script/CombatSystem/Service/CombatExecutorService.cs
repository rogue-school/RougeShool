using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Service;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 전투 실행 서비스.
    /// 슬롯에 배치된 카드를 순차적으로 실행하고 결과를 처리합니다.
    /// </summary>
    public class CombatExecutorService : ICombatExecutorService, ICombatExecutor
    {
        #region 필드 및 생성자

        private readonly ICombatSlotRegistry combatSlotRegistry;
        private ICardExecutionContextProvider contextProvider;
        private ICardExecutor cardExecutor;
        private readonly IEnemyHandManager enemyHandManager;
        private ICombatTurnManager turnManager;
        private PlayerTurnCardService playerTurnCardService;

        /// <summary>
        /// 생성자 - 전투 슬롯, 컨텍스트 제공자, 카드 실행기, 적 핸드 매니저를 주입받습니다.
        /// </summary>
        public CombatExecutorService(
            ICombatSlotRegistry combatSlotRegistry,
            ICardExecutionContextProvider contextProvider,
            ICardExecutor cardExecutor,
            IEnemyHandManager enemyHandManager)
        {
            this.combatSlotRegistry = combatSlotRegistry;
            this.contextProvider = contextProvider;
            this.cardExecutor = cardExecutor;
            this.enemyHandManager = enemyHandManager;
        }

        #endregion

        #region 전투 실행

        /// <summary>
        /// 전체 전투 페이즈를 실행합니다. (선공 → 후공)
        /// </summary>
        public IEnumerator ExecuteCombatPhase()
        {
            yield return PerformAttack(CombatSlotPosition.FIRST);
            yield return PerformAttack(CombatSlotPosition.SECOND);
            
            // 턴 완료 후 플레이어 핸드 새로고침
            if (playerTurnCardService != null)
            {
                yield return playerTurnCardService.RefreshHandForNextTurn();
            }
        }

        /// <summary>
        /// 지정한 슬롯 위치에 있는 카드의 효과를 실행합니다.
        /// </summary>
        /// <param name="slotPosition">실행할 슬롯 위치 (FIRST 또는 SECOND)</param>
        public IEnumerator PerformAttack(CombatSlotPosition slotPosition)
        {
            var fieldSlot = SlotPositionUtil.ToFieldSlot(slotPosition);
            var slot = combatSlotRegistry.GetCombatSlot(fieldSlot);

            if (slot == null || slot.IsEmpty())
            {
                Debug.LogWarning($"[Executor] 슬롯 {slotPosition}에 카드가 없습니다.");
                yield break;
            }

            var card = slot.GetCard();
            if (card == null)
            {
                Debug.LogWarning($"[Executor] 슬롯 {slotPosition}에 등록된 카드가 null입니다.");
                yield break;
            }

            var context = contextProvider.CreateContext(card);
            cardExecutor.Execute(card, context, turnManager);

            yield return new WaitForSeconds(0.5f);

            slot.ClearAll();
        }

        #endregion

        #region 종속성 주입

        /// <summary>
        /// 실행 관련 종속 객체(ICardExecutionContextProvider, ICardExecutor)를 동적으로 주입합니다.
        /// </summary>
        public void InjectExecutionDependencies(ICardExecutionContextProvider provider, ICardExecutor executor)
        {
            contextProvider = provider;
            cardExecutor = executor;
        }

        /// <summary>
        /// 전투 턴 매니저를 주입합니다.
        /// </summary>
        public void SetTurnManager(ICombatTurnManager manager)
        {
            turnManager = manager;
        }

        /// <summary>
        /// 플레이어 턴 카드 서비스를 주입합니다.
        /// </summary>
        public void SetPlayerTurnCardService(PlayerTurnCardService service)
        {
            playerTurnCardService = service;
        }

        #endregion
    }
}
