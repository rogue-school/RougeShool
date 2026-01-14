using Game.CharacterSystem.Interface;

namespace Game.ItemSystem.Interface
{
    /// <summary>
    /// 아이템 효과 인터페이스입니다.
    /// </summary>
    public interface IItemEffect
    {
        /// <summary>
        /// 효과 이름을 반환합니다.
        /// </summary>
        string GetEffectName();
        
        /// <summary>
        /// 효과 설명을 반환합니다.
        /// </summary>
        string GetDescription();
        
        /// <summary>
        /// 효과 아이콘을 반환합니다.
        /// </summary>
        UnityEngine.Sprite GetIcon();
        
        /// <summary>
        /// 효과 커맨드를 생성합니다.
        /// </summary>
        /// <param name="power">효과 파워</param>
        /// <returns>효과 커맨드</returns>
        IItemEffectCommand CreateEffectCommand(int power);
        
        /// <summary>
        /// 효과를 직접 적용합니다.
        /// </summary>
        /// <param name="context">사용 컨텍스트</param>
        /// <param name="value">효과 값</param>
        void ApplyEffect(IItemUseContext context, int value);
    }

    /// <summary>
    /// 아이템 효과 커맨드 인터페이스입니다.
    /// </summary>
    public interface IItemEffectCommand
    {
        /// <summary>
        /// 효과를 실행합니다.
        /// </summary>
        /// <param name="context">사용 컨텍스트</param>
        /// <returns>실행 성공 여부</returns>
        bool Execute(IItemUseContext context);
    }

    /// <summary>
    /// 아이템 사용 컨텍스트 인터페이스입니다.
    /// </summary>
    public interface IItemUseContext
    {
        /// <summary>
        /// 아이템 사용자입니다.
        /// </summary>
        ICharacter User { get; }
        
        /// <summary>
        /// 아이템 대상입니다.
        /// </summary>
        ICharacter Target { get; }
        
        /// <summary>
        /// 아이템 정의입니다.
        /// </summary>
        Data.ItemDefinition ItemDefinition { get; }
    }
}
