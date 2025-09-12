using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Game.UtilitySystem.Editor
{
	internal static class AnimTypeCache
	{
		private static Dictionary<Type, List<Type>> cache = new();
		public static List<Type> GetImplementations(Type iface)
		{
			if (cache.TryGetValue(iface, out var list)) return list;
			var found = TypeCache.GetTypesDerivedFrom(iface)
				.Where(t => !t.IsAbstract && !t.IsInterface)
				.OrderBy(t => t.Namespace).ThenBy(t => t.Name).ToList();
			cache[iface] = found; return found;
		}
	}

	[CustomEditor(typeof(Game.AnimationSystem.Data.PlayerCharacterAnimationDatabase))]
	[CanEditMultipleObjects]
	public class PlayerCharacterAnimationDatabaseProEditor : BaseAnimDbEditor
	{
		protected override SerializedProperty GetListProp(SerializedObject so) => so.FindProperty("characterAnimations");
		protected override string Header => "플레이어 캐릭터 애니메이션";
	}

	[CustomEditor(typeof(Game.AnimationSystem.Data.EnemyCharacterAnimationDatabase))]
	[CanEditMultipleObjects]
	public class EnemyCharacterAnimationDatabaseProEditor : BaseAnimDbEditor
	{
		protected override SerializedProperty GetListProp(SerializedObject so) => so.FindProperty("characterAnimations");
		protected override string Header => "적 캐릭터 애니메이션";
	}

	[CustomEditor(typeof(Game.AnimationSystem.Data.PlayerSkillCardAnimationDatabase))]
	[CanEditMultipleObjects]
	public class PlayerSkillCardAnimationDatabaseProEditor : BaseAnimDbEditor
	{
		protected override SerializedProperty GetListProp(SerializedObject so) => so.FindProperty("skillCardAnimations");
		protected override string Header => "플레이어 스킬카드 애니메이션";
	}

	[CustomEditor(typeof(Game.AnimationSystem.Data.EnemySkillCardAnimationDatabase))]
	[CanEditMultipleObjects]
	public class EnemySkillCardAnimationDatabaseProEditor : BaseAnimDbEditor
	{
		protected override SerializedProperty GetListProp(SerializedObject so) => so.FindProperty("skillCardAnimations");
		protected override string Header => "적 스킬카드 애니메이션";
	}

	public abstract class BaseAnimDbEditor : UnityEditor.Editor
	{
		protected abstract SerializedProperty GetListProp(SerializedObject so);
		protected abstract string Header { get; }
		private ReorderableList list;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			if (list == null)
			{
				list = new ReorderableList(serializedObject, GetListProp(serializedObject), true, true, true, true);
				list.draggable = false; // 드래그로 인한 클릭 인터셉트 방지
				list.drawHeaderCallback = r => EditorGUI.LabelField(r, Header + " 매핑");
				list.elementHeightCallback = i => GetEntryHeight(GetListProp(serializedObject).GetArrayElementAtIndex(i)) + 6f;
				list.drawElementCallback = (rect, index, active, focused) =>
				{
					var entry = GetListProp(serializedObject).GetArrayElementAtIndex(index);
					rect.y += 2f;
					DrawEntry(rect, entry);
				};
			}

			// 전역/폴백 제거: 상단 섹션 숨김
			list.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawEntry(Rect rect, SerializedProperty entry)
		{
			float line = EditorGUIUtility.singleLineHeight;
			float y = rect.y;
			Rect r = new Rect(rect.x, y, rect.width, line);

			// 대상 SO 필드(존재하는 것만)
			DrawObjectField(ref r, entry, "playerCharacter", typeof(UnityEngine.Object));
			DrawObjectField(ref r, entry, "enemyCharacter", typeof(UnityEngine.Object));
			DrawObjectField(ref r, entry, "playerSkillCard", typeof(UnityEngine.Object));
			DrawObjectField(ref r, entry, "enemySkillCard", typeof(UnityEngine.Object));

			bool isSkillCard = entry.FindPropertyRelative("playerSkillCard") != null || entry.FindPropertyRelative("enemySkillCard") != null;

			// 슬롯별 설정(있을 때만 그리기)
			DrawSettings(ref r, entry.FindPropertyRelative("spawnAnimation"), isSkillCard ? typeof(Game.AnimationSystem.Interface.ISkillCardSpawnAnimationScript) : typeof(Game.AnimationSystem.Interface.ICharacterSpawnAnimationScript), "Spawn Animation");
			DrawSettings(ref r, entry.FindPropertyRelative("deathAnimation"), isSkillCard ? typeof(Game.AnimationSystem.Interface.ISkillCardDeathAnimationScript) : typeof(Game.AnimationSystem.Interface.ICharacterDeathAnimationScript), "Death Animation");
			DrawSettings(ref r, entry.FindPropertyRelative("moveAnimation"), typeof(Game.AnimationSystem.Interface.ISkillCardMoveAnimationScript), "Move Animation");
			DrawSettings(ref r, entry.FindPropertyRelative("moveToCombatSlotAnimation"), typeof(Game.AnimationSystem.Interface.ISkillCardCombatSlotMoveAnimationScript), "Move To Combat Slot Animation");
			DrawSettings(ref r, entry.FindPropertyRelative("dragAnimation"), typeof(Game.AnimationSystem.Interface.ISkillCardDragAnimationScript), "Drag Animation");
			DrawSettings(ref r, entry.FindPropertyRelative("dropAnimation"), typeof(Game.AnimationSystem.Interface.ISkillCardDropAnimationScript), "Drop Animation");
			DrawSettings(ref r, entry.FindPropertyRelative("useAnimation"), typeof(Game.AnimationSystem.Interface.ISkillCardUseAnimationScript), "Use Animation");
			DrawSettings(ref r, entry.FindPropertyRelative("vanishAnimation"), typeof(Game.AnimationSystem.Interface.ISkillCardVanishAnimationScript), "Vanish Animation");
		}

		private void DrawIfExists(ref Rect r, SerializedProperty root, string child)
		{
			var p = root.FindPropertyRelative(child);
			if (p == null) return;
			EditorGUI.PropertyField(r, p, true);
			r.y += EditorGUI.GetPropertyHeight(p, true) + 4f;
		}

		private void DrawObjectField(ref Rect r, SerializedProperty root, string child, Type objectType)
		{
			var p = root.FindPropertyRelative(child);
			if (p == null) return;
			EditorGUI.BeginDisabledGroup(false);
			EditorGUI.ObjectField(r, p, objectType);
			EditorGUI.EndDisabledGroup();
			r.y += EditorGUIUtility.singleLineHeight + 6f;
		}

		private void DrawSettings(ref Rect r, SerializedProperty settings, Type iface, string label)
		{
			if (settings == null) return;
			EditorGUI.LabelField(r, label, EditorStyles.boldLabel);
			r.y += EditorGUIUtility.singleLineHeight + 2f;

			var typeProp = settings.FindPropertyRelative("animationScriptType");
			string current = typeProp != null ? typeProp.stringValue : string.Empty;
			var candidates = AnimTypeCache.GetImplementations(iface);
			if (candidates.Count == 0)
			{
				EditorGUI.LabelField(new Rect(r.x, r.y, r.width, EditorGUIUtility.singleLineHeight), "No scripts found for this slot.");
				r.y += EditorGUIUtility.singleLineHeight + 2f;
			}
			else
			{
				var display = candidates.Select(t => string.IsNullOrEmpty(t.Namespace) ? t.Name : ($"{t.Namespace}.{t.Name}")).ToArray();
				int index = 0;
				if (!string.IsNullOrEmpty(current))
				{
					int found = candidates.FindIndex(t => t.FullName == current);
					if (found >= 0) index = found;
				}
				index = EditorGUI.Popup(new Rect(r.x, r.y, r.width, EditorGUIUtility.singleLineHeight), index, display);
				r.y += EditorGUIUtility.singleLineHeight + 2f;
				if (typeProp != null)
				{
					typeProp.stringValue = candidates[index].FullName;
				}
			}

			// 파라미터
			DrawChildIfExists(ref r, settings, "duration");
			DrawChildIfExists(ref r, settings, "useEasing");
			DrawChildIfExists(ref r, settings, "customCurve");
			DrawChildIfExists(ref r, settings, "offset");
			DrawChildIfExists(ref r, settings, "scale");
		}

		private void DrawChildIfExists(ref Rect r, SerializedProperty root, string child)
		{
			var p = root.FindPropertyRelative(child);
			if (p == null) return;
			EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, EditorGUIUtility.singleLineHeight), p);
			r.y += EditorGUI.GetPropertyHeight(p) + 2f;
		}

		private void DrawGlobalDefaults()
		{
			EditorGUILayout.LabelField("Global Defaults", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("전역 기본값은 엔트리에서 비워진 슬롯에 적용됩니다.", MessageType.Info);
		}


		private float GetEntryHeight(SerializedProperty entry)
		{
			float h = 0f;
			float line = EditorGUIUtility.singleLineHeight;
			float pad = 6f;
			// 대상 SO 필드들
			Add(ref h, entry.FindPropertyRelative("playerCharacter"));
			Add(ref h, entry.FindPropertyRelative("enemyCharacter"));
			Add(ref h, entry.FindPropertyRelative("playerSkillCard"));
			Add(ref h, entry.FindPropertyRelative("enemySkillCard"));

			bool isSkillCard = entry.FindPropertyRelative("playerSkillCard") != null || entry.FindPropertyRelative("enemySkillCard") != null;
			// 슬롯별 설정 높이(레이블 + 드롭다운 + 파라미터들)
			h += GetSettingsHeight(entry.FindPropertyRelative("spawnAnimation"));
			h += GetSettingsHeight(entry.FindPropertyRelative("deathAnimation"));
			if (isSkillCard)
			{
				h += GetSettingsHeight(entry.FindPropertyRelative("moveAnimation"));
				h += GetSettingsHeight(entry.FindPropertyRelative("moveToCombatSlotAnimation"));
				h += GetSettingsHeight(entry.FindPropertyRelative("dragAnimation"));
				h += GetSettingsHeight(entry.FindPropertyRelative("dropAnimation"));
				h += GetSettingsHeight(entry.FindPropertyRelative("useAnimation"));
				h += GetSettingsHeight(entry.FindPropertyRelative("vanishAnimation"));
			}
			return Mathf.Max(h + pad, line + pad);
			
			void Add(ref float acc, SerializedProperty p)
			{
				if (p == null) return;
				acc += EditorGUI.GetPropertyHeight(p, true) + pad;
			}
		}

		private float GetSettingsHeight(SerializedProperty settings)
		{
			if (settings == null) return 0f;
			float line = EditorGUIUtility.singleLineHeight;
			float pad = 4f;
			float h = line /*label*/ + line /*popup*/ + pad;
			Add(ref h, settings.FindPropertyRelative("duration"));
			Add(ref h, settings.FindPropertyRelative("useEasing"));
			Add(ref h, settings.FindPropertyRelative("customCurve"));
			Add(ref h, settings.FindPropertyRelative("offset"));
			Add(ref h, settings.FindPropertyRelative("scale"));
			return h + pad;
			void Add(ref float acc, SerializedProperty p)
			{
				if (p == null) return;
				acc += EditorGUI.GetPropertyHeight(p, true) + pad;
			}
		}
	}
}


