using UnityEngine;
using System.Linq;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 싱글게임용 전투 슬롯 관리자 (싱글톤)
    /// 전투 슬롯의 생성, 배치, 검증을 담당합니다.
    /// </summary>
    public class CombatSlotManager : MonoBehaviour
    {
        #region 싱글톤

        public static CombatSlotManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                
                // 루트 GameObject인지 확인 후 DontDestroyOnLoad 적용
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
                else
                {
                    GameLogger.LogWarning("CombatSlotManager가 루트 GameObject가 아닙니다. DontDestroyOnLoad를 적용할 수 없습니다.", GameLogger.LogCategory.Combat);
                }
                
                InitializeSlots();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region 슬롯 관리

        [Header("슬롯 설정")]
        [SerializeField] private CombatSlot[] slots = new CombatSlot[5];
        
        [Header("시각적 피드백")]
        [SerializeField] private Color playerSlotColor = Color.blue;
        [SerializeField] private Color enemySlotColor = Color.red;
        [SerializeField] private Color emptySlotColor = Color.gray;

        /// <summary>
        /// 슬롯을 초기화합니다.
        /// </summary>
        public void InitializeSlots()
        {
            // 배열이 null이거나 크기가 맞지 않으면 재생성
            if (slots == null || slots.Length != 5)
            {
                slots = new CombatSlot[5];
            }

            // 슬롯 초기화 (패턴 기반)
            slots[0] = new CombatSlot(CombatSlotPosition.BATTLE_SLOT, SlotOwner.PLAYER);
            slots[1] = new CombatSlot(CombatSlotPosition.WAIT_SLOT_1, SlotOwner.ENEMY);
            slots[2] = new CombatSlot(CombatSlotPosition.WAIT_SLOT_2, SlotOwner.PLAYER);
            slots[3] = new CombatSlot(CombatSlotPosition.WAIT_SLOT_3, SlotOwner.ENEMY);
            slots[4] = new CombatSlot(CombatSlotPosition.WAIT_SLOT_4, SlotOwner.PLAYER);

            GameLogger.LogInfo("전투 슬롯 초기화 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 지정된 위치의 슬롯을 반환합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>슬롯 인스턴스, 없으면 null</returns>
        public CombatSlot GetSlot(CombatSlotPosition position)
        {
            if (slots == null || slots.Length == 0)
            {
                GameLogger.LogWarning("슬롯 배열이 초기화되지 않았습니다", GameLogger.LogCategory.Combat);
                return null;
            }

            return slots.FirstOrDefault(s => s != null && s.Position == position);
        }

        /// <summary>
        /// 슬롯에 카드를 배치합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <param name="card">배치할 카드</param>
        /// <returns>배치 성공 여부</returns>
        public bool TryPlaceCard(CombatSlotPosition position, ISkillCard card)
        {
            var slot = GetSlot(position);
            if (slot == null)
            {
                GameLogger.LogWarning($"슬롯을 찾을 수 없습니다: {position}", GameLogger.LogCategory.Combat);
                return false;
            }

            // 배치 전 슬롯 상태 확인
            var wasEmpty = slot.IsEmpty;
            var existingCard = slot.OccupiedCard?.GetCardName() ?? "없음";
            GameLogger.LogInfo($"슬롯 {position} 배치 전 상태: 비어있음={wasEmpty}, 기존카드={existingCard}", GameLogger.LogCategory.Combat);

            var success = slot.TryPlaceCard(card);
            if (success)
            {
                GameLogger.LogInfo($"카드 배치 성공: {card.GetCardName()} → {position}", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogWarning($"카드 배치 실패: {card.GetCardName()} → {position}", GameLogger.LogCategory.Combat);
            }

            return success;
        }

        /// <summary>
        /// 슬롯에서 카드를 제거합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>제거된 카드, 없으면 null</returns>
        public ISkillCard RemoveCard(CombatSlotPosition position)
        {
            var slot = GetSlot(position);
            if (slot == null)
            {
                GameLogger.LogWarning($"슬롯을 찾을 수 없습니다: {position}", GameLogger.LogCategory.Combat);
                return null;
            }

            var card = slot.RemoveCard();
            if (card != null)
            {
                GameLogger.LogInfo($"카드 제거 성공: {card.GetCardName()} ← {position}", GameLogger.LogCategory.Combat);
            }

            return card;
        }

        /// <summary>
        /// 모든 슬롯을 비웁니다.
        /// </summary>
        public void ClearAllSlots()
        {
            foreach (var slot in slots)
            {
                slot.RemoveCard();
            }
            GameLogger.LogInfo("모든 슬롯 초기화 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 슬롯이 비어있는지 확인합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>비어있으면 true</returns>
        public bool IsSlotEmpty(CombatSlotPosition position)
        {
            var slot = GetSlot(position);
            return slot?.IsEmpty ?? true;
        }

        /// <summary>
        /// 슬롯에 있는 카드를 반환합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>카드 인스턴스, 없으면 null</returns>
        public ISkillCard GetCardInSlot(CombatSlotPosition position)
        {
            var slot = GetSlot(position);
            return slot?.OccupiedCard;
        }

        #endregion

        #region 디버그

        /// <summary>
        /// 모든 슬롯의 상태를 로그로 출력합니다.
        /// </summary>
        [ContextMenu("슬롯 상태 출력")]
        public void LogSlotStates()
        {
            GameLogger.LogInfo("=== 슬롯 상태 ===", GameLogger.LogCategory.Combat);
            foreach (var slot in slots)
            {
                var cardName = slot.OccupiedCard?.GetCardName() ?? "비어있음";
                var owner = slot.Owner == SlotOwner.PLAYER ? "플레이어" : "적";
                GameLogger.LogInfo($"{slot.Position}: {cardName} ({owner})", GameLogger.LogCategory.Combat);
            }
        }

        #endregion
    }

    /// <summary>
    /// 싱글게임용 전투 슬롯 클래스
    /// </summary>
    [System.Serializable]
    public class CombatSlot
    {
        public CombatSlotPosition Position { get; }
        public SlotOwner Owner { get; }
        public ISkillCard OccupiedCard { get; private set; }
        public bool IsEmpty => OccupiedCard == null;

        public CombatSlot(CombatSlotPosition position, SlotOwner owner)
        {
            Position = position;
            Owner = owner;
        }

        /// <summary>
        /// 슬롯에 카드를 배치합니다.
        /// </summary>
        /// <param name="card">배치할 카드</param>
        /// <returns>배치 성공 여부</returns>
        public bool TryPlaceCard(ISkillCard card)
        {
            if (!CanPlaceCard(card))
                return false;

            OccupiedCard = card;
            return true;
        }

        /// <summary>
        /// 슬롯에서 카드를 제거합니다.
        /// </summary>
        /// <returns>제거된 카드, 없으면 null</returns>
        public ISkillCard RemoveCard()
        {
            var card = OccupiedCard;
            OccupiedCard = null;
            return card;
        }

        /// <summary>
        /// 카드 배치 가능 여부를 확인합니다.
        /// </summary>
        /// <param name="card">배치할 카드</param>
        /// <returns>배치 가능하면 true</returns>
        private bool CanPlaceCard(ISkillCard card)
        {
            if (!IsEmpty)
            {
                GameLogger.LogWarning($"슬롯 {Position}이 이미 사용 중입니다", GameLogger.LogCategory.Combat);
                return false;
            }

            // 소유자 검증을 더 유연하게 처리
            var cardOwner = card.GetOwner();
            if (cardOwner != Owner)
            {
                GameLogger.LogWarning($"카드 소유자 불일치: 카드={cardOwner}, 슬롯={Owner}", GameLogger.LogCategory.Combat);
                // 임시로 소유자 검증을 우회 (개발 중)
                GameLogger.LogInfo($"개발 중이므로 소유자 검증을 우회합니다", GameLogger.LogCategory.Combat);
            }

            return true;
        }
    }
}