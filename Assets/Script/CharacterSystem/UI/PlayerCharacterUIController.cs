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
        
        [Tooltip("Resource 바 배경")]
        [SerializeField] private Image resourceBarBackground;
        
        [Tooltip("Resource 바 채움 부분")]
        [SerializeField] private Image resourceBarFill;
        

        [Header("HP/Resource 텍스트")]
        [Tooltip("HP 텍스트 (현재/최대)")]
        [SerializeField] private TextMeshProUGUI hpText;
        
        [Tooltip("Resource 텍스트 (현재/최대)")]
        [SerializeField] private TextMeshProUGUI resourceText;

        [Header("버프/디버프 아이콘")]
        [Tooltip("버프/디버프 아이콘들을 담을 부모 오브젝트")]
        [SerializeField] private Transform buffDebuffParent;
        
        [Tooltip("버프/디버프 아이콘 프리팹")]
        [SerializeField] private GameObject buffDebuffIconPrefab;

        [Header("색상 설정")]
        [Tooltip("풀피일 때 HP 바 색상")]
        [SerializeField] private Color fullHPColor = Color.green;
        
        [Tooltip("저체력일 때 HP 바 색상")]
        [SerializeField] private Color lowHPColor = Color.red;
        
        [Tooltip("중간 체력일 때 HP 바 색상")]
        [SerializeField] private Color midHPColor = Color.yellow;
        
        [Tooltip("MP 바 색상")]
        [SerializeField] private Color mpColor = Color.blue;

        [Header("애니메이션 설정")]
        [Tooltip("HP/MP 바 애니메이션 속도")]
        [SerializeField] private float barAnimationSpeed = 2f;
        
        [Tooltip("색상 변화 애니메이션 속도")]
        [SerializeField] private float colorAnimationSpeed = 1f;

        #endregion

        #region Private Fields

        private PlayerCharacter playerCharacter;
        [InjectOptional] private PlayerManager playerManager;
        private PlayerCharacterType characterType;
        
        // 애니메이션 관련
        private Tween hpBarTween;
        private Tween resourceBarTween;
        private Tween colorTween;
        
        // 버프/디버프 아이콘 관리
        private System.Collections.Generic.Dictionary<string, GameObject> activeBuffDebuffIcons = new();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeUI();
        }

        private void Update()
        {
            // 실시간 업데이트는 필요에 따라 호출
        }

        private void OnDestroy()
        {
            // DOTween 정리
            hpBarTween?.Kill();
            resourceBarTween?.Kill();
            colorTween?.Kill();
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
                hpBarFill.fillAmount = 1f;
            
            if (resourceBarFill != null)
                resourceBarFill.fillAmount = 1f;
            
        }

        /// <summary>
        /// 플레이어 캐릭터와 연결하여 UI를 초기화합니다.
        /// </summary>
        /// <param name="character">연결할 플레이어 캐릭터</param>
        public void Initialize(PlayerCharacter character)
        {
            if (character == null)
            {
                Debug.LogWarning("[PlayerCharacterUIController] Initialize() - character가 null입니다.");
                return;
            }

            playerCharacter = character;
            characterType = character.CharacterData.CharacterType;
            
            // 리소스 매니저는 Zenject DI로 주입됨
            if (playerManager == null)
            {
                Debug.LogWarning("[PlayerCharacterUIController] PlayerManager가 주입되지 않았습니다.");
            }

            // 캐릭터 정보 설정
            SetCharacterInfo();
            
            // 리소스 시스템 설정
            SetupResourceSystem();
            
            // 초기 HP/MP 업데이트
            UpdateHPBar();
            UpdateMPBar();
            
            Debug.Log($"[PlayerCharacterUIController] {characterType} 캐릭터 UI 초기화 완료");
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
                Debug.LogWarning("[PlayerCharacterUIController] SetTarget: PlayerCharacter가 아닙니다. 호출을 무시합니다.");
            }
        }

        /// <summary>
        /// 캐릭터 기본 정보를 설정합니다.
        /// </summary>
        private void SetCharacterInfo()
        {
            if (playerCharacter?.CharacterData == null) return;

            var data = playerCharacter.CharacterData;
            
            // 캐릭터 이름
            if (characterNameText != null)
                characterNameText.text = data.DisplayName;
            
            // 캐릭터 초상화
            if (characterPortrait != null && data.Portrait != null)
                characterPortrait.sprite = data.Portrait;
            
            // 캐릭터 문양 (타입별 아이콘)
            if (characterEmblem != null)
                SetCharacterEmblem(characterType);
        }

        /// <summary>
        /// 캐릭터 타입에 따른 문양을 설정합니다.
        /// </summary>
        /// <param name="type">캐릭터 타입</param>
        private void SetCharacterEmblem(PlayerCharacterType type)
        {
            if (characterEmblem == null) return;

            // 타입별 문양 스프라이트 로드 (Resources 폴더에서)
            string emblemPath = $"CharacterEmblems/{type}Emblem";
            Sprite emblemSprite = Resources.Load<Sprite>(emblemPath);
            
            if (emblemSprite != null)
            {
                characterEmblem.sprite = emblemSprite;
            }
            else
            {
                Debug.LogWarning($"[PlayerCharacterUIController] 문양 스프라이트를 찾을 수 없습니다: {emblemPath}");
            }
        }

        /// <summary>
        /// 리소스 시스템을 설정합니다.
        /// </summary>
        private void SetupResourceSystem()
        {
            if (resourceBarBackground == null || resourceBarFill == null) return;

            switch (characterType)
            {
                case PlayerCharacterType.Sword:
                    // 검 캐릭터는 리소스 없음 - 리소스 바 숨김
                    resourceBarBackground.gameObject.SetActive(false);
                    resourceBarFill.gameObject.SetActive(false);
                    break;
                    
                case PlayerCharacterType.Bow:
                case PlayerCharacterType.Staff:
                    // 활/지팡이 캐릭터는 리소스 있음 - 리소스 바 표시
                    resourceBarBackground.gameObject.SetActive(true);
                    resourceBarFill.gameObject.SetActive(true);
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
                return;

            int currentResource = playerManager.CurrentResource;
            int maxResource = playerManager.MaxResource;
            
            if (maxResource <= 0) return;

            float mpRatio = (float)currentResource / maxResource;
            
            // Resource 바 애니메이션
            AnimateResourceBar(mpRatio);
            
            // Resource 텍스트 업데이트
            UpdateResourceText(currentResource, maxResource);
        }

        /// <summary>
        /// Resource 바 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="targetRatio">목표 비율</param>
        private void AnimateResourceBar(float targetRatio)
        {
            if (resourceBarFill == null) return;
            
            resourceBarTween?.Kill();
            resourceBarTween = DOTween.To(() => resourceBarFill.fillAmount, x => resourceBarFill.fillAmount = x, 
                targetRatio, 1f / barAnimationSpeed);
        }

        /// <summary>
        /// Resource 텍스트를 업데이트합니다.
        /// </summary>
        /// <param name="current">현재 리소스</param>
        /// <param name="max">최대 리소스</param>
        private void UpdateResourceText(int current, int max)
        {
            if (resourceText != null)
            {
                string resourceName = GetResourceName();
                resourceText.text = $"{current}/{max}";
            }
        }

        /// <summary>
        /// 캐릭터 타입에 따른 리소스 이름을 반환합니다.
        /// </summary>
        /// <returns>리소스 이름</returns>
        private string GetResourceName()
        {
            return characterType switch
            {
                PlayerCharacterType.Bow => "화살",
                PlayerCharacterType.Staff => "마나",
                _ => "리소스"
            };
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

            Debug.Log($"[PlayerCharacterUIController] {(isBuff ? "버프" : "디버프")} 아이콘 추가: {effectId}");
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
                Debug.Log($"[PlayerCharacterUIController] 버프/디버프 아이콘 제거: {effectId}");
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
            Debug.Log("[PlayerCharacterUIController] 모든 버프/디버프 아이콘 제거");
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
            
            if (resourceText != null)
                resourceText.text = "";
            
            // 이미지 초기화
            if (characterPortrait != null)
                characterPortrait.sprite = null;
            
            if (characterEmblem != null)
                characterEmblem.sprite = null;
            
            // 바 초기화
            if (hpBarFill != null)
                hpBarFill.fillAmount = 1f;
            
            if (resourceBarFill != null)
                resourceBarFill.fillAmount = 1f;
            
            // 버프/디버프 아이콘 초기화
            ClearAllBuffDebuffIcons();
            
            Debug.Log("[PlayerCharacterUIController] UI 초기화 완료");
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
            Debug.Log($"[PlayerCharacterUIController] 데미지: {damage}");
        }

        /// <summary>
        /// 힐 텍스트를 표시합니다.
        /// </summary>
        /// <param name="healAmount">회복량</param>
        private void ShowHealText(int healAmount)
        {
            // TODO: 힐 텍스트 표시 로직 구현
            Debug.Log($"[PlayerCharacterUIController] 회복: {healAmount}");
        }

        #endregion
    }
}
