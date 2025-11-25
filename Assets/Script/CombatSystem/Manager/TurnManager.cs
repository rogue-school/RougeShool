using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Game.CombatSystem.Data;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.State;
using Game.CombatSystem.Core;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CharacterSystem.Data;
using Zenject;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 턴 관리자 (Adapter 패턴)
    /// 기존 인터페이스를 유지하면서 새로운 분리된 클래스들에게 작업을 위임합니다.
    /// 점진적 마이그레이션을 위한 호환성 계층입니다.
    /// </summary>
    public class TurnManager : MonoBehaviour, ICombatTurnManager
    {
        #region 의존성 주입 (새로운 분리된 클래스들)

        private ITurnController _turnController;
        private ICardSlotRegistry _slotRegistry;
        private ISlotMovementController _slotMovement;

        // 이벤트 핸들러 참조 저장 (구독 해제를 위함)
        private readonly System.Collections.Generic.Dictionary<Action<TurnType>, Action<Interface.TurnType>> _turnChangedHandlers
            = new System.Collections.Generic.Dictionary<Action<TurnType>, Action<Interface.TurnType>>();
        private readonly System.Collections.Generic.HashSet<Action<int>> _turnCountChangedHandlers
            = new System.Collections.Generic.HashSet<Action<int>>();
        private readonly System.Collections.Generic.HashSet<Action> _gameStartedHandlers
            = new System.Collections.Generic.HashSet<Action>();
        private readonly System.Collections.Generic.HashSet<Action> _gameEndedHandlers
            = new System.Collections.Generic.HashSet<Action>();
        private readonly System.Collections.Generic.HashSet<Action> _cardStateChangedHandlers
            = new System.Collections.Generic.HashSet<Action>();

        [Inject]
        public void Construct(
            ITurnController turnController,
            ICardSlotRegistry slotRegistry,
            ISlotMovementController slotMovement)
        {
            _turnController = turnController;
            _slotRegistry = slotRegistry;
            _slotMovement = slotMovement;

            GameLogger.LogInfo("TurnManager (Adapter) 의존성 주입 완료", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            // Zenject Inject가 호출될 때까지 대기
            GameLogger.LogInfo("TurnManager (Adapter) Awake 호출", GameLogger.LogCategory.Combat);
        }

        private void OnDestroy()
        {
            // 모든 이벤트 핸들러 정리 (메모리 누수 방지)
            ClearAllEventHandlers();
        }

        #endregion

        #region ICombatTurnManager 구현 - 턴 관리 (Delegate to TurnController)

        public TurnType CurrentTurn => ConvertToLegacyTurnType(_turnController?.CurrentTurn ?? Interface.TurnType.Player);
        public int TurnCount => _turnController?.TurnCount ?? 1;
        public bool IsGameActive => _turnController?.IsGameActive ?? false;
        public float RemainingTurnTime => 0f;

        public event Action<TurnType> OnTurnChanged
        {
            add
            {
                if (_turnController != null && value != null)
                {
                    // 래핑 핸들러 생성 및 저장
                    Action<Interface.TurnType> wrappedHandler = (newTurn) => value.Invoke(ConvertToLegacyTurnType(newTurn));
                    _turnChangedHandlers[value] = wrappedHandler;
                    _turnController.OnTurnChanged += wrappedHandler;
                }
            }
            remove
            {
                if (_turnController != null && value != null && _turnChangedHandlers.TryGetValue(value, out var wrappedHandler))
                {
                    _turnController.OnTurnChanged -= wrappedHandler;
                    _turnChangedHandlers.Remove(value);
                }
            }
        }

        public event Action<int> OnTurnCountChanged
        {
            add
            {
                if (_turnController != null && value != null)
                {
                    _turnCountChangedHandlers.Add(value);
                    _turnController.OnTurnCountChanged += value;
                }
            }
            remove
            {
                if (_turnController != null && value != null && _turnCountChangedHandlers.Contains(value))
                {
                    _turnController.OnTurnCountChanged -= value;
                    _turnCountChangedHandlers.Remove(value);
                }
            }
        }

        public event Action OnGameStarted
        {
            add
            {
                if (_turnController != null && value != null)
                {
                    _gameStartedHandlers.Add(value);
                    _turnController.OnGameStarted += value;
                }
            }
            remove
            {
                if (_turnController != null && value != null && _gameStartedHandlers.Contains(value))
                {
                    _turnController.OnGameStarted -= value;
                    _gameStartedHandlers.Remove(value);
                }
            }
        }

        public event Action OnGameEnded
        {
            add
            {
                if (_turnController != null && value != null)
                {
                    _gameEndedHandlers.Add(value);
                    _turnController.OnGameEnded += value;
                }
            }
            remove
            {
                if (_turnController != null && value != null && _gameEndedHandlers.Contains(value))
                {
                    _turnController.OnGameEnded -= value;
                    _gameEndedHandlers.Remove(value);
                }
            }
        }

        public TurnType GetCurrentTurnType() => CurrentTurn;
        public int GetTurnCount() => TurnCount;
        public bool IsPlayerTurn() => _turnController?.IsPlayerTurn() ?? true;
        public bool IsEnemyTurn() => _turnController?.IsEnemyTurn() ?? false;

        public void SetTurnAndIncrement(TurnType turnType)
        {
            _turnController?.SetTurnAndIncrement(ConvertToNewTurnType(turnType));
        }

        public void SetTurn(TurnType turnType)
        {
            _turnController?.SetTurn(ConvertToNewTurnType(turnType));
        }

        public void ResetTurn()
        {
            _turnController?.ResetTurn();
        }

        public void StartGame()
        {
            _turnController?.StartGame();
        }

        public void EndGame()
        {
            _turnController?.EndGame();
        }

        public void ProcessAllCharacterTurnEffects()
        {
            _turnController?.ProcessAllCharacterTurnEffects();
        }

        #endregion

        #region 카드 레지스트리 관리 (Delegate to CardSlotRegistry)

        public event Action OnCardStateChanged
        {
            add
            {
                if (_slotRegistry != null && value != null)
                {
                    _cardStateChangedHandlers.Add(value);
                    _slotRegistry.OnCardStateChanged += value;
                }
            }
            remove
            {
                if (_slotRegistry != null && value != null && _cardStateChangedHandlers.Contains(value))
                {
                    _slotRegistry.OnCardStateChanged -= value;
                    _cardStateChangedHandlers.Remove(value);
                }
            }
        }

        public void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            _slotRegistry?.RegisterCard(position, card, ui, owner);
        }

        public ISkillCard GetCardInSlot(CombatSlotPosition slot)
        {
            return _slotRegistry?.GetCardInSlot(slot);
        }

        public void ClearSlot(CombatSlotPosition slot)
        {
            _slotRegistry?.ClearSlot(slot);
        }

        public void ClearAllSlots()
        {
            _slotRegistry?.ClearAllSlots();
        }

        public void ClearEnemyCardsOnly()
        {
            _slotRegistry?.ClearEnemyCardsOnly();
        }

        public void ClearWaitSlots()
        {
            _slotRegistry?.ClearWaitSlots();
        }

        public bool HasPlayerCard()
        {
            return _slotRegistry?.HasPlayerCard() ?? false;
        }

        public bool HasEnemyCard()
        {
            return _slotRegistry?.HasEnemyCard() ?? false;
        }

        public CombatSlotPosition? GetReservedEnemySlot()
        {
            return _slotRegistry?.GetReservedEnemySlot();
        }

        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            _slotRegistry?.ReserveNextEnemySlot(slot);
        }

        public void ClearAllCards()
        {
            _slotRegistry?.ClearAllSlots();
        }

        #endregion

        #region 슬롯 이동 관리 (Delegate to SlotMovementController)

        public IEnumerator AdvanceQueueAtTurnStartRoutine()
        {
            if (_slotMovement != null)
            {
                yield return _slotMovement.AdvanceQueueAtTurnStartRoutine();
            }
        }

        public IEnumerator SetupInitialEnemyQueueRoutine(EnemyCharacterData enemyData, string enemyName)
        {
            if (_slotMovement != null)
            {
                yield return _slotMovement.SetupInitialEnemyQueueRoutine(enemyData, enemyName);
            }
        }

        public void RegisterEnemyCardInSlot4(ISkillCard card)
        {
            _slotMovement?.RegisterEnemyCardInSlot4(card);
        }

        public void ClearEnemyCache()
        {
            _slotMovement?.ClearEnemyCache();
        }

        public void ResetSlotStates()
        {
            _slotMovement?.ResetSlotStates();
        }

        #endregion

        #region ICombatTurnManager 구현 - 레거시 메서드

        public void Initialize()
        {
            // TurnController는 생성자에서 초기화되므로 별도 작업 불필요
            GameLogger.LogInfo("TurnManager (Adapter) 초기화 완료", GameLogger.LogCategory.Combat);
        }

        public void Reset()
        {
            ResetTurn();
        }

        public void RequestStateChange(object nextState)
        {
            GameLogger.LogInfo($"상태 전이 요청: {nextState}", GameLogger.LogCategory.Combat);
        }

        public void ChangeState(object newState)
        {
            GameLogger.LogInfo($"상태 변경: {newState}", GameLogger.LogCategory.Combat);
        }

        public object GetCurrentState()
        {
            return CurrentTurn;
        }

        public object GetStateFactory()
        {
            return null;
        }

        public bool IsPlayerInputTurn()
        {
            return IsPlayerTurn();
        }

        #endregion

        #region 타입 변환 헬퍼

        /// <summary>
        /// 새로운 TurnType을 레거시 TurnType으로 변환
        /// </summary>
        private TurnType ConvertToLegacyTurnType(Interface.TurnType newType)
        {
            return newType == Interface.TurnType.Player ? TurnType.Player : TurnType.Enemy;
        }

        /// <summary>
        /// 레거시 TurnType을 새로운 TurnType으로 변환
        /// </summary>
        private Interface.TurnType ConvertToNewTurnType(TurnType legacyType)
        {
            return legacyType == TurnType.Player ? Interface.TurnType.Player : Interface.TurnType.Enemy;
        }

        #endregion

        #region 레거시 타입 정의 (하위 호환성)

        /// <summary>
        /// 레거시 턴 타입 (하위 호환성을 위해 유지)
        /// </summary>
        public enum TurnType
        {
            Player,
            Enemy
        }

        #endregion

        #region 이벤트 정리

        /// <summary>
        /// 모든 이벤트 핸들러를 정리합니다 (메모리 누수 방지)
        /// </summary>
        private void ClearAllEventHandlers()
        {
            // OnTurnChanged 핸들러 정리
            if (_turnController != null)
            {
                foreach (var kvp in _turnChangedHandlers)
                {
                    _turnController.OnTurnChanged -= kvp.Value;
                }
            }
            _turnChangedHandlers.Clear();

            // OnTurnCountChanged 핸들러 정리
            if (_turnController != null)
            {
                foreach (var handler in _turnCountChangedHandlers)
                {
                    _turnController.OnTurnCountChanged -= handler;
                }
            }
            _turnCountChangedHandlers.Clear();

            // OnGameStarted 핸들러 정리
            if (_turnController != null)
            {
                foreach (var handler in _gameStartedHandlers)
                {
                    _turnController.OnGameStarted -= handler;
                }
            }
            _gameStartedHandlers.Clear();

            // OnGameEnded 핸들러 정리
            if (_turnController != null)
            {
                foreach (var handler in _gameEndedHandlers)
                {
                    _turnController.OnGameEnded -= handler;
                }
            }
            _gameEndedHandlers.Clear();

            // OnCardStateChanged 핸들러 정리
            if (_slotRegistry != null)
            {
                foreach (var handler in _cardStateChangedHandlers)
                {
                    _slotRegistry.OnCardStateChanged -= handler;
                }
            }
            _cardStateChangedHandlers.Clear();

            GameLogger.LogInfo("TurnManager 이벤트 핸들러 정리 완료", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 디버그

        [ContextMenu("턴 정보 출력")]
        public void LogTurnInfo()
        {
            if (_turnController is TurnController controller)
            {
                controller.LogTurnInfo();
            }
        }

        #endregion
    }
}
