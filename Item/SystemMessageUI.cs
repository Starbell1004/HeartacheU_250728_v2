using Naninovel.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// �ý��� �޽����� ǥ���ϰ� �ڵ����� ������� �ϴ� UI
/// ���ϳ뺧 Configuration > UI > CustomUI�� ��� �ʿ�
/// </summary>
public class SystemMessageUI : CustomUI
{
    [Header("UI ������Ʈ")]
    [SerializeField] private Text messageText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("�ִϸ��̼� ����")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float defaultDisplayDuration = 2f;

    private Coroutine _currentMessageCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // ������Ʈ �ڵ� ����
        if (messageText == null)
            messageText = GetComponentInChildren<Text>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // �ʱ� ���� ����
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        // �ʱ⿡�� ����
        Hide();
    }

    /// <summary>
    /// �ý��� �޽����� ǥ���մϴ�
    /// </summary>
    /// <param name="text">ǥ���� �޽���</param>
    /// <param name="duration">ǥ�� �ð� (null�̸� �⺻�� ���)</param>
    public void ShowMessage(string text, float? duration = null)
    {
        if (string.IsNullOrEmpty(text))
            return;

        // ���� �޽��� �ڷ�ƾ�� ���� ���̸� �ߴ�
        if (_currentMessageCoroutine != null)
        {
            StopCoroutine(_currentMessageCoroutine);
        }

        _currentMessageCoroutine = StartCoroutine(ShowMessageCoroutine(text, duration ?? defaultDisplayDuration));
    }

    private IEnumerator ShowMessageCoroutine(string text, float displayDuration)
    {
        // �޽��� ����
        if (messageText != null)
            messageText.text = text;

        // UI ���̱�
        Show();

        // ���̵� ��
        if (canvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeInDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // ǥ�� �ð� ���
        yield return new WaitForSeconds(displayDuration);

        // ���̵� �ƿ�
        if (canvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        // UI �����
        Hide();

        _currentMessageCoroutine = null;
    }

    /// <summary>
    /// ���� ǥ�� ���� �޽����� ��� ����ϴ�
    /// </summary>
    public void HideMessage()
    {
        if (_currentMessageCoroutine != null)
        {
            StopCoroutine(_currentMessageCoroutine);
            _currentMessageCoroutine = null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        Hide();
    }
}