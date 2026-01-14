using UnityEngine;
using UnityEngine.EventSystems;
using Game.CoreSystem.Interface;
using Zenject;

public class SettingsUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject settingsPanel;   // MainCanvas/SettingsPanel
    [SerializeField] private GameObject confirmDialog;   // MainCanvas/ConfirmDialog

    [Header("First-Select (Optional)")]
    [SerializeField] private GameObject firstSelectOnOpen;      // SettingsPanel ���� �� ��Ŀ���� ��ư (ContinueButton)
    [SerializeField] private GameObject firstSelectOnConfirm;   // ConfirmDialog ���� �� ��Ŀ���� ��ư (ConfirmButton)

    private bool settingsOpen = false;
    private bool confirmOpen = false;
    
    // 의존성 주입
    [Inject] private IGameStateManager gameStateManager;

    private void Awake()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (confirmDialog != null) confirmDialog.SetActive(false);
    }

    // ? ��ư ������ ��
    public void OpenSettings()
    {
        if (settingsOpen) return;

        settingsOpen = true;
        settingsPanel.SetActive(true);
        confirmDialog.SetActive(false);
        confirmOpen = false;

        gameStateManager.PauseGame();

        // UI ��Ŀ��
        if (firstSelectOnOpen != null)
            EventSystem.current?.SetSelectedGameObject(firstSelectOnOpen);
    }

    // ����ϱ�
    public void OnContinue()
    {
        if (!settingsOpen) return;

        settingsPanel.SetActive(false);
        confirmDialog.SetActive(false);
        confirmOpen = false;
        settingsOpen = false;

        gameStateManager.ResumeGame();
    }

    // �����ϱ� (Ȯ��â ����)
    public void OnGiveUp()
    {
        if (!settingsOpen) return;

        // ���� �г��� �״�� �� ä�� Ȯ��â�� ���(���)
        confirmOpen = true;
        confirmDialog.SetActive(true);

        // ��Ŀ�� �̵�
        if (firstSelectOnConfirm != null)
            EventSystem.current?.SetSelectedGameObject(firstSelectOnConfirm);
    }

    // Ȯ��â: ��, �����մϴ�
    public void OnConfirmGiveUp()
    {
        // �̹� �Ͻ����� ���̹Ƿ� timeScale=0 �� �������� �� �� ������
        gameStateManager.ResetSession();
        gameStateManager.GoToMainMenu();
        // �� �̵� �� UI�� ������Ƿ� ���� ���� ���ʿ�
    }

    // Ȯ��â: �ƴϿ�
    public void OnCancelGiveUp()
    {
        confirmOpen = false;
        confirmDialog.SetActive(false);

        // ���� �г��� �����ִ� ���� ����
        if (firstSelectOnOpen != null)
            EventSystem.current?.SetSelectedGameObject(firstSelectOnOpen);
    }

    // �׸��ϱ� (�ʱ�ȭ ���� ��������)
    public void OnQuit()
    {
        gameStateManager.GoToMainMenu();
    }

    // ESC Ű ��� (���� ����)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Ȯ��â�� �� ������ �ݰ� ���� �г� ����
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
                OnContinue(); // ���� �ݰ� �簳
            }
        }
    }
}
