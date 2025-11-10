using System.Collections.Generic;

using Game.CombatSystem.Manager;

namespace Game.CoreSystem.Statistics
{
    /// <summary>
    /// 전투 스냅샷을 세션 누적 딕셔너리에 반영하는 순수 로직.
    /// </summary>
    public static class SessionAccumulator
    {
        public static void ApplyToSession(SessionStatisticsData session, CombatStatsSnapshot snapshot)
        {
            if (session == null || snapshot == null) return;

            // 스킬카드 생성
            if (snapshot.playerSkillCardSpawnByCardId != null)
            {
                foreach (var kv in snapshot.playerSkillCardSpawnByCardId)
                {
                    if (!session.skillCardSpawnCountByCardId.ContainsKey(kv.Key))
                        session.skillCardSpawnCountByCardId[kv.Key] = 0;
                    session.skillCardSpawnCountByCardId[kv.Key] += kv.Value;
                }
            }

            // 스킬카드 사용 (ID)
            if (snapshot.playerSkillUsageByCardId != null)
            {
                foreach (var kv in snapshot.playerSkillUsageByCardId)
                {
                    if (!session.skillCardUseCountByCardId.ContainsKey(kv.Key))
                        session.skillCardUseCountByCardId[kv.Key] = 0;
                    session.skillCardUseCountByCardId[kv.Key] += kv.Value;
                }
            }

            // 스킬 사용 (이름)
            if (snapshot.playerSkillUsageByName != null)
            {
                foreach (var kv in snapshot.playerSkillUsageByName)
                {
                    if (!session.skillUseCountByName.ContainsKey(kv.Key))
                        session.skillUseCountByName[kv.Key] = 0;
                    session.skillUseCountByName[kv.Key] += kv.Value;
                }
            }

            // 액티브 아이템 생성
            if (snapshot.activeItemSpawnByItemId != null)
            {
                foreach (var kv in snapshot.activeItemSpawnByItemId)
                {
                    if (!session.activeItemSpawnCountByItemId.ContainsKey(kv.Key))
                        session.activeItemSpawnCountByItemId[kv.Key] = 0;
                    session.activeItemSpawnCountByItemId[kv.Key] += kv.Value;
                }
            }

            // 액티브 아이템 사용 (이름)
            if (snapshot.activeItemUsageByName != null)
            {
                foreach (var kv in snapshot.activeItemUsageByName)
                {
                    if (!session.activeItemUseCountByName.ContainsKey(kv.Key))
                        session.activeItemUseCountByName[kv.Key] = 0;
                    session.activeItemUseCountByName[kv.Key] += kv.Value;
                }
            }

            // 액티브 아이템 버리기 (ID)
            if (snapshot.activeItemDiscardByItemId != null)
            {
                foreach (var kv in snapshot.activeItemDiscardByItemId)
                {
                    if (!session.activeItemDiscardCountByItemId.ContainsKey(kv.Key))
                        session.activeItemDiscardCountByItemId[kv.Key] = 0;
                    session.activeItemDiscardCountByItemId[kv.Key] += kv.Value;
                }
            }

            // 패시브 아이템 획득 (ID)
            if (snapshot.passiveItemAcquiredByItemId != null)
            {
                foreach (var kv in snapshot.passiveItemAcquiredByItemId)
                {
                    if (!session.passiveItemAcquiredCountByItemId.ContainsKey(kv.Key))
                        session.passiveItemAcquiredCountByItemId[kv.Key] = 0;
                    session.passiveItemAcquiredCountByItemId[kv.Key] += kv.Value;
                }
            }

            // 자원 합계
            session.totalResourceGained += snapshot.totalResourceGained;
            session.totalResourceSpent += snapshot.totalResourceSpent;
        }
    }
}

