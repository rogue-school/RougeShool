using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 캐릭터 사망 이벤트를 수신하는 리스너 인터페이스입니다.
    /// 외부 시스템(매니저, 상태 기계 등)이 캐릭터 사망을 감지하고 대응할 수 있도록 설계되었습니다.
    /// </summary>
    public interface ICharacterDeathListener
    {
        /// <summary>
        /// 캐릭터가 사망했을 때 호출됩니다.
        /// 게임 진행 흐름이나 UI 갱신 등 후처리 로직을 여기에 구현할 수 있습니다.
        /// </summary>
        /// <param name="character">
        /// 사망한 캐릭터 인스턴스입니다. <see cref="ICharacter"/>를 통해 플레이어 또는 적일 수 있습니다.
        /// </param>
        void OnCharacterDied(ICharacter character);
    }
}
