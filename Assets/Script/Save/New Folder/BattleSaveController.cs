using UnityEngine;
using System;
using System.Reflection;
using Game.Save;
using Game.CharacterSystem.Core; // CharacterBase, EnemyCharacter

/// <summary>
/// 배틀 씬에서 플레이어 HP와 적 상태를 저장/복원 (호환 버전)
/// 프로젝트마다 HP 접근 메서드 이름이 달라도 동작하게 리플렉션 사용
/// </summary>
public class BattleSaveController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("플레이어(또는 플레이어 캐릭터) - CharacterBase 계열")]
    public CharacterBase player;

    [Tooltip("현재 전투에 존재하는 적들 (씬에 존재하는 EnemyCharacter들을 순서대로 할당)")]
    public EnemyCharacter[] enemies;

    // ===== HP 읽기/쓰기 유틸 (리플렉션) =====

    int GetHP(object obj)
    {
        if (obj == null) return 0;
        var t = obj.GetType();

        // 1) GetCurrentHP()
        var mGetCurrent = t.GetMethod("GetCurrentHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (mGetCurrent != null && mGetCurrent.GetParameters().Length == 0)
        {
            try { return Convert.ToInt32(mGetCurrent.Invoke(obj, null)); }
            catch { }
        }

        // 2) CurrentHP 프로퍼티
        var pCurrent = t.GetProperty("CurrentHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (pCurrent != null && pCurrent.CanRead)
        {
            try { return Convert.ToInt32(pCurrent.GetValue(obj)); }
            catch { }
        }

        // 3) GetHP()
        var mGetHP = t.GetMethod("GetHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (mGetHP != null && mGetHP.GetParameters().Length == 0)
        {
            try { return Convert.ToInt32(mGetHP.Invoke(obj, null)); }
            catch { }
        }

        // 4) currentHP 필드
        var fCurrent = t.GetField("currentHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fCurrent != null)
        {
            try { return Convert.ToInt32(fCurrent.GetValue(obj)); }
            catch { }
        }

        Debug.LogWarning($"[BattleSaveController] HP 읽기 실패: {t.Name}");
        return 0;
    }

    void SetHP(object obj, int value)
    {
        if (obj == null) return;
        var t = obj.GetType();

        // 1) SetCurrentHP(int)
        var mSetCurrent = t.GetMethod("SetCurrentHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null);
        if (mSetCurrent != null)
        {
            try { mSetCurrent.Invoke(obj, new object[] { value }); return; }
            catch { }
        }

        // 2) CurrentHP 프로퍼티 set
        var pCurrent = t.GetProperty("CurrentHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (pCurrent != null && pCurrent.CanWrite)
        {
            try { pCurrent.SetValue(obj, value); return; }
            catch { }
        }

        // 3) 마지막 수단: Heal/TakeDamage로 보정
        int cur = GetHP(obj);
        int diff = value - cur;

        if (diff != 0)
        {
            if (diff > 0)
            {
                // Heal(int)
                var mHeal = t.GetMethod("Heal", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null);
                if (mHeal != null)
                {
                    try { mHeal.Invoke(obj, new object[] { diff }); return; }
                    catch { }
                }
            }
            else
            {
                // TakeDamage(int)
                var mDmg = t.GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null);
                if (mDmg != null)
                {
                    try { mDmg.Invoke(obj, new object[] { -diff }); return; }
                    catch { }
                }
            }
        }

        Debug.LogWarning($"[BattleSaveController] HP 쓰기 실패: {t.Name}");
    }

    bool GetIsDead(EnemyCharacter enemy)
    {
        try { return enemy != null && enemy.IsMarkedDead; }
        catch { return false; }
    }

    void ForceMarkDead(EnemyCharacter enemy)
    {
        if (enemy == null) return;

        // 우선 공개 메서드 시도
        try { enemy.MarkAsDead(); return; } catch { }

        // 혹시 비공개일 경우 리플렉션으로도 한 번 더 시도
        var t = enemy.GetType();
        var m = t.GetMethod("MarkAsDead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (m != null)
        {
            try { m.Invoke(enemy, null); return; } catch { }
        }

        Debug.LogWarning($"[BattleSaveController] MarkAsDead 호출 실패: {t.Name}");
    }

    // ===== 저장/불러오기 진입점 =====

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
            playerHp = Mathf.Max(0, GetHP(player)),
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
                enemyId = e.Data != null ? e.Data.name : "Unknown",
                currentHp = Mathf.Max(0, GetHP(e)),
                isDead = GetIsDead(e)
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
            SetHP(player, data.playerHp);
        }

        if (enemies == null || enemies.Length == 0) return;
        if (data.enemies == null) return;

        int count = Mathf.Min(enemies.Length, data.enemies.Length);

        // 인덱스 매칭 적용
        for (int i = 0; i < count; i++)
        {
            var enemy = enemies[i];
            var es = data.enemies[i];
            if (enemy == null || es == null) continue;

            SetHP(enemy, es.currentHp);

            if (es.isDead && !GetIsDead(enemy))
            {
                ForceMarkDead(enemy);
            }
        }

        Debug.Log("[BattleSaveController] 저장 데이터 적용 완료 (compat)");
    }
}
