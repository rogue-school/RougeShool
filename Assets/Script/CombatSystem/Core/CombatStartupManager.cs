using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.CombatSystem.Interface;
using Zenject;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 전투 초기화 순서를 제어하는 관리자 클래스입니다.
    /// 씬 내 모든 ICombatInitializerStep 구현체를 찾아 순서대로 초기화하고,
    /// 초기화가 완료되면 플레이어 입력 상태로 전이합니다.
    /// </summary>
    public class CombatStartupManager : MonoBehaviour
    {
        private List<ICombatInitializerStep> steps;

        [Inject] private ICombatTurnManager turnManager;
        [Inject] private ICombatStateFactory stateFactory;

        #region Unity Methods

        /// <summary>
        /// 초기화 스텝을 수집하여 정렬합니다.
        /// </summary>
        private void Awake()
        {
            steps = FindInitializerSteps();

            if (steps.Count == 0)
            {
                Debug.LogWarning("[CombatStartupManager] 초기화 스텝이 하나도 존재하지 않습니다.");
            }
            else
            {
                Debug.Log($"[CombatStartupManager] {steps.Count}개 초기화 스텝 수집 완료 (Order 기준 정렬)");
            }
        }

        /// <summary>
        /// 초기화 루틴을 코루틴으로 시작합니다.
        /// </summary>
        private void Start()
        {
            StartCoroutine(StartupRoutine());
        }

        #endregion

        #region Initialization Step Finder

        /// <summary>
        /// 씬 내 모든 ICombatInitializerStep 구현체를 찾아 순서대로 정렬합니다.
        /// </summary>
        private List<ICombatInitializerStep> FindInitializerSteps()
        {
            return Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .OfType<ICombatInitializerStep>()
                .OrderBy(step => step.Order)
                .ToList();
        }

        #endregion

        #region Startup Routine

        /// <summary>
        /// 순서대로 초기화 스텝을 실행하며, 예외 처리 및 로그를 포함합니다.
        /// 완료 후 플레이어 입력 상태로 전이합니다.
        /// </summary>
        private IEnumerator StartupRoutine()
        {
            foreach (var step in steps)
            {
                Debug.Log($"[CombatStartupManager] 초기화 시작: {step.GetType().Name} (Order: {step.Order})");

                IEnumerator routine = null;
                try
                {
                    routine = step.Initialize();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[CombatStartupManager] {step.GetType().Name} 초기화 중 예외 발생: {ex.Message}");
                    continue;
                }

                if (routine != null)
                    yield return routine;

                Debug.Log($"[CombatStartupManager] 초기화 완료: {step.GetType().Name}");
            }

            Debug.Log("<color=lime>[CombatStartupManager] 모든 초기화 단계 완료</color>");

            var playerInputState = stateFactory.CreatePlayerInputState();
            turnManager.RequestStateChange(playerInputState);
        }

        #endregion
    }
}
