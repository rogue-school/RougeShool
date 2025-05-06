using UnityEngine;
using Game.Battle;

namespace Game.Slots
{
    /// <summary>
    /// 씬 내 슬롯의 소유자, 용도, 전투 순서를 명시적으로 나타냅니다.
    /// 이 컴포넌트를 기반으로 슬롯 위치와 역할을 관리합니다.
    /// </summary>
    public enum SlotOwner
    {
        Player,
        Enemy
    }

    public enum SlotRole
    {
        CharacterSpawn,
        CardDrop,
        UIOnly
    }

    public class SlotAnchor : MonoBehaviour
    {
        [Header("Slot Owner")]
        public SlotOwner owner;

        [Header("Slot Role")]
        public SlotRole role;

        [Header("Battle Slot Position")]
        public BattleSlotPosition battleSlotPosition;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = owner == SlotOwner.Player ? Color.blue : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.3f,
                $"{owner} | {role} | {battleSlotPosition}"
            );
        }
#endif
    }
}
