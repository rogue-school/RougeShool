using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// 플레이어와 적 공통의 유닛 기능을 정의하는 기본 클래스
    /// </summary>
    public abstract class Unit : MonoBehaviour
    {
        public int currentHP = 10;
        public int maxHP = 10;
        public int block = 0;

        public virtual void TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(damage - block, 0);
            currentHP -= actualDamage;
            block = Mathf.Max(block - damage, 0);

            Debug.Log($"[{name}] 피해 {actualDamage} 입음. 남은 체력: {currentHP}");
        }

        public virtual void AddBlock(int amount)
        {
            block += amount;
            Debug.Log($"[{name}] 방어력 {amount} 증가. 총 방어력: {block}");
        }
    }
}
