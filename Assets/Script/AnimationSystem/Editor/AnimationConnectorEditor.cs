using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using AnimationSystem;
using AnimationSystem.Controllers;
using AnimationSystem.Manager;
using AnimationSystem.Data;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Core;
using Game.CharacterSystem.Data;

namespace AnimationSystem.Editor
{
    /// <summary>
    /// 비프로그래머도 쉽게 사용할 수 있는 애니메이션 연결 에디터
    /// 스킬카드와 캐릭터 데이터를 분리해서 보여주고, 드롭다운으로 쉽게 연결할 수 있습니다.
    /// </summary>
    [CustomEditor(typeof(AnimationManager))]
    public class AnimationConnectorEditor : UnityEditor.Editor
    {
        private AnimationManager animationManager;
        private bool showSkillCardSection = true;
        private bool showCharacterSection = true;
        private bool showSettingsSection = true;
        
        // 선택된 아이템들
        private string selectedSkillCard = "";
        private string selectedCharacter = "";
        private string selectedAnimationType = "";
        
        // 드롭다운 옵션들
        private string[] skillCardNames = new string[0];
        private string[] characterNames = new string[0];
        private string[] animationTypes = { "Spawn", "Move", "UseEffect", "Death", "Damage", "Heal" };
        
        // 미리보기 설정
        private GameObject previewTarget;
        private bool showPreview = false;
        
        private void OnEnable()
        {
            animationManager = (AnimationManager)target;
            RefreshDataLists();
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawHeader();
            DrawSettingsSection();
            DrawSkillCardSection();
            DrawCharacterSection();
            DrawPreviewSection();
            DrawActionButtons();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        #region UI Drawing Methods
        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("🎮 애니메이션 연결 도구", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("스킬카드와 캐릭터 애니메이션을 쉽게 연결하세요!", EditorStyles.miniLabel);
            EditorGUILayout.Space();
        }
        
        private void DrawSettingsSection()
        {
            showSettingsSection = EditorGUILayout.Foldout(showSettingsSection, "⚙️ 설정", true);
            if (showSettingsSection)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoLoadDataOnStart"), 
                    new GUIContent("시작 시 자동 로드", "게임 시작 시 데이터를 자동으로 로드합니다."));
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("enableAnimationLogging"), 
                    new GUIContent("애니메이션 로그", "애니메이션 실행 시 콘솔에 로그를 출력합니다."));
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
        }
        
