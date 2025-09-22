using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Manager;
using UnityEngine;
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
        #region 적 캐릭터 전용 설정

        // 적 캐릭터는 프리팹 내장 UI를 사용합니다.
        // EnemyCharaterCard.prefab에 UI가 내장되어 있어 별도 연결이 불필요합니다.
        // 적의 HP 바, 이펙트 아이콘 등은 프리팹 내부의 EnemyUIController가 관리합니다.

        #endregion

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

        #region BaseCoreManager 오버라이드

        /// <summary>
        /// 적 매니저는 캐릭터 프리팹이 필요하지 않습니다.
        /// </summary>
        protected override bool RequiresRelatedPrefab() => false;

        /// <summary>
        /// 적 매니저는 UI 컨트롤러가 선택사항입니다.
        /// </summary>
        protected override bool RequiresUIController() => false;

        /// <summary>
        /// 적 캐릭터 프리팹을 반환합니다. (적은 동적 생성되므로 null)
        /// </summary>
        protected override GameObject GetRelatedPrefab() => null;

        /// <summary>
        /// 적 UI 컨트롤러를 반환합니다.
        /// 적 캐릭터는 프리팹 내장 UI를 사용하므로 별도 UI 연결이 불필요합니다.
        /// </summary>
        protected override MonoBehaviour GetUIController() => null;

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

    /// <summary>
    /// 적 캐릭터가 배치될 슬롯을 반환합니다.
    /// </summary>
    public Transform GetCharacterSlot() => characterSlot;

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
