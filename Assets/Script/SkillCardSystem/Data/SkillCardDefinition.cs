using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Effect;

namespace Game.SkillCardSystem.Data
{
    /// <summary>
    /// 플레이어/적이 공용으로 사용하는 스킬 카드 정의 ScriptableObject 입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "SkillCardSystem/SkillCardDefinition", fileName = "SkillCardDefinition")]
    public class SkillCardDefinition : ScriptableObject
    {
        [Header("식별자/표시")]
        public string id;
        public string displayNameKO;
        [TextArea]
        public string descriptionKO;
        public Sprite icon;

        [Header("소유자 정책/카테고리")]
        public OwnerPolicy ownerPolicy = OwnerPolicy.Shared;
        public List<string> categories = new();
        public List<string> keywords = new();

        [Header("드로우/타겟/가중치")]
        public int drawWeight = 1;
        public int actionCost = 0;
        public TargetRule targetRule = TargetRule.Enemy;

        [Header("효과 구성")]
        public List<EffectRef> effects = new();

        [Header("소유자별 수치 보정(선택)")]
        public List<OwnerModifier> ownerModifiers = new();
    }

    public enum TargetRule { Self, Ally, Enemy, Any, None }

    [System.Serializable]
    public class EffectRef
    {
        public SkillCardEffectSO effect; // 기존 Effect SO 재사용
        public float magnitudeOverride = 0f; // 0이면 원본 유지
        public int durationOverride = 0; // 0이면 원본 유지
        public int order = 0; // 낮을수록 먼저 실행
    }

    [System.Serializable]
    public class OwnerModifier
    {
        public Owner owner;
        public float magnitudeMultiplier = 1f;
        public int durationDelta = 0;
    }

    public enum Owner { Player, Enemy }
}
