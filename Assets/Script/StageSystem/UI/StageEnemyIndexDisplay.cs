using UnityEngine;
using System.Collections.Generic;
using Game.StageSystem.Interface;
using Game.StageSystem.Data;
using Game.StageSystem.Manager;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.StageSystem.UI
{
    /// <summary>
    /// 스테이지의 적 인덱스를 시각적으로 표시하는 시스템입니다.
    /// TargetMarket을 현재 적 인덱스에 맞는 위치로 이동시킵니다.
    /// </summary>
    public class StageEnemyIndexDisplay : MonoBehaviour
    {
        [Header("인덱스 위치 설정")]
        [Tooltip("인덱스별 위치 RectTransform 리스트 (인덱스 1 = [0], 인덱스 2 = [1])")]
        [SerializeField] private List<RectTransform> indexPositions = new List<RectTransform>();

        [Header("타겟 마커 설정")]
        [Tooltip("이동할 타겟 마커 (트라이앵글 등 2D 오브젝트)")]
        [SerializeField] private RectTransform targetMarker;

        [Header("죽은 적 UI 설정")]
        [Tooltip("죽은 적 표시 UI 프리팹 (EnemyDeadUI)")]
        [SerializeField] private GameObject enemyDeadUIPrefab;

        [Header("설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] private bool enableDebugLogging = true;

        [Inject] private IStageManager stageManager;

        private int currentActiveIndex = -1;
        private Dictionary<int, GameObject> deadEnemyUIs = new Dictionary<int, GameObject>();

        private void Start()
        {
            if (stageManager == null)
            {
                GameLogger.LogError("[StageEnemyIndexDisplay] StageManager가 주입되지 않았습니다.", GameLogger.LogCategory.UI);
                return;
            }

            if (targetMarker == null)
            {
                GameLogger.LogError("[StageEnemyIndexDisplay] TargetMarker가 설정되지 않았습니다.", GameLogger.LogCategory.UI);
                return;
            }

            // 이벤트 구독
            SubscribeToEvents();

            // 초기 표시 (이미 스테이지가 로드되어 있을 수 있음)
            RefreshDisplay();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// StageManager 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (stageManager is StageManager concreteManager)
            {
                concreteManager.OnStageTransition += OnStageTransition;
                concreteManager.OnEnemyDefeated += OnEnemyDefeated;
                concreteManager.OnProgressChanged += OnProgressChanged;
            }
        }

        /// <summary>
        /// StageManager 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (stageManager is StageManager concreteManager)
            {
                concreteManager.OnStageTransition -= OnStageTransition;
                concreteManager.OnEnemyDefeated -= OnEnemyDefeated;
                concreteManager.OnProgressChanged -= OnProgressChanged;
            }
        }

        /// <summary>
        /// 스테이지 전환 시 호출됩니다.
        /// </summary>
        private void OnStageTransition(StageData previousStage, StageData newStage)
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"[StageEnemyIndexDisplay] 스테이지 전환: {previousStage?.stageName} → {newStage?.stageName}", GameLogger.LogCategory.UI);
            }

            // 이전 스테이지의 죽은 적 UI 모두 제거
            ClearAllDeadEnemyUIs();

            RefreshDisplay();
        }

        /// <summary>
        /// 적 처치 시 호출됩니다.
        /// </summary>
        private void OnEnemyDefeated(Game.CharacterSystem.Interface.ICharacter enemy)
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"[StageEnemyIndexDisplay] 적 처치: {enemy.GetCharacterName()}", GameLogger.LogCategory.UI);
            }

            // 죽은 적의 인덱스 계산
            // currentEnemyIndex는 적 스폰 시 증가하므로, 죽은 적의 인덱스는 currentEnemyIndex - 1
            int currentIndex = GetCurrentEnemyIndex();
            int deadEnemyIndex = currentIndex - 1;

            if (deadEnemyIndex < 0)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogWarning($"[StageEnemyIndexDisplay] 죽은 적 인덱스 계산 오류: currentIndex={currentIndex}, deadEnemyIndex={deadEnemyIndex}", GameLogger.LogCategory.UI);
                }
                deadEnemyIndex = 0; // 안전장치: 최소 0으로 설정
            }

            // 죽은 적의 인덱스에 EnemyDeadUI 표시
            ShowEnemyDeadUI(deadEnemyIndex);

            // 다음 적 인덱스로 업데이트 (적 처치 후 다음 적이 스폰되기 전이므로 인덱스는 그대로)
            RefreshDisplay();
        }

        /// <summary>
        /// 스테이지 진행 상태 변경 시 호출됩니다.
        /// </summary>
        private void OnProgressChanged(StageProgressState state)
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"[StageEnemyIndexDisplay] 진행 상태 변경: {state}", GameLogger.LogCategory.UI);
            }

            // 스테이지 시작 시 첫 번째 적 인덱스 표시
            if (state == StageProgressState.InProgress)
            {
                RefreshDisplay();
            }
        }

        /// <summary>
        /// 인덱스 표시를 새로고침합니다.
        /// </summary>
        public void RefreshDisplay()
        {
            if (stageManager == null)
            {
                GameLogger.LogWarning("[StageEnemyIndexDisplay] StageManager가 null입니다.", GameLogger.LogCategory.UI);
                return;
            }

            if (targetMarker == null)
            {
                GameLogger.LogWarning("[StageEnemyIndexDisplay] TargetMarker가 null입니다.", GameLogger.LogCategory.UI);
                return;
            }

            var currentStage = stageManager.GetCurrentStage();
            if (currentStage == null || currentStage.enemies == null || currentStage.enemies.Count == 0)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("[StageEnemyIndexDisplay] 스테이지 데이터가 없어 마커를 숨깁니다.", GameLogger.LogCategory.UI);
                }
                if (targetMarker != null)
                {
                    targetMarker.gameObject.SetActive(false);
                }
                return;
            }

            int currentEnemyIndex = GetCurrentEnemyIndex();

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"[StageEnemyIndexDisplay] 표시 새로고침 - 현재 인덱스: {currentEnemyIndex + 1} (0-based: {currentEnemyIndex})", GameLogger.LogCategory.UI);
            }

            // 타겟 마커를 현재 인덱스 위치로 이동
            UpdateTargetMarkerPosition(currentEnemyIndex);
        }

        /// <summary>
        /// 현재 적 인덱스를 가져옵니다.
        /// </summary>
        private int GetCurrentEnemyIndex()
        {
            if (stageManager is StageManager concreteManager)
            {
                return concreteManager.CurrentEnemyIndex;
            }
            return 0;
        }

        /// <summary>
        /// 타겟 마커를 현재 인덱스 위치로 이동시킵니다.
        /// </summary>
        private void UpdateTargetMarkerPosition(int enemyIndex)
        {
            // 인덱스는 0부터 시작하지만 리스트 인덱스도 0부터 시작
            // 인덱스 1 = 리스트[0], 인덱스 2 = 리스트[1]
            int listIndex = enemyIndex;

            if (listIndex < 0 || listIndex >= indexPositions.Count)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogWarning($"[StageEnemyIndexDisplay] 인덱스 {enemyIndex + 1}에 해당하는 위치가 없습니다. (리스트 크기: {indexPositions.Count})", GameLogger.LogCategory.UI);
                }
                if (targetMarker != null)
                {
                    targetMarker.gameObject.SetActive(false);
                }
                return;
            }

            RectTransform targetPosition = indexPositions[listIndex];
            if (targetPosition == null)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogWarning($"[StageEnemyIndexDisplay] 인덱스 {enemyIndex + 1} 위치 RectTransform이 null입니다.", GameLogger.LogCategory.UI);
                }
                return;
            }

            // 타겟 마커 활성화
            if (!targetMarker.gameObject.activeSelf)
            {
                targetMarker.gameObject.SetActive(true);
            }

            // 부모를 인덱스 위치 자체로 설정 (인덱스 위치의 자식으로 배치)
            if (targetMarker.parent != targetPosition)
            {
                targetMarker.SetParent(targetPosition, false);
            }

            // RectTransform 위치 설정 (x, y, z = 0, 0, -1)
            targetMarker.anchoredPosition3D = new Vector3(0f, 0f, -1f);

            currentActiveIndex = enemyIndex;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"[StageEnemyIndexDisplay] 타겟 마커 이동: 인덱스 {enemyIndex + 1} 위치로 이동 (위치: {targetPosition.position})", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 죽은 적의 인덱스에 EnemyDeadUI를 표시합니다.
        /// </summary>
        private void ShowEnemyDeadUI(int enemyIndex)
        {
            if (enemyDeadUIPrefab == null)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogWarning("[StageEnemyIndexDisplay] EnemyDeadUI 프리팹이 설정되지 않았습니다.", GameLogger.LogCategory.UI);
                }
                return;
            }

            // 이미 해당 인덱스에 UI가 있으면 건너뜀
            if (deadEnemyUIs.ContainsKey(enemyIndex))
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo($"[StageEnemyIndexDisplay] 인덱스 {enemyIndex + 1}에 이미 EnemyDeadUI가 표시되어 있습니다.", GameLogger.LogCategory.UI);
                }
                return;
            }

            // 인덱스 범위 확인
            if (enemyIndex < 0 || enemyIndex >= indexPositions.Count)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogWarning($"[StageEnemyIndexDisplay] 인덱스 {enemyIndex + 1}에 해당하는 위치가 없습니다.", GameLogger.LogCategory.UI);
                }
                return;
            }

            RectTransform targetPosition = indexPositions[enemyIndex];
            if (targetPosition == null)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogWarning($"[StageEnemyIndexDisplay] 인덱스 {enemyIndex + 1} 위치 RectTransform이 null입니다.", GameLogger.LogCategory.UI);
                }
                return;
            }

            // EnemyDeadUI 생성
            GameObject deadUI = Instantiate(enemyDeadUIPrefab, targetPosition);
            deadUI.name = $"EnemyDeadUI_Index{enemyIndex + 1}";

            // RectTransform 위치 설정 (x, y, z = 0, 0, -1)
            RectTransform deadUIRect = deadUI.GetComponent<RectTransform>();
            if (deadUIRect != null)
            {
                deadUIRect.anchoredPosition3D = new Vector3(0f, 0f, -1f);
            }

            // 딕셔너리에 저장
            deadEnemyUIs[enemyIndex] = deadUI;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"[StageEnemyIndexDisplay] EnemyDeadUI 표시: 인덱스 {enemyIndex + 1} 위치에 생성", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 모든 죽은 적 UI를 제거합니다.
        /// </summary>
        private void ClearAllDeadEnemyUIs()
        {
            foreach (var deadUI in deadEnemyUIs.Values)
            {
                if (deadUI != null)
                {
                    Destroy(deadUI);
                }
            }

            deadEnemyUIs.Clear();

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("[StageEnemyIndexDisplay] 모든 EnemyDeadUI 제거", GameLogger.LogCategory.UI);
            }
        }
    }
}

