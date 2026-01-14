using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.ItemSystem.Data.Reward
{
	[CreateAssetMenu(fileName = "RewardPool", menuName = "ItemSystem/Reward/RewardPool")]
	public class RewardPool : ScriptableObject
	{
		[Serializable]
		public class WeightedEntry
		{
			public ItemDefinition item;
			[Min(0)] public int weight = 1;
			public string[] tags;
			public int minStage = 0;
			public int maxStage = 9999;
			public bool uniquePerRun = false;
		}

		[Header("보상 후보 목록")]
		public List<WeightedEntry> entries = new List<WeightedEntry>();
	}
}
