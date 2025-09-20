#nullable enable
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CharacterSystem.Manager;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 싱글게임용 슬롯 실행 시스템
    /// 카드 실행, 슬롯 이동, 턴 전환을 관리합니다.
    /// </summary>
    public class SlotExecutionSystem : MonoBehaviour
    {
        #region 싱글톤

        public static SlotExecutionSystem? Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region 설정

        [Header("실행 설정")]
        [SerializeField] private float slotMoveDuration = 0.5f;
        [SerializeField] private float cardExecuteDelay = 0.2f;
        [SerializeField] private bool enableAutoExecution = true;

        [Header("의존성")]
        [SerializeField] private PlayerManager? playerManager;
        [SerializeField] private EnemyManager? enemyManager;

        #endregion

        #region 초기화

        private void Start()
        {
            // 의존성 자동 주입
            if (playerManager == null)
                playerManager = FindFirstObjectByType<PlayerManager>();
            if (enemyManager == null)
                enemyManager = FindFirstObjectByType<EnemyManager>();

            GameLogger.LogInfo("슬롯 실행 시스템 초기화 완료", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 전투 시퀀스 실행

        /// <summary>
        /// 전투 시퀀스를 실행합니다.
        /// </summary>
        /// <returns>실행 완료 Task</returns>
        public async Task ExecuteCombatSequence()
        {
            GameLogger.LogInfo("전투 시퀀스 실행 시작", GameLogger.LogCategory.Combat);

            // 1. 전투 슬롯의 카드 실행
            var battleSlot = CombatSlotManager.Instance.GetSlot(CombatSlotPosition.BATTLE_SLOT);
            if (battleSlot?.OccupiedCard != null)
            {
                await ExecuteCard(battleSlot.OccupiedCard);
            }

            // 2. 모든 슬롯을 1칸씩 앞으로 이동
            await MoveAllSlotsForward();

            // 3. 턴 전환
            TurnManager.Instance.SwitchTurn();

            // 4. 다음 카드가 전투 슬롯에 도착했는지 확인
            var nextCard = battleSlot?.OccupiedCard;
            if (nextCard != null && enableAutoExecution)
            {
                // 적의 카드면 자동으로 다음 실행
                if (nextCard.GetOwner() == SlotOwner.ENEMY)
                {
                    GameLogger.LogInfo("적 카드 자동 실행", GameLogger.LogCategory.Combat);
                    await ExecuteCombatSequence();
                }
            }

            GameLogger.LogInfo("전투 시퀀스 실행 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 카드를 실행합니다.
        /// </summary>
        /// <param name="card">실행할 카드</param>
        /// <returns>실행 완료 Task</returns>
        private async Task ExecuteCard(ISkillCard card)
        {
            GameLogger.LogInfo($"카드 실행: {card.GetCardName()}", GameLogger.LogCategory.Combat);

            try
            {
                // 카드 실행 로직
                var source = GetSourceCharacter(card);
                var target = GetTargetCharacter(card);

                if (source != null && target != null)
                {
                    // 애니메이션 건너뛰기 (AnimationSystem 제거로 인해 임시 비활성화)
                    Debug.Log($"[SlotExecutionSystem] 카드 애니메이션을 건너뜁니다: {card.GetCardName()}");

                    // 카드 효과 실행
                    card.ExecuteSkill(source, target);
                }
                else
                {
                    GameLogger.LogWarning($"카드 실행 실패: 소스 또는 타겟이 null - {card.GetCardName()}", GameLogger.LogCategory.Combat);
                }

                await Task.Delay((int)(cardExecuteDelay * 1000));
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"카드 실행 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 카드 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="card">카드</param>
        /// <param name="source">소스 캐릭터</param>
        /// <param name="target">타겟 캐릭터</param>
        /// <returns>애니메이션 완료 Task</returns>
        private async Task ExecuteCardAnimation(ISkillCard card, ICharacter source, ICharacter target)
        {
            try
            {
                // 카드 사용 애니메이션
                var cardUI = FindCardUI(card);
                if (cardUI != null)
                {
                    // 카드 UI 애니메이션 (DOTween 사용)
                    await AnimateCardUse(cardUI);
                }

                // 캐릭터 애니메이션 건너뛰기 (AnimationSystem 제거로 인해 임시 비활성화)
                Debug.Log($"[SlotExecutionSystem] 캐릭터 애니메이션을 건너뜁니다: {source.GetCharacterName()}");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"카드 애니메이션 실행 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        #endregion

        #region 슬롯 이동

        /// <summary>
        /// 모든 슬롯을 1칸씩 앞으로 이동합니다.
        /// </summary>
        /// <returns>이동 완료 Task</returns>
        private async Task MoveAllSlotsForward()
        {
            GameLogger.LogInfo("슬롯 이동 시작", GameLogger.LogCategory.Combat);

            var positions = new[]
            {
                CombatSlotPosition.WAIT_SLOT_4,
                CombatSlotPosition.WAIT_SLOT_3,
                CombatSlotPosition.WAIT_SLOT_2,
                CombatSlotPosition.WAIT_SLOT_1,
                CombatSlotPosition.BATTLE_SLOT
            };

            for (int i = 0; i < positions.Length - 1; i++)
            {
                var fromPosition = positions[i];
                var toPosition = positions[i + 1];

                var fromSlot = CombatSlotManager.Instance.GetSlot(fromPosition);
                var toSlot = CombatSlotManager.Instance.GetSlot(toPosition);

                if (fromSlot?.OccupiedCard != null && toSlot?.IsEmpty == true)
                {
                    var card = fromSlot.RemoveCard();
                    toSlot.TryPlaceCard(card);

                    // 이동 애니메이션
                    await AnimateCardMove(fromPosition, toPosition);
                }
            }

            GameLogger.LogInfo("슬롯 이동 완료", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 애니메이션

        /// <summary>
        /// 카드 이동 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="from">시작 위치</param>
        /// <param name="to">도착 위치</param>
        /// <returns>애니메이션 완료 Task</returns>
        private async Task AnimateCardMove(CombatSlotPosition from, CombatSlotPosition to)
        {
            // TODO: DOTween을 사용한 카드 이동 애니메이션 구현
            // 현재는 지연 시간만 적용
            await Task.Delay((int)(slotMoveDuration * 1000));
        }

        /// <summary>
        /// 카드 사용 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="cardUI">카드 UI</param>
        /// <returns>애니메이션 완료 Task</returns>
        private async Task AnimateCardUse(SkillCardUI cardUI)
        {
            // TODO: DOTween을 사용한 카드 사용 애니메이션 구현
            // 현재는 지연 시간만 적용
            await Task.Delay(200);
        }

        /// <summary>
        /// 캐릭터 공격 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="isEnemy">적 여부</param>
        /// <returns>애니메이션 완료 Task</returns>
        private async Task AnimateCharacterAttack(string characterId, bool isEnemy)
        {
            // TODO: AnimationSystem 재구현 후 캐릭터 공격 애니메이션 구현
            // 현재는 지연 시간만 적용
            await Task.Delay(300);
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 카드의 소스 캐릭터를 반환합니다.
        /// </summary>
        /// <param name="card">카드</param>
        /// <returns>소스 캐릭터</returns>
        private ICharacter? GetSourceCharacter(ISkillCard card)
        {
            return card.GetOwner() == SlotOwner.PLAYER 
                ? playerManager?.GetPlayer() 
                : enemyManager?.GetEnemy();
        }

        /// <summary>
        /// 카드의 타겟 캐릭터를 반환합니다.
        /// </summary>
        /// <param name="card">카드</param>
        /// <returns>타겟 캐릭터</returns>
        private ICharacter? GetTargetCharacter(ISkillCard card)
        {
            return card.GetOwner() == SlotOwner.PLAYER 
                ? enemyManager?.GetEnemy() 
                : playerManager?.GetPlayer();
        }

        /// <summary>
        /// 카드에 해당하는 UI를 찾습니다.
        /// </summary>
        /// <param name="card">카드</param>
        /// <returns>카드 UI, 없으면 null</returns>
        private SkillCardUI? FindCardUI(ISkillCard card)
        {
            // TODO: 카드 UI 찾기 로직 구현
            // 현재는 null 반환
            return null;
        }

        #endregion

        #region 디버그

        /// <summary>
        /// 전투 시퀀스를 수동으로 실행합니다.
        /// </summary>
        [ContextMenu("전투 시퀀스 실행")]
        public async void ExecuteCombatSequenceManual()
        {
            await ExecuteCombatSequence();
        }

        #endregion
    }
}
