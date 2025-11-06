using Game.CharacterSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 가드 버프 효과 클래스입니다.
    /// 1턴 동안 모든 데미지와 상태이상을 차단합니다.
    /// </summary>
    public class GuardBuff : OwnTurnEffectBase
    {
        /// <summary>
        /// 가드 버프 적용 시 재생할 이펙트 프리팹
        /// </summary>
        public GameObject ActivateEffectPrefab { get; private set; }

        /// <summary>
        /// 가드가 데미지를 차단할 때 재생할 이펙트 프리팹
        /// </summary>
        public GameObject BlockEffectPrefab { get; private set; }

        /// <summary>
        /// 가드가 데미지를 차단할 때 재생할 사운드
        /// </summary>
        public AudioClip BlockSfxClip { get; private set; }

        /// <summary>
        /// 가드 버프를 생성합니다.
        /// </summary>
        /// <param name="duration">지속 턴 수 (기본값: 1)</param>
        /// <param name="icon">UI 아이콘</param>
        public GuardBuff(int duration = 1, Sprite icon = null) : base(duration, icon)
        {
            ActivateEffectPrefab = null;
            BlockEffectPrefab = null;
            BlockSfxClip = null;
        }

        /// <summary>
        /// 가드 버프를 생성합니다 (가드 차단 이펙트/사운드 포함).
        /// </summary>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">UI 아이콘</param>
        /// <param name="blockEffectPrefab">가드 차단 이펙트 프리팹</param>
        /// <param name="blockSfxClip">가드 차단 사운드</param>
        public GuardBuff(int duration, Sprite icon, GameObject blockEffectPrefab, AudioClip blockSfxClip) : base(duration, icon)
        {
            ActivateEffectPrefab = null;
            BlockEffectPrefab = blockEffectPrefab;
            BlockSfxClip = blockSfxClip;
        }

        /// <summary>
        /// 가드 버프를 생성합니다 (가드 적용/차단 이펙트/사운드 포함).
        /// </summary>
        /// <param name="duration">지속 턴 수</param>
        /// <param name="icon">UI 아이콘</param>
        /// <param name="activateEffectPrefab">가드 버프 적용 이펙트 프리팹</param>
        /// <param name="blockEffectPrefab">가드 차단 이펙트 프리팹</param>
        /// <param name="blockSfxClip">가드 차단 사운드</param>
        public GuardBuff(int duration, Sprite icon, GameObject activateEffectPrefab, GameObject blockEffectPrefab, AudioClip blockSfxClip) : base(duration, icon)
        {
            ActivateEffectPrefab = activateEffectPrefab;
            BlockEffectPrefab = blockEffectPrefab;
            BlockSfxClip = blockSfxClip;
        }

        /// <summary>
        /// 턴 감소 시 가드 상태를 업데이트합니다.
        /// </summary>
        /// <param name="target">가드 버프가 적용된 캐릭터</param>
        protected override void OnTurnDecrement(ICharacter target)
        {
            target.SetGuarded(!IsExpired);
        }
    }
}
