#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using Game.ItemSystem.Data.Reward;

namespace Game.ItemSystem.Editor
{
	public static class RewardValidationEditor
	{
		[MenuItem("Tools/ItemSystem/Validate Reward Assets")]
		public static void ValidateRewards()
		{
			var pools = AssetDatabase.FindAssets("t:RewardPool").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<RewardPool>).ToArray();
			var players = AssetDatabase.FindAssets("t:PlayerRewardProfile").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<PlayerRewardProfile>).ToArray();
			var enemies = AssetDatabase.FindAssets("t:EnemyRewardConfig").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<EnemyRewardConfig>).ToArray();
			var profiles = AssetDatabase.FindAssets("t:RewardProfile").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<RewardProfile>).ToArray();

			int issues = 0;
			// Pool checks
			foreach (var p in pools)
			{
				if (p == null) continue;
				for (int i = 0; i < p.entries.Count; i++)
				{
					var e = p.entries[i];
					if (e.item == null) { Debug.LogWarning($"[RewardValidate] Pool {p.name} entry {i} item=null"); issues++; }
					if (e.weight <= 0) { Debug.LogWarning($"[RewardValidate] Pool {p.name} entry {i} weight<=0"); issues++; }
					if (e.minStage > e.maxStage) { Debug.LogWarning($"[RewardValidate] Pool {p.name} entry {i} stage range invalid"); issues++; }
				}
			}

			// Player profile checks
			foreach (var prof in players)
			{
				if (prof == null) continue;
				var dupTags = prof.tagWeightAdjust.GroupBy(t => t.tag).Where(g => g.Count() > 1).ToArray();
				if (dupTags.Length > 0) { Debug.LogWarning($"[RewardValidate] PlayerProfile {prof.name} duplicate tag weights: {string.Join(",", dupTags.Select(d=>d.Key))}"); issues++; }
			}

			// Enemy config checks
			foreach (var ec in enemies)
			{
				if (ec == null) continue;
				if (ec.activeCount < 0 || ec.passiveCount < 0) { Debug.LogWarning($"[RewardValidate] EnemyConfig {ec.name} negative counts"); issues++; }
			}

			Debug.Log(issues == 0 ? "[RewardValidate] OK (no issues)" : $"[RewardValidate] Issues found: {issues}");
		}
	}
}
#endif
