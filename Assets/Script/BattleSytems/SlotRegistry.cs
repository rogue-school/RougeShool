using UnityEngine;
using System.Collections.Generic;
using Game.Slots;

namespace Game.Managers
{
    /// <summary>
    /// 씬 내의 모든 슬롯(SlotAnchor)을 수집하고 포지션/용도 기준으로 조회할 수 있는 슬롯 관리자입니다.
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
        /// 전투 실행 슬롯 (선/후공 슬롯) 조회
        /// </summary>
        public SlotAnchor GetBattleSlot(BattleSlotPosition position)
        {
            return allAnchors.Find(a => a.battleSlotPosition == position);
        }

        /// <summary>
        /// 캐릭터 배치 슬롯 (플레이어 또는 적 위치) 조회
        /// </summary>
        public SlotAnchor GetCharacterSlot(CharacterSlotPosition position)
        {
            return allAnchors.Find(a =>
                a.role == SlotRole.CharacterSpawn &&
                a.characterSlotPosition == position
            );
        }
        /// <summary>
        /// 스킬 카드 핸드 슬롯 (플레이어 또는 적 핸드 슬롯) 조회
        /// </summary>
        public SlotAnchor GetSkillCardSlot(SkillCardSlotPosition position)
        {
            return allAnchors.Find(a => a.skillCardSlotPosition == position);
        }

        /// <summary>
        /// 디버깅용: 모든 슬롯 반환
        /// </summary>
        public List<SlotAnchor> GetAll() => allAnchors;
    }
}
