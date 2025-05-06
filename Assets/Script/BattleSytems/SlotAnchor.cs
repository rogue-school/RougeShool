using UnityEngine;

namespace Game.Slots
{
    /// <summary>
    /// 씬 내 슬롯의 소유자와 포지션 역할을 나타냅니다.
    /// 역할에 따라 적절한 위치 Enum을 할당합니다.
    /// </summary>
    public class SlotAnchor : MonoBehaviour
    {
        [Header("슬롯 소유자")]
        public SlotOwner owner;

        [Header("슬롯의 용도")]
        public SlotRole role;

        [Header("전투 턴 슬롯 위치 (선공/후공)")]
        public BattleSlotPosition battleSlotPosition;

        [Header("캐릭터 배치 슬롯 위치 (플레이어/적)")]
        public CharacterSlotPosition characterSlotPosition;

        [Header("스킬 카드 슬롯 위치 (손 패 슬롯)")]
        public SkillCardSlotPosition skillCardSlotPosition;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = owner == SlotOwner.Player ? Color.cyan : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);

            string label = $"Owner: {owner}\n" +
                           $"Role: {role}\n" +
                           $"Battle: {battleSlotPosition}\n" +
                           $"Character: {characterSlotPosition}\n" +
                           $"SkillCard: {skillCardSlotPosition}";

            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, label);
        }
#endif
    }
}
