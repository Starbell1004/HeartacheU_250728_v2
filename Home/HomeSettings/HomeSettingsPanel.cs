using UnityEngine;
using UnityEngine.UI;
using Naninovel;

/// <summary>
/// 홈화면 전용 설정 패널 - MonoBehaviour 사용
/// </summary>
public class HomeSettingsPanel : MonoBehaviour  //  CustomUI → MonoBehaviour
{
    [Header("UI 기본")]
    [SerializeField] private Button closeButton;

    [Header("개별 설정 컴포넌트들")]
    [SerializeField] private HomeTextSpeedSettings textSpeedSettings;
    [SerializeField] private HomeScreenModeSettings screenModeSettings;
    [SerializeField] private HomeBGMVolumeSettings bgmVolumeSettings;
    [SerializeField] private HomeSFXVolumeSettings sfxVolumeSettings;

    // 나니노벨 서비스
    private IStateManager stateManager;

    private void Awake()
    {
        Debug.Log("[HomeSettingsPanel] Awake 호출");

        // 서비스 초기화
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
            Debug.Log("[HomeSettingsPanel] 나니노벨 서비스 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeSettingsPanel] 서비스 초기화 오류: {ex.Message}");
        }
    }

    private void SetupCloseButton()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => {
                Debug.Log("[HomeSettingsPanel] 닫기 버튼 클릭됨!");
                Hide();
            });
        }
        else
        {
            Debug.LogError("[HomeSettingsPanel] closeButton이 null입니다!");
        }
    }

    public void Show()  //  override 제거
    {
        Debug.Log("[HomeSettingsPanel] Show 호출");
        gameObject.SetActive(true);

        RefreshAllSettings();

        // 텍스트 미리보기 시작
        if (textSpeedSettings != null)
        {
            textSpeedSettings.ShowPreview();
        }
    }

    public void Hide()  //  override 제거
    {
        Debug.Log("[HomeSettingsPanel] Hide 호출");

        // 텍스트 미리보기 중지
        if (textSpeedSettings != null)
        {
            textSpeedSettings.HidePreview();
        }

        // 직접 처리
        gameObject.SetActive(false);

        // HomePanel 표시
        var homePanel = transform.parent?.Find("HomePanel");
        if (homePanel != null)
        {
            homePanel.gameObject.SetActive(true);
            Debug.Log("[HomeSettingsPanel] HomePanel 표시됨");
        }

        // 설정 저장
        SaveSettingsAsync().Forget();
    }

    private async UniTask SaveSettingsAsync()
    {
        if (stateManager != null)
        {
            await stateManager.SaveSettings();
            Debug.Log("[HomeSettingsPanel] 설정 저장 완료");
        }
    }

    private void RefreshAllSettings()
    {
        Debug.Log("[HomeSettingsPanel] 모든 설정 새로고침");

        if (textSpeedSettings != null) textSpeedSettings.RefreshSettings();
        if (screenModeSettings != null) screenModeSettings.RefreshSettings();
        if (bgmVolumeSettings != null) bgmVolumeSettings.RefreshSettings();
        if (sfxVolumeSettings != null) sfxVolumeSettings.RefreshSettings();

        Debug.Log("[HomeSettingsPanel] 설정 새로고침 완료");
    }

    public void RefreshSettings()
    {
        RefreshAllSettings();
    }

    private void Start()
    {
        Debug.Log("[HomeSettingsPanel] Start - 컴포넌트 연결 상태 확인");
        Debug.Log($"closeButton: {(closeButton != null ? "연결됨" : "null")}");
        Debug.Log($"textSpeedSettings: {(textSpeedSettings != null ? "연결됨" : "null")}");
        Debug.Log($"screenModeSettings: {(screenModeSettings != null ? "연결됨" : "null")}");
        Debug.Log($"bgmVolumeSettings: {(bgmVolumeSettings != null ? "연결됨" : "null")}");
        Debug.Log($"sfxVolumeSettings: {(sfxVolumeSettings != null ? "연결됨" : "null")}");
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