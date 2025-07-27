using UnityEngine;
using UnityEngine.Video;
using System.Collections;

[RequireComponent(typeof(VideoPlayer))]
public class VideoLoopMonitor : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private bool wasPlaying = false;
    private float lastPlayTime = 0f;
    private float stuckCheckInterval = 1f; // 1�ʸ��� üũ
    private double lastFrameTime = 0;

    [Header("����͸� ����")]
    [SerializeField] private bool enableMonitoring = true;
    [SerializeField] private float restartDelay = 0.5f; // ����� �� ��� �ð�

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        // �⺻ ���� ���� Ȯ��
        if (!videoPlayer.isLooping)
        {
            Debug.Log("[VideoLoopMonitor] ���� ���� ���� Ȱ��ȭ");
            videoPlayer.isLooping = true;
        }

        // �̺�Ʈ ������ �߰�
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.errorReceived += OnVideoError;
    }

    private void Start()
    {
        if (enableMonitoring)
        {
            StartCoroutine(MonitorVideoPlayback());
        }
    }

    private IEnumerator MonitorVideoPlayback()
    {
        while (true)
        {
            yield return new WaitForSeconds(stuckCheckInterval);

            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                // �������� ����ǰ� �ִ��� üũ
                double currentTime = videoPlayer.time;

                if (currentTime == lastFrameTime && currentTime > 0)
                {
                    Debug.LogWarning($"[VideoLoopMonitor] ������ ���� �� �����ϴ�. ���� �ð�: {currentTime}");
                    RestartVideo();
                }

                lastFrameTime = currentTime;
            }
            else if (videoPlayer != null && !videoPlayer.isPlaying && videoPlayer.isPrepared)
            {
                // �غ�Ǿ��µ� ��� ���� �ƴ� ���
                Debug.LogWarning("[VideoLoopMonitor] ������ ������� �ʰ� �ֽ��ϴ�. ������մϴ�.");
                RestartVideo();
            }
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("[VideoLoopMonitor] ���� �� ���� - ���� ���");
        // isLooping�� true�� �ڵ����� ����۵�����, Ȥ�� �𸣴� üũ
        if (!vp.isPlaying)
        {
            StartCoroutine(DelayedRestart());
        }
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[VideoLoopMonitor] ���� ����: {message}");
        StartCoroutine(DelayedRestart());
    }

    private void RestartVideo()
    {
        StartCoroutine(DelayedRestart());
    }

    private IEnumerator DelayedRestart()
    {
        yield return new WaitForSeconds(restartDelay);

        if (videoPlayer != null)
        {
            Debug.Log("[VideoLoopMonitor] ���� �����");
            videoPlayer.Stop();
            videoPlayer.time = 0;
            videoPlayer.Play();
        }
    }

    // ȭ�� ��� �����̳� ��Ŀ�� ���� �� ȣ��
    private void OnApplicationFocus(bool hasFocus)
    {
        Debug.Log($"[VideoLoopMonitor] ��Ŀ�� ����: {hasFocus}");

        if (hasFocus)
        {
            // ��Ŀ���� �ٽ� ����� �� ���� �����
            StartCoroutine(EnsureVideoPlaying());
        }
        else
        {
            // ��Ŀ���� �Ҿ��� �� ���� ����
            wasPlaying = videoPlayer != null && videoPlayer.isPlaying;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log($"[VideoLoopMonitor] �Ͻ����� ����: {pauseStatus}");

        if (!pauseStatus)
        {
            StartCoroutine(EnsureVideoPlaying());
        }
    }

    private IEnumerator EnsureVideoPlaying()
    {
        yield return new WaitForSeconds(0.2f); // �ణ�� ������

        if (videoPlayer != null)
        {
            if (!videoPlayer.isPlaying)
            {
                Debug.Log("[VideoLoopMonitor] ���� ��� ���� ����");
                videoPlayer.Play();
            }

            // �׷��� �� �Ǹ� �����
            yield return new WaitForSeconds(0.5f);
            if (!videoPlayer.isPlaying)
            {
                Debug.LogWarning("[VideoLoopMonitor] ���� ������ ���� - ���� �����");
                RestartVideo();
            }
        }
    }

    // �������� ������� �ʿ��� ��� ȣ��
    public void ForceRestart()
    {
        Debug.Log("[VideoLoopMonitor] ���� ����� ��û");
        RestartVideo();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }
}