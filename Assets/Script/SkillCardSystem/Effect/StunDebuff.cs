using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 기절(스턴) 디버프입니다. 지속 시간 동안 스킬 및 아이템 사용을 불가능하게 합니다.
    /// </summary>
    public class StunDebuff : IStatusEffectDebuff
    {
        /// <summary>남은 턴 수</summary>
        public int RemainingTurns { get; private set; }

        /// <summary>효과 아이콘</summary>
        public Sprite Icon { get; private set; }

        /// <summary>만료 여부</summary>
        public bool IsExpired => RemainingTurns <= 0;

        public StunDebuff(int duration, Sprite icon = null)
        {
            RemainingTurns = duration;
            Icon = icon;
        }

        /// <summary>
        /// 턴 시작 시 남은 턴을 감소시킵니다.
        /// </summary>
        public void OnTurnStart(ICharacter target)
        {
            if (target == null) return;
            RemainingTurns--;
            Game.CoreSystem.Utility.GameLogger.LogInfo($"[StunDebuff] {target.GetCharacterName()} 기절 디버프 턴 감소 (남은 턴: {RemainingTurns})", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
        }
    }
}


