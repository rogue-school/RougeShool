using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Core
{
    public class CombatStartupManager : MonoBehaviour
    {
        private List<ICombatInitializerStep> steps;

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

        private void Start()
        {
            StartCoroutine(StartupRoutine());
        }

        private List<ICombatInitializerStep> FindInitializerSteps()
        {
            return Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .OfType<ICombatInitializerStep>()
                .OrderBy(step => step.Order)
                .ToList();
        }

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
        }
    }
}
