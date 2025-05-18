using System.Collections;
using UnityEngine;
using Game.CombatSystem.Manager;
using Game.Utility;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Core; // ← CombatFlowCoordinator 네임스페이스 추가

[DefaultExecutionOrder(-1000)]
public class CombatStartupManager : MonoBehaviour
{
    [Header("의존성 매니저")]
    [SerializeField] private SlotRegistry slotRegistry;
    [SerializeField] private SceneAutoBinderManager autoBinder;
    [SerializeField] private CombatStateFactoryInstaller stateFactoryInstaller;
    [SerializeField] private CombatBootstrapInstaller bootstrapInstaller;
    [SerializeField] private CombatFlowCoordinator flowCoordinator; // ← 올바른 타입으로 수정

    private void Start()
    {
        StartCoroutine(StartupRoutine());
    }

    private IEnumerator StartupRoutine()
    {
        slotRegistry.Initialize();
        autoBinder.Initialize();
        yield return null;
        yield return null;

        stateFactoryInstaller.Initialize();
        yield return null;

        bootstrapInstaller.Initialize();
        yield return null;

        flowCoordinator.StartCombatFlow(); // ← 전투 흐름 시작
        yield return null;
    }

    private bool ValidateAll()
    {
        return slotRegistry != null &&
               autoBinder != null &&
               stateFactoryInstaller != null &&
               bootstrapInstaller != null &&
               flowCoordinator != null; // ← 이름 변경 반영
    }
}
