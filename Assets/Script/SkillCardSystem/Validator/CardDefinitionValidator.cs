using System.Linq;
using UnityEngine;
using Game.SkillCardSystem.Data;

namespace Game.SkillCardSystem.Validator
{
    /// <summary>
    /// 카드 정의 유효성 검사기. 기본적인 누락/상충을 사전에 차단합니다.
    /// </summary>
    public static class CardDefinitionValidator
    {
        public static bool Validate(SkillCardDefinition def, out string message)
        {
            if (def == null) { message = "정의가 null"; return false; }
            if (string.IsNullOrEmpty(def.cardId)) { message = "cardId 누락"; return false; }
            if (string.IsNullOrWhiteSpace(def.displayName)) { message = "이름 누락"; return false; }
            if (!def.configuration.hasEffects || def.configuration.effects == null || def.configuration.effects.Count == 0) { message = "효과 미지정"; return false; }

            // 타겟 규칙과 효과의 기본 호환성(확장 지점)
            if (def.configuration.targetRule == "None" && def.configuration.effects.Any(e => e != null && e.effectSO != null))
            { message = "타겟 없음인데 효과가 지정됨"; return false; }

            // order 정렬 검증(음수 허용 안 함)
            if (def.configuration.effects.Any(e => e != null && e.order < 0)) { message = "effect order는 0 이상"; return false; }

            message = "OK";
            return true;
        }
    }
}


