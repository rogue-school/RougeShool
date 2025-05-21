using System.Collections;
using UnityEngine;
using Game.CombatSystem.Manager;
using Game.Utility;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Core;

[DefaultExecutionOrder(-1000)]
public class CombatStartupManager : MonoBehaviour
{
    [Header("의존성 매니저")]
    [SerializeField] private SlotRegistry slotRegistry;
    [SerializeField] private SceneAutoBinderManager autoBinder;
    [SerializeField] private CombatStateFactoryInstaller stateFactoryInstaller;
    [SerializeField] private CombatBootstrapInstaller bootstrapInstaller;
    [SerializeField] private CombatFlowCoordinator flowCoordinator;

    private void Start()
    {
        StartCoroutine(StartupRoutine());
    }

    private IEnumerator StartupRoutine()
    {
        Debug.Log("[CombatStartupManager] 슬롯 레지스트리 초기화 시작");
        slotRegistry.Initialize();

        var slotInitializer = Object.FindFirstObjectByType<SlotInitializer>();
        if (slotInitializer != null)
        {
            slotInitializer.AutoBindAllSlots();
            Debug.Log("[CombatStartupManager] 슬롯 자동 바인딩 완료");
        }
        else
        {
            Debug.LogError("[CombatStartupManager] SlotInitializer를 찾을 수 없습니다.");
        }

        autoBinder.Initialize();
        yield return null;

        stateFactoryInstaller.Initialize();
        yield return null;

        bootstrapInstaller.Initialize();
        yield return null;

        var turnManager = Object.FindFirstObjectByType<CombatTurnManager>();
        if (turnManager != null)
        {
            turnManager.Initialize();
            Debug.Log("[CombatStartupManager] CombatTurnManager 초기화 완료");
        }
        else
        {
            Debug.LogError("[CombatStartupManager] CombatTurnManager를 찾을 수 없습니다.");
        }

        flowCoordinator.StartCombatFlow();
        yield return null;
    }
}
