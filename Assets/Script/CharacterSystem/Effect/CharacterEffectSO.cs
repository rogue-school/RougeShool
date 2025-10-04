using UnityEngine;
using Game.CharacterSystem.Interface;

namespace Game.CharacterSystem.Effect
{
    /// <summary>
    /// 캐릭터 이펙트를 정의하는 ScriptableObject 기반의 추상 클래스입니다.
    /// 모든 구체 이펙트는 이 클래스를 상속하여 구현됩니다.
    /// </summary>
    [System.Serializable]
    public abstract class CharacterEffectSO : ScriptableObject, ICharacterEffect
    {
        [Header("기본 정보")]
        [Tooltip("이펙트 이름 (UI, 로그 등에서 사용)")]
        [SerializeField] protected string effectName;

        [Tooltip("이펙트 설명 (툴팁 또는 상세 정보에 사용)")]
        [TextArea]
        [SerializeField] protected string description;

        [Header("아이콘 (버프/디버프 UI용)")]
        [Tooltip("이 효과를 나타낼 기본 아이콘")]
        [SerializeField] protected Sprite effectIcon;

        public virtual string GetEffectName() => effectName;
        public virtual string GetDescription() => description;
        public Sprite GetIcon() => effectIcon;

        public abstract void Initialize(ICharacter character);
        public abstract void OnHealthChanged(ICharacter character, int previousHP, int currentHP);
        public abstract void OnDeath(ICharacter character);
        public abstract void Cleanup(ICharacter character);
    }
}
