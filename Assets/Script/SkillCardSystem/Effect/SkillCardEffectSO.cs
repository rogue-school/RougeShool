using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 스킬 카드의 이펙트를 정의하는 ScriptableObject 기반의 추상 클래스입니다.
    /// 모든 구체 이펙트는 이 클래스를 상속하여 구현됩니다.
    /// </summary>
    [System.Serializable]
    public abstract class SkillCardEffectSO : ScriptableObject, ICardEffect
    {
        [Header("기본 정보")]
        [Tooltip("이펙트 이름 (UI, 로그 등에서 사용)")]
        [SerializeField] private string effectName;

        [Tooltip("이펙트 설명 (툴팁 또는 상세 정보에 사용)")]
        [TextArea]
        [SerializeField] private string description;

        [Header("아이콘 (버프/디버프 UI용)")]
        [Tooltip("이 효과를 나타낼 기본 아이콘")]
        [UnityEngine.Serialization.FormerlySerializedAs("icon")]
        [SerializeField] protected Sprite effectIcon;

        public Sprite GetIcon() => effectIcon;

        /// <summary>
        /// 이펙트의 이름을 반환합니다.
        /// </summary>
        public string GetEffectName() => effectName;

        /// <summary>
        /// 이펙트의 설명을 반환합니다.
        /// </summary>
        public string GetDescription() => description;

        /// <summary>
        /// 실행 가능한 커맨드 객체를 생성합니다.
        /// </summary>
        /// <param name="power">효과의 추가 파워 (예: 카드 스탯 등으로 계산됨)</param>
        /// <returns>ICardEffectCommand 인스턴스</returns>
        public abstract ICardEffectCommand CreateEffectCommand(int power);

        /// <summary>
        /// 실제 이펙트를 실행합니다.
        /// 주로 테스트 또는 런타임 예외 처리 목적입니다.
        /// </summary>
        /// <param name="context">실행 컨텍스트 (시전자, 대상 등)</param>
        /// <param name="value">실제 효과 수치</param>
        /// <param name="controller">전투 턴 매니저 (선택적)</param>
        public abstract void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null);
    }
}
