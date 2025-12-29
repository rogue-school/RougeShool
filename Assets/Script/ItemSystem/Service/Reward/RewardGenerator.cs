using System;
using System.Collections.Generic;
using UnityEngine;
using Game.ItemSystem.Data;
using Game.ItemSystem.Data.Reward;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Service.Reward
{
    public class RewardGenerator : IRewardGenerator
    {
        // 진정한 랜덤을 위한 카운터 추가
        private static int _enemyCounter = 0;

        public ActiveItemDefinition[] GenerateActive(EnemyRewardConfig enemy, PlayerRewardProfile player, int stageIndex, int runSeed)
        {
            // 기본값 처리로 설정 누락 방지
            if (enemy == null)
            {
                GameLogger.LogWarning("[RewardGenerator] EnemyRewardConfig가 null입니다. 기본 보상을 생성합니다.", GameLogger.LogCategory.UI);
                return GenerateDefaultActiveReward();
            }

            // PlayerRewardProfile이 null이면 필터링 없이 진행

            var pool = MergePools(enemy.activePools);
            var candidates = FilterByStage(pool, stageIndex);
            candidates = FilterByPlayer(candidates, player, passive: false);
            AdjustWeightsByTags(candidates, player);
            return SampleActive(candidates, Math.Max(1, enemy.activeCount), stageIndex, runSeed);
        }

        public PassiveItemDefinition[] GeneratePassive(EnemyRewardConfig enemy, PlayerRewardProfile player, int stageIndex, int runSeed)
        {
            // 기본값 처리로 설정 누락 방지
            if (enemy == null)
            {
                GameLogger.LogWarning("[RewardGenerator] EnemyRewardConfig가 null입니다. 기본 패시브 보상을 생성합니다.", GameLogger.LogCategory.UI);
                return GenerateDefaultPassiveReward();
            }

            var pool = MergePools(enemy.passivePools);
            var candidates = FilterByStage(pool, stageIndex);
            candidates = FilterByPlayer(candidates, player, passive: true);
            AdjustWeightsByTags(candidates, player);
            return SamplePassive(candidates, Math.Max(0, enemy.passiveCount), stageIndex, runSeed);
        }

        private List<RewardPool.WeightedEntry> MergePools(RewardPool[] pools)
        {
            var list = new List<RewardPool.WeightedEntry>();
            if (pools == null) return list;
            foreach (var p in pools)
            {
                if (p == null || p.entries == null) continue;
                list.AddRange(p.entries);
            }
            return list;
        }

        private List<RewardPool.WeightedEntry> FilterByStage(List<RewardPool.WeightedEntry> src, int stage)
        {
            var dst = new List<RewardPool.WeightedEntry>();
            foreach (var e in src)
            {
                if (e.item == null) continue;
                if (stage < e.minStage || stage > e.maxStage) continue;
                dst.Add(e);
            }
            return dst;
        }

        private List<RewardPool.WeightedEntry> FilterByPlayer(List<RewardPool.WeightedEntry> src, PlayerRewardProfile prof, bool passive)
        {
            if (prof == null) return src;
            var dst = new List<RewardPool.WeightedEntry>();
            foreach (var e in src)
            {
                if (e.item == null) continue;

                // 아이템 타입별 금지 확인
                bool isBanned = false;
                if (passive && e.item is PassiveItemDefinition passiveItem)
                {
                    isBanned = prof.IsPassiveItemBanned(passiveItem);
                }
                else if (!passive && e.item is ActiveItemDefinition activeItem)
                {
                    isBanned = prof.IsActiveItemBanned(activeItem);
                }

                if (isBanned) continue;

                // 태그 필터
                if (!PassTagFilters(e.tags, prof.allowedTags, prof.bannedTags)) continue;
                // 패시브 전용 필터
                if (passive && !PassTagFilters(e.tags, prof.allowedPassiveTags, prof.bannedPassiveTags)) continue;
                dst.Add(e);
            }
            return dst.Count > 0 ? dst : src; // 필터링 결과가 비어있으면 원본 반환
        }

        private bool PassTagFilters(string[] itemTags, string[] allowed, string[] banned)
        {
            if (banned != null && itemTags != null)
                foreach (var t in itemTags)
                    if (Array.Exists(banned, b => b == t)) return false;
            if (allowed != null && allowed.Length > 0)
            {
                if (itemTags == null) return false;
                bool any = false;
                foreach (var t in itemTags)
                    if (Array.Exists(allowed, a => a == t)) { any = true; break; }
                return any;
            }
            return true;
        }

        private void AdjustWeightsByTags(List<RewardPool.WeightedEntry> list, PlayerRewardProfile prof)
        {
            if (prof?.tagWeightAdjust == null) return;
            foreach (var adj in prof.tagWeightAdjust)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    if (e.tags == null) continue;
                    if (Array.Exists(e.tags, t => t == adj.tag))
                    {
                        int newWeight = (int)System.MathF.Round(e.weight * System.MathF.Max(0f, adj.multiplier));
                        e.weight = System.Math.Max(0, newWeight);
                    }
                }
            }
        }

        private ActiveItemDefinition[] SampleActive(List<RewardPool.WeightedEntry> list, int count, int stage, int runSeed)
        {
            var rng = new System.Random(BuildSeed(DefaultRewardPolicy.SeedPolicy, stage, runSeed, 1));
            var picked = new List<ActiveItemDefinition>();
            var used = new HashSet<ItemDefinition>();
            for (int i = 0; i < count; i++)
            {
                var e = WeightedPick(list, rng, used, DefaultRewardPolicy.DuplicatePolicy);
                if (e == null || e.item is not ActiveItemDefinition a) break;
                picked.Add(a);
                if (DefaultRewardPolicy.DuplicatePolicy == RewardDuplicatePolicy.RerollOnDuplicate || e.uniquePerRun)
                {
                    used.Add(e.item);
                }
            }
            return picked.ToArray();
        }

        private PassiveItemDefinition[] SamplePassive(List<RewardPool.WeightedEntry> list, int count, int stage, int runSeed)
        {
            var rng = new System.Random(BuildSeed(DefaultRewardPolicy.SeedPolicy, stage, runSeed, 2));
            var picked = new List<PassiveItemDefinition>();
            var used = new HashSet<ItemDefinition>();
            for (int i = 0; i < count; i++)
            {
                var e = WeightedPick(list, rng, used, DefaultRewardPolicy.DuplicatePolicy);
                if (e == null || e.item is not PassiveItemDefinition p) break;
                picked.Add(p);
                if (DefaultRewardPolicy.DuplicatePolicy == RewardDuplicatePolicy.RerollOnDuplicate || e.uniquePerRun)
                {
                    used.Add(e.item);
                }
            }
            return picked.ToArray();
        }

        private RewardPool.WeightedEntry WeightedPick(List<RewardPool.WeightedEntry> list, System.Random rng, HashSet<ItemDefinition> used, RewardDuplicatePolicy duplicatePolicy)
        {
            int total = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e.weight <= 0) continue;
                if (used.Contains(e.item)) continue;
                total += e.weight;
            }
            if (total <= 0) return null;
            int roll = rng.Next(total);
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e.weight <= 0) continue;
                if (used.Contains(e.item)) continue;
                roll -= e.weight;
                if (roll < 0) return e;
            }
            return null;
        }

        private int BuildSeed(RewardSeedPolicy seedPolicy, int stage, int runSeed, int salt)
        {
            return seedPolicy switch
            {
                RewardSeedPolicy.PerRun => HashCombine(runSeed, salt),
                RewardSeedPolicy.PerStage => HashCombine(runSeed, stage, salt),
                // PerEnemy: 카운터 + TickCount + Guid로 완전한 랜덤 보장
                RewardSeedPolicy.PerEnemy => HashCombine(runSeed, stage, ++_enemyCounter, Environment.TickCount, Guid.NewGuid().GetHashCode(), salt),
                _ => HashCombine(runSeed, stage, salt)
            };
        }

        private int HashCombine(params int[] values)
        {
            unchecked
            {
                int h = 17;
                for (int i = 0; i < values.Length; i++) h = h * 31 + values[i];
                return h;
            }
        }

        /// <summary>
        /// 설정이 누락된 경우 사용할 기본 액티브 보상을 생성합니다.
        /// </summary>
        private ActiveItemDefinition[] GenerateDefaultActiveReward()
        {
            // DefaultRewardService를 사용하여 중복 코드 제거
            return Game.ItemSystem.Service.DefaultRewardService.GenerateDefaultActiveReward();
        }

        /// <summary>
        /// 설정이 누락된 경우 사용할 기본 패시브 보상을 생성합니다.
        /// </summary>
        private PassiveItemDefinition[] GenerateDefaultPassiveReward()
        {
            // DefaultRewardService를 사용하여 중복 코드 제거
            return Game.ItemSystem.Service.DefaultRewardService.GenerateDefaultPassiveReward();
        }
    }
}