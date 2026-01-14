using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.Collections.Generic;
using System.Linq;

namespace Game.Editor
{
    /// <summary>
    /// Addressables 리소스 자동 설정 도구
    /// 코드에서 사용하는 리소스를 자동으로 Addressable로 마크합니다
    /// </summary>
    public class AddressablesSetup : EditorWindow
    {
        [MenuItem("Tools/Addressables/🚀 모든 리소스 자동 설정 (통합)")]
        public static void SetupAllResources()
        {
            Debug.Log("=== Addressables 통합 자동 설정 시작 ===\n");
            
            // 파일 시스템 변경사항을 Unity 에디터에 반영
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings를 찾을 수 없습니다. Addressables 패키지가 설치되어 있는지 확인하세요.");
                return;
            }

            var defaultGroup = settings.DefaultGroup;
            if (defaultGroup == null)
            {
                Debug.LogError("Default Local Group을 찾을 수 없습니다.");
                return;
            }

            int totalSuccess = 0;
            int totalFail = 0;

            // 1. 필수 프리팹 및 이미지
            Debug.Log("--- 1단계: 필수 프리팹 및 이미지 설정 ---");
            var essentialResources = new Dictionary<string, string>
            {
                { "Assets/Resources/Prefab/BuffDebuffTooltip.prefab", "BuffDebuffTooltip" },
                { "Assets/Resources/Prefab/SkillCardTooltip.prefab", "SkillCardTooltip" },
                { "Assets/Resources/Prefab/ItemTooltip.prefab", "ItemTooltip.prefab" },
                { "Assets/Resources/Prefab/SkillCard.prefab", "Prefab/SkillCard" },
                { "Assets/Resources/Prefab/SettingsPanelController.prefab", "Prefab/SettingsPanel" }
                // shield_icon.png는 찾을 수 없어서 제외 (필요시 수동 추가)
            };
            var result1 = SetupResources(settings, defaultGroup, essentialResources, null);
            totalSuccess += result1.success;
            totalFail += result1.fail;

            // 2. PlayerCharacterData
            Debug.Log("\n--- 2단계: PlayerCharacterData 설정 ---");
            var characterDataPaths = new[]
            {
                "Assets/Resources/Data/Character/PlayerCharacters/Serene.asset",
                "Assets/Resources/Data/Character/PlayerCharacters/Amera.asset",
                "Assets/Resources/Data/Character/PlayerCharacters/Akein.asset"
            };
            var result2 = SetupResourcesWithLabel(settings, defaultGroup, characterDataPaths, "CharacterData");
            totalSuccess += result2.success;
            totalFail += result2.fail;

            // 3. SkillCardDefinition (모든 스킬 카드)
            Debug.Log("\n--- 3단계: SkillCardDefinition 설정 ---");
            var skillCardPaths = FindAssetsByType("Assets/Resources/Data/SkillCard/Skill", "SkillCardDefinition");
            var result3 = SetupResourcesWithLabel(settings, defaultGroup, skillCardPaths, "SkillCards");
            totalSuccess += result3.success;
            totalFail += result3.fail;

            // 4. ActiveItemDefinition
            Debug.Log("\n--- 4단계: ActiveItemDefinition 설정 ---");
            var activeItemPaths = FindAssetsByType("Assets/Resources/Data/Item/ActiveItem", "ActiveItemDefinition");
            var result4 = SetupResourcesWithLabel(settings, defaultGroup, activeItemPaths, "Data/Item");
            totalSuccess += result4.success;
            totalFail += result4.fail;

            // 5. PassiveItemDefinition
            Debug.Log("\n--- 5단계: PassiveItemDefinition 설정 ---");
            var passiveItemPaths = FindAssetsByType("Assets/Resources/Data/Item/PassiveItem", "PassiveItemDefinition");
            var result5 = SetupResourcesWithLabel(settings, defaultGroup, passiveItemPaths, "Data/Item");
            totalSuccess += result5.success;
            totalFail += result5.fail;

            // 6. RewardPool
            Debug.Log("\n--- 6단계: RewardPool 설정 ---");
            var rewardPoolPaths = FindAssetsByType("Assets/Resources/Data/Reward", "RewardPool");
            var result6 = SetupResourcesWithLabel(settings, defaultGroup, rewardPoolPaths, "Data/Reward");
            totalSuccess += result6.success;
            totalFail += result6.fail;

