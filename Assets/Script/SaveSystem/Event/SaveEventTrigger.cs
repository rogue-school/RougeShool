using UnityEngine;
using Game.SaveSystem.Manager;
using Game.SaveSystem.Data;
using Zenject;

namespace Game.SaveSystem.Event
{
    /// <summary>
    /// 저장 이벤트 트리거
    /// 슬레이 더 스파이어 방식: 특정 이벤트 발생 시 자동 저장 트리거
    /// </summary>
    public class SaveEventTrigger : MonoBehaviour
    {
        #region 의존성 주입

        [Inject] private AutoSaveManager autoSaveManager;

        #endregion

        #region 턴 관련 이벤트

        /// <summary>
        /// 적 카드 배치 후 저장 트리거
        /// </summary>
        public void OnEnemyCardPlaced()
        {
            Debug.Log("[SaveEventTrigger] 적 카드 배치 후 저장 트리거");
            autoSaveManager?.TriggerAutoSave("EnemyCardPlaced");
        }

        /// <summary>
        /// 턴 시작 버튼 누르기 전 저장 트리거
        /// </summary>
        public void OnTurnStartButtonPressed()
        {
            Debug.Log("[SaveEventTrigger] 턴 시작 버튼 누르기 전 저장 트리거");
            autoSaveManager?.TriggerAutoSave("BeforeTurnStart");
        }

        /// <summary>
        /// 턴 실행 중 저장 트리거
        /// </summary>
        public void OnTurnExecution()
        {
            Debug.Log("[SaveEventTrigger] 턴 실행 중 저장 트리거");
            autoSaveManager?.TriggerAutoSave("DuringTurnExecution");
        }

        /// <summary>
        /// 턴 완료 후 저장 트리거
        /// </summary>
        public void OnTurnCompleted()
        {
            Debug.Log("[SaveEventTrigger] 턴 완료 후 저장 트리거");
            autoSaveManager?.TriggerAutoSave("TurnCompleted");
        }

        #endregion

        #region 스테이지 관련 이벤트

        /// <summary>
        /// 스테이지 완료 후 저장 트리거
        /// </summary>
        public void OnStageCompleted()
        {
            Debug.Log("[SaveEventTrigger] 스테이지 완료 후 저장 트리거");
            autoSaveManager?.TriggerAutoSave("StageCompleted");
        }

        /// <summary>
        /// 준보스 처치 후 저장 트리거
        /// </summary>
        public void OnSubBossDefeated()
        {
            Debug.Log("[SaveEventTrigger] 준보스 처치 후 저장 트리거");
            autoSaveManager?.TriggerAutoSave("SubBossDefeated");
        }

        /// <summary>
        /// 보스 처치 후 저장 트리거
        /// </summary>
        public void OnBossDefeated()
        {
            Debug.Log("[SaveEventTrigger] 보스 처치 후 저장 트리거");
            autoSaveManager?.TriggerAutoSave("BossDefeated");
        }

        #endregion

        #region 게임 상태 관련 이벤트

        /// <summary>
        /// 게임 종료 시 저장 트리거
        /// </summary>
        public void OnGameExit()
        {
            Debug.Log("[SaveEventTrigger] 게임 종료 시 저장 트리거");
            autoSaveManager?.TriggerAutoSave("GameExit");
        }

        /// <summary>
        /// 씬 전환 시 저장 트리거
        /// </summary>
        public void OnSceneTransition()
        {
            Debug.Log("[SaveEventTrigger] 씬 전환 시 저장 트리거");
            autoSaveManager?.TriggerAutoSave("SceneTransition");
        }

        #endregion

        #region 수동 저장/로드

        /// <summary>
        /// 수동 저장 트리거
        /// </summary>
        /// <param name="saveName">저장 이름</param>
        public void OnManualSave(string saveName = "ManualSave")
        {
            Debug.Log($"[SaveEventTrigger] 수동 저장 트리거: {saveName}");
            autoSaveManager?.SaveGameState(saveName);
        }

        /// <summary>
        /// 수동 로드 트리거
        /// </summary>
        /// <param name="filePath">저장 파일 경로</param>
        public void OnManualLoad(string filePath)
        {
            Debug.Log($"[SaveEventTrigger] 수동 로드 트리거: {filePath}");
            autoSaveManager?.LoadGameState(filePath);
        }

        #endregion

        #region 설정 관리

        /// <summary>
        /// 자동 저장 활성화/비활성화
        /// </summary>
        /// <param name="enabled">활성화 여부</param>
        public void SetAutoSaveEnabled(bool enabled)
        {
            autoSaveManager?.SetAutoSaveEnabled(enabled);
        }

        /// <summary>
        /// 특정 자동 저장 조건 활성화/비활성화
        /// </summary>
        /// <param name="conditionName">조건 이름</param>
        /// <param name="enabled">활성화 여부</param>
        public void SetConditionEnabled(string conditionName, bool enabled)
        {
            autoSaveManager?.SetConditionEnabled(conditionName, enabled);
        }

        #endregion

        #region 디버그

        /// <summary>
        /// 자동 저장 설정 출력
        /// </summary>
        [ContextMenu("자동 저장 설정 출력")]
        public void PrintAutoSaveSettings()
        {
            autoSaveManager?.PrintAutoSaveSettings();
        }

        #endregion
    }
}
