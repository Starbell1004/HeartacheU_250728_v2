using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;

public class HomeBGMVolumeSettings : MonoBehaviour
{
    [Header("BGM ���� ����")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;

    private IAudioManager audioManager;

    private void Awake()
    {
        Debug.Log("[HomeBGMVolumeSettings] Awake ȣ��");

        // ���ϳ뺧 ���� �ʱ�ȭ
        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeServices;
        }
    }

    private void Start()
    {
        Debug.Log("[HomeBGMVolumeSettings] Start ȣ��");
        SetupVolumeSlider();
    }

    private void InitializeServices()
    {
        try
        {
            audioManager = Engine.GetService<IAudioManager>();
            Debug.Log("[HomeBGMVolumeSettings] ���ϳ뺧 ����� ���� �ʱ�ȭ �Ϸ�");

            // ���� �ʱ�ȭ �� �����̴� �� ����
            RefreshVolumeFromNaninovel();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeBGMVolumeSettings] ���� �ʱ�ȭ ����: {ex.Message}");
        }
    }

    private void SetupVolumeSlider()
    {
        Debug.Log("[HomeBGMVolumeSettings] SetupVolumeSlider ����");

        if (volumeSlider != null)
        {
            Debug.Log("[HomeBGMVolumeSettings] BGM �����̴� ���� ��...");
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            // �̺�Ʈ ������ ���
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

            Debug.Log("[HomeBGMVolumeSettings] BGM �����̴� �̺�Ʈ ��� �Ϸ�");

            // �ʱⰪ ����
            RefreshVolumeFromNaninovel();
        }
        else
        {
            Debug.LogError("[HomeBGMVolumeSettings] volumeSlider�� null�Դϴ�!");
        }
    }

    private void RefreshVolumeFromNaninovel()
    {
        if (volumeSlider == null) return;

        float currentVolume = 1f;

        if (audioManager != null)
        {
            currentVolume = audioManager.BgmVolume;
            Debug.Log($"[HomeBGMVolumeSettings] ���ϳ뺧���� BGM ���� �ε�: {currentVolume}");
        }
        else
        {
            currentVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            Debug.Log($"[HomeBGMVolumeSettings] PlayerPrefs���� BGM ���� �ε�: {currentVolume}");
        }

        volumeSlider.value = currentVolume;
        UpdateVolumeDisplay(currentVolume);
    }

    private void OnVolumeChanged(float volume)
    {
        Debug.Log($"[HomeBGMVolumeSettings] BGM ���� �����: {volume}");

        // ���ϳ뺧�� ����
        if (audioManager != null)
        {
            audioManager.BgmVolume = volume;
            Debug.Log($"[HomeBGMVolumeSettings] ���ϳ뺧 BGM ���� ����: {volume}");
        }
        else
        {
            Debug.LogWarning("[HomeBGMVolumeSettings] audioManager�� null! PlayerPrefs�� ����");
            PlayerPrefs.SetFloat("BGMVolume", volume);
        }

        // UI ������Ʈ
        UpdateVolumeDisplay(volume);
    }

    private void UpdateVolumeDisplay(float volume)
    {
        Debug.Log($"[HomeBGMVolumeSettings] BGM ���� �ؽ�Ʈ ������Ʈ: {volume:F1}");

        if (volumeText != null)
        {
            volumeText.text = $"{volume:F1}";
            Debug.Log($"[HomeBGMVolumeSettings] BGM �ؽ�Ʈ ���� �Ϸ�: {volumeText.text}");
        }
        else
        {
            Debug.LogError("[HomeBGMVolumeSettings] volumeText�� null!");
        }
    }

    // �ܺο��� ȣ���
    public void RefreshSettings()
    {
        RefreshVolumeFromNaninovel();
    }

    // �׽�Ʈ�� �Լ�
    [ContextMenu("Test Volume Change")]
    public void TestVolumeChange()
    {
        Debug.Log("[HomeBGMVolumeSettings] �׽�Ʈ: BGM ������ 0.5�� ����");
        if (volumeSlider != null)
        {
            volumeSlider.value = 0.5f;
        }
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeServices;

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveAllListeners();
        }
    }
}