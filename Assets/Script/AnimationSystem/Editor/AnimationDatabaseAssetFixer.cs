using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using AnimationSystem.Data;

public class AnimationDatabaseAssetFixer : EditorWindow
{
    [MenuItem("Tools/AnimationSystem/애니메이션 타입명 자동 정정 (SkillCard/캐릭터 전체)")]
    public static void FixAllAnimationScriptTypes()
    {
        int fixCount = 0;
        int processedAssets = 0;
        
        Debug.Log("[AssetFixer] 애니메이션 타입명 자동 정정 시작...");
        
        // 모든 Animation Database 에셋 경로 찾기
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null) continue;
            
            // EnemySkillCardAnimationDatabase
            if (asset is EnemySkillCardAnimationDatabase enemySkillDB)
            {
                Debug.Log($"[AssetFixer] EnemySkillCardAnimationDatabase 처리 중: {path}");
                int count = FixSkillCardAnimationEntries(enemySkillDB.SkillCardAnimations);
                fixCount += count;
                Debug.Log($"[AssetFixer] EnemySkillCardAnimationDatabase 수정된 필드: {count}개");
                EditorUtility.SetDirty(enemySkillDB);
                processedAssets++;
            }
            // PlayerSkillCardAnimationDatabase
            else if (asset is PlayerSkillCardAnimationDatabase playerSkillDB)
            {
                Debug.Log($"[AssetFixer] PlayerSkillCardAnimationDatabase 처리 중: {path}");
                int count = FixSkillCardAnimationEntries(playerSkillDB.SkillCardAnimations);
                fixCount += count;
                Debug.Log($"[AssetFixer] PlayerSkillCardAnimationDatabase 수정된 필드: {count}개");
                EditorUtility.SetDirty(playerSkillDB);
                processedAssets++;
            }
            // PlayerCharacterAnimationDatabase
            else if (asset is PlayerCharacterAnimationDatabase playerCharDB)
            {
                Debug.Log($"[AssetFixer] PlayerCharacterAnimationDatabase 처리 중: {path}");
                int count = FixCharacterAnimationEntries(playerCharDB.CharacterAnimations);
                fixCount += count;
                Debug.Log($"[AssetFixer] PlayerCharacterAnimationDatabase 수정된 필드: {count}개");
                EditorUtility.SetDirty(playerCharDB);
                processedAssets++;
            }
            // EnemyCharacterAnimationDatabase
            else if (asset is EnemyCharacterAnimationDatabase enemyCharDB)
            {
                Debug.Log($"[AssetFixer] EnemyCharacterAnimationDatabase 처리 중: {path}");
                int count = FixCharacterAnimationEntries(enemyCharDB.CharacterAnimations);
                fixCount += count;
                Debug.Log($"[AssetFixer] EnemyCharacterAnimationDatabase 수정된 필드: {count}개");
                EditorUtility.SetDirty(enemyCharDB);
                processedAssets++;
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"[AnimationDatabaseAssetFixer] animationScriptType 자동 정정 완료! 처리된 에셋: {processedAssets}개, 총 {fixCount}개 필드 수정");
    }

    // SkillCard용
    private static int FixSkillCardAnimationEntries<T>(List<T> entries)
    {
        int count = 0;
        foreach (var entry in entries)
        {
            // PlayerSkillCardAnimationEntry인 경우
            if (entry is PlayerSkillCardAnimationEntry playerEntry)
            {
                // SpawnAnimation 처리
                if (playerEntry.SpawnAnimation != null)
                {
                    string fixedName = GetFixedTypeName(playerEntry.SpawnAnimation.AnimationScriptType);
                    if (!string.IsNullOrEmpty(fixedName) && playerEntry.SpawnAnimation.AnimationScriptType != fixedName)
                    {
                        playerEntry.SpawnAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] PlayerSkillCard SpawnAnimation 타입 수정: {playerEntry.SpawnAnimation.AnimationScriptType}");
                    }
                }
                
                // UseAnimation 처리
                if (playerEntry.UseAnimation != null)
                {
                    string fixedName = GetFixedTypeName(playerEntry.UseAnimation.AnimationScriptType);
                    if (!string.IsNullOrEmpty(fixedName) && playerEntry.UseAnimation.AnimationScriptType != fixedName)
                    {
                        playerEntry.UseAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] PlayerSkillCard UseAnimation 타입 수정: {playerEntry.UseAnimation.AnimationScriptType}");
                    }
                }
                
                // DragAnimation 처리
                if (playerEntry.DragAnimation != null)
                {
                    string fixedName = GetFixedTypeName(playerEntry.DragAnimation.AnimationScriptType);
                    if (!string.IsNullOrEmpty(fixedName) && playerEntry.DragAnimation.AnimationScriptType != fixedName)
                    {
                        playerEntry.DragAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] PlayerSkillCard DragAnimation 타입 수정: {playerEntry.DragAnimation.AnimationScriptType}");
                    }
                }
                
                // DropAnimation 처리
                if (playerEntry.DropAnimation != null)
                {
                    string fixedName = GetFixedTypeName(playerEntry.DropAnimation.AnimationScriptType);
                    if (!string.IsNullOrEmpty(fixedName) && playerEntry.DropAnimation.AnimationScriptType != fixedName)
                    {
                        playerEntry.DropAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] PlayerSkillCard DropAnimation 타입 수정: {playerEntry.DropAnimation.AnimationScriptType}");
                    }
                }
                
                // VanishAnimation 처리
                if (playerEntry.VanishAnimation != null)
                {
                    string fixedName = GetFixedTypeName(playerEntry.VanishAnimation.AnimationScriptType);
                    if (!string.IsNullOrEmpty(fixedName) && playerEntry.VanishAnimation.AnimationScriptType != fixedName)
                    {
                        playerEntry.VanishAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] PlayerSkillCard VanishAnimation 타입 수정: {playerEntry.VanishAnimation.AnimationScriptType}");
                    }
                }
            }
            // EnemySkillCardAnimationEntry인 경우
            else if (entry is EnemySkillCardAnimationEntry enemyEntry)
            {
                // SpawnAnimation 처리
                if (enemyEntry.SpawnAnimation != null)
                {
                    string fixedName = GetFixedTypeName(enemyEntry.SpawnAnimation.AnimationScriptType);
                    if (!string.IsNullOrEmpty(fixedName) && enemyEntry.SpawnAnimation.AnimationScriptType != fixedName)
                    {
                        enemyEntry.SpawnAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] EnemySkillCard SpawnAnimation 타입 수정: {enemyEntry.SpawnAnimation.AnimationScriptType}");
                    }
                }
                
                // MoveAnimation 처리
                if (enemyEntry.MoveAnimation != null)
                {
                    string fixedName = GetFixedTypeName(enemyEntry.MoveAnimation.AnimationScriptType);
                    if (!string.IsNullOrEmpty(fixedName) && enemyEntry.MoveAnimation.AnimationScriptType != fixedName)
                    {
                        enemyEntry.MoveAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] EnemySkillCard MoveAnimation 타입 수정: {enemyEntry.MoveAnimation.AnimationScriptType}");
                    }
                }
                
                // MoveToCombatSlotAnimation 처리
                if (enemyEntry.MoveToCombatSlotAnimation != null)
                {
                    string fixedName = GetFixedTypeName(enemyEntry.MoveToCombatSlotAnimation.AnimationScriptType);
                    if (!string.IsNullOrEmpty(fixedName) && enemyEntry.MoveToCombatSlotAnimation.AnimationScriptType != fixedName)
                    {
                        enemyEntry.MoveToCombatSlotAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] EnemySkillCard MoveToCombatSlotAnimation 타입 수정: {enemyEntry.MoveToCombatSlotAnimation.AnimationScriptType}");
                    }
                }
                
                // UseAnimation 처리
                if (enemyEntry.UseAnimation != null)
                {
                    string fixedName = GetFixedTypeName(enemyEntry.UseAnimation.AnimationScriptType);
                    if (!string.IsNullOrEmpty(fixedName) && enemyEntry.UseAnimation.AnimationScriptType != fixedName)
                    {
                        enemyEntry.UseAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] EnemySkillCard UseAnimation 타입 수정: {enemyEntry.UseAnimation.AnimationScriptType}");
                    }
                }
                
                // VanishAnimation 처리
                if (enemyEntry.VanishAnimation != null)
                {
                    string fixedName = GetFixedTypeName(enemyEntry.VanishAnimation.AnimationScriptType);
                    if (!string.IsNullOrEmpty(fixedName) && enemyEntry.VanishAnimation.AnimationScriptType != fixedName)
                    {
                        enemyEntry.VanishAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] EnemySkillCard VanishAnimation 타입 수정: {enemyEntry.VanishAnimation.AnimationScriptType}");
                    }
                }
            }
        }
        return count;
    }

    // 캐릭터용
    private static int FixCharacterAnimationEntries<T>(List<T> entries)
    {
        int count = 0;
        foreach (var entry in entries)
        {
            // PlayerCharacterAnimationEntry인 경우
            if (entry is PlayerCharacterAnimationEntry playerEntry)
            {
                Debug.Log($"[AssetFixer] PlayerCharacter Entry 처리 중: {playerEntry.PlayerCharacter?.name}");
                
                // SpawnAnimation 처리
                if (playerEntry.SpawnAnimation != null)
                {
                    Debug.Log($"[AssetFixer] PlayerCharacter SpawnAnimation 현재 값: '{playerEntry.SpawnAnimation.AnimationScriptType}'");
                    string fixedName = GetFixedTypeName(playerEntry.SpawnAnimation.AnimationScriptType, "spawn");
                    if (!string.IsNullOrEmpty(fixedName) && playerEntry.SpawnAnimation.AnimationScriptType != fixedName)
                    {
                        playerEntry.SpawnAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] PlayerCharacter SpawnAnimation 타입 수정: {playerEntry.SpawnAnimation.AnimationScriptType}");
                    }
                    else
                    {
                        Debug.Log($"[AssetFixer] PlayerCharacter SpawnAnimation 이미 올바른 값: {playerEntry.SpawnAnimation.AnimationScriptType}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[AssetFixer] PlayerCharacter SpawnAnimation이 null입니다!");
                }
                
                // DeathAnimation 처리
                if (playerEntry.DeathAnimation != null)
                {
                    Debug.Log($"[AssetFixer] PlayerCharacter DeathAnimation 현재 값: '{playerEntry.DeathAnimation.AnimationScriptType}'");
                    string fixedName = GetFixedTypeName(playerEntry.DeathAnimation.AnimationScriptType, "death");
                    if (!string.IsNullOrEmpty(fixedName) && playerEntry.DeathAnimation.AnimationScriptType != fixedName)
                    {
                        playerEntry.DeathAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] PlayerCharacter DeathAnimation 타입 수정: {playerEntry.DeathAnimation.AnimationScriptType}");
                    }
                    else
                    {
                        Debug.Log($"[AssetFixer] PlayerCharacter DeathAnimation 이미 올바른 값: {playerEntry.DeathAnimation.AnimationScriptType}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[AssetFixer] PlayerCharacter DeathAnimation이 null입니다!");
                }
            }
            // EnemyCharacterAnimationEntry인 경우
            else if (entry is EnemyCharacterAnimationEntry enemyEntry)
            {
                Debug.Log($"[AssetFixer] EnemyCharacter Entry 처리 중: {enemyEntry.EnemyCharacter?.name}");
                
                // SpawnAnimation 처리
                if (enemyEntry.SpawnAnimation != null)
                {
                    Debug.Log($"[AssetFixer] EnemyCharacter SpawnAnimation 현재 값: '{enemyEntry.SpawnAnimation.AnimationScriptType}'");
                    string fixedName = GetFixedTypeName(enemyEntry.SpawnAnimation.AnimationScriptType, "spawn");
                    if (!string.IsNullOrEmpty(fixedName) && enemyEntry.SpawnAnimation.AnimationScriptType != fixedName)
                    {
                        enemyEntry.SpawnAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] EnemyCharacter SpawnAnimation 타입 수정: {enemyEntry.SpawnAnimation.AnimationScriptType}");
                    }
                    else
                    {
                        Debug.Log($"[AssetFixer] EnemyCharacter SpawnAnimation 이미 올바른 값: {enemyEntry.SpawnAnimation.AnimationScriptType}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[AssetFixer] EnemyCharacter SpawnAnimation이 null입니다!");
                }
                
                // DeathAnimation 처리
                if (enemyEntry.DeathAnimation != null)
                {
                    Debug.Log($"[AssetFixer] EnemyCharacter DeathAnimation 현재 값: '{enemyEntry.DeathAnimation.AnimationScriptType}'");
                    string fixedName = GetFixedTypeName(enemyEntry.DeathAnimation.AnimationScriptType, "death");
                    if (!string.IsNullOrEmpty(fixedName) && enemyEntry.DeathAnimation.AnimationScriptType != fixedName)
                    {
                        enemyEntry.DeathAnimation.AnimationScriptType = fixedName;
                        count++;
                        Debug.Log($"[AssetFixer] EnemyCharacter DeathAnimation 타입 수정: {enemyEntry.DeathAnimation.AnimationScriptType}");
                    }
                    else
                    {
                        Debug.Log($"[AssetFixer] EnemyCharacter DeathAnimation 이미 올바른 값: {enemyEntry.DeathAnimation.AnimationScriptType}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[AssetFixer] EnemyCharacter DeathAnimation이 null입니다!");
                }
            }
        }
        return count;
    }

    // 잘못된 타입명을 AssemblyQualifiedName으로 변환
    private static string GetFixedTypeName(string oldTypeName, string typeSuffix = "")
    {
        if (string.IsNullOrEmpty(oldTypeName))
        {
            // 빈 값인 경우 기본값 반환
            Debug.Log("[AssetFixer] 빈 값 감지, 기본값으로 설정합니다.");
            return "AnimationSystem.Animator.CharacterAnimation.SpawnAnimation.DefaultCharacterSpawnAnimation, Assembly-CSharp";
        }
        
        // 이미 AssemblyQualifiedName이면 그대로 반환
        if (oldTypeName.Contains(", Assembly-")) return oldTypeName;
        
        // AnimationSystem 네임스페이스의 주요 애니메이션 스크립트들
        var animationTypes = new Dictionary<string, string>
        {
            // Character Animations
            { "DefaultCharacterSpawnAnimation", "AnimationSystem.Animator.CharacterAnimation.SpawnAnimation.DefaultCharacterSpawnAnimation, Assembly-CSharp" },
            { "DefaultCharacterDeathAnimation", "AnimationSystem.Animator.CharacterAnimation.DeathAnimation.DefaultCharacterDeathAnimation, Assembly-CSharp" },
            
            // 잘못된 타입명들을 올바른 타입으로 매핑
            { "CharacterSpawnAnimator", "AnimationSystem.Animator.CharacterAnimation.SpawnAnimation.DefaultCharacterSpawnAnimation, Assembly-CSharp" },
            { "CharacterDeathAnimator", "AnimationSystem.Animator.CharacterAnimation.DeathAnimation.DefaultCharacterDeathAnimation, Assembly-CSharp" },
            { "AnimationSystem.Animator.CharacterSpawnAnimator", "AnimationSystem.Animator.CharacterAnimation.SpawnAnimation.DefaultCharacterSpawnAnimation, Assembly-CSharp" },
            { "AnimationSystem.Animator.CharacterDeathAnimator", "AnimationSystem.Animator.CharacterAnimation.DeathAnimation.DefaultCharacterDeathAnimation, Assembly-CSharp" },
            
            // SkillCard Animations
            { "DefaultSkillCardSpawnAnimation", "AnimationSystem.Animator.SkillCardAnimation.SpawnAnimation.DefaultSkillCardSpawnAnimation, Assembly-CSharp" },
            { "DefaultSkillCardUseAnimation", "AnimationSystem.Animator.SkillCardAnimation.UseAnimation.DefaultSkillCardUseAnimation, Assembly-CSharp" },
            { "DefaultSkillCardDragAnimation", "AnimationSystem.Animator.SkillCardAnimation.DragAnimation.DefaultSkillCardDragAnimation, Assembly-CSharp" },
            { "DefaultSkillCardDropAnimation", "AnimationSystem.Animator.SkillCardAnimation.DropAnimation.DefaultSkillCardDropAnimation, Assembly-CSharp" },
            { "DefaultSkillCardVanishAnimation", "AnimationSystem.Animator.SkillCardAnimation.VanishAnimation.DefaultSkillCardVanishAnimation, Assembly-CSharp" },
            { "DefaultSkillCardMoveAnimation", "AnimationSystem.Animator.SkillCardAnimation.MoveAnimation.DefaultSkillCardMoveAnimation, Assembly-CSharp" },
            { "DefaultSkillCardMoveToCombatSlotAnimation", "AnimationSystem.Animator.SkillCardAnimation.MoveToCombatSlotAnimation.DefaultSkillCardMoveToCombatSlotAnimation, Assembly-CSharp" },
            
            // 잘못된 SkillCard 타입명들
            { "SkillCardSpawnAnimator", "AnimationSystem.Animator.SkillCardAnimation.SpawnAnimation.DefaultSkillCardSpawnAnimation, Assembly-CSharp" },
            { "SkillCardUseAnimator", "AnimationSystem.Animator.SkillCardAnimation.UseAnimation.DefaultSkillCardUseAnimation, Assembly-CSharp" },
            { "SkillCardDragAnimator", "AnimationSystem.Animator.SkillCardAnimation.DragAnimation.DefaultSkillCardDragAnimation, Assembly-CSharp" },
            { "SkillCardDropAnimator", "AnimationSystem.Animator.SkillCardAnimation.DropAnimation.DefaultSkillCardDropAnimation, Assembly-CSharp" },
            { "SkillCardVanishAnimator", "AnimationSystem.Animator.SkillCardAnimation.VanishAnimation.DefaultSkillCardVanishAnimation, Assembly-CSharp" },
            { "AnimationSystem.Animator.SkillCardSpawnAnimator", "AnimationSystem.Animator.SkillCardAnimation.SpawnAnimation.DefaultSkillCardSpawnAnimation, Assembly-CSharp" },
            { "AnimationSystem.Animator.SkillCardUseAnimator", "AnimationSystem.Animator.SkillCardAnimation.UseAnimation.DefaultSkillCardUseAnimation, Assembly-CSharp" },
            
            // 네임스페이스 포함된 경우들
            { "AnimationSystem.Animator.CharacterAnimation.SpawnAnimation.DefaultCharacterSpawnAnimation", "AnimationSystem.Animator.CharacterAnimation.SpawnAnimation.DefaultCharacterSpawnAnimation, Assembly-CSharp" },
            { "AnimationSystem.Animator.SkillCardAnimation.SpawnAnimation.DefaultSkillCardSpawnAnimation", "AnimationSystem.Animator.SkillCardAnimation.SpawnAnimation.DefaultSkillCardSpawnAnimation, Assembly-CSharp" }
        };
        
        // 딕셔너리에서 직접 매칭
        if (animationTypes.ContainsKey(oldTypeName))
        {
            return animationTypes[oldTypeName];
        }
        
        // 리플렉션으로 찾기 (기존 방식)
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == oldTypeName || t.FullName == oldTypeName);
        if (type != null)
        {
            return type.AssemblyQualifiedName;
        }
        
        Debug.LogWarning($"[AssetFixer] 타입을 찾을 수 없음: {oldTypeName}");
        return null;
    }
} 