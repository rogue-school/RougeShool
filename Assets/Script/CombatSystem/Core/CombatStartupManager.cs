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
            steps = Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .OfType<ICombatInitializerStep>()
                .OrderBy(step => step.Order)
                .ToList();

            if (steps.Count == 0)
                Debug.LogWarning("[CombatStartupManager] 초기화 스텝이 없습니다.");
            else
                Debug.Log($"[CombatStartupManager] {steps.Count}개 초기화 스텝 수집 (Order 기준 정렬)");
        }

        private void Start()
        {
            StartCoroutine(StartupRoutine());
        }

        private IEnumerator StartupRoutine()
        {
            foreach (var step in steps)
            {
                Debug.Log($"[CombatStartupManager] 초기화: {step.GetType().Name} (Order: {step.Order})");
                yield return step.Initialize();
            }

            Debug.Log("[CombatStartupManager] 모든 초기화 단계 완료");
        }
    }
}