        private void DrawSkillCardSection()
        {
            showSkillCardSection = EditorGUILayout.Foldout(showSkillCardSection, "🃏 스킬카드 애니메이션", true);
            if (showSkillCardSection)
            {
                EditorGUI.indentLevel++;
                
                // 스킬카드 선택
                EditorGUILayout.LabelField("스킬카드 선택", EditorStyles.boldLabel);
                int skillCardIndex = System.Array.IndexOf(skillCardNames, selectedSkillCard);
                int newSkillCardIndex = EditorGUILayout.Popup("카드", skillCardIndex, skillCardNames);
                
                if (newSkillCardIndex != skillCardIndex && newSkillCardIndex >= 0)
                {
                    selectedSkillCard = skillCardNames[newSkillCardIndex];
                    selectedAnimationType = "";
                }
                
                if (!string.IsNullOrEmpty(selectedSkillCard))
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"선택된 카드: {selectedSkillCard}", EditorStyles.boldLabel);
                    
                    // 애니메이션 타입 선택
                    EditorGUILayout.LabelField("애니메이션 타입", EditorStyles.boldLabel);
                    int animTypeIndex = System.Array.IndexOf(animationTypes, selectedAnimationType);
                    int newAnimTypeIndex = EditorGUILayout.Popup("타입", animTypeIndex, 
                        new string[] { "Spawn", "Move", "UseEffect" });
                    
                    if (newAnimTypeIndex != animTypeIndex && newAnimTypeIndex >= 0)
                    {
                        selectedAnimationType = new string[] { "Spawn", "Move", "UseEffect" }[newAnimTypeIndex];
                    }
                    
                    // 애니메이션 설정 편집
                    if (!string.IsNullOrEmpty(selectedAnimationType))
                    {
                        DrawSkillCardAnimationSettings(selectedSkillCard, selectedAnimationType);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
        }
        
        private void DrawCharacterSection()
        {
            showCharacterSection = EditorGUILayout.Foldout(showCharacterSection, "👤 캐릭터 애니메이션", true);
            if (showCharacterSection)
            {
                EditorGUI.indentLevel++;
                
                // 캐릭터 선택
                EditorGUILayout.LabelField("캐릭터 선택", EditorStyles.boldLabel);
                int characterIndex = System.Array.IndexOf(characterNames, selectedCharacter);
                int newCharacterIndex = EditorGUILayout.Popup("캐릭터", characterIndex, characterNames);
                
                if (newCharacterIndex != characterIndex && newCharacterIndex >= 0)
                {
                    selectedCharacter = characterNames[newCharacterIndex];
                    selectedAnimationType = "";
                }
                
                if (!string.IsNullOrEmpty(selectedCharacter))
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"선택된 캐릭터: {selectedCharacter}", EditorStyles.boldLabel);
                    
                    // 애니메이션 타입 선택
                    EditorGUILayout.LabelField("애니메이션 타입", EditorStyles.boldLabel);
                    int animTypeIndex = System.Array.IndexOf(animationTypes, selectedAnimationType);
                    int newAnimTypeIndex = EditorGUILayout.Popup("타입", animTypeIndex, 
                        new string[] { "Spawn", "Death", "Damage", "Heal" });
                    
                    if (newAnimTypeIndex != animTypeIndex && newAnimTypeIndex >= 0)
                    {
                        selectedAnimationType = new string[] { "Spawn", "Death", "Damage", "Heal" }[newAnimTypeIndex];
                    }
                    
                    // 애니메이션 설정 편집
                    if (!string.IsNullOrEmpty(selectedAnimationType))
                    {
                        DrawCharacterAnimationSettings(selectedCharacter, selectedAnimationType);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
        }
        
        private void DrawSkillCardAnimationSettings(string cardName, string animationType)
        {
            var controller = animationManager.GetSkillCardController(cardName);
            if (controller == null) return;
            
            var settings = controller.GetSettings();
            
            EditorGUILayout.LabelField($"{animationType} 애니메이션 설정", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            switch (animationType)
            {
                case "Spawn":
                    DrawSpawnSettings(settings);
                    break;
                case "Move":
                    DrawMoveSettings(settings);
                    break;
                case "UseEffect":
                    DrawUseEffectSettings(settings);
                    break;
            }
            
            // 미리보기 버튼
            if (GUILayout.Button($"미리보기 - {animationType}", GUILayout.Height(25)))
            {
                PlayPreviewAnimation(cardName, animationType);
            }
        }
        
        private void DrawCharacterAnimationSettings(string characterName, string animationType)
        {
            var controller = animationManager.GetCharacterController(characterName);
            if (controller == null) return;
            
            var settings = controller.GetSettings();
            
            EditorGUILayout.LabelField($"{animationType} 애니메이션 설정", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            switch (animationType)
            {
                case "Spawn":
                    DrawCharacterSpawnSettings(settings);
                    break;
                case "Death":
                    DrawDeathSettings(settings);
                    break;
                case "Damage":
                    DrawDamageSettings(settings);
                    break;
                case "Heal":
                    DrawHealSettings(settings);
                    break;
            }
            
            // 미리보기 버튼
            if (GUILayout.Button($"미리보기 - {animationType}", GUILayout.Height(25)))
            {
                PlayPreviewAnimation(characterName, animationType);
            }
        }
        
        private void DrawSpawnSettings(SkillCardAnimationController.AnimationSettings settings)
        {
            settings.spawnDuration = EditorGUILayout.Slider("지속시간", settings.spawnDuration, 0.1f, 3.0f);
            settings.spawnEase = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("이징", settings.spawnEase);
            settings.spawnStartScale = EditorGUILayout.Vector3Field("시작 스케일", settings.spawnStartScale);
            settings.spawnEndScale = EditorGUILayout.Vector3Field("종료 스케일", settings.spawnEndScale);
            settings.useSpawnGlow = EditorGUILayout.Toggle("글로우 사용", settings.useSpawnGlow);
            if (settings.useSpawnGlow)
            {
                settings.spawnGlowColor = EditorGUILayout.ColorField("글로우 색상", settings.spawnGlowColor);
                settings.spawnGlowIntensity = EditorGUILayout.Slider("글로우 강도", settings.spawnGlowIntensity, 0.1f, 5.0f);
            }
        }
        
        private void DrawMoveSettings(SkillCardAnimationController.AnimationSettings settings)
        {
            settings.moveDuration = EditorGUILayout.Slider("지속시간", settings.moveDuration, 0.1f, 3.0f);
            settings.moveEase = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("이징", settings.moveEase);
            settings.useArcMovement = EditorGUILayout.Toggle("아크 이동", settings.useArcMovement);
            if (settings.useArcMovement)
            {
                settings.arcHeight = EditorGUILayout.Slider("아크 높이", settings.arcHeight, 0.1f, 10.0f);
            }
        }
        
        private void DrawUseEffectSettings(SkillCardAnimationController.AnimationSettings settings)
        {
            settings.useEffectDuration = EditorGUILayout.Slider("지속시간", settings.useEffectDuration, 0.1f, 3.0f);
            settings.useEffectEase = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("이징", settings.useEffectEase);
            settings.useUseEffectGlow = EditorGUILayout.Toggle("글로우 사용", settings.useUseEffectGlow);
            if (settings.useUseEffectGlow)
            {
                settings.useEffectGlowColor = EditorGUILayout.ColorField("글로우 색상", settings.useEffectGlowColor);
                settings.useEffectGlowIntensity = EditorGUILayout.Slider("글로우 강도", settings.useEffectGlowIntensity, 0.1f, 5.0f);
            }
        }
        
        private void DrawCharacterSpawnSettings(CharacterAnimationController.AnimationSettings settings)
        {
            settings.spawnDuration = EditorGUILayout.Slider("지속시간", settings.spawnDuration, 0.1f, 3.0f);
            settings.spawnEase = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("이징", settings.spawnEase);
            settings.useSpawnGlow = EditorGUILayout.Toggle("글로우 사용", settings.useSpawnGlow);
            if (settings.useSpawnGlow)
            {
                settings.spawnGlowColor = EditorGUILayout.ColorField("글로우 색상", settings.spawnGlowColor);
                settings.spawnGlowIntensity = EditorGUILayout.Slider("글로우 강도", settings.spawnGlowIntensity, 0.1f, 5.0f);
            }
        }
        
        private void DrawDeathSettings(CharacterAnimationController.AnimationSettings settings)
        {
            settings.deathDuration = EditorGUILayout.Slider("지속시간", settings.deathDuration, 0.1f, 3.0f);
            settings.deathEase = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("이징", settings.deathEase);
            settings.useDeathFade = EditorGUILayout.Toggle("페이드 아웃", settings.useDeathFade);
            if (settings.useDeathFade)
            {
                settings.deathFadeOutTime = EditorGUILayout.Slider("페이드 시간", settings.deathFadeOutTime, 0.1f, 2.0f);
            }
            settings.deathGlowColor = EditorGUILayout.ColorField("글로우 색상", settings.deathGlowColor);
        }
        
        private void DrawDamageSettings(CharacterAnimationController.AnimationSettings settings)
        {
            settings.damageDuration = EditorGUILayout.Slider("지속시간", settings.damageDuration, 0.1f, 1.0f);
            settings.damageEase = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("이징", settings.damageEase);
            settings.useDamageShake = EditorGUILayout.Toggle("흔들림", settings.useDamageShake);
            if (settings.useDamageShake)
            {
                settings.damageShakeStrength = EditorGUILayout.Slider("흔들림 강도", settings.damageShakeStrength, 0.01f, 1.0f);
            }
            settings.damageFlashColor = EditorGUILayout.ColorField("플래시 색상", settings.damageFlashColor);
        }
        
        private void DrawHealSettings(CharacterAnimationController.AnimationSettings settings)
        {
            settings.healDuration = EditorGUILayout.Slider("지속시간", settings.healDuration, 0.1f, 2.0f);
            settings.healEase = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("이징", settings.healEase);
            settings.useHealGlow = EditorGUILayout.Toggle("글로우 사용", settings.useHealGlow);
            if (settings.useHealGlow)
            {
                settings.healGlowColor = EditorGUILayout.ColorField("글로우 색상", settings.healGlowColor);
                settings.healGlowIntensity = EditorGUILayout.Slider("글로우 강도", settings.healGlowIntensity, 0.1f, 5.0f);
            }
        }
        
        private void DrawPreviewSection()
        {
            EditorGUILayout.LabelField("🎬 미리보기", EditorStyles.boldLabel);
            
            previewTarget = (GameObject)EditorGUILayout.ObjectField("미리보기 대상", previewTarget, typeof(GameObject), true);
            
            if (previewTarget == null)
            {
                EditorGUILayout.HelpBox("미리보기를 위해 GameObject를 선택해주세요.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.LabelField("🔧 액션", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 데이터 새로고침 버튼
            if (GUILayout.Button("데이터 새로고침", GUILayout.Height(30)))
            {
                RefreshDataLists();
            }
            
            EditorGUILayout.Space();
            
            // 상태 출력 버튼
            if (GUILayout.Button("상태 출력", GUILayout.Height(25)))
            {
                animationManager.PrintStatus();
            }
            
            // 모든 애니메이션 테스트 버튼
            if (GUILayout.Button("모든 애니메이션 테스트", GUILayout.Height(25)))
            {
                TestAllAnimations();
            }
        }
        #endregion
        
        #region Utility Methods
        private void RefreshDataLists()
        {
            // 스킬카드 목록 새로고침
            var playerCards = animationManager.GetAllPlayerSkillCards();
            var enemyCards = animationManager.GetAllEnemySkillCards();
            
            var allCards = new List<string>();
            allCards.AddRange(playerCards.Select(card => card.name));
            allCards.AddRange(enemyCards.Select(card => card.name));
            skillCardNames = allCards.ToArray();
            
            // 캐릭터 목록 새로고침
            var playerChars = animationManager.GetAllPlayerCharacters();
            var enemyChars = animationManager.GetAllEnemyCharacters();
            
            var allCharacters = new List<string>();
            allCharacters.AddRange(playerChars.Select(character => character.name));
            allCharacters.AddRange(enemyChars.Select(character => character.name));
            characterNames = allCharacters.ToArray();
            
            Debug.Log($"[AnimationConnectorEditor] 데이터 새로고침 완료 - 스킬카드: {skillCardNames.Length}개, 캐릭터: {characterNames.Length}개");
        }
        
        private void PlayPreviewAnimation(string itemName, string animationType)
        {
            if (previewTarget == null)
            {
                Debug.LogWarning("[AnimationConnectorEditor] 미리보기 대상을 선택해주세요.");
                return;
            }
            
            // 스킬카드 애니메이션인지 확인
            if (skillCardNames.Contains(itemName))
            {
                animationManager.PlaySkillCardAnimation(itemName, animationType, previewTarget);
            }
            // 캐릭터 애니메이션인지 확인
            else if (characterNames.Contains(itemName))
            {
                animationManager.PlayCharacterAnimation(itemName, animationType, previewTarget);
            }
            
            Debug.Log($"[AnimationConnectorEditor] 미리보기 실행: {itemName} - {animationType}");
        }
        
        private void TestAllAnimations()
        {
            if (previewTarget == null)
            {
                Debug.LogWarning("[AnimationConnectorEditor] 미리보기 대상을 선택해주세요.");
                return;
            }
            
            Debug.Log("[AnimationConnectorEditor] 모든 애니메이션 테스트 시작...");
            
            // 스킬카드 애니메이션 테스트
            foreach (var cardName in skillCardNames)
            {
                animationManager.PlaySkillCardAnimation(cardName, "Spawn", previewTarget);
            }
            
            // 캐릭터 애니메이션 테스트
            foreach (var characterName in characterNames)
            {
                animationManager.PlayCharacterAnimation(characterName, "Spawn", previewTarget);
            }
            
            Debug.Log("[AnimationConnectorEditor] 모든 애니메이션 테스트 완료!");
        }
        #endregion
    }
} 