using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.ItemSystem.Data.Reward
{
	/// <summary>
	/// 플레이어별 보상 프로필
	/// EnemyRewardConfig의 보상 풀에서 금지할 아이템만 선택 가능
	/// </summary>
	[CreateAssetMenu(fileName = "PlayerRewardProfile", menuName = "ItemSystem/Reward/PlayerRewardProfile")]
	public class PlayerRewardProfile : ScriptableObject
	{
		[Header("태그 기반 필터링")]
		[Tooltip("허용할 태그 목록 (빈 배열이면 모든 태그 허용)")]
		public string[] allowedTags = new string[0];
		
		[Tooltip("금지할 태그 목록")]
		public string[] bannedTags = new string[0];
		
		[Tooltip("태그별 가중치 조정")]
		public List<TagWeight> tagWeightAdjust = new List<TagWeight>();

		[Header("아이템 기반 필터링")]
		[Tooltip("EnemyRewardConfig의 액티브 풀에서 금지할 아이템들")]
		public ActiveItemDefinition[] bannedActiveItems = new ActiveItemDefinition[0];
		
		[Tooltip("EnemyRewardConfig의 패시브 풀에서 금지할 아이템들")]
		public PassiveItemDefinition[] bannedPassiveItems = new PassiveItemDefinition[0];

		[Header("패시브 아이템 태그 필터링")]
		[Tooltip("허용할 패시브 태그 목록")]
		public string[] allowedPassiveTags = new string[0];
		
		[Tooltip("금지할 패시브 태그 목록")]
		public string[] bannedPassiveTags = new string[0];

		[Serializable]
		public class TagWeight
		{
			public string tag;
			public float multiplier = 1f;
		}

		/// <summary>
		/// 액티브 아이템이 금지되었는지 확인
		/// </summary>
		public bool IsActiveItemBanned(ActiveItemDefinition item)
		{
			if (item == null) return true;
			
			foreach (var bannedItem in bannedActiveItems)
			{
				if (bannedItem != null && bannedItem == item)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 패시브 아이템이 금지되었는지 확인
		/// </summary>
		public bool IsPassiveItemBanned(PassiveItemDefinition item)
		{
			if (item == null) return true;
			
			foreach (var bannedItem in bannedPassiveItems)
			{
				if (bannedItem != null && bannedItem == item)
					return true;
			}
			return false;
		}
	}
}
