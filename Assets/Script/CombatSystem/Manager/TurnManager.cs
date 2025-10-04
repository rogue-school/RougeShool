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
                if (_turnController != null)
                {
                    _turnController.OnTurnChanged += (newTurn) => value?.Invoke(ConvertToLegacyTurnType(newTurn));
                }
            }
            remove { /* 구현 생략 */ }
        }

        public event Action<int> OnTurnCountChanged
        {
            add
            {
                if (_turnController != null)
                {
                    _turnController.OnTurnCountChanged += value;
                }
            }
            remove { /* 구현 생략 */ }
        }

        public event Action OnGameStarted
        {
            add
            {
                if (_turnController != null)
                {
                    _turnController.OnGameStarted += value;
                }
            }
            remove { /* 구현 생략 */ }
        }

        public event Action OnGameEnded
        {
            add
            {
                if (_turnController != null)
                {
                    _turnController.OnGameEnded += value;
                }
            }
            remove { /* 구현 생략 */ }
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

        public void RestoreTurnState(int turnCount, TurnType turnType)
        {
            if (_turnController is TurnController controller)
            {
                controller.RestoreTurnState(turnCount, ConvertToNewTurnType(turnType));
            }
        }

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
                if (_slotRegistry != null)
                {
                    _slotRegistry.OnCardStateChanged += value;
                }
            }
            remove { /* 구현 생략 */ }
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
