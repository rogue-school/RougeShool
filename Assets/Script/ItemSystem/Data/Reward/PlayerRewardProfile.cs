using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.ItemSystem.Data.Reward
{
	[CreateAssetMenu(fileName = "PlayerRewardProfile", menuName = "ItemSystem/Reward/PlayerRewardProfile")]
	public class PlayerRewardProfile : ScriptableObject
	{
		[Header("허용/금지 태그")]
		public string[] allowedTags;
		public string[] bannedTags;

		[Header("허용/금지 아이템(선택)")]
		public ItemDefinition[] allowedItems;
		public ItemDefinition[] bannedItems;

		[Serializable]
		public class TagWeight
		{
			public string tag;
			public float multiplier = 1f;
		}
		[Header("태그 가중치 보정")]
		public List<TagWeight> tagWeightAdjust = new List<TagWeight>();

		[Header("패시브 규칙")]
		public string[] allowedPassiveTags;
		public string[] bannedPassiveTags;
	}
}
