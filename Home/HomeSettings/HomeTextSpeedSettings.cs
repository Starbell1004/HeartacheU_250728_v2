using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;

public class HomeTextSpeedSettings : MonoBehaviour
{
    [Header("텍스트 속도 설정")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedDisplayText;
    [SerializeField] private HomeTextSpeedPreview textPreview;

    private ITextPrinterManager printerManager;

    //  단순 매핑 - 표시값은 10배
    private const float DEFAULT_SPEED = 0.1f;  // 나니노벨 기본값

    private void Awake()
    {
        Debug.Log("[HomeTextSpeedSettings] Awake 호출");

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
        Debug.Log("[HomeTextSpeedSettings] Start 호출");
        SetupTextSpeed();
    }

    private void InitializeServices()
    {
        try
        {
            printerManager = Engine.GetService<ITextPrinterManager>();
            Debug.Log("[HomeTextSpeedSettings] 나니노벨 서비스 초기화 완료");
            RefreshSpeedFromNaninovel();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeTextSpeedSettings] 서비스 초기화 오류: {ex.Message}");
        }
    }

    private void SetupTextSpeed()
    {
        Debug.Log("[HomeTextSpeedSettings] SetupTextSpeed 시작");

        if (speedSlider != null)
        {
            //  슬라이더는 1~10으로 표시 (실제값의 10배)
            speedSlider.minValue = 1f;    // 0.1 표시
            speedSlider.maxValue = 10f;   // 1.0 표시
            speedSlider.value = 1f;       // 기본값 0.1

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

        //  실제값을 슬라이더값으로 (10배)
        float sliderValue = actualSpeed * 10f;
        speedSlider.value = sliderValue;

        // 여기가 중요! 슬라이더 값으로 표시
        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"{sliderValue:F1}x";
        }
    }

    private void OnSpeedChanged(float sliderValue)
    {
        float actualSpeed = sliderValue / 10f;

        Debug.Log($"[HomeTextSpeedSettings] 슬라이더: {sliderValue}, 실제 속도: {actualSpeed}");

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
            speedDisplayText.text = $"{sliderValue:F1}x";  // 직접 설정!
            Debug.Log($"[HomeTextSpeedSettings] 텍스트 변경: {speedDisplayText.text}");
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
            //  x를 붙여서 배속으로 표시
            float displayMultiplier = actualSpeed * 10f;  // 0.1 → 1.0x
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