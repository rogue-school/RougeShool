using UnityEngine;
using System.Collections;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Core;
using Game.CombatSystem.Enemy;
using Game.CombatSystem.Player;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Stage;

namespace Game.CombatSystem.Intialization
{
    /// <summary>
    /// 전투 씬에서 모든 초기화 과정을 수행하는 전담 매니저입니다.
    /// </summary>
    public class CombatInitializerManager : MonoBehaviour
    {
        [Header("초기화 대상")]
        [SerializeField] private SlotInitializer slotInitializer;
        [SerializeField] private PlayerCharacterInitializer playerInitializer;
        [SerializeField] private EnemyInitializer enemyInitializer;
        [SerializeField] private PlayerHandManager playerHandManager;
        [SerializeField] private EnemyHandManager enemyHandManager;

        private void Start()
        {
            StartCoroutine(InitializeRoutine());
        }

        private IEnumerator InitializeRoutine()
        {
            Debug.Log("[CombatInitializerManager] 전투 초기화 시작");

            // 1. 슬롯 자동 등록
            slotInitializer?.AutoBindAllSlots();

            // 2. 슬롯 정보 인식
            playerHandManager?.Initialize();

            // 3. 캐릭터 배치
            playerInitializer?.Setup();

            // 4. 스테이지에서 적 데이터 가져오기 → 적 소환
            var stage = StageManager.Instance.GetCurrentStage();
            if (stage != null && stage.enemies != null && stage.enemies.Count > 0)
            {
                enemyInitializer?.SetupWithData(stage.enemies[0]);
            }
            else
            {
                Debug.LogError("[CombatInitializerManager] 현재 스테이지에 적 데이터가 없습니다.");
                yield break;
            }

            // 5. 프레임 종료까지 대기 (UI 포함 모든 생성 완료 보장)
            yield return null;

            // 6. 적 핸드 매니저 초기화
            EnemyCharacter enemy = enemyInitializer.GetSpawnedEnemy();
            if (enemy == null)
            {
                Debug.LogError("[CombatInitializerManager] 적 캐릭터 인스턴스가 생성되지 않았습니다.");
                yield break;
            }

            //  EnemyManager에 적 등록
            EnemyManager.Instance.SetEnemy(enemy);

            enemyHandManager?.Initialize(enemy);

            // 7. 핸드 카드 생성
            playerHandManager?.GenerateInitialHand();
            enemyHandManager?.GenerateInitialHand();

            // 8. 적 첫 번째 카드 CombatTurnManager에 등록
            CombatTurnManager.Instance.BeginEnemyTurn();
        }
    }
}
