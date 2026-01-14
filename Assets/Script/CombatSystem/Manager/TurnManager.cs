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
        }

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            // Zenject Inject가 호출될 때까지 대기
        }

        private void OnDestroy()
        {
            // 모든 이벤트 핸들러 정리 (메모리 누수 방지)
            ClearAllEventHandlers();
        }

        #endregion

        #region ICombatTurnManager 구현 - 턴 관리 (Delegate to TurnController)

        /// <summary>
        /// 현재 턴 타입
        /// </summary>
        public TurnType CurrentTurn => ConvertToLegacyTurnType(_turnController?.CurrentTurn ?? Interface.TurnType.Player);
        
        /// <summary>
        /// 현재 턴 카운트
        /// </summary>
        public int TurnCount => _turnController?.TurnCount ?? 1;
        
        /// <summary>
        /// 게임 활성 상태 여부
        /// </summary>
        public bool IsGameActive => _turnController?.IsGameActive ?? false;
        
        /// <summary>
        /// 남은 턴 시간 (현재 미사용)
        /// </summary>
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

        /// <summary>
        /// 현재 턴 타입을 반환합니다
        /// </summary>
        /// <returns>현재 턴 타입</returns>
        public TurnType GetCurrentTurnType() => CurrentTurn;
        
        /// <summary>
        /// 현재 턴 카운트를 반환합니다
        /// </summary>
        /// <returns>현재 턴 카운트</returns>
        public int GetTurnCount() => TurnCount;
        
        /// <summary>
        /// 플레이어 턴인지 확인합니다
        /// </summary>
        /// <returns>플레이어 턴이면 true</returns>
        public bool IsPlayerTurn() => _turnController?.IsPlayerTurn() ?? true;
        
        /// <summary>
        /// 적 턴인지 확인합니다
        /// </summary>
        /// <returns>적 턴이면 true</returns>
        public bool IsEnemyTurn() => _turnController?.IsEnemyTurn() ?? false;

        /// <summary>
        /// 턴을 설정하고 카운트를 증가시킵니다
        /// </summary>
        /// <param name="turnType">설정할 턴 타입</param>
        public void SetTurnAndIncrement(TurnType turnType)
        {
            _turnController?.SetTurnAndIncrement(ConvertToNewTurnType(turnType));
        }

        /// <summary>
        /// 턴을 설정합니다
        /// </summary>
        /// <param name="turnType">설정할 턴 타입</param>
        public void SetTurn(TurnType turnType)
        {
            _turnController?.SetTurn(ConvertToNewTurnType(turnType));
        }

        /// <summary>
        /// 턴을 초기화합니다
        /// </summary>
        public void ResetTurn()
        {
            _turnController?.ResetTurn();
        }

        /// <summary>
        /// 게임을 시작합니다
        /// </summary>
        public void StartGame()
        {
            _turnController?.StartGame();
        }

        /// <summary>
        /// 게임을 종료합니다
        /// </summary>
        public void EndGame()
        {
            _turnController?.EndGame();
        }

        /// <summary>
        /// 모든 캐릭터의 턴 효과를 처리합니다
        /// </summary>
        public void ProcessAllCharacterTurnEffects()
        {
            _turnController?.ProcessAllCharacterTurnEffects();
        }

        /// <summary>
        /// 턴 상태를 복원합니다 (세이브/로드용)
        /// </summary>
        /// <param name="turnCount">복원할 턴 카운트</param>
        /// <param name="turnType">복원할 턴 타입</param>
        public void RestoreTurnState(int turnCount, TurnType turnType)
        {
            if (_turnController is TurnController controller)
            {
                controller.RestoreTurnState(turnCount, ConvertToNewTurnType(turnType));
            }
        }

        /// <summary>
        /// 턴 카운트를 설정합니다
        /// </summary>
        /// <param name="count">설정할 턴 카운트</param>
        public void SetTurnCount(int count)
        {
            if (_turnController is TurnController controller)
            {
                controller.SetTurnCount(count);
            }
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

        /// <summary>
        /// 카드를 슬롯에 등록합니다
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <param name="card">등록할 스킬 카드</param>
        /// <param name="ui">카드 UI</param>
        /// <param name="owner">카드 소유자</param>
        public void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            _slotRegistry?.RegisterCard(position, card, ui, owner);
        }

        /// <summary>
        /// 슬롯에 있는 카드를 반환합니다
        /// </summary>
        /// <param name="slot">슬롯 위치</param>
        /// <returns>해당 슬롯의 카드, 없으면 null</returns>
        public ISkillCard GetCardInSlot(CombatSlotPosition slot)
        {
            return _slotRegistry?.GetCardInSlot(slot);
        }

        /// <summary>
        /// 슬롯을 비웁니다
        /// </summary>
        /// <param name="slot">비울 슬롯 위치</param>
        public void ClearSlot(CombatSlotPosition slot)
        {
            _slotRegistry?.ClearSlot(slot);
        }

        /// <summary>
        /// 모든 슬롯을 비웁니다
        /// </summary>
        public void ClearAllSlots()
        {
            _slotRegistry?.ClearAllSlots();
        }

        /// <summary>
        /// 적 카드만 비웁니다
        /// </summary>
        public void ClearEnemyCardsOnly()
        {
            _slotRegistry?.ClearEnemyCardsOnly();
        }

        /// <summary>
        /// 대기 슬롯을 비웁니다
        /// </summary>
        public void ClearWaitSlots()
        {
            _slotRegistry?.ClearWaitSlots();
        }

        /// <summary>
        /// 플레이어 카드가 있는지 확인합니다
        /// </summary>
        /// <returns>플레이어 카드가 있으면 true</returns>
        public bool HasPlayerCard()
        {
            return _slotRegistry?.HasPlayerCard() ?? false;
        }

        /// <summary>
        /// 적 카드가 있는지 확인합니다
        /// </summary>
        /// <returns>적 카드가 있으면 true</returns>
        public bool HasEnemyCard()
        {
            return _slotRegistry?.HasEnemyCard() ?? false;
        }

        /// <summary>
        /// 예약된 적 슬롯을 반환합니다
        /// </summary>
        /// <returns>예약된 적 슬롯 위치, 없으면 null</returns>
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

        public void SetCurrentTurn(int turn)
        {
            SetTurnCount(turn);
        }

        public int GetCurrentTurn()
        {
            return TurnCount;
        }

        public void ApplyGuardEffect()
        {
            GameLogger.LogInfo("가드 효과 적용 요청됨", GameLogger.LogCategory.Combat);
        }

        public void PauseTurn()
        {
            GameLogger.LogInfo("턴 일시정지", GameLogger.LogCategory.Combat);
        }

        public void ResumeTurn()
        {
            GameLogger.LogInfo("턴 재개", GameLogger.LogCategory.Combat);
        }

        public void ResetTurnTimer()
        {
            GameLogger.LogInfo("턴 시간 리셋", GameLogger.LogCategory.Combat);
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
