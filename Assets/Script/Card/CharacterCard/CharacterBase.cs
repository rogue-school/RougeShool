using UnityEngine;

namespace Game.Characters
{
    /// <summary>
    /// 모든 캐릭터(플레이어 및 적)의 기본 클래스입니다.
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour
    {
        [Header("체력 정보")]
        [SerializeField] protected int currentHP;
        [SerializeField] protected int maxHP;

        /// <summary>
        /// 캐릭터 이름 반환 (상속 클래스에서 구현)
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// 캐릭터 초상화 반환 (상속 클래스에서 구현)
        /// </summary>
        public abstract Sprite GetPortrait();

        /// <summary>
        /// 데미지를 받아 체력을 감소시킵니다.
        /// </summary>
        public virtual void TakeDamage(int value)
        {
            currentHP -= value;
            currentHP = Mathf.Max(currentHP, 0);
        }

        /// <summary>
        /// 기본 체력 초기화
        /// </summary>
        public virtual void Initialize(int hp)
        {
            maxHP = hp;
            currentHP = maxHP;
        }

        public int GetCurrentHP() => currentHP;
        public int GetMaxHP() => maxHP;
    }
}
