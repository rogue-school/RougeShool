using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 대상에게 스턴 디버프를 적용하는 커맨드입니다.
    /// </summary>
    public class StunEffectCommand : ICardEffectCommand
    {
        private readonly int duration;
        private readonly Sprite icon;

        public StunEffectCommand(int duration, Sprite icon = null)
        {
            this.duration = Mathf.Max(1, duration);
            this.icon = icon;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Target == null)
            {
                Game.CoreSystem.Utility.GameLogger.LogWarning("[StunEffectCommand] 대상이 null입니다.", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                return;
            }

            // 원본 효과 SO 이름 가져오기 (툴팁 표시용)
            string sourceEffectName = GetSourceEffectName(context);
            
            var stun = new StunDebuff(duration, icon, sourceEffectName);
            if (context.Target.RegisterStatusEffect(stun))
            {
                Game.CoreSystem.Utility.GameLogger.LogInfo($"[StunEffectCommand] {context.Target.GetCharacterName()} 스턴 적용 ({duration}턴)", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
            }
            else
            {
                Game.CoreSystem.Utility.GameLogger.LogInfo($"[StunEffectCommand] 보호 상태로 스턴 차단", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// EffectConfiguration에서 원본 효과 SO 이름을 가져옵니다 (툴팁 표시용).
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <returns>원본 효과 SO 이름 (없으면 null)</returns>
        private string GetSourceEffectName(ICardExecutionContext context)
        {
            if (context?.Card?.CardDefinition == null)
            {
                return null;
            }

            var cardDefinition = context.Card.CardDefinition;
            if (!cardDefinition.configuration.hasEffects)
            {
                return null;
            }

            foreach (var effectConfig in cardDefinition.configuration.effects)
            {
                if (effectConfig.effectSO is StunEffectSO stunEffectSO)
                {
                    string effectName = stunEffectSO.GetEffectName();
                    if (!string.IsNullOrWhiteSpace(effectName))
                    {
                        return effectName;
                    }
                }
            }

            return null;
        }
    }
}


