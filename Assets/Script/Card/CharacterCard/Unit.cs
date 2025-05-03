using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// 플레이어와 적 유닛의 공통 전투 로직을 담당하는 클래스입니다.
    /// 체력은 인스펙터에서 설정합니다.
    /// </summary>
    public abstract class Unit : MonoBehaviour, IUnit
    {
        [Header("전투 수치 (인스펙터에서 설정 가능)")]
        public int maxHP = 20;
        public int currentHP;

        protected virtual void Awake()
        {
            currentHP = maxHP; // 시작 시 최대 체력으로 초기화
        }

        public virtual void TakeDamage(int damage)
        {
            currentHP -= damage;
            currentHP = Mathf.Max(currentHP, 0);

            Debug.Log($"[Unit] {gameObject.name}이(가) {damage} 피해를 입음 (HP: {currentHP})");
        }

        public virtual void Heal(int amount)
        {
            currentHP += amount;
            currentHP = Mathf.Min(currentHP, maxHP);

            Debug.Log($"[Unit] {gameObject.name}이(가) {amount} 회복함 (HP: {currentHP})");
        }

        public int GetCurrentHP() => currentHP;

        // 자식이 override 할 수 있는 기본 메서드 추가
        public virtual string GetName() => "Unnamed";
        public virtual Sprite GetPortrait() => null;
        public virtual int GetMaxHP() => maxHP;
    }
}
