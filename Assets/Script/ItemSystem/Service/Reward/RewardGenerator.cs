using System;
using System.Collections.Generic;
using Game.ItemSystem.Data;
using Game.ItemSystem.Data.Reward;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Service.Reward
{
	public class RewardGenerator : IRewardGenerator
	{
		public ActiveItemDefinition[] GenerateActive(EnemyRewardConfig enemy, PlayerRewardProfile player, RewardProfile profile, int stageIndex, int runSeed)
		{
			var pool = MergePools(enemy?.activePools);
			var candidates = FilterByStage(pool, stageIndex);
			candidates = FilterByPlayer(candidates, player, passive:false);
			AdjustWeightsByTags(candidates, player);
			return SampleActive(candidates, Math.Max(0, enemy?.activeCount ?? 0), profile, stageIndex, runSeed);
		}

		public PassiveItemDefinition[] GeneratePassive(EnemyRewardConfig enemy, PlayerRewardProfile player, RewardProfile profile, int stageIndex, int runSeed)
		{
			var pool = MergePools(enemy?.passivePools);
			var candidates = FilterByStage(pool, stageIndex);
			candidates = FilterByPlayer(candidates, player, passive:true);
			AdjustWeightsByTags(candidates, player);
			return SamplePassive(candidates, Math.Max(0, enemy?.passiveCount ?? 0), profile, stageIndex, runSeed);
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
				// 아이템 블랙리스트
				if (prof.bannedItems != null && Array.Exists(prof.bannedItems, i => i == e.item)) continue;
				// 아이템 화이트리스트 체크(있으면 필수)
				if (prof.allowedItems != null && prof.allowedItems.Length > 0 && !Array.Exists(prof.allowedItems, i => i == e.item)) continue;
				// 태그 필터
				if (!PassTagFilters(e.tags, prof.allowedTags, prof.bannedTags)) continue;
				// 패시브 전용 필터
				if (passive && !PassTagFilters(e.tags, prof.allowedPassiveTags, prof.bannedPassiveTags)) continue;
				dst.Add(e);
			}
			return dst;
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

		private ActiveItemDefinition[] SampleActive(List<RewardPool.WeightedEntry> list, int count, RewardProfile profile, int stage, int runSeed)
		{
			var rng = new System.Random(BuildSeed(profile, stage, runSeed, 1));
			var picked = new List<ActiveItemDefinition>();
			var used = new HashSet<ItemDefinition>();
			for (int i = 0; i < count; i++)
			{
				var e = WeightedPick(list, rng, used, profile);
				if (e == null || e.item is not ActiveItemDefinition a) break;
				picked.Add(a);
				if (profile.duplicatePolicy == RewardDuplicatePolicy.RerollOnDuplicate || e.uniquePerRun)
				{
					used.Add(e.item);
				}
			}
			return picked.ToArray();
		}

		private PassiveItemDefinition[] SamplePassive(List<RewardPool.WeightedEntry> list, int count, RewardProfile profile, int stage, int runSeed)
		{
			var rng = new System.Random(BuildSeed(profile, stage, runSeed, 2));
			var picked = new List<PassiveItemDefinition>();
			var used = new HashSet<ItemDefinition>();
			for (int i = 0; i < count; i++)
			{
				var e = WeightedPick(list, rng, used, profile);
				if (e == null || e.item is not PassiveItemDefinition p) break;
				picked.Add(p);
				if (profile.duplicatePolicy == RewardDuplicatePolicy.RerollOnDuplicate || e.uniquePerRun)
				{
					used.Add(e.item);
				}
			}
			return picked.ToArray();
		}

		private RewardPool.WeightedEntry WeightedPick(List<RewardPool.WeightedEntry> list, System.Random rng, HashSet<ItemDefinition> used, RewardProfile profile)
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

		private int BuildSeed(RewardProfile profile, int stage, int runSeed, int salt)
		{
			return profile.seedPolicy switch
			{
				RewardSeedPolicy.PerRun => HashCombine(runSeed, salt),
				RewardSeedPolicy.PerStage => HashCombine(runSeed, stage, salt),
				RewardSeedPolicy.PerEnemy => HashCombine(runSeed, stage, Environment.TickCount, salt),
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
	}
}
