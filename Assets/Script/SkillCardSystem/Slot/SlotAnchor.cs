using UnityEngine;
using Game.CharacterSystem.Slot;
using Game.CombatSystem.Data;
using Game.SkillCardSystem.Slot;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 씬 내 슬롯 오브젝트의 소유자와 역할, 위치 정보를 담는 클래스입니다.
    /// 전투, 캐릭터, 스킬 카드 슬롯 위치를 명시적으로 설정합니다.
    /// </summary>
    public class SlotAnchor : MonoBehaviour
    {
        #region 인스펙터 필드

        [Header("슬롯 소유자")]
        [Tooltip("PLAYER 또는 ENEMY")]
        public SlotOwner owner;

        [Header("슬롯의 용도")]
        [Tooltip("슬롯이 전투용인지, 캐릭터용인지, 손패용인지 등 역할 정의")]
        public SlotRole role;

        [Header("전투 턴 슬롯 위치 (선공/후공)")]
        public CombatSlotPosition battleSlotPosition;

        [Header("캐릭터 배치 슬롯 위치 (플레이어/적)")]
        public CharacterSlotPosition characterSlotPosition;

        [Header("스킬 카드 슬롯 위치 (손 패 슬롯)")]
        public SkillCardSlotPosition skillCardSlotPosition;

        #endregion

#if UNITY_EDITOR
        #region 디버그 시각화 (Gizmos)

        private void OnDrawGizmos()
        {
            Gizmos.color = owner == SlotOwner.PLAYER ? Color.cyan : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);

            string label = $"Owner: {owner}\n" +
                           $"Role: {role}\n" +
                           $"Battle: {battleSlotPosition}\n" +
                           $"Character: {characterSlotPosition}\n" +
                           $"SkillCard: {skillCardSlotPosition}";

            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, label);
        }

        #endregion
#endif
    }
}
