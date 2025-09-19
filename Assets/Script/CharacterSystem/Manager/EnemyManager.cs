using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.IManager;
using UnityEngine;
using Game.CharacterSystem.UI;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 전투 중 등장한 적 캐릭터를 관리하는 매니저 클래스입니다.
    /// 현재 적 캐릭터와 관련된 상태를 보관하거나 초기화합니다.
    /// 적 카드는 핸드 없이 대기 슬롯에서 직접 관리됩니다.
    /// </summary>
    public class EnemyManager : MonoBehaviour, IEnemyManager
{
    private IEnemyCharacter currentEnemy;

    #region 등록 / 설정

    /// <summary>
    /// 적 캐릭터를 등록합니다.
    /// </summary>
    /// <param name="enemy">등록할 적 캐릭터</param>
    public void RegisterEnemy(IEnemyCharacter enemy)
    {
        currentEnemy = enemy;

            // 적 UI 컨트롤러 자동 연결(있을 때만)
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
    /// 적 캐릭터 등록을 해제합니다.
    /// </summary>
    public void UnregisterEnemy()
    {
        currentEnemy = null;
        Debug.Log("[EnemyManager] 적 등록 해제");
    }

    // 적 핸드 매니저 관련 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

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

    // 적 핸드 매니저 조회 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

    /// <summary>
    /// 적 캐릭터가 등록되어 있는지 여부를 확인합니다.
    /// </summary>
    public bool HasEnemy() => currentEnemy != null;

    #endregion

    #region 초기화

    /// <summary>
    /// 등록된 적 캐릭터를 초기화합니다.
    /// </summary>
    public void ClearEnemy()
    {
        currentEnemy = null;
        Debug.Log("[EnemyManager] 적 캐릭터 초기화 완료");
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
