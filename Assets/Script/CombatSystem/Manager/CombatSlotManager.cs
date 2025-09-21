using UnityEngine;
using System.Linq;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 슬롯 배치 패턴 열거형
    /// </summary>
    public enum SlotPattern
    {
        [Tooltip("교대로 배치 (플레이어-적-플레이어-적-플레이어)")]
        ALTERNATING,
        
        [Tooltip("플레이어 우선 배치 (플레이어-플레이어-적-적-플레이어)")]
        PLAYER_FIRST,
        
        [Tooltip("적 우선 배치 (적-적-플레이어-플레이어-적)")]
        ENEMY_FIRST,
        
        [Tooltip("랜덤 배치")]
        RANDOM
    }

    /// <summary>
    /// 싱글게임용 전투 슬롯 관리자 (Zenject DI)
    /// 전투 슬롯의 생성, 배치, 검증을 담당합니다.
    /// </summary>
    public class CombatSlotManager : MonoBehaviour, ICombatSlotManager
    {
        #region 초기화 (Zenject DI)

        private void Awake()
        {
            InitializeSlots();
        }

        #endregion

        #region 슬롯 관리

        [System.Serializable]
        public class SlotConfiguration
        {
            [Header("슬롯 구성")]
            [Tooltip("전투 슬롯의 총 개수 (5개 고정)")]
            [Range(5, 5)]
            public int slotCount = 5;
            
            [Space(5)]
            [Header("슬롯 패턴")]
            [Tooltip("슬롯 배치 패턴을 설정합니다")]
            public SlotPattern slotPattern = SlotPattern.ALTERNATING;
        }

        [System.Serializable]
        public class VisualSettings
        {
            [Header("색상 설정")]
            [Tooltip("플레이어 슬롯의 색상")]
            public Color playerSlotColor = new Color(0.2f, 0.6f, 1f, 0.8f);
            
            [Tooltip("적 슬롯의 색상")]
            public Color enemySlotColor = new Color(1f, 0.3f, 0.3f, 0.8f);
            
            [Tooltip("빈 슬롯의 색상")]
            public Color emptySlotColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            
            [Space(5)]
            [Header("애니메이션")]
            [Tooltip("슬롯 전환 애니메이션 시간")]
            [Range(0.1f, 2f)]
            public float transitionDuration = 0.5f;
            
            [Tooltip("슬롯 하이라이트 강도")]
            [Range(0f, 1f)]
            public float highlightIntensity = 0.3f;
        }

        [System.Serializable]
        public class DebugSettings
        {
            [Header("디버그 옵션")]
            [Tooltip("슬롯 상태를 시각적으로 표시")]
            public bool showSlotStates = false;
            
            [Tooltip("슬롯 경계를 표시")]
            public bool showSlotBounds = false;
            
            [Tooltip("디버그 로그 출력")]
            public bool enableDebugLogs = true;
        }

        [Header("⚔️ 전투 슬롯 설정")]
        [SerializeField] private SlotConfiguration slotConfig = new SlotConfiguration();
        
        [Space(10)]
        [Header("🎨 시각적 설정")]
        [SerializeField] private VisualSettings visualSettings = new VisualSettings();
        
        [Space(10)]
        [Header("🔧 디버그 설정")]
        [SerializeField] private DebugSettings debugSettings = new DebugSettings();

        [Space(10)]
        [Header("📋 슬롯 목록")]
        [SerializeField] private CombatSlot[] slots = new CombatSlot[5];

        /// <summary>
        /// 슬롯을 초기화합니다.
        /// </summary>
        public void InitializeSlots()
        {
            // 배열이 null이거나 크기가 맞지 않으면 재생성
            if (slots == null || slots.Length != slotConfig.slotCount)
            {
                slots = new CombatSlot[slotConfig.slotCount];
            }

            // 설정된 패턴에 따라 슬롯 초기화
            InitializeSlotsByPattern();

            if (debugSettings.enableDebugLogs)
            {
                GameLogger.LogInfo($"전투 슬롯 초기화 완료 (패턴: {slotConfig.slotPattern})", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 설정된 패턴에 따라 슬롯을 초기화합니다.
        /// </summary>
        private void InitializeSlotsByPattern()
        {
            CombatSlotPosition[] positions = {
                CombatSlotPosition.BATTLE_SLOT,
                CombatSlotPosition.WAIT_SLOT_1,
                CombatSlotPosition.WAIT_SLOT_2,
                CombatSlotPosition.WAIT_SLOT_3,
                CombatSlotPosition.WAIT_SLOT_4
            };

            SlotOwner[] owners = GetSlotOwnersByPattern();

            for (int i = 0; i < slots.Length && i < positions.Length; i++)
            {
                slots[i] = new CombatSlot(positions[i], owners[i]);
            }
        }

        /// <summary>
        /// 패턴에 따라 슬롯 소유자를 결정합니다.
        /// </summary>
        private SlotOwner[] GetSlotOwnersByPattern()
        {
            return slotConfig.slotPattern switch
            {
                SlotPattern.ALTERNATING => new SlotOwner[] { SlotOwner.PLAYER, SlotOwner.ENEMY, SlotOwner.PLAYER, SlotOwner.ENEMY, SlotOwner.PLAYER },
                SlotPattern.PLAYER_FIRST => new SlotOwner[] { SlotOwner.PLAYER, SlotOwner.PLAYER, SlotOwner.ENEMY, SlotOwner.ENEMY, SlotOwner.PLAYER },
                SlotPattern.ENEMY_FIRST => new SlotOwner[] { SlotOwner.ENEMY, SlotOwner.ENEMY, SlotOwner.PLAYER, SlotOwner.PLAYER, SlotOwner.ENEMY },
                SlotPattern.RANDOM => GenerateRandomPattern(),
                _ => new SlotOwner[] { SlotOwner.PLAYER, SlotOwner.ENEMY, SlotOwner.PLAYER, SlotOwner.ENEMY, SlotOwner.PLAYER }
            };
        }

        /// <summary>
        /// 랜덤 패턴을 생성합니다.
        /// </summary>
        private SlotOwner[] GenerateRandomPattern()
        {
            SlotOwner[] owners = new SlotOwner[5];
            for (int i = 0; i < owners.Length; i++)
            {
                owners[i] = Random.Range(0, 2) == 0 ? SlotOwner.PLAYER : SlotOwner.ENEMY;
            }
            return owners;
        }

        #endregion

        #region 시각적 피드백

        /// <summary>
        /// 슬롯 소유자에 따른 색상을 반환합니다.
        /// </summary>
        /// <param name="owner">슬롯 소유자</param>
        /// <returns>해당하는 색상</returns>
        public Color GetSlotColor(SlotOwner owner)
        {
            return owner switch
            {
                SlotOwner.PLAYER => visualSettings.playerSlotColor,
                SlotOwner.ENEMY => visualSettings.enemySlotColor,
                _ => visualSettings.emptySlotColor
            };
        }

        /// <summary>
        /// 플레이어 슬롯 색상을 반환합니다.
        /// </summary>
        public Color GetPlayerSlotColor() => visualSettings.playerSlotColor;

        /// <summary>
        /// 적 슬롯 색상을 반환합니다.
        /// </summary>
        public Color GetEnemySlotColor() => visualSettings.enemySlotColor;

        /// <summary>
        /// 빈 슬롯 색상을 반환합니다.
        /// </summary>
        public Color GetEmptySlotColor() => visualSettings.emptySlotColor;

        /// <summary>
        /// 슬롯 전환 애니메이션 시간을 반환합니다.
        /// </summary>
        public float GetTransitionDuration() => visualSettings.transitionDuration;

        /// <summary>
        /// 슬롯 하이라이트 강도를 반환합니다.
        /// </summary>
        public float GetHighlightIntensity() => visualSettings.highlightIntensity;

        #endregion

        #region 디버그 기능

        /// <summary>
        /// 디버그 설정을 확인합니다.
        /// </summary>
        public bool IsDebugEnabled() => debugSettings.enableDebugLogs;

        /// <summary>
        /// 슬롯 상태 표시 여부를 확인합니다.
        /// </summary>
        public bool ShouldShowSlotStates() => debugSettings.showSlotStates;

        /// <summary>
        /// 슬롯 경계 표시 여부를 확인합니다.
        /// </summary>
        public bool ShouldShowSlotBounds() => debugSettings.showSlotBounds;

        /// <summary>
        /// 지정된 위치의 슬롯을 반환합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>슬롯 인스턴스, 없으면 null</returns>
        public ICombatCardSlot GetSlot(CombatSlotPosition position)
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
            bool wasEmpty = slot.IsEmpty();
            string existingCard = slot.GetCard()?.GetCardName() ?? "없음";
            GameLogger.LogInfo($"슬롯 {position} 배치 전 상태: 비어있음={wasEmpty}, 기존카드={existingCard}", GameLogger.LogCategory.Combat);

            // 카드 배치
            slot.SetCard(card);
            bool success = true;
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

            var card = slot.GetCard();
            if (card != null)
            {
                slot.ClearAll();
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
        /// 슬롯 이동 (새로운 5슬롯 시스템)
        /// </summary>
        public void MoveSlotsForwardNew()
        {
            // 5슬롯 시스템: 1→2→3→4→5→제거
            for (int i = 0; i < slots.Length - 1; i++)
            {
                if (slots[i].HasCard() && !slots[i + 1].HasCard())
                {
                    var card = slots[i].RemoveCard();
                    slots[i + 1].SetCard(card);
                }
            }
            
            // 마지막 슬롯(5번)의 카드는 제거
            if (slots[4].HasCard())
            {
                slots[4].RemoveCard();
            }
            
            GameLogger.LogInfo("5슬롯 시스템으로 슬롯 이동 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 슬롯 이동 (레거시 4슬롯 시스템)
        /// </summary>
        public void MoveSlotsForward()
        {
            // 4슬롯 시스템: 1→2→3→4→제거
            for (int i = 0; i < 3; i++)
            {
                if (slots[i].HasCard() && !slots[i + 1].HasCard())
                {
                    var card = slots[i].RemoveCard();
                    slots[i + 1].SetCard(card);
                }
            }
            
            // 마지막 슬롯(4번)의 카드는 제거
            if (slots[3].HasCard())
            {
                slots[3].RemoveCard();
            }
            
            GameLogger.LogInfo("4슬롯 시스템으로 슬롯 이동 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 슬롯이 비어있는지 확인합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>비어있으면 true</returns>
        public bool IsSlotEmpty(CombatSlotPosition position)
        {
            var slot = GetSlot(position);
            return slot?.IsEmpty() ?? true;
        }

        /// <summary>
        /// 슬롯에 있는 카드를 반환합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>카드 인스턴스, 없으면 null</returns>
        public ISkillCard GetCardInSlot(CombatSlotPosition position)
        {
            var slot = GetSlot(position);
            return slot?.GetCard();
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
    public class CombatSlot : ICombatCardSlot
    {
        public CombatSlotPosition Position { get; }
        public SlotOwner Owner { get; }
        public ISkillCard OccupiedCard { get; private set; }
        
        private ISkillCardUI cardUI;

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
            if (OccupiedCard != null)
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

        #region ICombatCardSlot 구현

        /// <summary>
        /// 슬롯의 전체 필드 포지션 정보를 반환합니다.
        /// </summary>
        public CombatFieldSlotPosition GetCombatPosition()
        {
            // 기본 구현: Position을 CombatFieldSlotPosition으로 변환
            return Position switch
            {
                CombatSlotPosition.BATTLE_SLOT => CombatFieldSlotPosition.FIELD_LEFT,
                CombatSlotPosition.WAIT_SLOT_1 => CombatFieldSlotPosition.FIELD_RIGHT,
                CombatSlotPosition.WAIT_SLOT_2 => CombatFieldSlotPosition.FIELD_LEFT,
                CombatSlotPosition.WAIT_SLOT_3 => CombatFieldSlotPosition.FIELD_RIGHT,
                CombatSlotPosition.WAIT_SLOT_4 => CombatFieldSlotPosition.FIELD_LEFT,
                _ => CombatFieldSlotPosition.NONE
            };
        }

        /// <summary>
        /// 슬롯에 현재 등록된 스킬 카드 데이터를 반환합니다.
        /// </summary>
        public ISkillCard GetCard()
        {
            return OccupiedCard;
        }

        /// <summary>
        /// 슬롯에 스킬 카드 데이터를 등록합니다.
        /// </summary>
        public void SetCard(ISkillCard card)
        {
            OccupiedCard = card;
        }

        /// <summary>
        /// 슬롯에 등록된 카드 UI 객체를 반환합니다.
        /// </summary>
        public ISkillCardUI GetCardUI()
        {
            return cardUI;
        }

        /// <summary>
        /// 카드 UI를 슬롯에 등록합니다.
        /// </summary>
        public void SetCardUI(ISkillCardUI cardUI)
        {
            this.cardUI = cardUI;
        }

        /// <summary>
        /// 카드 데이터와 카드 UI 모두를 제거합니다.
        /// </summary>
        public void ClearAll()
        {
            OccupiedCard = null;
            cardUI = null;
        }

        /// <summary>
        /// 카드 UI만 제거합니다. 카드 데이터는 유지됩니다.
        /// </summary>
        public void ClearCardUI()
        {
            cardUI = null;
        }

        /// <summary>
        /// 슬롯에 카드 데이터가 존재하는지 여부를 반환합니다.
        /// </summary>
        public bool HasCard()
        {
            return OccupiedCard != null;
        }

        /// <summary>
        /// 슬롯이 완전히 비어 있는지 확인합니다 (카드 + UI 모두 없음).
        /// </summary>
        public bool IsEmpty()
        {
            return OccupiedCard == null && cardUI == null;
        }

        /// <summary>
        /// 슬롯에 등록된 카드의 효과를 자동 실행합니다.
        /// </summary>
        public void ExecuteCardAutomatically()
        {
            if (OccupiedCard != null)
            {
                // 기본 컨텍스트로 카드 실행
                OccupiedCard.ExecuteSkill();
            }
        }

        /// <summary>
        /// 주어진 컨텍스트를 사용하여 카드 효과를 실행합니다.
        /// </summary>
        public void ExecuteCardAutomatically(ICardExecutionContext ctx)
        {
            if (OccupiedCard != null)
            {
                OccupiedCard.ExecuteCardAutomatically(ctx);
            }
        }

        /// <summary>
        /// 카드 UI가 배치될 슬롯의 트랜스폼을 반환합니다.
        /// </summary>
        public Transform GetTransform()
        {
            // 기본 구현: null 반환 (실제 구현에서는 슬롯의 Transform을 반환해야 함)
            return null;
        }

        #endregion
    }
}