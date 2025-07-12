using System;
using System.Collections;
using Game.IManager;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// CombatSystem의 모든 기능을 통합하는 파사드 인터페이스
    /// 복잡한 의존성을 단순화하여 클라이언트가 쉽게 사용할 수 있도록 합니다.
    /// </summary>
    public interface ICombatSystemFacade
    {
        #region 전투 흐름 관리
        ICombatFlowCoordinator FlowCoordinator { get; }
        ICombatTurnManager TurnManager { get; }
        ICombatStateFactory StateFactory { get; }
        #endregion

        #region 매니저 접근
        IStageManager StageManager { get; }
        IPlayerManager PlayerManager { get; }
        IEnemyManager EnemyManager { get; }
        IPlayerHandManager PlayerHandManager { get; }
        IEnemyHandManager EnemyHandManager { get; }
        ICombatSlotManager CombatSlotManager { get; }
        IVictoryManager VictoryManager { get; }
        IGameOverManager GameOverManager { get; }
        #endregion

        #region 서비스 접근
        ITurnCardRegistry TurnCardRegistry { get; }
        ICombatPreparationService PreparationService { get; }
        ISlotRegistry SlotRegistry { get; }
        ICardExecutor CardExecutor { get; }
        #endregion

        #region 전투 제어
        void StartCombat();
        void EndCombat(bool isVictory);
        void PauseCombat();
        void ResumeCombat();
        bool IsCombatActive { get; }
        #endregion

        #region 상태 관리
        void ChangeState<T>() where T : ICombatTurnState;
        ICombatTurnState CurrentState { get; }
        #endregion

        #region 이벤트 관리
        void SubscribeToCombatEvents();
        void UnsubscribeFromCombatEvents();
        #endregion

        #region 초기화
        void Initialize();
        void Cleanup();
        #endregion
    }
} 