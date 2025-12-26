using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CoreSystem.Utility;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 분신 버프를 적용하는 커맨드. 소스 캐릭터에게 CloneBuff를 적용합니다.
    /// </summary>
    public class CloneEffectCommand : ICardEffectCommand
    {
        private readonly int cloneHP;
        private readonly Sprite icon;

        public CloneEffectCommand(int cloneHP = 10, Sprite icon = null)
        {
            this.cloneHP = cloneHP;
            this.icon = icon;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source is not ICharacter character)
            {
                GameLogger.LogWarning("[CloneEffectCommand] 소스가 캐릭터가 아니거나 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // SkillCardDefinition의 커스텀 설정이 있으면 우선 사용
            int finalCloneHP = cloneHP;
            Sprite finalIcon = icon;
            
            if (context.Card?.CardDefinition != null)
            {
                var cfg = context.Card.CardDefinition.configuration;
                if (cfg != null && cfg.hasEffects)
                {
                    // 해당 카드의 EffectConfiguration 중 CloneEffectSO 항목의 커스텀 설정을 확인
                    foreach (var eff in cfg.effects)
                    {
                        if (eff?.effectSO is CloneEffectSO && eff.useCustomSettings)
                        {
                            // 커스텀 설정에서 cloneHP 읽기
                            finalCloneHP = eff.customSettings.cloneHP;
                            GameLogger.LogInfo($"[CloneEffectCommand] 커스텀 설정 사용: cloneHP={finalCloneHP}", GameLogger.LogCategory.SkillCard);
                            break;
                        }
                    }
                }
            }

            var buff = new CloneBuff(finalCloneHP, finalIcon);
            character.RegisterPerTurnEffect(buff);
            
            // CharacterBase의 cloneHP도 동기화
            if (character is CharacterBase characterBase)
            {
                characterBase.SetCloneHP(finalCloneHP);
            }
            
            GameLogger.LogInfo($"[CloneEffectCommand] {character.GetCharacterName()}에게 분신 버프 적용 (추가 체력: {finalCloneHP})", GameLogger.LogCategory.SkillCard);
        }
    }
}

