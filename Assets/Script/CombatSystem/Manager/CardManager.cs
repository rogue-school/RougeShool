using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Manager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Manager;
using Game.CombatSystem.DragDrop;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 통합 카드 매니저 - 플레이어와 적의 카드를 통합 관리
    /// 플레이어는 드래그/드롭 기반, 적은 확률 기반으로 다른 로직을 사용하지만
    /// 공통 인터페이스를 통해 일관성 있는 카드 관리 제공
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        #region 싱글톤 패턴
        
        public static CardManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        #endregion
        
        #region 카드 관리
        
        // 플레이어 핸드 매니저 (기존 PlayerHandManager 통합)
        private PlayerHandManager playerHandManager;
        
        // 카드 팩토리
        private SkillCardFactory cardFactory;
        
        // 카드 UI 프리팹
        private SkillCardUI cardUIPrefab;
        
        #endregion
        
        #region 초기화
        
        /// <summary>
        /// 카드 매니저 초기화
        /// </summary>
        private void Initialize()
        {
            GameLogger.LogInfo("CardManager 초기화 시작", GameLogger.LogCategory.Combat);
            
            // 기존 매니저들 초기화
            InitializePlayerHandManager();
            InitializeCardFactory();
            InitializeCardUIPrefab();
            
            GameLogger.LogInfo("CardManager 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 플레이어 핸드 매니저 초기화
        /// </summary>
        private void InitializePlayerHandManager()
        {
            playerHandManager = FindFirstObjectByType<PlayerHandManager>();
            if (playerHandManager == null)
            {
                var playerHandManagerObj = new GameObject("PlayerHandManager");
                playerHandManager = playerHandManagerObj.AddComponent<PlayerHandManager>();
            }
            GameLogger.LogInfo("플레이어 핸드 매니저 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 카드 팩토리 초기화
        /// </summary>
        private void InitializeCardFactory()
        {
            cardFactory = new SkillCardFactory();
            GameLogger.LogInfo("카드 팩토리 초기화 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 카드 UI 프리팹 초기화
        /// </summary>
        private void InitializeCardUIPrefab()
        {
            cardUIPrefab = Resources.Load<SkillCardUI>("Prefabs/SkillCardUI");
            if (cardUIPrefab == null)
            {
                GameLogger.LogWarning("SkillCardUI 프리팹을 찾을 수 없습니다", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogInfo("카드 UI 프리팹 초기화 완료", GameLogger.LogCategory.Combat);
            }
        }
        
        #endregion
        
        #region 플레이어 카드 관리
        
        /// <summary>
        /// 플레이어 카드 생성
        /// </summary>
        public void GeneratePlayerCards()
        {
            GameLogger.LogInfo("플레이어 카드 생성 시작", GameLogger.LogCategory.Combat);
            
            if (playerHandManager != null)
            {
                // 플레이어 핸드 매니저를 통해 카드 생성
                var handCards = playerHandManager.GetAllHandCards();
                GameLogger.LogInfo($"플레이어 핸드 카드 생성 완료: {handCards.Count()}장", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogWarning("PlayerHandManager가 null입니다", GameLogger.LogCategory.Combat);
            }
        }
        
        /// <summary>
        /// 플레이어 카드 드래그 앤 드롭 활성화
        /// </summary>
        public void EnablePlayerCardDragDrop()
        {
            GameLogger.LogInfo("플레이어 카드 드래그 앤 드롭 활성화", GameLogger.LogCategory.Combat);
            
            // 드래그 핸들러 활성화
            var dragHandlers = FindObjectsByType<CardDragHandler>(FindObjectsSortMode.None);
            foreach (var handler in dragHandlers)
            {
                handler.enabled = true;
            }
            
            // 드롭 핸들러 활성화
            var dropHandlers = FindObjectsByType<CardDropToSlotHandler>(FindObjectsSortMode.None);
            foreach (var handler in dropHandlers)
            {
                handler.enabled = true;
            }
            
            GameLogger.LogInfo($"드래그 핸들러 {dragHandlers.Length}개, 드롭 핸들러 {dropHandlers.Length}개 활성화", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 플레이어 카드 드래그 앤 드롭 비활성화
        /// </summary>
        public void DisablePlayerCardDragDrop()
        {
            GameLogger.LogInfo("플레이어 카드 드래그 앤 드롭 비활성화", GameLogger.LogCategory.Combat);
            
            // 드래그 핸들러 비활성화
            var dragHandlers = FindObjectsByType<CardDragHandler>(FindObjectsSortMode.None);
            foreach (var handler in dragHandlers)
            {
                handler.enabled = false;
            }
            
            // 드롭 핸들러 비활성화
            var dropHandlers = FindObjectsByType<CardDropToSlotHandler>(FindObjectsSortMode.None);
            foreach (var handler in dropHandlers)
            {
                handler.enabled = false;
            }
            
            GameLogger.LogInfo($"드래그 핸들러 {dragHandlers.Length}개, 드롭 핸들러 {dropHandlers.Length}개 비활성화", GameLogger.LogCategory.Combat);
        }
        
        #endregion
        
        #region 적 카드 관리
        
        /// <summary>
        /// 적 카드 생성 (확률 기반)
        /// </summary>
        public void GenerateEnemyCards()
        {
            GameLogger.LogInfo("적 카드 생성 시작", GameLogger.LogCategory.Combat);
            
            // 적 카드는 대기 슬롯에 직접 배치
            PlaceEnemyCardsInWaitSlots();
            
            GameLogger.LogInfo("적 카드 생성 완료", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 적 카드를 대기 슬롯에 배치
        /// </summary>
        private void PlaceEnemyCardsInWaitSlots()
        {
            var slotManager = CombatSlotManager.Instance;
            if (slotManager == null)
            {
                GameLogger.LogError("CombatSlotManager가 null입니다", GameLogger.LogCategory.Error);
                return;
            }
            
            // 적 카드 배치 위치들 (대기 슬롯)
            var enemyPositions = new[]
            {
                CombatSlotPosition.WAIT_SLOT_1,
                CombatSlotPosition.WAIT_SLOT_3
            };
            
            foreach (var position in enemyPositions)
            {
                var slot = slotManager.GetSlot(position);
                if (slot != null && slot.IsEmpty)
                {
                    // 기본 적 카드 생성
                    var enemyCard = CreateDefaultEnemyCard();
                    if (enemyCard != null)
                    {
                        // 카드 UI 생성
                        var cardUI = CreateEnemyCardUI(enemyCard, slot);
                        if (cardUI != null)
                        {
                            // 슬롯에 카드 배치
                            if (slot.TryPlaceCard(enemyCard))
                            {
                                GameLogger.LogInfo($"적 카드 배치 성공: {enemyCard.GetCardName()} → {position}", GameLogger.LogCategory.Combat);
                            }
                            else
                            {
                                GameLogger.LogWarning($"적 카드 배치 실패: {enemyCard.GetCardName()} → {position}", GameLogger.LogCategory.Combat);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 기본 적 카드 생성
        /// </summary>
        private ISkillCard CreateDefaultEnemyCard()
        {
            try
            {
                // 기본 카드 정의 로드
                var cardDefinition = Resources.Load<SkillCardDefinition>("SkillCards/DefaultEnemyCard");
                if (cardDefinition != null)
                {
                    var enemyCard = cardFactory.CreateFromDefinition(cardDefinition, Owner.Enemy, "적");
                    GameLogger.LogInfo($"기본 적 카드 생성: {enemyCard.GetCardName()}", GameLogger.LogCategory.Combat);
                    return enemyCard;
                }
                else
                {
                    GameLogger.LogWarning("기본 적 카드 정의를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"적 카드 생성 실패: {ex.Message}", GameLogger.LogCategory.Error);
                return null;
            }
        }
        
        /// <summary>
        /// 적 카드 UI 생성
        /// </summary>
        private SkillCardUI CreateEnemyCardUI(ISkillCard card, CombatSlot slot)
        {
            try
            {
                if (cardUIPrefab != null)
                {
                    // 슬롯 GameObject 찾기
                    var slotGameObject = FindSlotGameObject(slot.Position);
                    if (slotGameObject != null)
                    {
                        var cardUI = Instantiate(cardUIPrefab, slotGameObject.transform);
                        cardUI.SetCard(card);
                        GameLogger.LogInfo($"적 카드 UI 생성: {card.GetCardName()}", GameLogger.LogCategory.Combat);
                        return cardUI;
                    }
                    else
                    {
                        GameLogger.LogWarning($"슬롯 GameObject를 찾을 수 없습니다: {slot.Position}", GameLogger.LogCategory.Combat);
                        return null;
                    }
                }
                else
                {
                    GameLogger.LogWarning("카드 UI 프리팹이 null입니다", GameLogger.LogCategory.Combat);
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"적 카드 UI 생성 실패: {ex.Message}", GameLogger.LogCategory.Error);
                return null;
            }
        }
        
        /// <summary>
        /// 슬롯 GameObject 찾기
        /// </summary>
        private GameObject FindSlotGameObject(CombatSlotPosition position)
        {
            // 다양한 네이밍 컨벤션으로 슬롯 GameObject 찾기
            var possibleNames = new[]
            {
                $"Slot_{position}",
                $"CombatSlot_{position}",
                $"BattleSlot_{position}",
                position.ToString()
            };
            
            foreach (var name in possibleNames)
            {
                var slotObj = GameObject.Find(name);
                if (slotObj != null)
                {
                    return slotObj;
                }
            }
            
            // 태그로 찾기 시도
            try
            {
                var slotObjects = GameObject.FindGameObjectsWithTag("CombatSlot");
                foreach (var obj in slotObjects)
                {
                    if (obj.name.Contains(position.ToString()))
                    {
                        return obj;
                    }
                }
            }
            catch (UnityException)
            {
                GameLogger.LogWarning("CombatSlot 태그가 정의되지 않았습니다", GameLogger.LogCategory.Combat);
            }
            
            GameLogger.LogWarning($"슬롯 GameObject를 찾을 수 없습니다: {position}", GameLogger.LogCategory.Combat);
            return null;
        }
        
        #endregion
        
        #region 공통 카드 관리
        
        /// <summary>
        /// 카드를 슬롯에 배치
        /// </summary>
        public bool PlaceCardInSlot(ISkillCard card, CombatSlotPosition position)
        {
            var slotManager = CombatSlotManager.Instance;
            if (slotManager == null)
            {
                GameLogger.LogError("CombatSlotManager가 null입니다", GameLogger.LogCategory.Error);
                return false;
            }
            
            var slot = slotManager.GetSlot(position);
            if (slot == null)
            {
                GameLogger.LogError($"슬롯을 찾을 수 없습니다: {position}", GameLogger.LogCategory.Error);
                return false;
            }
            
            if (slot.TryPlaceCard(card))
            {
                GameLogger.LogInfo($"카드 배치 성공: {card.GetCardName()} → {position}", GameLogger.LogCategory.Combat);
                return true;
            }
            else
            {
                GameLogger.LogWarning($"카드 배치 실패: {card.GetCardName()} → {position}", GameLogger.LogCategory.Combat);
                return false;
            }
        }
        
        /// <summary>
        /// 카드 배치 가능 여부 확인
        /// </summary>
        public bool CanPlaceCard(ISkillCard card, CombatSlotPosition position)
        {
            var slotManager = CombatSlotManager.Instance;
            if (slotManager == null)
            {
                return false;
            }
            
            var slot = slotManager.GetSlot(position);
            if (slot == null)
            {
                return false;
            }
            
            return slot.IsEmpty && slot.Owner == card.GetOwner();
        }
        
        /// <summary>
        /// 카드 매니저 초기화
        /// </summary>
        public void ClearCards()
        {
            GameLogger.LogInfo("카드 매니저 초기화", GameLogger.LogCategory.Combat);
            
            // 플레이어 핸드 초기화
            if (playerHandManager != null)
            {
                // PlayerHandManager의 초기화 메서드가 있다면 호출
                GameLogger.LogInfo("플레이어 핸드 초기화", GameLogger.LogCategory.Combat);
            }
            
            // 슬롯 초기화
            var slotManager = CombatSlotManager.Instance;
            if (slotManager != null)
            {
                slotManager.ClearAllSlots();
            }
        }
        
        #endregion
        
        #region 매니저 접근자
        
        /// <summary>
        /// 플레이어 핸드 매니저 접근
        /// </summary>
        public PlayerHandManager GetPlayerHandManager() => playerHandManager;
        
        /// <summary>
        /// 카드 팩토리 접근
        /// </summary>
        public SkillCardFactory GetCardFactory() => cardFactory;
        
        #endregion
    }
}
