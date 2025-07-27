using UnityEngine;
using UnityEngine.UI;
using Naninovel;

/// <summary>
/// Ȩȭ�� ���� ���� �г� - MonoBehaviour ���
/// </summary>
public class HomeSettingsPanel : MonoBehaviour  //  CustomUI �� MonoBehaviour
{
    [Header("UI �⺻")]
    [SerializeField] private Button closeButton;

    [Header("���� ���� ������Ʈ��")]
    [SerializeField] private HomeTextSpeedSettings textSpeedSettings;
    [SerializeField] private HomeScreenModeSettings screenModeSettings;
    [SerializeField] private HomeBGMVolumeSettings bgmVolumeSettings;
    [SerializeField] private HomeSFXVolumeSettings sfxVolumeSettings;

    // ���ϳ뺧 ����
    private IStateManager stateManager;

    private void Awake()
    {
        Debug.Log("[HomeSettingsPanel] Awake ȣ��");

        // ���� �ʱ�ȭ
        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeServices;
        }

        SetupCloseButton();
    }

    private void InitializeServices()
    {
        try
        {
            stateManager = Engine.GetService<IStateManager>();
            Debug.Log("[HomeSettingsPanel] ���ϳ뺧 ���� �ʱ�ȭ �Ϸ�");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeSettingsPanel] ���� �ʱ�ȭ ����: {ex.Message}");
        }
    }

    private void SetupCloseButton()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => {
                Debug.Log("[HomeSettingsPanel] �ݱ� ��ư Ŭ����!");
                Hide();
            });
        }
        else
        {
            Debug.LogError("[HomeSettingsPanel] closeButton�� null�Դϴ�!");
        }
    }

    public void Show()  //  override ����
    {
        Debug.Log("[HomeSettingsPanel] Show ȣ��");
        gameObject.SetActive(true);

        RefreshAllSettings();

        // �ؽ�Ʈ �̸����� ����
        if (textSpeedSettings != null)
        {
            textSpeedSettings.ShowPreview();
        }
    }

    public void Hide()  //  override ����
    {
        Debug.Log("[HomeSettingsPanel] Hide ȣ��");

        // �ؽ�Ʈ �̸����� ����
        if (textSpeedSettings != null)
        {
            textSpeedSettings.HidePreview();
        }

        // ���� ó��
        gameObject.SetActive(false);

        // HomePanel ǥ��
        var homePanel = transform.parent?.Find("HomePanel");
        if (homePanel != null)
        {
            homePanel.gameObject.SetActive(true);
            Debug.Log("[HomeSettingsPanel] HomePanel ǥ�õ�");
        }

        // ���� ����
        SaveSettingsAsync().Forget();
    }

    private async UniTask SaveSettingsAsync()
    {
        if (stateManager != null)
        {
            await stateManager.SaveSettings();
            Debug.Log("[HomeSettingsPanel] ���� ���� �Ϸ�");
        }
    }

    private void RefreshAllSettings()
    {
        Debug.Log("[HomeSettingsPanel] ��� ���� ���ΰ�ħ");

        if (textSpeedSettings != null) textSpeedSettings.RefreshSettings();
        if (screenModeSettings != null) screenModeSettings.RefreshSettings();
        if (bgmVolumeSettings != null) bgmVolumeSettings.RefreshSettings();
        if (sfxVolumeSettings != null) sfxVolumeSettings.RefreshSettings();

        Debug.Log("[HomeSettingsPanel] ���� ���ΰ�ħ �Ϸ�");
    }

    public void RefreshSettings()
    {
        RefreshAllSettings();
    }

    private void Start()
    {
        Debug.Log("[HomeSettingsPanel] Start - ������Ʈ ���� ���� Ȯ��");
        Debug.Log($"closeButton: {(closeButton != null ? "�����" : "null")}");
        Debug.Log($"textSpeedSettings: {(textSpeedSettings != null ? "�����" : "null")}");
        Debug.Log($"screenModeSettings: {(screenModeSettings != null ? "�����" : "null")}");
        Debug.Log($"bgmVolumeSettings: {(bgmVolumeSettings != null ? "�����" : "null")}");
        Debug.Log($"sfxVolumeSettings: {(sfxVolumeSettings != null ? "�����" : "null")}");
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeServices;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
        }
    }
}