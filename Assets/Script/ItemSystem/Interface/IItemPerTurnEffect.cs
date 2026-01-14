using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.ItemSystem.Interface
{
    /// <summary>
    /// 아이템으로 인해 발생하는 턴별 효과의 인터페이스입니다.
    /// IPerTurnEffect를 상속하여 기존 시스템과 호환되면서도,
    /// 아이템 전용 턴 정책을 추가로 제공합니다.
    /// </summary>
    public interface IItemPerTurnEffect : IPerTurnEffect
    {
        /// <summary>
        /// 턴 감소 정책을 반환합니다. (아이템 전용)
        /// </summary>
        ItemEffectTurnPolicy TurnPolicy { get; }

        /// <summary>
        /// 효과를 즉시 만료시킵니다.
        /// </summary>
        void Expire();
    }

    /// <summary>
    /// 아이템 효과의 턴 감소 정책을 정의합니다.
    /// </summary>
    public enum ItemEffectTurnPolicy
    {
        /// <summary>즉시 소모 (1회성, 턴 감소 없음)</summary>
        Immediate,

        /// <summary>매 턴마다 감소 (플레이어/적 턴 구분 없음)</summary>
        EveryTurn,

        /// <summary>대상의 턴에만 감소 (플레이어에게 적용 시 플레이어 턴, 적에게 적용 시 적 턴)</summary>
        TargetTurnOnly,

        /// <summary>대상의 상대 턴에만 감소 (플레이어에게 적용 시 적 턴, 적에게 적용 시 플레이어 턴)</summary>
        OpponentTurnOnly
    }
}
