using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Effect;
using Game.ItemSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시전자에게 공격력 증가 버프를 부여하는 커맨드입니다.
    /// ItemSystem의 AttackPowerBuffEffect를 사용하여 데미지 계산과 연동됩니다.
    /// </summary>
    public class AttackPowerBuffSkillCommand : ICardEffectCommand
    {
        private readonly int _attackPowerBonus;
        private readonly int _duration;
        private readonly Sprite _icon;
        private readonly string _effectDisplayName;

        public AttackPowerBuffSkillCommand(int attackPowerBonus, int duration, Sprite icon = null, string effectDisplayName = null)
        {
            _attackPowerBonus = attackPowerBonus;
            _duration = duration;
            _icon = icon;
            _effectDisplayName = effectDisplayName;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source == null)
            {
                GameLogger.LogWarning("[AttackPowerBuffSkillCommand] 시전자가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (_attackPowerBonus <= 0)
            {
                GameLogger.LogWarning("[AttackPowerBuffSkillCommand] 공격력 증가 수치가 0 이하입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (_duration <= 0)
            {
                GameLogger.LogWarning("[AttackPowerBuffSkillCommand] 버프 지속 턴 수가 0 이하입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (context.Source is not ICharacter character)
            {
                GameLogger.LogWarning("[AttackPowerBuffSkillCommand] 시전자가 캐릭터가 아닙니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 버프 이름은 스킬 카드 이름을 우선 사용하고, 없으면 SO에서 전달된 기본 이름을 사용한다.
            string sourceEffectName = _effectDisplayName;
            if (context.Card != null)
            {
                try
                {
                    string cardName = context.Card.GetCardName();
                    if (!string.IsNullOrWhiteSpace(cardName))
                    {
                        sourceEffectName = cardName;
                    }
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogWarning($"[AttackPowerBuffSkillCommand] 카드 이름 조회 중 오류: {ex.Message}", GameLogger.LogCategory.SkillCard);
                }
            }

            // 아이템 전용 턴 정책(TargetTurnOnly)을 그대로 재사용 (대상 자신의 턴에만 감소)
            var effect = new AttackPowerBuffEffect(
                _attackPowerBonus,
                _duration,
                ItemEffectTurnPolicy.TargetTurnOnly,
                _icon,
                sourceItemName: null,
                sourceEffectName: sourceEffectName);

            character.RegisterPerTurnEffect(effect);
        }
    }
}


