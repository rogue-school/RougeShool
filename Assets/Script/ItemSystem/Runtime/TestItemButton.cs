using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Game.ItemSystem.Service;
using Game.ItemSystem.Data;
using Game.ItemSystem.Interface;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.ItemSystem.Runtime
{
    /// <summary>
    /// 테스트용 아이템 지급 버튼
    /// 버튼을 누르면 랜덤 액티브 아이템을 1개 지급합니다
    /// </summary>
    public class TestItemButton : MonoBehaviour
    {
        [Header("테스트 설정")]
        [Tooltip("아이템 리소스 경로")]
        [SerializeField] private string itemResourcePath = "Data/Item";
        
        [Tooltip("버튼 컴포넌트 (자동 할당)")]
        [SerializeField] private Button button;
        
        [Tooltip("버튼 텍스트")]
        [SerializeField] private Text buttonText;

        // 의존성 주입
        [Inject] private IItemService itemService;

        private void Awake()
        {
            // 버튼 컴포넌트 자동 할당
            if (button == null)
                button = GetComponent<Button>();
            
            if (buttonText == null)
                buttonText = GetComponentInChildren<Text>();
        }

        private void Start()
        {
            // 버튼 이벤트 연결
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
                GameLogger.LogInfo("[TestItemButton] 테스트 아이템 버튼 초기화 완료", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogError("[TestItemButton] Button 컴포넌트를 찾을 수 없습니다", GameLogger.LogCategory.UI);
            }

            // 버튼 텍스트 설정
            if (buttonText != null)
            {
                buttonText.text = "테스트 아이템 지급";
            }
        }

        private void OnDestroy()
        {
            // 이벤트 해제
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 메서드
        /// </summary>
        private void OnButtonClicked()
        {
            GameLogger.LogInfo("[TestItemButton] 테스트 아이템 지급 버튼 클릭됨", GameLogger.LogCategory.UI);
            
            // 랜덤 아이템 생성 및 지급
            GiveRandomItem();
        }

        /// <summary>
        /// 랜덤 아이템을 생성하고 지급합니다
        /// </summary>
        private void GiveRandomItem()
        {
            if (itemService == null)
            {
                GameLogger.LogError("[TestItemButton] ItemService가 주입되지 않았습니다", GameLogger.LogCategory.UI);
                return;
            }

            // 인벤토리가 가득 찬 경우
            if (itemService.IsActiveInventoryFull())
            {
                GameLogger.LogWarning("[TestItemButton] 인벤토리가 가득 찼습니다. 아이템을 지급할 수 없습니다", GameLogger.LogCategory.UI);
                return;
            }

            // 랜덤 아이템 생성
            var randomItem = GenerateRandomItem();
            
            if (randomItem != null)
            {
                // 아이템 지급
                bool success = itemService.AddActiveItem(randomItem);
                
                if (success)
                {
                    GameLogger.LogInfo($"[TestItemButton] 아이템 지급 성공: {randomItem.DisplayName}", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogError($"[TestItemButton] 아이템 지급 실패: {randomItem.DisplayName}", GameLogger.LogCategory.UI);
                }
            }
            else
            {
                GameLogger.LogError("[TestItemButton] 랜덤 아이템 생성 실패", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 랜덤 아이템을 생성합니다
        /// </summary>
        /// <returns>생성된 아이템 정의</returns>
        private ActiveItemDefinition GenerateRandomItem()
        {
            try
            {
                // Addressables에서 모든 액티브 아이템 로드
                var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<ActiveItemDefinition>(itemResourcePath, null);
                var result = handle.WaitForCompletion();
                var allItems = result != null ? result.ToArray() : new ActiveItemDefinition[0];
                
                if (allItems == null || allItems.Length == 0)
                {
                    GameLogger.LogError($"[TestItemButton] 아이템을 찾을 수 없습니다. 경로: {itemResourcePath}", GameLogger.LogCategory.UI);
                    return null;
                }

                // 랜덤 선택
                int randomIndex = Random.Range(0, allItems.Length);
                var selectedItem = allItems[randomIndex];
                
                GameLogger.LogInfo($"[TestItemButton] 랜덤 아이템 선택: {selectedItem.DisplayName} (인덱스: {randomIndex})", GameLogger.LogCategory.UI);
                
                return selectedItem;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[TestItemButton] 아이템 생성 중 오류 발생: {ex.Message}", GameLogger.LogCategory.UI);
                return null;
            }
        }
    }
}
