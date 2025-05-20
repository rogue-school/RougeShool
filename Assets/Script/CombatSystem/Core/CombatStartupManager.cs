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

        //  슬롯 자동 바인딩 호출 (신규 API 사용)
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

        // 씬 내 컴포넌트 자동 바인딩
        autoBinder.Initialize();
        yield return null;
        yield return null;

        // 상태 팩토리 생성
        stateFactoryInstaller.Initialize();
        yield return null;

        // 전투 의존성 주입
        bootstrapInstaller.Initialize();
        yield return null;

        // 전투 흐름 시작
        flowCoordinator.StartCombatFlow();
        yield return null;
    }

    private bool ValidateAll()
    {
        return slotRegistry != null &&
               autoBinder != null &&
               stateFactoryInstaller != null &&
               bootstrapInstaller != null &&
               flowCoordinator != null;
    }
}
