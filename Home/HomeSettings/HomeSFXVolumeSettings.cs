using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;

public class HomeSFXVolumeSettings : MonoBehaviour
{
    [Header("SFX ���� ����")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private Button testSfxButton; // ȿ���� �׽�Ʈ ��ư

    [Header("�׽�Ʈ ȿ���� (AudioClip ���� ����)")]
    [SerializeField] private AudioClip testAudioClip; // AudioClip ���� �Ҵ�

    private IAudioManager audioManager;

    private void Awake()
    {
        Debug.Log("[HomeSFXVolumeSettings] Awake ȣ��");

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
        Debug.Log("[HomeSFXVolumeSettings] Start ȣ��");
        SetupVolumeSlider();
    }

    private void InitializeServices()
    {
        try
        {
            audioManager = Engine.GetService<IAudioManager>();
            Debug.Log("[HomeSFXVolumeSettings] ���ϳ뺧 ����� ���� �ʱ�ȭ �Ϸ�");

            // ���� �ʱ�ȭ �� �����̴� �� ���� (�ణ ���� �� ����)
            Invoke(nameof(RefreshVolumeFromNaninovel), 0.1f);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeSFXVolumeSettings] ���� �ʱ�ȭ ����: {ex.Message}");
            // ���� �ÿ��� �⺻������ �ʱ�ȭ
            Invoke(nameof(RefreshVolumeFromNaninovel), 0.1f);
        }
    }

    private void SetupVolumeSlider()
    {
        Debug.Log("[HomeSFXVolumeSettings] SetupVolumeSlider ����");

        if (volumeSlider != null)
        {
            Debug.Log("[HomeSFXVolumeSettings] SFX �����̴� ���� ��...");
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            // �̺�Ʈ ������ ���
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

            Debug.Log("[HomeSFXVolumeSettings] SFX �����̴� �̺�Ʈ ��� �Ϸ�");

            // �ʱⰪ ���� (���� ����)
            Invoke(nameof(RefreshVolumeFromNaninovel), 0.2f);
        }
        else
        {
            Debug.LogError("[HomeSFXVolumeSettings] volumeSlider�� null�Դϴ�!");
        }

        // �׽�Ʈ ��ư ����
        SetupTestButton();
    }

    private void SetupTestButton()
    {
        if (testSfxButton != null)
        {
            Debug.Log("[HomeSFXVolumeSettings] �׽�Ʈ ��ư ���� ��...");
            testSfxButton.onClick.RemoveAllListeners();
            testSfxButton.onClick.AddListener(PlayTestSfx);
            Debug.Log("[HomeSFXVolumeSettings] �׽�Ʈ ��ư �̺�Ʈ ��� �Ϸ�");
        }
        else
        {
            Debug.LogWarning("[HomeSFXVolumeSettings] testSfxButton�� null�Դϴ�!");
        }
    }

    private void RefreshVolumeFromNaninovel()
    {
        if (volumeSlider == null)
        {
            Debug.LogError("[HomeSFXVolumeSettings] volumeSlider�� null! RefreshVolumeFromNaninovel �ߴ�");
            return;
        }

        float currentVolume = 1f;

        if (audioManager != null)
        {
            currentVolume = audioManager.SfxVolume;
            Debug.Log($"[HomeSFXVolumeSettings] ���ϳ뺧���� SFX ���� �ε�: {currentVolume}");
        }
        else
        {
            currentVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            Debug.Log($"[HomeSFXVolumeSettings] PlayerPrefs���� SFX ���� �ε�: {currentVolume}");
        }

        // �����̴� �� ���� (�̺�Ʈ �߻� ������ ���� �ӽ÷� ������ ����)
        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.value = currentVolume;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        UpdateVolumeDisplay(currentVolume);

        Debug.Log($"[HomeSFXVolumeSettings] ���� �ʱ�ȭ �Ϸ� - �����̴�: {volumeSlider.value}, �ؽ�Ʈ: {(volumeText != null ? volumeText.text : "null")}");
    }

