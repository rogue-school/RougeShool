using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Zenject;
using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.CoreSystem.Utility;
using UnityEngine.Serialization;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Game.ItemSystem.UI;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 플레이어 캐릭터의 통합 UI 컨트롤러입니다.
    /// 리그 오브 레전드 스타일의 HP/MP 바와 캐릭터 정보를 표시합니다.
    /// </summary>
    public class PlayerCharacterUIController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("캐릭터 기본 정보")]
        [Tooltip("캐릭터 초상화 이미지")]
        [SerializeField] private Image characterPortrait;
        
        [Tooltip("캐릭터 문양/클래스 아이콘")]
        [SerializeField] private Image characterEmblem;
        
        [Tooltip("캐릭터 이름 텍스트")]
        [SerializeField] private TextMeshProUGUI characterNameText;

        [Header("HP/Resource 바 시스템")]
        [Tooltip("HP 바 배경")]
        [SerializeField] private Image hpBarBackground;
        
        [Tooltip("HP 바 채움 부분")]
        [SerializeField] private Image hpBarFill;
		
		[Header("리소스 핍(Pip)")]
		[Tooltip("리소스 핍들을 담는 부모 오브젝트 (활/지팡이 등)")]
		[SerializeField] private Transform resourcePipParent;
		
		[Tooltip("리소스 핍 프리팹(단일 사각/아이콘)")]
		[SerializeField] private GameObject resourcePipPrefab;

        [Header("HP/Resource 텍스트")]
        [Tooltip("HP 텍스트 (현재/최대)")]
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("버프/디버프 아이콘")]
        [Tooltip("버프/디버프 아이콘들을 담을 부모 오브젝트")]
        [SerializeField] private Transform buffDebuffParent;
        
        [Tooltip("버프/디버프 아이콘 프리팹")]
        [SerializeField] private GameObject buffDebuffIconPrefab;

        [Header("패시브 아이템 아이콘")]
        [Tooltip("패시브 아이템 아이콘들을 담을 부모 오브젝트")]
        [SerializeField] private Transform passiveItemParent;
        
        [Tooltip("패시브 아이템 아이콘 프리팹")]
        [SerializeField] private GameObject passiveItemIconPrefab;

        [Header("색상 설정")]
        [Tooltip("풀피일 때 HP 바 색상")]
        [SerializeField] private Color fullHPColor = Color.green;
        
        [Tooltip("저체력일 때 HP 바 색상")]
        [SerializeField] private Color lowHPColor = Color.red;
        
        [Tooltip("중간 체력일 때 HP 바 색상")]
        [SerializeField] private Color midHPColor = Color.yellow;
        [Tooltip("Resource Active Color (켜진 핍 색상)")]
        [FormerlySerializedAs("mpColor")]
        [SerializeField] private Color resourceActiveColor = Color.blue;
		
		[Tooltip("리소스 핍 비활성 색상(자원 없음: 하늘색 계열)")]
		[SerializeField] private Color pipInactiveColor = new Color(0.6f, 1f, 1f, 0.9f);
		
		[Tooltip("리소스 핍 사용 불가 색상(검 캐릭터 전용 회색)")]
		[SerializeField] private Color pipUnavailableColor = new Color(0.7f, 0.7f, 0.7f, 0.9f);

        // 핍 개수는 데이터의 MaxResource에서 가져옵니다(0이면 1개 회색으로 표시)

        [Header("애니메이션 설정")]
        [Tooltip("HP/MP 바 애니메이션 속도")]
        [SerializeField] private float barAnimationSpeed = 2f;
        
        [Tooltip("색상 변화 애니메이션 속도")]
        [SerializeField] private float colorAnimationSpeed = 1f;

        #endregion

        #region Private Fields

        private PlayerCharacter playerCharacter;
        [InjectOptional] private PlayerManager playerManager;
        [InjectOptional] private IItemService itemService;
        private PlayerCharacterType characterType;
        
        // 애니메이션 관련
        private Tween hpBarTween;
        private Tween resourceBarTween;
        private Tween colorTween;
        
		// 버프/디버프 아이콘 관리
		private System.Collections.Generic.Dictionary<string, GameObject> activeBuffDebuffIcons = new();
		
		// 패시브 아이템 아이콘 관리
		private Dictionary<string, PassiveItemIcon> activePassiveItemIcons = new();
		
		// 리소스 핍 관리
		private readonly System.Collections.Generic.List<Image> resourcePips = new();
		private bool usePipResource = false;

        // 이벤트 구독 상태
        private bool isSubscribed = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeUI();
        }

        private void OnDestroy()
        {
            // DOTween 정리
            hpBarTween?.Kill();
            resourceBarTween?.Kill();
            colorTween?.Kill();

            UnsubscribeCharacterEvents();
            UnsubscribeItemServiceEvents();
            
            // 패시브 아이템 아이콘 정리
            ClearAllPassiveItemIcons();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// UI 초기화
        /// </summary>
		private void InitializeUI()
        {
            // 초기 상태 설정
            if (hpBarFill != null)
            {
                EnsureHpImageConfigured(hpBarFill);
                hpBarFill.fillAmount = 1f;
            }
            if (hpBarBackground != null)
            {
                EnsureSpriteIfMissing(hpBarBackground);
            }
            
            // 리소스 바는 사용하지 않음(핍 방식)
			
			// 핍 컨테이너는 기본 비활성(캐릭터 타입 결정 후 On)
			if (resourcePipParent != null)
				resourcePipParent.gameObject.SetActive(false);
        }

        /// <summary>
        /// HP Fill 이미지가 스프라이트 없이도 동작하도록 기본 스프라이트/타입을 설정합니다.
        /// </summary>
        /// <param name="img">대상 이미지</param>
        private void EnsureHpImageConfigured(Image img)
        {
            if (img == null) return;
            EnsureSpriteIfMissing(img);
            if (img.type != Image.Type.Filled)
            {
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                img.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }

        /// <summary>
        /// 스프라이트가 없으면 Unity 기본 UISprite로 대체합니다.
        /// </summary>
        private void EnsureSpriteIfMissing(Image img)
        {
            if (img == null) return;
            if (img.sprite == null)
            {
                // Unity 기본 리소스 경로가 환경마다 다를 수 있어, 1x1 단색 스프라이트를 동적으로 생성
                var tex = Texture2D.whiteTexture;
                var rect = new Rect(0, 0, 1, 1);
                var pivot = new Vector2(0.5f, 0.5f);
                var generated = Sprite.Create(tex, rect, pivot, 1f);
                img.sprite = generated;
            }
        }

        /// <summary>
        /// 부모 크기를 유지한 채, 이미지 비율만 보호하도록 AspectRatioFitter를 보장합니다.
        /// SetNativeSize를 호출하지 않고, FitInParent로만 비율을 맞춥니다.
        /// </summary>
        private void EnsureAspectFit(Image targetImage, Sprite sprite)
        {
            if (targetImage == null) return;
            var fitter = targetImage.GetComponent<UnityEngine.UI.AspectRatioFitter>();
            if (fitter == null)
            {
                fitter = targetImage.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            }
            fitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
            if (sprite != null && sprite.rect.height > 0)
            {
                fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
            }
        }

        /// <summary>
        /// 플레이어 캐릭터와 연결하여 UI를 초기화합니다.
        /// </summary>
        /// <param name="character">연결할 플레이어 캐릭터</param>
        public void Initialize(PlayerCharacter character)
        {
            if (character == null)
            {
                GameLogger.LogWarning("[PlayerCharacterUIController] Initialize() - character가 null입니다.", GameLogger.LogCategory.Character);
                return;
            }

            if (character.CharacterData == null)
            {
                GameLogger.LogWarning("[PlayerCharacterUIController] Initialize() - character.CharacterData가 null입니다. 나중에 다시 시도합니다.", GameLogger.LogCategory.Character);
                // 나중에 다시 시도하도록 예약
                StartCoroutine(RetryInitializeWhenReady(character));
                return;
            }

            playerCharacter = character;
            characterType = (character.CharacterData as PlayerCharacterData)?.CharacterType ?? PlayerCharacterType.Sword;
            
            // 리소스 매니저는 Zenject DI로 주입됨
            if (playerManager == null)
            {
                GameLogger.LogWarning("[PlayerCharacterUIController] PlayerManager가 주입되지 않았습니다.", GameLogger.LogCategory.Character);
            }

            // 캐릭터 정보 설정
            SetCharacterInfo();

            // 리소스/HP UI 즉시 설정 (CharacterData가 이미 준비된 경우를 대비)
            SetupResourceSystem();
            UpdateHPBar();
            UpdateMPBar();

            SubscribeCharacterEvents();
            SubscribeItemServiceEvents();
            
            // 초기 패시브 아이템 로드
            RefreshPassiveItemIcons();
        }
        
        /// <summary>
        /// CharacterData가 준비될 때까지 대기한 후 다시 초기화를 시도합니다.
        /// </summary>
        private System.Collections.IEnumerator RetryInitializeWhenReady(PlayerCharacter character)
        {
            // CharacterData가 설정될 때까지 대기 (최대 5초)
            int maxWaitFrames = 300; // 최대 5초 대기 (60fps 기준)
            int waitFrames = 0;
            
            while (character.CharacterData == null && waitFrames < maxWaitFrames)
            {
                yield return new UnityEngine.WaitForSeconds(0.1f);
                waitFrames++;
            }
            
            if (character.CharacterData == null)
            {
                GameLogger.LogError("[PlayerCharacterUIController] CharacterData가 설정되지 않았습니다. 초기화를 건너뜁니다.", GameLogger.LogCategory.Character);
                yield break;
            }
            
            // 다시 초기화 시도
            Initialize(character);
            
            // 리소스 시스템 설정
            SetupResourceSystem();
            
            // 초기 HP/MP 업데이트
            UpdateHPBar();
            UpdateMPBar();
            
        }

        /// <summary>
        /// 호환성 메서드: 기존 코드에서 ICharacter 기반 SetTarget을 호출하는 경우를 위해 유지.
        /// PlayerCharacter이면 Initialize로 연결한다.
        /// </summary>
        /// <param name="character">대상 캐릭터</param>
        public void SetTarget(Game.CharacterSystem.Interface.ICharacter character)
        {
            if (character is PlayerCharacter pc)
            {
                Initialize(pc);
            }
            else
            {
                GameLogger.LogWarning("[PlayerCharacterUIController] SetTarget: PlayerCharacter가 아닙니다. 호출을 무시합니다.", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 캐릭터 HP 변경 이벤트 구독
        /// </summary>
        private void SubscribeCharacterEvents()
        {
            if (isSubscribed || playerCharacter == null) return;
            playerCharacter.OnHPChanged += OnHpChangedHandler;
            playerCharacter.OnBuffsChanged += OnBuffsChangedHandler;
            if (playerManager != null)
            {
                playerManager.OnResourceChanged += OnResourceChangedByManager;
            }
            isSubscribed = true;
        }

        /// <summary>
        /// 캐릭터 HP 변경 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeCharacterEvents()
        {
            if (!isSubscribed || playerCharacter == null) return;
            playerCharacter.OnHPChanged -= OnHpChangedHandler;
            playerCharacter.OnBuffsChanged -= OnBuffsChangedHandler;
            if (playerManager != null)
            {
                playerManager.OnResourceChanged -= OnResourceChangedByManager;
            }
            isSubscribed = false;
        }

        /// <summary>
        /// ItemService 이벤트 구독
        /// </summary>
        private void SubscribeItemServiceEvents()
        {
            if (itemService == null) return;
            
            itemService.OnPassiveItemAdded += OnPassiveItemAddedHandler;
            itemService.OnEnhancementUpgraded += OnEnhancementUpgradedHandler;
        }

        /// <summary>
        /// ItemService 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeItemServiceEvents()
        {
            if (itemService == null) return;
            
            itemService.OnPassiveItemAdded -= OnPassiveItemAddedHandler;
            itemService.OnEnhancementUpgraded -= OnEnhancementUpgradedHandler;
        }

        /// <summary>
        /// 패시브 아이템 추가 이벤트 핸들러
        /// </summary>
        /// <param name="itemDefinition">추가된 패시브 아이템 정의</param>
        private void OnPassiveItemAddedHandler(PassiveItemDefinition itemDefinition)
        {
            if (itemDefinition == null || itemService == null) return;

            // 강화 단계 계산
            int enhancementLevel = 0; // 기본값 0 (강화 안됨)
            string skillId = null;

            if (itemDefinition.IsPlayerHealthBonus)
            {
                skillId = $"__PLAYER_HP__:{itemDefinition.ItemId}";
            }
            else if (itemDefinition.TargetSkill != null)
            {
                skillId = !string.IsNullOrEmpty(itemDefinition.TargetSkill.displayName) 
                    ? itemDefinition.TargetSkill.displayName 
                    : itemDefinition.TargetSkill.cardId;
            }

            if (!string.IsNullOrEmpty(skillId))
            {
                enhancementLevel = itemService.GetSkillEnhancementLevel(skillId);
                // 강화 레벨이 0보다 작으면 0으로 설정 (이미 0 이상이므로 사실상 불필요하지만 안전장치)
                if (enhancementLevel < 0)
                    enhancementLevel = 0;
            }

            AddPassiveItemIcon(itemDefinition, enhancementLevel);
        }

        /// <summary>
        /// 강화 단계 업그레이드 이벤트 핸들러
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <param name="newLevel">새로운 강화 단계</param>
        private void OnEnhancementUpgradedHandler(string skillId, int newLevel)
        {
            if (itemService == null) return;

            // 해당 스킬 ID와 관련된 패시브 아이템 찾기
            var passiveItems = itemService.GetPassiveItems();
            foreach (var item in passiveItems)
            {
                if (item == null || string.IsNullOrEmpty(item.ItemId))
                    continue;

                string itemSkillId = null;
                if (item.IsPlayerHealthBonus)
                {
                    itemSkillId = $"__PLAYER_HP__:{item.ItemId}";
                }
                else if (item.TargetSkill != null)
                {
                    itemSkillId = !string.IsNullOrEmpty(item.TargetSkill.displayName) 
                        ? item.TargetSkill.displayName 
                        : item.TargetSkill.cardId;
                }

                if (itemSkillId == skillId)
                {
                    // 해당 패시브 아이템의 강화 단계 업데이트
                    if (activePassiveItemIcons.TryGetValue(item.ItemId, out PassiveItemIcon icon))
                    {
                        icon.UpdateEnhancementLevel(newLevel);
                    }
                    else
                    {
                        // 아이콘이 없으면 새로 추가
                        AddPassiveItemIcon(item, newLevel);
                    }
                    break;
                }
            }
        }

        private void OnHpChangedHandler(int current, int max)
        {
            UpdateHPBar();
        }

        private void OnBuffsChangedHandler(System.Collections.Generic.IReadOnlyList<Game.SkillCardSystem.Interface.IPerTurnEffect> effects)
        {
            if (buffDebuffParent == null) return;
            // 모두 제거 후 다시 구성(간단/안전)
            foreach (Transform child in buffDebuffParent)
            {
                if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
            }

            foreach (var e in effects)
            {
                if (e.Icon == null)
                {
                    Game.CoreSystem.Utility.GameLogger.LogWarning("[PlayerCharacterUI] 효과 아이콘이 비어 있습니다. SO에 Sprite가 지정되었는지 확인하세요.", Game.CoreSystem.Utility.GameLogger.LogCategory.UI);
                }
                var slotObj = Instantiate(buffDebuffIconPrefab, buffDebuffParent);
                var view = slotObj.GetComponent<BuffDebuffSlotView>();
                if (view != null)
                {
                    // 효과 데이터를 설정하여 툴팁 기능 활성화
                    view.SetEffectData(e);
                }
                else
                {
                    // 최소 폴백: Image에 직접 아이콘만 지정
                    var img = slotObj.GetComponent<UnityEngine.UI.Image>();
                    if (img != null) img.sprite = e.Icon;
                }
            }
        }

        /// <summary>
        /// 캐릭터 기본 정보를 설정합니다.
        /// </summary>
        private void SetCharacterInfo()
        {
            if (playerCharacter?.CharacterData == null) return;

            var data = playerCharacter.CharacterData as PlayerCharacterData;
            if (data == null) return;
            
            // 캐릭터 이름
            if (characterNameText != null)
                characterNameText.text = data.DisplayName;
            
            // 캐릭터 초상화 (UI 전용)
            if (characterPortrait != null)
            {
                var uiPortrait = data.PlayerUIPortrait;
                if (uiPortrait != null)
                {
                    characterPortrait.sprite = uiPortrait;
                    characterPortrait.preserveAspect = true; // 비율만 유지
                    // RectTransform(위치/크기/앵커/피벗)은 변경하지 않음
                }
            }
            
            // 캐릭터 문양 (데이터에서 직접 설정)
            if (characterEmblem != null)
                SetCharacterEmblem(data);
        }

        /// <summary>
        /// 캐릭터 데이터에서 문양을 설정합니다.
        /// </summary>
        /// <param name="data">플레이어 캐릭터 데이터</param>
        private void SetCharacterEmblem(PlayerCharacterData data)
        {
            if (characterEmblem == null) return;

            // 데이터에서 직접 문양 설정
            if (data.Emblem != null)
            {
                characterEmblem.sprite = data.Emblem;
            }
            else
            {
                // 문양이 설정되지 않은 경우 기본 문양 로드 시도
                SetCharacterEmblemFallback(data.CharacterType);
            }
        }

        /// <summary>
        /// 문양이 설정되지 않은 경우 기본 문양을 로드합니다.
        /// </summary>
        /// <param name="type">캐릭터 타입</param>
        private void SetCharacterEmblemFallback(PlayerCharacterType type)
        {
            // 타입별 기본 문양 스프라이트 로드 (Resources 폴더에서)
            string emblemPath = $"CharacterEmblems/{type}Emblem";
            Sprite emblemSprite = Resources.Load<Sprite>(emblemPath);
            
            if (emblemSprite != null)
            {
                characterEmblem.sprite = emblemSprite;
            }
            else
            {
                GameLogger.LogWarning($"[PlayerCharacterUIController] 기본 문양 스프라이트를 찾을 수 없습니다: {emblemPath}", GameLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// 리소스 시스템을 설정합니다.
        /// </summary>
        private void SetupResourceSystem()
        {
            switch (characterType)
            {
                case PlayerCharacterType.Sword:
                    // 검 캐릭터는 리소스 없음 - 핍은 1개 회색으로 표시
					usePipResource = false;
					if (resourcePipParent != null)
					{
						resourcePipParent.gameObject.SetActive(true);
						int count = Mathf.Max(1, GetMaxResourceFromData());
						RebuildPips(count);
						UpdateUnavailablePips();
					}
                    break;
                    
                case PlayerCharacterType.Bow:
                case PlayerCharacterType.Staff:
                    // 활/지팡이 캐릭터는 리소스 있음 - 핍 방식 사용
					usePipResource = true;
					if (resourcePipParent != null)
					{
						resourcePipParent.gameObject.SetActive(true);
						int count = Mathf.Max(1, GetMaxResourceFromData());
						RebuildPips(count);
					}
                    break;
            }
        }

        #endregion

        #region HP 바 업데이트

        /// <summary>
        /// HP 바를 업데이트합니다.
        /// </summary>
        public void UpdateHPBar()
        {
            if (playerCharacter == null || hpBarFill == null) return;

            int currentHP = playerCharacter.GetCurrentHP();
            int maxHP = playerCharacter.GetMaxHP();
            
            if (maxHP <= 0) return;

            float hpRatio = (float)currentHP / maxHP;
            
            // HP 바 애니메이션
            AnimateHPBar(hpRatio);
            
            // HP 텍스트 업데이트
            UpdateHPText(currentHP, maxHP);
            
            // HP 바 색상 업데이트
            UpdateHPBarColor(hpRatio);
        }

        /// <summary>
        /// HP 바 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="targetRatio">목표 비율</param>
        private void AnimateHPBar(float targetRatio)
        {
            hpBarTween?.Kill();
            hpBarTween = DOTween.To(() => hpBarFill.fillAmount, x => hpBarFill.fillAmount = x, 
                targetRatio, 1f / barAnimationSpeed);
        }

        /// <summary>
        /// HP 텍스트를 업데이트합니다.
        /// </summary>
        /// <param name="current">현재 HP</param>
        /// <param name="max">최대 HP</param>
        private void UpdateHPText(int current, int max)
        {
            if (hpText != null)
                hpText.text = $"{current}/{max}";
        }

        /// <summary>
        /// HP 바 색상을 업데이트합니다.
        /// </summary>
        /// <param name="hpRatio">HP 비율</param>
        private void UpdateHPBarColor(float hpRatio)
        {
            Color targetColor;
            
            if (hpRatio > 0.6f)
                targetColor = fullHPColor;
            else if (hpRatio > 0.3f)
                targetColor = midHPColor;
            else
                targetColor = lowHPColor;

            colorTween?.Kill();
            colorTween = DOTween.To(() => hpBarFill.color, x => hpBarFill.color = x, 
                targetColor, 1f / colorAnimationSpeed);
        }

        #endregion

        #region MP/리소스 바 업데이트

        /// <summary>
        /// MP/리소스 바를 업데이트합니다.
        /// </summary>
		public void UpdateMPBar()
        {
            if (playerCharacter == null || playerManager == null) return;
            
            // 검 캐릭터는 리소스가 없음
            if (characterType == PlayerCharacterType.Sword)
            {
                UpdateUnavailablePips();
                return;
            }

            int currentResource = playerManager.CurrentResource;
            int maxResource = playerManager.MaxResource;
            
            if (maxResource <= 0) return;
			
			if (usePipResource)
			{
				// 데이터의 MaxResource 기반으로 갱신
				if (resourcePipParent != null && resourcePipPrefab != null)
				{
					int desired = Mathf.Max(1, maxResource);
					if (resourcePips.Count != desired)
						RebuildPips(desired);
					UpdateResourcePips(currentResource, desired);
				}
			}
			else
			{
				// 검 캐릭터: 회색 1칸 유지
				UpdateUnavailablePips();
			}
        }
		
		/// <summary>
		/// 지정 개수로 리소스 핍을 다시 생성합니다.
		/// </summary>
		private void RebuildPips(int count)
		{
			if (resourcePipParent == null || resourcePipPrefab == null) return;
			// 모두 제거
			for (int i = resourcePipParent.childCount - 1; i >= 0; i--)
			{
				var child = resourcePipParent.GetChild(i);
				if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
			}
			resourcePips.Clear();
			// 지정 개수 생성(최소 1)
			int create = Mathf.Max(1, count);
			for (int i = 0; i < create; i++)
			{
				var obj = Instantiate(resourcePipPrefab, resourcePipParent);
				var img = obj.GetComponent<Image>();
				if (img != null) resourcePips.Add(img);
			}
		}

		/// <summary>
		/// 리소스 핍의 on/off를 갱신합니다.
		/// </summary>
		private void UpdateResourcePips(int current, int max)
		{
			if (resourcePips.Count == 0) return;
            var activeColor = resourceActiveColor;
			var inactiveColor = pipInactiveColor;
			for (int i = 0; i < resourcePips.Count; i++)
			{
				var img = resourcePips[i];
				if (img == null) continue;
				img.color = i < current ? activeColor : inactiveColor;
			}
		}

		/// <summary>
		/// 검 캐릭터: 모든 핍을 회색으로 설정해 비활성 상태 표현.
		/// </summary>
		private void UpdateUnavailablePips()
		{
			if (resourcePips.Count == 0)
			{
				RebuildPips(1);
			}
			for (int i = 0; i < resourcePips.Count; i++)
			{
				var img = resourcePips[i];
				if (img == null) continue;
				img.color = pipUnavailableColor;
			}
		}

		/// <summary>
		/// 데이터/매니저에서 최대 리소스를 가져옵니다.
		/// </summary>
		private int GetMaxResourceFromData()
		{
			if (playerManager != null && playerManager.MaxResource > 0)
				return playerManager.MaxResource;
			var data = playerCharacter?.CharacterData as PlayerCharacterData;
			return data != null ? Mathf.Max(0, data.MaxResource) : 0;
		}

        #endregion

        #region 버프/디버프 시스템

        /// <summary>
        /// 버프/디버프 아이콘을 추가합니다.
        /// </summary>
        /// <param name="effectId">효과 ID</param>
        /// <param name="iconSprite">아이콘 스프라이트</param>
        /// <param name="isBuff">버프 여부 (true: 버프, false: 디버프)</param>
        /// <param name="duration">지속 시간 (초, -1이면 영구)</param>
        public void AddBuffDebuffIcon(string effectId, Sprite iconSprite, bool isBuff, float duration = -1f)
        {
            if (buffDebuffParent == null || buffDebuffIconPrefab == null) return;

            // 이미 같은 효과가 있으면 제거
            RemoveBuffDebuffIcon(effectId);

            // 새 아이콘 생성
            GameObject iconObj = Instantiate(buffDebuffIconPrefab, buffDebuffParent);
            var iconImage = iconObj.GetComponent<Image>();
            var iconText = iconObj.GetComponentInChildren<TextMeshProUGUI>();

            if (iconImage != null)
                iconImage.sprite = iconSprite;
            
            if (iconText != null && duration > 0)
                iconText.text = duration.ToString("F0");

            // 색상 설정 (버프: 파란색, 디버프: 빨간색)
            if (iconImage != null)
                iconImage.color = isBuff ? Color.blue : Color.red;

            // 딕셔너리에 저장
            activeBuffDebuffIcons[effectId] = iconObj;

        }

        /// <summary>
        /// 버프/디버프 아이콘을 제거합니다.
        /// </summary>
        /// <param name="effectId">효과 ID</param>
        public void RemoveBuffDebuffIcon(string effectId)
        {
            if (activeBuffDebuffIcons.TryGetValue(effectId, out GameObject iconObj))
            {
                Destroy(iconObj);
                activeBuffDebuffIcons.Remove(effectId);
            }
        }

        /// <summary>
        /// 모든 버프/디버프 아이콘을 제거합니다.
        /// </summary>
        public void ClearAllBuffDebuffIcons()
        {
            foreach (var icon in activeBuffDebuffIcons.Values)
            {
                if (icon != null)
                    Destroy(icon);
            }
            activeBuffDebuffIcons.Clear();
        }

        #endregion

        #region 패시브 아이템 시스템

        /// <summary>
        /// 패시브 아이템 아이콘을 추가합니다.
        /// </summary>
        /// <param name="itemDefinition">패시브 아이템 정의</param>
        /// <param name="enhancementLevel">강화 단계 (0-3, 0 = 강화 안됨)</param>
        public void AddPassiveItemIcon(PassiveItemDefinition itemDefinition, int enhancementLevel = 0)
        {
            if (passiveItemParent == null || passiveItemIconPrefab == null || itemDefinition == null)
            {
                GameLogger.LogWarning("[PlayerCharacterUIController] 패시브 아이템 아이콘을 추가할 수 없습니다 (컴포넌트 없음)", GameLogger.LogCategory.Character);
                return;
            }

            string itemId = itemDefinition.ItemId;
            if (string.IsNullOrEmpty(itemId))
            {
                GameLogger.LogError("[PlayerCharacterUIController] 패시브 아이템 ID가 비어있습니다", GameLogger.LogCategory.Character);
                return;
            }

            // 이미 같은 아이템이 있으면 강화 단계만 업데이트
            if (activePassiveItemIcons.TryGetValue(itemId, out PassiveItemIcon existingIcon))
            {
                existingIcon.UpdateEnhancementLevel(enhancementLevel);
                return;
            }

            // 새 아이콘 생성
            GameObject iconObj = Instantiate(passiveItemIconPrefab, passiveItemParent);
            PassiveItemIcon iconComponent = iconObj.GetComponent<PassiveItemIcon>();
            
            if (iconComponent == null)
            {
                GameLogger.LogError("[PlayerCharacterUIController] PassiveItemIcon 컴포넌트를 찾을 수 없습니다", GameLogger.LogCategory.Character);
                Destroy(iconObj);
                return;
            }

            iconComponent.SetupIcon(itemDefinition, enhancementLevel);

            // 딕셔너리에 저장
            activePassiveItemIcons[itemId] = iconComponent;

        }

        /// <summary>
        /// 패시브 아이템 아이콘을 제거합니다.
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        public void RemovePassiveItemIcon(string itemId)
        {
            if (activePassiveItemIcons.TryGetValue(itemId, out PassiveItemIcon icon))
            {
                icon.FadeOut(() => {
                    if (icon != null && icon.gameObject != null)
                        Destroy(icon.gameObject);
                });
                activePassiveItemIcons.Remove(itemId);
            }
        }

        /// <summary>
        /// 모든 패시브 아이템 아이콘을 제거합니다.
        /// </summary>
        public void ClearAllPassiveItemIcons()
        {
            foreach (var icon in activePassiveItemIcons.Values)
            {
                if (icon != null && icon.gameObject != null)
                    Destroy(icon.gameObject);
            }
            activePassiveItemIcons.Clear();
        }

        /// <summary>
        /// 패시브 아이템 아이콘을 새로고침합니다.
        /// ItemService에서 현재 보유한 패시브 아이템을 가져와 표시합니다.
        /// </summary>
        public void RefreshPassiveItemIcons()
        {
            if (itemService == null)
            {
                GameLogger.LogWarning("[PlayerCharacterUIController] ItemService가 주입되지 않았습니다", GameLogger.LogCategory.Character);
                return;
            }

            ClearAllPassiveItemIcons();

            var passiveItems = itemService.GetPassiveItems();
            if (passiveItems == null || passiveItems.Count == 0)
            {
                return;
            }

            foreach (var item in passiveItems)
            {
                if (item == null || string.IsNullOrEmpty(item.ItemId))
                    continue;

                // 강화 단계 계산 (같은 아이템이 여러 번 추가될 수 있으므로)
                int enhancementLevel = 0; // 기본값 0 (강화 안됨)
                
                // 아이템의 타겟 스킬 ID를 기반으로 강화 단계 확인
                string skillId = null;
                if (item.IsPlayerHealthBonus)
                {
                    skillId = $"__PLAYER_HP__:{item.ItemId}";
                }
                else if (item.TargetSkill != null)
                {
                    skillId = !string.IsNullOrEmpty(item.TargetSkill.displayName) ? item.TargetSkill.displayName : item.TargetSkill.cardId;
                }

                if (!string.IsNullOrEmpty(skillId))
                {
                    enhancementLevel = itemService.GetSkillEnhancementLevel(skillId);
                    // 강화 레벨이 0보다 작으면 0으로 설정 (이미 0 이상이므로 사실상 불필요하지만 안전장치)
                    if (enhancementLevel < 0)
                        enhancementLevel = 0;
                }

                AddPassiveItemIcon(item, enhancementLevel);
            }

        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 캐릭터가 데미지를 받았을 때 호출됩니다.
        /// </summary>
        /// <param name="damage">받은 데미지</param>
        public void OnTakeDamage(int damage)
        {
            UpdateHPBar();
            
            // 데미지 텍스트 표시 (선택사항)
            ShowDamageText(damage);
        }

        /// <summary>
        /// 캐릭터가 힐을 받았을 때 호출됩니다.
        /// </summary>
        /// <param name="healAmount">회복량</param>
        public void OnHeal(int healAmount)
        {
            UpdateHPBar();
            
            // 힐 텍스트 표시 (선택사항)
            ShowHealText(healAmount);
        }

        /// <summary>
        /// 리소스가 변경되었을 때 호출됩니다.
        /// </summary>
        /// <param name="oldAmount">이전 리소스량</param>
        /// <param name="newAmount">새 리소스량</param>
        public void OnResourceChanged(int oldAmount, int newAmount)
        {
            UpdateMPBar();
        }

        /// <summary>
        /// PlayerManager 리소스 변경 이벤트 핸들러 (현재/최대 전달)
        /// </summary>
        /// <param name="current">현재 리소스</param>
        /// <param name="max">최대 리소스</param>
        private void OnResourceChangedByManager(int current, int max)
        {
            UpdateMPBar();
        }

        /// <summary>
        /// 모든 UI를 초기화합니다.
        /// </summary>
        public void ClearUI()
        {
            playerCharacter = null;
            playerManager = null;
            
            // 텍스트 초기화
            if (characterNameText != null)
                characterNameText.text = "";
            
            if (hpText != null)
                hpText.text = "";
            
            // 리소스 텍스트는 사용하지 않음
            
            // 이미지 초기화
            if (characterPortrait != null)
                characterPortrait.sprite = null;
            
            if (characterEmblem != null)
                characterEmblem.sprite = null;
            
            // 바 초기화
            if (hpBarFill != null)
                hpBarFill.fillAmount = 1f;
            
            // 리소스 바는 사용하지 않음
            
            // 버프/디버프 아이콘 초기화
            ClearAllBuffDebuffIcons();
            
        }

        #endregion

        #region 헬퍼 메서드

        /// <summary>
        /// 데미지 텍스트를 표시합니다.
        /// </summary>
        /// <param name="damage">데미지량</param>
        private void ShowDamageText(int damage)
        {
            // TODO: 데미지 텍스트 표시 로직 구현
            GameLogger.LogInfo($"[PlayerCharacterUIController] 데미지: {damage}", GameLogger.LogCategory.Character);
        }

        /// <summary>
        /// 힐 텍스트를 표시합니다.
        /// </summary>
        /// <param name="healAmount">회복량</param>
        private void ShowHealText(int healAmount)
        {
            // TODO: 힐 텍스트 표시 로직 구현
            GameLogger.LogInfo($"[PlayerCharacterUIController] 회복: {healAmount}", GameLogger.LogCategory.Character);
        }

        #endregion
    }
}
