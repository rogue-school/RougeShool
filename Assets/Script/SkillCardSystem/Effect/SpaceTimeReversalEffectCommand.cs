using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CoreSystem.Utility;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시공간 역행 효과를 적용하는 커맨드.
    /// 소스 캐릭터의 체력을 지정된 턴 수 전 상태로 복원합니다.
    /// </summary>
    public class SpaceTimeReversalEffectCommand : ICardEffectCommand
    {
        private readonly int turnsAgo;

        public SpaceTimeReversalEffectCommand(int turnsAgo = 3)
        {
            this.turnsAgo = turnsAgo;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source is not ICharacter character)
            {
                GameLogger.LogWarning("[SpaceTimeReversalEffectCommand] 소스가 캐릭터가 아니거나 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // SkillCardDefinition의 커스텀 설정이 있으면 우선 사용
            int finalTurnsAgo = turnsAgo;
            
            if (context.Card?.CardDefinition != null)
            {
                var cfg = context.Card.CardDefinition.configuration;
                if (cfg != null && cfg.hasEffects)
                {
                    // 해당 카드의 EffectConfiguration 중 SpaceTimeReversalEffectSO 항목의 커스텀 설정을 확인
                    foreach (var eff in cfg.effects)
                    {
                        if (eff?.effectSO is SpaceTimeReversalEffectSO && eff.useCustomSettings)
                        {
                            // 커스텀 설정에서 turnsAgo 읽기
                            finalTurnsAgo = eff.customSettings.spaceTimeReversalTurnsAgo;
                            GameLogger.LogInfo($"[SpaceTimeReversalEffectCommand] 커스텀 설정 사용: turnsAgo={finalTurnsAgo}", GameLogger.LogCategory.SkillCard);
                            break;
                        }
                    }
                }
            }

            // CharacterBase의 RestoreHPFromHistory 메서드 호출
            if (character is CharacterBase characterBase)
            {
                characterBase.RestoreHPFromHistory(finalTurnsAgo);
                GameLogger.LogInfo($"[SpaceTimeReversalEffectCommand] {character.GetCharacterName()}의 체력을 {finalTurnsAgo}턴 전 상태로 복원", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogWarning($"[SpaceTimeReversalEffectCommand] {character.GetCharacterName()}는 CharacterBase가 아닙니다. 시공간 역행을 적용할 수 없습니다.", GameLogger.LogCategory.SkillCard);
            }
        }
    }
}

