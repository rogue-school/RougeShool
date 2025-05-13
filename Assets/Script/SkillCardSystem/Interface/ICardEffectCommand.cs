namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 커맨드 패턴을 위한 카드 이펙트 명령 인터페이스입니다.
    /// </summary>
    public interface ICardEffectCommand
    {
        void Execute();
    }
}