            // 변경사항 저장
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"\n=== 🎉 Addressables 통합 자동 설정 완료 ===");
            Debug.Log($"✅ 성공: {totalSuccess}개");
            Debug.Log($"❌ 실패: {totalFail}개");
            Debug.Log($"\nUnity Editor에서 Window → Asset Management → Addressables → Groups를 열어 확인하세요.");
        }

        [MenuItem("Tools/Addressables/자동 설정 (필수 리소스 4개)")]
        public static void SetupEssentialResources()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings를 찾을 수 없습니다. Addressables 패키지가 설치되어 있는지 확인하세요.");
                return;
            }

            // Default Local Group 가져오기
            var defaultGroup = settings.DefaultGroup;
            if (defaultGroup == null)
            {
                Debug.LogError("Default Local Group을 찾을 수 없습니다.");
                return;
            }

            // 필수 리소스 4개 설정
            var resources = new Dictionary<string, string>
            {
                { "Assets/Resources/Prefab/BuffDebuffTooltip.prefab", "BuffDebuffTooltip" },
                { "Assets/Resources/Prefab/SkillCard.prefab", "Prefab/SkillCard" },
                { "Assets/Resources/Prefab/SettingsPanelController.prefab", "Prefab/SettingsPanel" },
                { "Assets/Resources/Image/UI (1)/UI/shield_icon.png", "Image/UI (1)/UI/shield_icon" }
            };

            int successCount = 0;
            int failCount = 0;

            foreach (var resource in resources)
            {
                var guid = AssetDatabase.AssetPathToGUID(resource.Key);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning($"리소스를 찾을 수 없습니다: {resource.Key}");
                    failCount++;
                    continue;
                }

                // 이미 Addressable로 마크되어 있는지 확인
                var entry = settings.FindAssetEntry(guid);
                if (entry != null)
                {
                    // 이미 존재하면 주소만 업데이트
                    if (entry.address != resource.Value)
                    {
                        entry.address = resource.Value;
                        Debug.Log($"주소 업데이트: {resource.Key} → {resource.Value}");
                    }
                    else
                    {
                        Debug.Log($"이미 설정됨: {resource.Key} ({resource.Value})");
                    }
                    successCount++;
                    continue;
                }

                // Addressable로 추가
                entry = settings.CreateOrMoveEntry(guid, defaultGroup, false, false);
                if (entry != null)
                {
                    entry.address = resource.Value;
                    Debug.Log($"✅ Addressable 추가 완료: {resource.Key} → {resource.Value}");
                    successCount++;
                }
                else
                {
                    Debug.LogError($"❌ Addressable 추가 실패: {resource.Key}");
                    failCount++;
                }
            }

            // 변경사항 저장
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"\n=== Addressables 설정 완료 ===");
            Debug.Log($"성공: {successCount}개");
            Debug.Log($"실패: {failCount}개");
            Debug.Log($"\nUnity Editor에서 Window → Asset Management → Addressables → Groups를 열어 확인하세요.");
        }

        [MenuItem("Tools/Addressables/PlayerCharacterData 자동 설정")]
        public static void SetupPlayerCharacterData()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings를 찾을 수 없습니다. Addressables 패키지가 설치되어 있는지 확인하세요.");
                return;
            }

            var defaultGroup = settings.DefaultGroup;
            if (defaultGroup == null)
            {
                Debug.LogError("Default Local Group을 찾을 수 없습니다.");
                return;
            }

            // PlayerCharacterData 에셋 경로들
            var characterDataPaths = new[]
            {
                "Assets/Resources/Data/Character/PlayerCharacters/Serene.asset",
                "Assets/Resources/Data/Character/PlayerCharacters/Amera.asset",
                "Assets/Resources/Data/Character/PlayerCharacters/Akein.asset"
            };

            const string LABEL = "CharacterData";
            int successCount = 0;
            int failCount = 0;

            // 라벨이 없으면 생성
            EnsureLabelExists(settings, LABEL);

            foreach (var assetPath in characterDataPaths)
            {
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning($"리소스를 찾을 수 없습니다: {assetPath}");
                    failCount++;
                    continue;
                }

                // 이미 Addressable로 마크되어 있는지 확인
                var entry = settings.FindAssetEntry(guid);
                if (entry == null)
                {
                    // Addressable로 추가
                    entry = settings.CreateOrMoveEntry(guid, defaultGroup, false, false);
                    if (entry == null)
                    {
                        Debug.LogError($"❌ Addressable 추가 실패: {assetPath}");
                        failCount++;
                        continue;
                    }
                }

                // 주소 설정 (에셋 이름 사용)
                var assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                entry.address = assetName;

                // 라벨 추가
                if (!entry.labels.Contains(LABEL))
                {
                    entry.labels.Add(LABEL);
                }

                Debug.Log($"✅ {assetName} 설정 완료 (Address: {entry.address}, Label: {LABEL})");
                successCount++;
            }

            // 변경사항 저장
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"\n=== PlayerCharacterData 설정 완료 ===");
            Debug.Log($"성공: {successCount}개");
            Debug.Log($"실패: {failCount}개");
            Debug.Log($"라벨 '{LABEL}'가 모든 PlayerCharacterData에 추가되었습니다.");
        }

        [MenuItem("Tools/Addressables/모든 리소스 확인")]
        public static void CheckAllResources()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings를 찾을 수 없습니다.");
                return;
            }

            Debug.Log("=== 현재 Addressable로 마크된 리소스 ===");
            var allEntries = settings.groups.SelectMany(g => g.entries);
            int count = 0;
            foreach (var entry in allEntries)
            {
                var labels = string.Join(", ", entry.labels);
                Debug.Log($"[{++count}] {entry.address} → {entry.AssetPath} (Labels: {labels})");
            }

            if (count == 0)
            {
                Debug.LogWarning("Addressable로 마크된 리소스가 없습니다.");
            }
            else
            {
                Debug.Log($"총 {count}개의 리소스가 Addressable로 마크되어 있습니다.");
            }
        }

        #region 헬퍼 메서드

        /// <summary>
        /// 리소스를 Addressables에 추가합니다 (라벨 없음)
        /// </summary>
        private static (int success, int fail) SetupResources(
            AddressableAssetSettings settings,
            AddressableAssetGroup group,
            Dictionary<string, string> resources,
            string label)
        {
            int successCount = 0;
            int failCount = 0;

            foreach (var resource in resources)
            {
                var guid = AssetDatabase.AssetPathToGUID(resource.Key);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning($"리소스를 찾을 수 없습니다: {resource.Key}");
                    failCount++;
                    continue;
                }

                var entry = settings.FindAssetEntry(guid);
                if (entry == null)
                {
                    entry = settings.CreateOrMoveEntry(guid, group, false, false);
                    if (entry == null)
                    {
                        Debug.LogError($"❌ Addressable 추가 실패: {resource.Key}");
                        failCount++;
                        continue;
                    }
                }

                entry.address = resource.Value;

                if (!string.IsNullOrEmpty(label))
                {
                    EnsureLabelExists(settings, label);
                    if (!entry.labels.Contains(label))
                    {
                        entry.labels.Add(label);
                    }
                }

                Debug.Log($"✅ {System.IO.Path.GetFileName(resource.Key)} → {resource.Value}");
                successCount++;
            }

            return (successCount, failCount);
        }

        /// <summary>
        /// 리소스를 Addressables에 추가합니다 (라벨 포함)
        /// </summary>
        private static (int success, int fail) SetupResourcesWithLabel(
            AddressableAssetSettings settings,
            AddressableAssetGroup group,
            string[] assetPaths,
            string label)
        {
            if (assetPaths == null || assetPaths.Length == 0)
            {
                Debug.LogWarning($"설정할 리소스가 없습니다. (라벨: {label})");
                return (0, 0);
            }

            // 라벨이 없으면 생성
            EnsureLabelExists(settings, label);

            int successCount = 0;
            int failCount = 0;

            foreach (var assetPath in assetPaths)
            {
                // 파일이 실제로 존재하는지 먼저 확인
                if (!System.IO.File.Exists(assetPath))
                {
                    Debug.LogWarning($"리소스를 찾을 수 없습니다: {assetPath}");
                    Debug.LogWarning($"  → 파일이 파일 시스템에 존재하지 않습니다.");
                    failCount++;
                    continue;
                }
                
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning($"리소스를 찾을 수 없습니다: {assetPath}");
                    Debug.LogWarning($"  → Unity 에디터가 파일을 인식하지 못했습니다. Assets → Refresh를 실행하거나 에디터를 재시작해주세요.");
                    failCount++;
                    continue;
                }

                // 에셋이 실제로 존재하는지 확인
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (asset == null)
                {
                    Debug.LogWarning($"에셋을 로드할 수 없습니다: {assetPath} (GUID: {guid})");
                    failCount++;
                    continue;
                }

                var entry = settings.FindAssetEntry(guid);
                if (entry == null)
                {
                    entry = settings.CreateOrMoveEntry(guid, group, false, false);
                    if (entry == null)
                    {
                        Debug.LogError($"❌ Addressable 추가 실패: {assetPath}");
                        failCount++;
                        continue;
                    }
                }

                // 주소 설정 (에셋 이름 사용)
                var assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                entry.address = assetName;

                // 라벨 추가
                if (!entry.labels.Contains(label))
                {
                    entry.labels.Add(label);
                }

                Debug.Log($"✅ {assetName} (Label: {label}, Path: {assetPath})");
                successCount++;
            }

            return (successCount, failCount);
        }

        /// <summary>
        /// 라벨이 존재하는지 확인하고 없으면 생성합니다
        /// </summary>
        private static void EnsureLabelExists(AddressableAssetSettings settings, string label)
        {
            var existingLabels = settings.GetLabels();
            if (!existingLabels.Contains(label))
            {
                settings.AddLabel(label);
                Debug.Log($"라벨 '{label}' 생성 완료");
            }
        }

        /// <summary>
        /// 특정 타입의 에셋을 찾습니다
        /// </summary>
        private static string[] FindAssetsByType(string searchPath, string typeName)
        {
            var guids = AssetDatabase.FindAssets($"t:{typeName}", new[] { searchPath });
            var paths = new List<string>();
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && !path.Contains("/ItemEffect/")) // ItemEffect는 제외
                {
                    paths.Add(path);
                }
            }

            return paths.ToArray();
        }

        #endregion
    }
}


