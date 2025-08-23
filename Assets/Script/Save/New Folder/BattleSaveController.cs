using UnityEngine;
using Game.Save;

// 외부 타입들 (프로젝트에 이미 존재한다고 가정)
using Game.CharacterSystem.Core; // CharacterBase, EnemyCharacter

/// <summary>
/// 배틀 씬에서 플레이어 HP와 적 상태를 저장/복원하는 컨트롤러
/// </summary>
public class BattleSaveController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("플레이어(또는 플레이어 캐릭터) - CharacterBase 계열")]
    public CharacterBase player;

    [Tooltip("현재 전투에 존재하는 적들 (씬에 존재하는 EnemyCharacter들을 순서대로 할당)")]
    public EnemyCharacter[] enemies;

    /// <summary>
    /// 현재 씬의 상태를 BattleSaveData로 스냅샷
    /// </summary>
    public BattleSaveData Capture()
    {
        if (player == null)
        {
            Debug.LogError("[BattleSaveController] player가 비어있습니다.");
            return null;
        }

        var data = new BattleSaveData
        {
            playerHp = Mathf.Max(0, player.GetCurrentHP()),
            enemies = new EnemyState[enemies != null ? enemies.Length : 0]
        };

        for (int i = 0; i < data.enemies.Length; i++)
        {
            var e = enemies[i];
            if (e == null)
            {
                data.enemies[i] = new EnemyState
                {
                    enemyId = "NULL",
                    currentHp = 0,
                    isDead = true
                };
                continue;
            }

            data.enemies[i] = new EnemyState
            {
                // ScriptableObject의 name을 ID로 사용 (고유 프리팹 식별)
                enemyId = e.Data != null ? e.Data.name : "Unknown",
                currentHp = Mathf.Max(0, e.GetCurrentHP()),
                isDead = e.IsMarkedDead
            };
        }

        return data;
    }

    /// <summary>
    /// 저장 데이터를 현재 씬 오브젝트에 적용 (플레이어 HP / 적 HP & 사망 여부)
    /// </summary>
    public void Apply(BattleSaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[BattleSaveController] 적용할 데이터가 없습니다.");
            return;
        }

        if (player != null)
        {
            // 플레이어 HP 복원
            player.SetCurrentHP(data.playerHp);
        }

        if (enemies == null || enemies.Length == 0) return;
        if (data.enemies == null) return;

        int count = Mathf.Min(enemies.Length, data.enemies.Length);

        // 간단히 "인덱스 매칭"으로 적용
        for (int i = 0; i < count; i++)
        {
            var enemy = enemies[i];
            var es = data.enemies[i];

            if (enemy == null || es == null) continue;

            // 적 HP 복원
            enemy.SetCurrentHP(es.currentHp);

            // 사망 상태 복원
            if (es.isDead && !enemy.IsMarkedDead)
            {
                enemy.MarkAsDead();
            }
        }

        Debug.Log("[BattleSaveController] 저장 데이터 적용 완료");
    }
}
