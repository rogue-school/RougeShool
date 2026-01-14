using Game.CharacterSystem.Interface;
using Game.ItemSystem.Interface;
using UnityEngine;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 실드 브레이커 디버프 효과입니다.
    /// 적의 방어/반격 효과를 무시할 수 있게 해줍니다.
    /// ItemEffectBase를 상속하여 아이템 전용 턴 관리 시스템을 사용합니다.
    /// </summary>
    public class ShieldBreakerDebuffEffect : ItemEffectBase
    {
        /// <summary>
        /// 실드 브레이커 디버프 효과를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="turnPolicy">턴 감소 정책</param>
        /// <param name="icon">UI 아이콘</param>
        /// <param name="sourceItemName">원본 아이템 이름 (선택적)</param>
        public ShieldBreakerDebuffEffect(
            int duration,
            ItemEffectTurnPolicy turnPolicy,
            Sprite icon = null,
            string sourceItemName = null)
            : base(duration, turnPolicy, icon, sourceItemName)
        {
        }

        /// <summary>
        /// 턴 감소 시 추가 동작 (실드 브레이커는 별도 동작 없음)
        /// </summary>
        protected override void OnTurnDecrement(ICharacter target)
        {
            // 실드 브레이커는 턴 감소 외 특별한 동작 없음
        }

        /// <summary>
        /// 실드 브레이커 효과가 활성화되어 있는지 확인합니다.
        /// </summary>
        /// <returns>효과 활성화 여부</returns>
        public bool IsShieldBreakerActive()
        {
            return !IsExpired;
        }
    }
}
