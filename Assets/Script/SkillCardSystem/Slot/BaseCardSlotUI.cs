using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Slot
{
    /// <summary>
    /// 모든 스킬 카드 슬롯 UI의 기본 클래스입니다.
    /// 캐릭터 정보와 슬롯 상호작용을 관리하며, 실제 실행 로직은 파생 클래스에 위임됩니다.
    /// </summary>
    public abstract class BaseCardSlotUI : MonoBehaviour
    {
        [Header("이 슬롯에서 카드를 실행할 주체 (캐스터)")]
        [Tooltip("슬롯에서 카드를 실행할 캐릭터")]
        protected ICharacter caster;

        [Header("이 슬롯의 대상 (타겟)")]
        [Tooltip("카드 실행 시 효과를 받을 대상 캐릭터")]
        protected ICharacter target;

        /// <summary>
        /// 슬롯 위치를 기준으로 자동으로 캐스터/타겟 등을 바인딩합니다.
        /// 파생 클래스에서 슬롯 위치 또는 소유자 정보를 기반으로 구현해야 합니다.
        /// 예: 플레이어 슬롯 → caster = 플레이어, target = 적
        /// </summary>
        public virtual void AutoBind()
        {
            // 예시: 파생 클래스에서 캐릭터 슬롯 위치 기반으로 자동 연결 구현
        }

        /// <summary>
        /// 슬롯에 존재하는 드롭된 스킬 카드를 반환합니다.
        /// 실제 구현은 파생 클래스에서 정의되어야 하며, 드롭 구조에 따라 다를 수 있습니다.
        /// </summary>
        /// <returns>슬롯에 존재하는 스킬 카드 인스턴스</returns>
        public abstract ISkillCard GetCard();

        /// <summary>
        /// 슬롯에 있는 카드를 즉시 실행합니다.
        /// 실제 실행 로직은 파생 클래스에서 정의되어야 합니다.
        /// </summary>
        public abstract void ExecuteCardAutomatically();
    }
}
