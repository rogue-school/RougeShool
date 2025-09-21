using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Manager;
using UnityEngine;
using Game.CharacterSystem.UI;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 전투 중 등장한 적 캐릭터를 관리하는 매니저 클래스입니다.
    /// 현재 적 캐릭터와 관련된 상태를 보관하거나 초기화합니다.
    /// 적 카드는 핸드 없이 대기 슬롯에서 직접 관리됩니다.
    /// </summary>
    public class EnemyManager : BaseCharacterManager<ICharacter>
{

    #region DI

    /// <summary>
    /// Zenject 의존성 주입 (확장용)
    /// </summary>
    [Inject]
    public void Construct()
    {
        // 필요시 의존성 주입 로직 추가
    }

    #endregion

    #region 등록 / 설정

    /// <summary>
    /// 적 캐릭터를 생성하고 등록합니다.
    /// </summary>
    public override void CreateAndRegisterCharacter()
    {
        // 적 캐릭터는 StageManager에서 생성되므로 여기서는 등록만 처리
        GameLogger.LogInfo("적 캐릭터 등록 대기 중", GameLogger.LogCategory.Character);
    }

    /// <summary>
    /// 적 캐릭터를 등록합니다.
    /// </summary>
    /// <param name="enemy">등록할 적 캐릭터</param>
    public void RegisterEnemy(ICharacter enemy)
    {
        SetCharacter(enemy);
        
        // 적 UI 컨트롤러 자동 연결(있을 때만)
        ConnectCharacterUI(enemy);
        
        // 추가 UI 연결 (EnemyCharacterUIController)
        if (enemy is Component enemyComp)
        {
            var ui = enemyComp.GetComponentInChildren<EnemyCharacterUIController>(true);
            if (ui != null && enemy is ICharacter ic)
            {
                ui.SetTarget(ic);
            }
        }
    }

    /// <summary>
    /// 적 캐릭터를 설정합니다.
    /// </summary>
    public override void SetCharacter(ICharacter character)
    {
        currentCharacter = character;
    }

    /// <summary>
    /// 적 캐릭터 등록을 해제합니다.
    /// </summary>
    public override void UnregisterCharacter()
    {
        currentCharacter = null;
        GameLogger.LogInfo("적 캐릭터 등록 해제", GameLogger.LogCategory.Character);
    }

    /// <summary>
    /// 적 캐릭터 등록을 해제합니다. (호환성 유지)
    /// </summary>
    public void UnregisterEnemy()
    {
        UnregisterCharacter();
    }

    // 적 핸드 매니저 관련 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

    #endregion

    #region 조회

    /// <summary>
    /// 현재 등록된 적 캐릭터를 반환합니다.
    /// </summary>
    public override ICharacter GetCharacter() => currentCharacter;

    /// <summary>
    /// 현재 등록된 적 캐릭터를 반환합니다. (호환성 유지)
    /// </summary>
    public ICharacter GetEnemy() => currentCharacter;

    /// <summary>
    /// 현재 등록된 적 캐릭터를 반환합니다. (명시적 이름)
    /// </summary>
    public ICharacter GetCurrentEnemy() => currentCharacter;

    // 적 핸드 매니저 조회 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

    /// <summary>
    /// 적 캐릭터가 등록되어 있는지 여부를 확인합니다.
    /// </summary>
    public bool HasEnemy() => currentCharacter != null;

    #endregion

    #region 초기화

    /// <summary>
    /// 등록된 적 캐릭터를 초기화합니다. (호환성 유지)
    /// </summary>
    public void ClearEnemy()
    {
        UnregisterCharacter();
    }

    /// <summary>
    /// 매니저 상태를 초기화합니다.
    /// </summary>
    public override void ResetCharacter()
    {
        UnregisterCharacter();
        GameLogger.LogInfo("EnemyManager 초기화 완료", GameLogger.LogCategory.Character);
    }

    #endregion
}
}
