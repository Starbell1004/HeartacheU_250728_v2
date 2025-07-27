using System.Threading;
using UnityEngine;
using TMPro;
using Naninovel;
using Naninovel.UI;
using System;
using Unity.VisualScripting;

public class HomeTextSpeedPreview : MonoBehaviour
{
    [Header("미리보기 설정")]
    [SerializeField] private TextMeshProUGUI previewText;
    [SerializeField]
    private string[] previewMessages = {
        "텍스트 출력 속도를 미리 확인해보세요.",
        "이 속도로 게임 대화가 출력됩니다.",
        "슬라이더를 조정하여 원하는 속도를 설정하세요.",
        "빠른 속도로 설정하면 대화를 빠르게 읽을 수 있어요."
    };

    private ITextPrinterManager printerManager;
    private CancellationTokenSource revealCTS;
    private int currentMessageIndex = 0;
    private bool isActive = false;

    // 디버그 로그 제어
    [Header("디버그 설정")]
    [SerializeField] private bool enableDebugLogs = false;

    private void Awake()
    {
        // 나니노벨 서비스 가져오기
        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeServices;
        }
    }

    private void InitializeServices()
    {
        try
        {
            printerManager = Engine.GetService<ITextPrinterManager>();
            if (enableDebugLogs)
                Debug.Log("[HomeTextSpeedPreview] 나니노벨 서비스 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeTextSpeedPreview] 서비스 초기화 오류: {ex.Message}");
        }
    }

    public void Show()
    {
        if (enableDebugLogs)
            Debug.Log("[HomeTextSpeedPreview] Show 호출");
        isActive = true;
        StartPreview();
    }

    public void Hide()
    {
        if (enableDebugLogs)
            Debug.Log("[HomeTextSpeedPreview] Hide 호출");
        isActive = false;
        StopPreview();
    }

    public void StartPreview()
    {
        if (!isActive || previewText == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[HomeTextSpeedPreview] StartPreview 조건 실패!");
            return;
        }

        // 이전 애니메이션 중지
        StopPreview();

        // 현재 메시지 가져오기
        string message = GetCurrentMessage();

        // 텍스트 리셋
        previewText.text = "";
        previewText.maxVisibleCharacters = 0;

        // 새로운 애니메이션 시작
        revealCTS = new CancellationTokenSource();
        RevealTextAsync(message, revealCTS.Token).Forget();
    }

    private void StopPreview()
    {
        revealCTS?.Cancel();
        revealCTS?.Dispose();
        revealCTS = null;
    }

    private string GetCurrentMessage()
    {
        if (previewMessages == null || previewMessages.Length == 0)
        {
            return "텍스트 속도 미리보기입니다.";
        }

        string message = previewMessages[currentMessageIndex];
        currentMessageIndex = (currentMessageIndex + 1) % previewMessages.Length;

        return message;
    }

    private async UniTask RevealTextAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (previewText == null) return;

            previewText.text = message;
            previewText.maxVisibleCharacters = 0;

            float actualSpeed = 0.1f;
            if (printerManager != null)
            {
                actualSpeed = printerManager.BaseRevealSpeed;
            }

            // ⭐ 속도 반전 공식!
            // actualSpeed가 0.1(느림표시)일 때 → 딜레이 길게
            // actualSpeed가 1.0(빠름표시)일 때 → 딜레이 짧게
            float charDelay = (1.1f - actualSpeed) * 0.05f;

            // 또는 더 직관적으로:
            // float charDelay = 0.055f - (actualSpeed * 0.05f);

            if (enableDebugLogs || true)
            {
                Debug.Log($"[Preview] 슬라이더값: {actualSpeed * 10}x, 실제속도: {actualSpeed}, 딜레이: {charDelay}초");
            }

            for (int i = 0; i <= message.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested) return;

                previewText.maxVisibleCharacters = i;

                if (i < message.Length)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(charDelay), cancellationToken: cancellationToken);
                }
            }

            await WaitAndRestart(cancellationToken);
        }
        catch (System.OperationCanceledException)
        {
            // 취소됨
        }
    }

    private async UniTask WaitAndRestart(CancellationToken cancellationToken)
    {
        // 완료 후 2초 대기
        await UniTask.Delay(2000, cancellationToken: cancellationToken);

        if (!cancellationToken.IsCancellationRequested && isActive)
        {
            StartPreview(); // 다음 메시지로 자동 재시작
        }
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeServices;
        StopPreview();
    }
}