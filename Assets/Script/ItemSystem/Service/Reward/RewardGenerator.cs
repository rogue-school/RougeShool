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
	public ActiveItemDefinition[] GenerateActive(EnemyRewardConfig enemy, PlayerRewardProfile player, int stageIndex, int runSeed)
	{
		// 기본값 처리로 설정 누락 방지
		if (enemy == null)
		{
			GameLogger.LogWarning("[RewardGenerator] EnemyRewardConfig가 null입니다. 기본 보상을 생성합니다.", GameLogger.LogCategory.UI);
			return GenerateDefaultActiveReward();
		}

		if (player == null)
		{
			GameLogger.LogInfo("[RewardGenerator] PlayerRewardProfile이 null입니다. 필터링 없이 진행합니다.", GameLogger.LogCategory.UI);
		}

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

		/// <summary>
		/// 설정이 누락된 경우 사용할 기본 액티브 보상을 생성합니다.
		/// </summary>
		private ActiveItemDefinition[] GenerateDefaultActiveReward()
		{
			// Resources 폴더에서 모든 액티브 아이템을 찾아서 랜덤 선택
			var allActiveItems = Resources.LoadAll<ActiveItemDefinition>("Data/Item");
			
			if (allActiveItems.Length == 0)
			{
				GameLogger.LogError("[RewardGenerator] Resources에서 액티브 아이템을 찾을 수 없습니다.", GameLogger.LogCategory.UI);
				return new ActiveItemDefinition[0];
			}

			// 기본적으로 3개 아이템 제공
			var selectedCount = Math.Min(3, allActiveItems.Length);
			var selected = new List<ActiveItemDefinition>();
			
			for (int i = 0; i < selectedCount; i++)
			{
				var randomIndex = UnityEngine.Random.Range(0, allActiveItems.Length);
				selected.Add(allActiveItems[randomIndex]);
			}

			GameLogger.LogInfo($"[RewardGenerator] 기본 액티브 보상 생성: {selected.Count}개", GameLogger.LogCategory.UI);
			return selected.ToArray();
		}

		/// <summary>
		/// 설정이 누락된 경우 사용할 기본 패시브 보상을 생성합니다.
		/// </summary>
		private PassiveItemDefinition[] GenerateDefaultPassiveReward()
		{
			// Resources 폴더에서 모든 패시브 아이템을 찾아서 랜덤 선택
			var allPassiveItems = Resources.LoadAll<PassiveItemDefinition>("Data/Item");
			
			if (allPassiveItems.Length == 0)
			{
				GameLogger.LogInfo("[RewardGenerator] Resources에서 패시브 아이템을 찾을 수 없습니다. 빈 배열을 반환합니다.", GameLogger.LogCategory.UI);
				return new PassiveItemDefinition[0];
			}

			// 기본적으로 1개 아이템 제공
			var randomIndex = UnityEngine.Random.Range(0, allPassiveItems.Length);
			var selected = new PassiveItemDefinition[] { allPassiveItems[randomIndex] };

			GameLogger.LogInfo($"[RewardGenerator] 기본 패시브 보상 생성: {selected.Length}개", GameLogger.LogCategory.UI);
			return selected;
		}
	}
}
