using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;

public class HomeTextSpeedSettings : MonoBehaviour
{
    [Header("�ؽ�Ʈ �ӵ� ����")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedDisplayText;
    [SerializeField] private HomeTextSpeedPreview textPreview;

    private ITextPrinterManager printerManager;

    //  �ܼ� ���� - ǥ�ð��� 10��
    private const float DEFAULT_SPEED = 0.1f;  // ���ϳ뺧 �⺻��

    private void Awake()
    {
        Debug.Log("[HomeTextSpeedSettings] Awake ȣ��");

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
        Debug.Log("[HomeTextSpeedSettings] Start ȣ��");
        SetupTextSpeed();
    }

    private void InitializeServices()
    {
        try
        {
            printerManager = Engine.GetService<ITextPrinterManager>();
            Debug.Log("[HomeTextSpeedSettings] ���ϳ뺧 ���� �ʱ�ȭ �Ϸ�");
            RefreshSpeedFromNaninovel();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeTextSpeedSettings] ���� �ʱ�ȭ ����: {ex.Message}");
        }
    }

    private void SetupTextSpeed()
    {
        Debug.Log("[HomeTextSpeedSettings] SetupTextSpeed ����");

        if (speedSlider != null)
        {
            //  �����̴��� 1~10���� ǥ�� (�������� 10��)
            speedSlider.minValue = 1f;    // 0.1 ǥ��
            speedSlider.maxValue = 10f;   // 1.0 ǥ��
            speedSlider.value = 1f;       // �⺻�� 0.1

            speedSlider.onValueChanged.RemoveAllListeners();
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);

            RefreshSpeedFromNaninovel();
        }

        if (textPreview != null)
        {
            textPreview.Show();
        }
    }

    private void RefreshSpeedFromNaninovel()
    {
        if (speedSlider == null) return;

        float actualSpeed = DEFAULT_SPEED;

        if (printerManager != null)
        {
            actualSpeed = printerManager.BaseRevealSpeed;
        }
        else
        {
            actualSpeed = PlayerPrefs.GetFloat("TextSpeed", DEFAULT_SPEED);
        }

        //  �������� �����̴������� (10��)
        float sliderValue = actualSpeed * 10f;
        speedSlider.value = sliderValue;

        // ���Ⱑ �߿�! �����̴� ������ ǥ��
        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"{sliderValue:F1}x";
        }
    }

    private void OnSpeedChanged(float sliderValue)
    {
        float actualSpeed = sliderValue / 10f;

        Debug.Log($"[HomeTextSpeedSettings] �����̴�: {sliderValue}, ���� �ӵ�: {actualSpeed}");

        if (printerManager != null)
        {
            printerManager.BaseRevealSpeed = actualSpeed;
        }
        else
        {
            PlayerPrefs.SetFloat("TextSpeed", actualSpeed);
            PlayerPrefs.Save();
        }

     
        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"{sliderValue:F1}x";  // ���� ����!
            Debug.Log($"[HomeTextSpeedSettings] �ؽ�Ʈ ����: {speedDisplayText.text}");
        }

        if (textPreview != null)
        {
            textPreview.StartPreview();
        }
    }

    private void UpdateSpeedDisplay(float actualSpeed)
    {
        if (speedDisplayText != null)
        {
            //  x�� �ٿ��� ������� ǥ��
            float displayMultiplier = actualSpeed * 10f;  // 0.1 �� 1.0x
            speedDisplayText.text = $"{displayMultiplier:F1}x";
        }
    }

    public void ShowPreview()
    {
        if (textPreview != null)
        {
            textPreview.Show();
        }
    }

    public void HidePreview()
    {
        if (textPreview != null)
        {
            textPreview.Hide();
        }
    }

    public void RefreshSettings()
    {
        RefreshSpeedFromNaninovel();
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeServices;

        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveAllListeners();
        }
    }
}