using UnityEngine;
using System.Collections.Generic;
using Game.Slots;
using Game.Battle;

namespace Game.Managers
{
    /// <summary>
    /// 씬 내의 모든 슬롯(SlotAnchor)을 수집하고 필터링하는 중앙 슬롯 관리자입니다.
    /// 캐릭터, 카드 드롭, UI용 슬롯을 위치/역할 기준으로 쉽게 조회할 수 있습니다.
    /// </summary>
    public class SlotRegistry : MonoBehaviour
    {
        public static SlotRegistry Instance { get; private set; }

        private List<SlotAnchor> allAnchors;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            allAnchors = new List<SlotAnchor>(FindObjectsOfType<SlotAnchor>());
            Debug.Log($"[SlotRegistry] 슬롯 {allAnchors.Count}개 수집 완료");
        }

        /// <summary>
        /// 소유자, 용도, 전투 위치에 정확히 일치하는 슬롯 하나를 반환합니다.
        /// </summary>
        public SlotAnchor GetSlot(SlotOwner owner, SlotRole role, BattleSlotPosition battleSlotPosition)
        {
            return allAnchors.Find(a =>
                a.owner == owner &&
                a.role == role &&
                a.battleSlotPosition == battleSlotPosition
            );
        }

        /// <summary>
        /// 소유자와 용도 기준으로 슬롯 여러 개를 반환합니다.
        /// 예: 모든 Player 카드 드롭 슬롯
        /// </summary>
        public List<SlotAnchor> GetAll(SlotOwner owner, SlotRole role)
        {
            return allAnchors.FindAll(a => a.owner == owner && a.role == role);
        }

        /// <summary>
        /// 전체 슬롯 리스트 반환 (디버깅용)
        /// </summary>
        public List<SlotAnchor> GetAll() => allAnchors;
    }
}
