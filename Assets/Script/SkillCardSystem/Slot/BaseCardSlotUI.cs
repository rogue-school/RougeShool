using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Slot
{
    /// <summary>
    /// 카드가 드롭될 수 있는 모든 슬롯 UI의 기본 베이스 클래스입니다.
    /// 캐스터, 타겟, 슬롯 위치와 관련된 기본 동작을 정의합니다.
    /// </summary>
    public abstract class BaseCardSlotUI : MonoBehaviour
    {
        [Header("이 슬롯에서 카드를 실행할 주체 (캐스터)")]
        protected ICharacter caster;

        [Header("이 슬롯의 대상 (타겟)")]
        protected ICharacter target;

        /// <summary>
        /// 슬롯의 Anchor 정보를 기반으로 자동 설정 (파생 클래스에서 오버라이드)
        /// </summary>
        public virtual void AutoBind()
        {
            // 기본 구현은 없음. 파생 클래스에서 필요 시 SlotAnchor를 기반으로 바인딩
        }

        /// <summary>
        /// 슬롯에 드롭된 카드를 반환합니다.
        /// 실제 구현은 파생 클래스에서 정의
        /// </summary>
        public virtual ISkillCard GetCard()
        {
            return GetComponentInChildren<ISkillCard>();
        }

        /// <summary>
        /// 슬롯에 드롭된 카드를 자동 실행합니다.
        /// 반드시 파생 클래스에서 오버라이드해야 합니다.
        /// </summary>
        public abstract void ExecuteCardAutomatically();
    }
}
