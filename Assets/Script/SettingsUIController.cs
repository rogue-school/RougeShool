using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject settingsPanel;   // MainCanvas/SettingsPanel
    [SerializeField] private GameObject confirmDialog;   // MainCanvas/ConfirmDialog

    [Header("First-Select (Optional)")]
    [SerializeField] private GameObject firstSelectOnOpen;      // SettingsPanel 열릴 때 포커스할 버튼 (ContinueButton)
    [SerializeField] private GameObject firstSelectOnConfirm;   // ConfirmDialog 열릴 때 포커스할 버튼 (ConfirmButton)

    private bool settingsOpen = false;
    private bool confirmOpen = false;

    private void Awake()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (confirmDialog != null) confirmDialog.SetActive(false);
    }

    // ? 버튼 눌렀을 때
    public void OpenSettings()
    {
        if (settingsOpen) return;

        settingsOpen = true;
        settingsPanel.SetActive(true);
        confirmDialog.SetActive(false);
        confirmOpen = false;

        GameManager.Instance.PauseGame();

        // UI 포커스
        if (firstSelectOnOpen != null)
            EventSystem.current?.SetSelectedGameObject(firstSelectOnOpen);
    }

    // 계속하기
    public void OnContinue()
    {
        if (!settingsOpen) return;

        settingsPanel.SetActive(false);
        confirmDialog.SetActive(false);
        confirmOpen = false;
        settingsOpen = false;

        GameManager.Instance.ResumeGame();
    }

    // 포기하기 (확인창 열기)
    public void OnGiveUp()
    {
        if (!settingsOpen) return;

        // 설정 패널은 그대로 둔 채로 확인창만 띄움(모달)
        confirmOpen = true;
        confirmDialog.SetActive(true);

        // 포커스 이동
        if (firstSelectOnConfirm != null)
            EventSystem.current?.SetSelectedGameObject(firstSelectOnConfirm);
    }

    // 확인창: 예, 포기합니다
    public void OnConfirmGiveUp()
    {
        // 이미 일시정지 중이므로 timeScale=0 → 메인으로 갈 때 복구됨
        GameManager.Instance.ResetSession();
        GameManager.Instance.GoToMainMenu();
        // 씬 이동 시 UI는 사라지므로 별도 정리 불필요
    }

    // 확인창: 아니오
    public void OnCancelGiveUp()
    {
        confirmOpen = false;
        confirmDialog.SetActive(false);

        // 설정 패널은 열려있는 상태 유지
        if (firstSelectOnOpen != null)
            EventSystem.current?.SetSelectedGameObject(firstSelectOnOpen);
    }

    // 그만하기 (초기화 없이 메인으로)
    public void OnQuit()
    {
        GameManager.Instance.GoToMainMenu();
    }

    // ESC 키 토글 (선택 사항)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 확인창이 떠 있으면 닫고 설정 패널 유지
            if (confirmOpen)
            {
                OnCancelGiveUp();
                return;
            }

            if (!settingsOpen)
            {
                OpenSettings();
            }
            else
            {
                OnContinue(); // 설정 닫고 재개
            }
        }
    }
}
