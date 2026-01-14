using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.CoreSystem.Utility;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 무적 버프 효과 클래스입니다.
    /// 지정된 턴 동안 모든 데미지를 완전히 차단합니다.
    /// 가드와 달리 어떠한 스킬로도 데미지를 줄 수 없는 완전 무적 상태입니다.
    /// </summary>
    public class InvincibilityBuff : OwnTurnEffectBase
    {
        /// <summary>
        /// 무적 버프 적용 시 재생할 이펙트 프리팹
        /// </summary>
        public GameObject ActivateEffectPrefab { get; private set; }

        /// <summary>
        /// 무적 버프를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수 (기본값: 1)</param>
        /// <param name="icon">UI 아이콘</param>
        public InvincibilityBuff(int duration = 1, Sprite icon = null) : base(duration, icon)
        {
            ActivateEffectPrefab = null;
        }

        /// <summary>
        /// 무적 버프를 생성합니다 (적용 이펙트 포함).
        /// </summary>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">UI 아이콘</param>
        /// <param name="activateEffectPrefab">무적 버프 적용 이펙트 프리팹</param>
        public InvincibilityBuff(int duration, Sprite icon, GameObject activateEffectPrefab) : base(duration, icon)
        {
            ActivateEffectPrefab = activateEffectPrefab;
        }

        /// <summary>
        /// 턴 감소 시 무적 상태를 업데이트합니다.
        /// </summary>
        /// <param name="target">무적 버프가 적용된 캐릭터</param>
        protected override void OnTurnDecrement(ICharacter target)
        {
            if (target is CharacterBase characterBase)
            {
                // 만료되지 않았으면 무적 상태 유지, 만료되었으면 해제
                characterBase.SetInvincible(!IsExpired);
                GameLogger.LogInfo($"[InvincibilityBuff] 무적 상태 업데이트: {!IsExpired} (남은 턴: {RemainingTurns})", GameLogger.LogCategory.Character);
            }
        }
    }
}

