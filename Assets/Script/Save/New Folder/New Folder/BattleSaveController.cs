using UnityEngine;
using System;
using System.Reflection;
using Game.Save;
using Game.CharacterSystem.Core; // CharacterBase, EnemyCharacter

/// <summary>
/// 플레이어 HP + 적 상태(HP/사망)만 스냅샷/적용하는 컨트롤러(호환 버전)
/// </summary>
public class BattleSaveController : MonoBehaviour
{
    [Header("References")]
    public CharacterBase player;
    public EnemyCharacter[] enemies;

    // ---------- HP 읽기/쓰기 유틸(리플렉션) ----------
    int GetHP(object obj)
    {
        if (obj == null) return 0;
        var t = obj.GetType();

        var mGetCurrent = t.GetMethod("GetCurrentHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (mGetCurrent != null && mGetCurrent.GetParameters().Length == 0)
        { try { return Convert.ToInt32(mGetCurrent.Invoke(obj, null)); } catch { } }

        var pCurrent = t.GetProperty("CurrentHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (pCurrent != null && pCurrent.CanRead)
        { try { return Convert.ToInt32(pCurrent.GetValue(obj)); } catch { } }

        var mGetHP = t.GetMethod("GetHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (mGetHP != null && mGetHP.GetParameters().Length == 0)
        { try { return Convert.ToInt32(mGetHP.Invoke(obj, null)); } catch { } }

        var fCurrent = t.GetField("currentHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fCurrent != null)
        { try { return Convert.ToInt32(fCurrent.GetValue(obj)); } catch { } }

        Debug.LogWarning($"[BattleSaveController] HP 읽기 실패: {t.Name}");
        return 0;
    }

    void SetHP(object obj, int value)
    {
        if (obj == null) return;
        var t = obj.GetType();

        var mSetCurrent = t.GetMethod("SetCurrentHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null);
        if (mSetCurrent != null)
        { try { mSetCurrent.Invoke(obj, new object[] { value }); return; } catch { } }

        var pCurrent = t.GetProperty("CurrentHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (pCurrent != null && pCurrent.CanWrite)
        { try { pCurrent.SetValue(obj, value); return; } catch { } }

        // 마지막 수단: Heal/TakeDamage로 보정
        int cur = GetHP(obj);
        int diff = value - cur;
        if (diff != 0)
        {
            if (diff > 0)
            {
                var mHeal = t.GetMethod("Heal", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null);
                if (mHeal != null) { try { mHeal.Invoke(obj, new object[] { diff }); return; } catch { } }
            }
            else
            {
                var mDmg = t.GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null);
                if (mDmg != null) { try { mDmg.Invoke(obj, new object[] { -diff }); return; } catch { } }
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
        try { enemy.MarkAsDead(); return; } catch { }
        var t = enemy.GetType();
        var m = t.GetMethod("MarkAsDead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (m != null) { try { m.Invoke(enemy, null); return; } catch { } }
        Debug.LogWarning($"[BattleSaveController] MarkAsDead 호출 실패: {t.Name}");
    }
    // --------------------------------------------------

    /// <summary>현재 전투 상태 스냅샷</summary>
    public BattleSaveData Capture()
    {
        if (player == null)
        {
            Debug.LogError("[BattleSaveController] player 비어있음");
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
                data.enemies[i] = new EnemyState { enemyId = "NULL", currentHp = 0, isDead = true };
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

    /// <summary>저장 데이터 적용</summary>
    public void Apply(BattleSaveData data)
    {
        if (data == null) { Debug.LogWarning("[BattleSaveController] 적용할 데이터 없음"); return; }

        if (player != null) SetHP(player, data.playerHp);

        if (enemies != null && enemies.Length > 0 && data.enemies != null)
        {
            int count = Mathf.Min(enemies.Length, data.enemies.Length);
            for (int i = 0; i < count; i++)
            {
                var enemy = enemies[i];
                var es = data.enemies[i];
                if (enemy == null || es == null) continue;

                SetHP(enemy, es.currentHp);
                if (es.isDead && !GetIsDead(enemy)) ForceMarkDead(enemy);
            }
        }

        Debug.Log("[BattleSaveController] 저장 데이터 적용 완료");
    }
}
