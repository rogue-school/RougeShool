using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 캐릭터가 사망했을 때 알림을 받는 리스너 인터페이스입니다.
    /// 외부 시스템(매니저, 상태기 등)에서 캐릭터 사망 이벤트를 감지할 때 사용됩니다.
    /// </summary>
    public interface ICharacterDeathListener
    {
        /// <summary>
        /// 캐릭터가 사망했을 때 호출됩니다.
        /// </summary>
        /// <param name="character">사망한 캐릭터 인스턴스</param>
        void OnCharacterDied(ICharacter character);
    }
}
