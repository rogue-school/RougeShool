using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using UnityEngine;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 전투 중 등장한 적 캐릭터와 핸드 매니저를 관리하는 매니저 클래스입니다.
    /// 현재 적 캐릭터와 관련된 상태를 보관하거나 초기화합니다.
    /// </summary>
    public class EnemyManager : MonoBehaviour, IEnemyManager
{
    private IEnemyCharacter currentEnemy;
    private IEnemyHandManager enemyHandManager;

    #region 등록 / 설정

    /// <summary>
    /// 적 캐릭터를 등록합니다.
    /// </summary>
    /// <param name="enemy">등록할 적 캐릭터</param>
    public void RegisterEnemy(IEnemyCharacter enemy)
    {
        currentEnemy = enemy;
    }

    /// <summary>
    /// 적 캐릭터 등록을 해제합니다.
    /// </summary>
    public void UnregisterEnemy()
    {
        currentEnemy = null;
        Debug.Log("[EnemyManager] 적 등록 해제");
    }

    /// <summary>
    /// 적 핸드 매니저를 설정합니다.
    /// </summary>
    /// <param name="handManager">적 핸드 매니저</param>
    public void SetEnemyHandManager(IEnemyHandManager handManager)
    {
        enemyHandManager = handManager;
    }

    #endregion

    #region 조회

    /// <summary>
    /// 현재 등록된 적 캐릭터를 반환합니다.
    /// </summary>
    public IEnemyCharacter GetEnemy() => currentEnemy;

    /// <summary>
    /// 현재 등록된 적 캐릭터를 반환합니다. (명시적 이름)
    /// </summary>
    public IEnemyCharacter GetCurrentEnemy() => currentEnemy;

    /// <summary>
    /// 현재 등록된 적 핸드 매니저를 반환합니다.
    /// </summary>
    public IEnemyHandManager GetEnemyHandManager() => enemyHandManager;

    /// <summary>
    /// 적 캐릭터가 등록되어 있는지 여부를 확인합니다.
    /// </summary>
    public bool HasEnemy() => currentEnemy != null;

    #endregion

    #region 초기화

    /// <summary>
    /// 등록된 적 캐릭터 및 핸드 매니저를 모두 초기화합니다.
    /// </summary>
    public void ClearEnemy()
    {
        currentEnemy = null;
        enemyHandManager = null;
    }

    /// <summary>
    /// 매니저 상태를 초기화합니다. (현재는 디버그 로그만 출력)
    /// </summary>
    public void Reset()
    {
        Debug.Log("[EnemyManager] Reset");
    }

    #endregion
}
}
