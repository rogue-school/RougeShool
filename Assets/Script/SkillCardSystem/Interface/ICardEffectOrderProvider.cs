using System.Collections.Generic;
using Game.SkillCardSystem.Effect;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 카드가 효과 실행 순서를 제공할 수 있을 때 구현합니다.
    /// 미구현 시 기존 등록 순서를 사용합니다.
    /// </summary>
    public interface ICardEffectOrderProvider
    {
        List<SkillCardEffectSO> GetOrderedEffects();
    }
}


