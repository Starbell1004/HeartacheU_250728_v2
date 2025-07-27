using UnityEngine;
using Naninovel;
using Naninovel.Commands;

public class HomeUIBGMController : MonoBehaviour
{
    [Header("BGM ����")]
    [SerializeField] private string bgmPath = "MainTheme"; // Naninovel BGM ���ҽ� ���
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float defaultVolume = 1f;
    [SerializeField] private bool loop = true;

    private IAudioManager audioManager;
    private bool isPlaying = false;

    private void Start()
    {
        // ���� AudioSource�� �ִٸ� ��Ȱ��ȭ
        var audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.enabled = false;
            Debug.Log("[HomeUIBGMController] ���� AudioSource ��Ȱ��ȭ");
        }

        // Naninovel �ʱ�ȭ ���
        if (Engine.Initialized)
        {
            InitializeBGM();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeBGM;
        }
    }

    private void InitializeBGM()
    {
        audioManager = Engine.GetService<IAudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("[HomeUIBGMController] AudioManager�� ã�� �� ����!");
            return;
        }

        PlayBGM();
    }

    public async void PlayBGM()
    {
        if (isPlaying) return;

        try
        {
            Debug.Log($"[HomeUIBGMController] BGM ��� ����: {bgmPath}");

            // Naninovel�� @bgm ��ɾ�� ������ ������� ���
            var playBgm = new PlayBgm
            {
                BgmPath = bgmPath,
                Volume = defaultVolume,
                Loop = loop,
                FadeInDuration = fadeInDuration, // Fade ��� FadeInDuration
                Wait = false
            };

            await playBgm.Execute();
            isPlaying = true;

            Debug.Log($"[HomeUIBGMController] BGM ��� ��: {bgmPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HomeUIBGMController] BGM ��� ����: {e.Message}");
        }
    }

    public async void StopBGM()
    {
        if (!isPlaying) return;

        try
        {
            Debug.Log("[HomeUIBGMController] BGM ����");

            // Naninovel�� @stopBgm ��ɾ�� ������ ������� ����
            var stopBgm = new StopBgm
            {
                FadeOutDuration = fadeInDuration, // Fade ��� FadeOutDuration
                Wait = false
            };

            await stopBgm.Execute();
            isPlaying = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HomeUIBGMController] BGM ���� ����: {e.Message}");
        }
    }

    // HomeUI�� ������ �� BGM ����
    private void OnDisable()
    {
        if (isPlaying)
        {
            StopBGM();
        }
    }
    private void OnEnable()
    {
        Debug.Log("[HomeUIBGMController] OnEnable - BGM ��� �õ�");

        // HomeUI�� �ٽ� Ȱ��ȭ�� �� BGM ���
        if (Engine.Initialized && audioManager != null)
        {
            PlayBGM();
        }
    }

   
    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeBGM;
    }
}