using System.Collections;
using UnityEngine;
using Game.CombatSystem.Manager;
using Game.Utility;
using Game.CombatSystem.Slot;

[DefaultExecutionOrder(-1000)]  // 가장 먼저 실행되도록 설정
public class CombatStartupManager : MonoBehaviour
{
    [Header("의존성 매니저")]
    [SerializeField] private SlotRegistry slotRegistry;
    [SerializeField] private SceneAutoBinderManager autoBinder;
    [SerializeField] private CombatStateFactoryInstaller stateFactoryInstaller;
    [SerializeField] private CombatBootstrapInstaller bootstrapInstaller;
    [SerializeField] private CombatInitializerManager initializerManager;

    private void Awake()
    {
       // Debug.Log("[CombatStartupManager] Awake() → 순차 초기화 준비");

        if (!ValidateAll())
        {
            //Debug.LogError("[CombatStartupManager] 필수 구성 요소가 누락되었습니다.");
            return;
        }
    }

    private void Start()
    {
        //Debug.Log("[CombatStartupManager] Start() → 초기화 루틴 시작");
        StartCoroutine(StartupRoutine());
    }

    private IEnumerator StartupRoutine()
    {
        // Step 1. 슬롯 시스템 초기화
        //Debug.Log("[CombatStartupManager] Step 1: 슬롯 레지스트리 초기화");
        slotRegistry.Initialize();
        autoBinder.Initialize();

        // 추가 대기: 슬롯 등록 안정화
        yield return null;
        yield return null;

        // Step 2. 상태 팩토리 생성 및 주입
        //Debug.Log("[CombatStartupManager] Step 2: 상태 팩토리 생성 및 주입");
        stateFactoryInstaller.Initialize();
        yield return null;

        // Step 3. 의존성 주입
        //Debug.Log("[CombatStartupManager] Step 3: 의존성 주입");
        bootstrapInstaller.Initialize();
        yield return null;

        // Step 4. 전투 초기화
        //Debug.Log("[CombatStartupManager] Step 4: 전투 초기화 시작");
        initializerManager.InitializeCombat();
        yield return null;

        //Debug.Log("[CombatStartupManager] 초기화 루틴 완료");
    }

    private bool ValidateAll()
    {
        return slotRegistry != null &&
               autoBinder != null &&
               stateFactoryInstaller != null &&
               bootstrapInstaller != null &&
               initializerManager != null;
    }
}
