namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 버프/디버프 구분을 위한 마커 인터페이스입니다.
    /// 런타임에서 유형 판별만을 목적으로 하며 멤버는 없습니다.
    /// </summary>
    public interface IStatusEffectMarker { }

    /// <summary>
    /// 버프 효과를 나타내는 마커 인터페이스입니다.
    /// </summary>
    public interface IStatusEffectBuff : IPerTurnEffect, IStatusEffectMarker { }

    /// <summary>
    /// 디버프 효과를 나타내는 마커 인터페이스입니다.
    /// </summary>
    public interface IStatusEffectDebuff : IPerTurnEffect, IStatusEffectMarker { }
}


