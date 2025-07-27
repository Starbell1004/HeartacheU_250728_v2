using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HomeScreenModeSettings : MonoBehaviour
{
    [Header("ȭ�� ��� ����")]
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Button windowedButton;

    [Header("��������Ʈ ����")]
    [SerializeField] private Sprite fullscreenActiveSprite;   // ��üȭ�� ��ũ
    [SerializeField] private Sprite fullscreenInactiveSprite; // ��üȭ�� ȸ��
    [SerializeField] private Sprite windowActiveSprite;       // â��� ��ũ
    [SerializeField] private Sprite windowInactiveSprite;     // â��� ȸ��

    [Header("â��� ũ��")]
    [SerializeField] private int windowWidth = 1280;
    [SerializeField] private int windowHeight = 720;

    private Image fullscreenImg;
    private Image windowedImg;
    private bool isChangingMode = false; // ��� ��ȯ �� �÷���
    private VideoLoopMonitor videoMonitor; // ���� ����� �߰�

    private void Start()
    {
        Debug.Log("[HomeScreenModeSettings] Start ȣ��");
        InitializeComponents();
        SetupButtons();
        RefreshButtonStates();

        // VideoLoopMonitor ã��
        videoMonitor = FindObjectOfType<VideoLoopMonitor>();
    }

    private void InitializeComponents()
    {
        if (fullscreenButton != null)
            fullscreenImg = fullscreenButton.GetComponent<Image>();
        if (windowedButton != null)
            windowedImg = windowedButton.GetComponent<Image>();

        if (fullscreenImg == null || windowedImg == null)
        {
            Debug.LogError("[HomeScreenModeSettings] ��ư Image ������Ʈ�� ã�� �� �����ϴ�!");
        }
    }

    private void SetupButtons()
    {
        Debug.Log("[HomeScreenModeSettings] ��ư ���� ��...");

        if (fullscreenButton != null)
        {
            fullscreenButton.onClick.RemoveAllListeners();
            fullscreenButton.onClick.AddListener(SetFullscreen);
            Debug.Log("[HomeScreenModeSettings] ��üȭ�� ��ư �̺�Ʈ ���");
        }
        else
        {
            Debug.LogError("[HomeScreenModeSettings] fullscreenButton�� null!");
        }

        if (windowedButton != null)
        {
            windowedButton.onClick.RemoveAllListeners();
            windowedButton.onClick.AddListener(SetWindowed);
            Debug.Log("[HomeScreenModeSettings] â��� ��ư �̺�Ʈ ���");
        }
        else
        {
            Debug.LogError("[HomeScreenModeSettings] windowedButton�� null!");
        }
    }

    public void SetFullscreen()
    {
        if (isChangingMode) return; // ��� ��ȯ ���̸� ����

        Debug.Log("[HomeScreenModeSettings] ��üȭ�� ����");
        isChangingMode = true;

        // ��� UI ������Ʈ (����Ǵ� ���·�)
        UpdateButtonSprites(true);

        // ȭ�� ��� ����
        Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);

        // ���� �����
        if (videoMonitor != null)
            videoMonitor.ForceRestart();

        // �ణ�� ������ �� Ȯ��
        StartCoroutine(VerifyModeChange(true));
    }

    public void SetWindowed()
    {
        if (isChangingMode) return; // ��� ��ȯ ���̸� ����

        Debug.Log($"[HomeScreenModeSettings] â��� ���� ({windowWidth}x{windowHeight})");
        isChangingMode = true;

        // ��� UI ������Ʈ (����Ǵ� ���·�)
        UpdateButtonSprites(false);

        // ȭ�� ��� ����
        Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);

        // ���� �����
        if (videoMonitor != null)
            videoMonitor.ForceRestart();

        // �ణ�� ������ �� Ȯ��
        StartCoroutine(VerifyModeChange(false));
    }

    private IEnumerator VerifyModeChange(bool expectedFullscreen)
    {
        // ȭ�� ��� ������ �Ϸ�� ������ ���
        yield return new WaitForSeconds(0.5f);

        // ���� ���� Ȯ�� �� UI ������Ʈ
        RefreshButtonStates();
        isChangingMode = false;
    }

    private void UpdateButtonSprites(bool isFullscreen)
    {
        if (fullscreenImg == null || windowedImg == null) return;

        if (isFullscreen)
        {
            fullscreenImg.sprite = fullscreenActiveSprite;
            windowedImg.sprite = windowInactiveSprite;
        }
        else
        {
            fullscreenImg.sprite = fullscreenInactiveSprite;
            windowedImg.sprite = windowActiveSprite;
        }
    }

    public void RefreshButtonStates()
    {
        Debug.Log("[HomeScreenModeSettings] ��ư ���� ���ΰ�ħ");

        if (fullscreenImg == null || windowedImg == null)
        {
            Debug.LogError("[HomeScreenModeSettings] Image ������Ʈ�� null�Դϴ�!");
            return;
        }

        bool isFullscreen = Screen.fullScreenMode == FullScreenMode.FullScreenWindow ||
                           Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ||
                           Screen.fullScreenMode == FullScreenMode.MaximizedWindow;

        UpdateButtonSprites(isFullscreen);

        Debug.Log($"[HomeScreenModeSettings] ���� ���: {(isFullscreen ? "��üȭ��" : "â���")}");
    }

    // �ܺο��� ȣ���
    public void RefreshSettings()
    {
        RefreshButtonStates();
    }

    private void OnDestroy()
    {
        if (fullscreenButton != null)
            fullscreenButton.onClick.RemoveAllListeners();

        if (windowedButton != null)
            windowedButton.onClick.RemoveAllListeners();
    }
}