    private void OnVolumeChanged(float volume)
    {
        Debug.Log($"[HomeSFXVolumeSettings] SFX ���� �����: {volume}");

        // ���ϳ뺧�� ����
        if (audioManager != null)
        {
            audioManager.SfxVolume = volume;
            Debug.Log($"[HomeSFXVolumeSettings] ���ϳ뺧 SFX ���� ����: {volume}");
        }
        else
        {
            Debug.LogWarning("[HomeSFXVolumeSettings] audioManager�� null! PlayerPrefs�� ����");
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }

        // UI ������Ʈ
        UpdateVolumeDisplay(volume);
    }

    private void UpdateVolumeDisplay(float volume)
    {
        Debug.Log($"[HomeSFXVolumeSettings] SFX ���� �ؽ�Ʈ ������Ʈ: {volume:F1}");

        if (volumeText != null)
        {
            volumeText.text = $"{volume:F1}";
            Debug.Log($"[HomeSFXVolumeSettings] SFX �ؽ�Ʈ ���� �Ϸ�: {volumeText.text}");
        }
        else
        {
            Debug.LogError("[HomeSFXVolumeSettings] volumeText�� null!");
        }
    }

    private void PlayTestSfx()
    {
        Debug.Log("[HomeSFXVolumeSettings] �׽�Ʈ ȿ���� ���");

        try
        {
            if (testAudioClip != null)
            {
                // AudioClip ���� ���
                PlayAudioClipDirectly();
            }
            else
            {
                Debug.LogWarning("[HomeSFXVolumeSettings] testAudioClip�� �������� �ʾҽ��ϴ�! �⺻ �� ���");
                PlayDefaultTestSound();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeSFXVolumeSettings] �׽�Ʈ ȿ���� ��� ����: {ex.Message}");
            PlayDefaultTestSound();
        }
    }

    private void PlayAudioClipDirectly()
    {
        if (testAudioClip != null)
        {
            // AudioSource�� �ӽ÷� �����ؼ� ���
            GameObject tempObj = new GameObject("TempSFXPlayer");
            AudioSource tempSource = tempObj.AddComponent<AudioSource>();

            tempSource.clip = testAudioClip;
            tempSource.volume = volumeSlider != null ? volumeSlider.value : 0.5f;
            tempSource.Play();

            Debug.Log($"[HomeSFXVolumeSettings] AudioClip ���� ���: {testAudioClip.name}, ����: {tempSource.volume}");

            // ��� �Ϸ� �� ����
            Destroy(tempObj, testAudioClip.length + 0.1f);
        }
        else
        {
            Debug.LogWarning("[HomeSFXVolumeSettings] testAudioClip�� null�Դϴ�!");
            PlayDefaultTestSound();
        }
    }

    private void PlayDefaultTestSound()
    {
        // �⺻ �׽�Ʈ ���� (440Hz ��)
        Debug.Log("[HomeSFXVolumeSettings] �⺻ �׽�Ʈ ���� ���");

        GameObject tempObj = new GameObject("DefaultTestSound");
        AudioSource tempSource = tempObj.AddComponent<AudioSource>();

        // 440Hz �� ���� (�� ��)
        int sampleRate = 44100;
        float frequency = 440f;
        float duration = 0.3f;
        int samples = Mathf.RoundToInt(sampleRate * duration);

        AudioClip clip = AudioClip.Create("TestTone", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            data[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate) * 0.3f;
        }

        clip.SetData(data, 0);
        tempSource.clip = clip;
        tempSource.volume = volumeSlider != null ? volumeSlider.value : 0.5f;
        tempSource.Play();

        Debug.Log($"[HomeSFXVolumeSettings] ���α׷��� ��� �׽�Ʈ �� ���, ����: {tempSource.volume}");

        // ��� �Ϸ� �� ����
        Destroy(tempObj, duration + 0.1f);
    }

    // �ܺο��� ȣ���
    public void RefreshSettings()
    {
        Debug.Log("[HomeSFXVolumeSettings] RefreshSettings ȣ��");
        RefreshVolumeFromNaninovel();
    }

    // �׽�Ʈ�� �Լ�
    [ContextMenu("Test Volume Change")]
    public void TestVolumeChange()
    {
        Debug.Log("[HomeSFXVolumeSettings] �׽�Ʈ: SFX ������ 0.5�� ����");
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

        if (testSfxButton != null)
        {
            testSfxButton.onClick.RemoveAllListeners();
        }
    }
}