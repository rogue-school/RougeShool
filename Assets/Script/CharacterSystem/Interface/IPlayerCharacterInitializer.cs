using Game.IManager;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.IManager
{
    /// <summary>
    /// 플레이어 캐릭터 초기화 책임을 가지는 인터페이스입니다.
    /// 캐릭터 생성, 슬롯 배치 등을 수행합니다.
    /// </summary>
    public interface IPlayerCharacterInitializer
    {
        /// <summary>
        /// 플레이어 캐릭터의 초기화 루틴을 수행합니다.
        /// 예: 캐릭터 생성, 슬롯 위치 설정 등
        /// </summary>
        void Setup();

        /// <summary>
        /// 외부 매니저 인스턴스를 주입받아 내부 로직에 사용합니다.
        /// </summary>
        /// <param name="playerManager">플레이어 매니저 인스턴스</param>
        /// <param name="slotRegistry">슬롯 레지스트리 인스턴스</param>
        /// <param name="animationFacade">애니메이션 파사드 인스턴스</param>
        /// <param name="playerCharacterSelectionManager">플레이어 캐릭터 선택 매니저 인스턴스</param>
        /// <param name="gameStateManager">게임 상태 매니저 인스턴스</param>
        void Inject(IPlayerManager playerManager, ISlotRegistry slotRegistry, Game.AnimationSystem.Interface.IAnimationFacade animationFacade, Game.CoreSystem.Interface.IPlayerCharacterSelectionManager playerCharacterSelectionManager, Game.CoreSystem.Interface.IGameStateManager gameStateManager);
    }
}
