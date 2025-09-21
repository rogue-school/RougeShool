using Game.CombatSystem.Interface;
using Game.CombatSystem.State;
using Zenject;

namespace Game.CombatSystem.Factory
{
    /// <summary>
    /// 전투 턴 상태 객체들을 생성하는 팩토리입니다.
    /// Zenject의 IFactory를 활용하여 의존성 주입 기반 상태 생성을 제공합니다.
    /// </summary>
    public class CombatStateFactory
    {
        #region Injected Factories

        [Inject] private IFactory<CombatPrepareState> prepareFactory;
        [Inject] private IFactory<CombatPlayerInputState> inputFactory;
        [Inject] private IFactory<CombatAttackState> attackFactory;
        [Inject] private IFactory<CombatResultState> resultFactory;
        [Inject] private IFactory<CombatVictoryState> victoryFactory;
        [Inject] private IFactory<CombatGameOverState> gameOverFactory;

        #endregion

        #region State Creation Methods

        /// <summary>
        /// 준비 상태 생성
        /// </summary>
        public CombatPrepareState CreatePrepareState() => prepareFactory.Create();

        /// <summary>
        /// 플레이어 입력 상태 생성
        /// </summary>
        public CombatPlayerInputState CreatePlayerInputState() => inputFactory.Create();

        /// <summary>
        /// 공격 상태 생성
        /// </summary>
        public CombatAttackState CreateAttackState() => attackFactory.Create();

        /// <summary>
        /// 결과 계산 상태 생성
        /// </summary>
        public CombatResultState CreateResultState() => resultFactory.Create();

        /// <summary>
        /// 승리 상태 생성
        /// </summary>
        public CombatVictoryState CreateVictoryState() => victoryFactory.Create();

        /// <summary>
        /// 게임 오버 상태 생성
        /// </summary>
        public CombatGameOverState CreateGameOverState() => gameOverFactory.Create();

        #endregion
    }
}
