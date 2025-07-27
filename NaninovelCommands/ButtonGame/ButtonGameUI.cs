using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using TMPro;

public class ButtonGameUI : CustomUI
{
    [SerializeField] private Button[] puzzleButtons; // 6�� ��ư �迭
    [SerializeField] private Image filledBar; // Ÿ�̹� UI
    [SerializeField] private float buttonSpacing = 100f; // ��ư �� �ּ� �Ÿ�
    [SerializeField] private CanvasGroup canvasGroup; // ĵ���� �׷�
    [SerializeField] private RectTransform buttonAreaRect;

    [Header("Audio Settings")]
    [SerializeField] private string correctClickSfx = "ButtonCorrect"; // ���� ��ư �Ҹ�
    [SerializeField] private string wrongClickSfx = "ButtonWrong"; // ���� ��ư �Ҹ�
    [SerializeField] private string shirtClickSfx = "ButtonShirt"; // ���� ��ư �Ҹ��� SFX ���

    private float totalTime;
    private float currentTime;
    private int currentIndex;
    private Action onSuccess;
    private Action<string> onFail;
    private bool isRunning;
    private Dictionary<Button, string> buttonIdMap = new Dictionary<Button, string>();
    private List<string> correctSequence = new List<string>();
    private string shirtButtonId = "shirt";

    private IAudioManager audioManager;

    protected override void Awake()
    {
        base.Awake();
        if (Engine.Initialized)
        {
            audioManager = Engine.GetService<IAudioManager>();
        }
        else
        {
            Engine.OnInitializationFinished += () => {
                audioManager = Engine.GetService<IAudioManager>();
            };
        }
        // ĵ���� �׷��� �Ҵ���� �ʾҴٸ� ã�ų� ����
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        // �ʱ� ���� ���� - �����ϰ�
        canvasGroup.alpha = 0;

        CompleteReset();
    }

    protected override void Update()
    {
        base.Update();
        if (!isRunning || filledBar == null) return;

        currentTime -= Time.deltaTime;
        filledBar.fillAmount = Mathf.Clamp01(currentTime / totalTime);

        if (currentTime <= 0)
        {
            Debug.Log("[UI] �ð��ʰ��� ���� ó��");
            FailGame("timeout");
        }
    }

    public override void Show()
    {
        base.Show();
        Debug.Log("[UI] Show() ȣ���");

        // UI ǥ�� (���İ��� 1��)
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public override void Hide()
    {
        // UI ����� (���İ��� 0����)
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        base.Hide();
    }

    /// <summary>
    /// �� �ٽ�: ������ ���ο� ���� ���� (�׻� ó������)
    /// �ؼ��� ���ϳ뺧 ��ũ��Ʈ���� 15_2_0 �󺧷� ���ƿ� ������ ȣ���
    /// </summary>
    public void StartNewGame(float duration, List<string> sequence, Action onSuccess, Action<string> onFail)
    {
        Debug.Log($"[StartNewGame] ���� �� ���� ���� - duration={duration}, sequenceCount={sequence?.Count ?? -1}");

        // ������ �ʱ�ȭ
        CompleteReset();

        // �� ���� ����
        totalTime = currentTime = duration;
        this.onSuccess = onSuccess;
        this.onFail = onFail;

        // ������ ����
        if (sequence != null && sequence.Count > 0)
        {
            SetupButtonsWithCustomSequence(sequence);
        }
        else
        {
            SetupButtonsWithDefaultSequence();
        }

        // ��ư ��ġ ���� ��ġ
        ScatterButtons();

        isRunning = true;
        filledBar.fillAmount = 1f;

        Debug.Log($"[StartNewGame] �� ���� �غ� �Ϸ�. currentTime={currentTime}, totalTime={totalTime}");

        // UI ǥ��
        Show();
    }

    /// <summary>
    /// ������ �ʱ�ȭ - ��� ���¸� ó������ �ǵ���
    /// </summary>
    private void CompleteReset()
    {
        Debug.Log("[CompleteReset] ���� �ʱ�ȭ ����");

        isRunning = false;
        currentIndex = 0;
        currentTime = 0f;
        correctSequence.Clear();
        buttonIdMap.Clear();

        if (filledBar != null)
            filledBar.fillAmount = 1f;

        // ��� ��ư ���� �ʱ�ȭ
        foreach (var button in puzzleButtons)
        {
            if (button != null)
            {
                // Ŭ�� �̺�Ʈ ����
                button.onClick.RemoveAllListeners();

                // ��ȣ�ۿ� Ȱ��ȭ
                button.interactable = true;

                // �ܰ� ���� ����
                ResetButtonAppearance(button);

                // ��Ȱ��ȭ
                button.gameObject.SetActive(false);
            }
        }

        Debug.Log("[CompleteReset] ���� �ʱ�ȭ �Ϸ�");
    }

    /// <summary>
    /// ��ư �ܰ� ���� ����
    /// </summary>
    private void ResetButtonAppearance(Button button)
    {
        Image buttonImage = button.GetComponent<Image>();
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = 1f; // ���� ������
            buttonImage.color = color;
        }

        if (buttonText != null)
        {
            Color color = buttonText.color;
            color.a = 1f; // ���� ������
            buttonText.color = color;
        }
    }

    // ����� ���� �������� ��ư ����
    private void SetupButtonsWithCustomSequence(List<string> sequence)
    {
        // ����� ���� ������ ����
        correctSequence = new List<string>(sequence);

        // ��� ��ư �ʱ�ȭ
        buttonIdMap.Clear();

        // ������ + ���̼��� ��ư��ŭ Ȱ��ȭ
        int activeCount = Mathf.Min(puzzleButtons.Length, sequence.Count + 1);

        // ���� ��ư ��ġ
        for (int i = 0; i < sequence.Count && i < puzzleButtons.Length - 1; i++)
        {
            Button button = puzzleButtons[i];
            string buttonId = sequence[i];

            buttonIdMap[button] = buttonId;

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = buttonId;
            }

            // ��ư Ŭ�� �̺�Ʈ ����
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnButtonClick(button));
            button.interactable = true;
            button.gameObject.SetActive(true);
            ResetButtonAppearance(button);
        }

        // ���̼��� ��ư ��ġ (������ ��ư����)
        int shirtIndex = Mathf.Min(sequence.Count, puzzleButtons.Length - 1);
        Button shirtButton = puzzleButtons[shirtIndex];
        buttonIdMap[shirtButton] = shirtButtonId;

        TMP_Text shirtText = shirtButton.GetComponentInChildren<TMP_Text>();
        if (shirtText != null)
        {
            shirtText.text = "���̼��� ����";
        }

        // ���̼��� ��ư �̺�Ʈ ����
        shirtButton.onClick.RemoveAllListeners();
        shirtButton.onClick.AddListener(() => OnButtonClick(shirtButton));
        shirtButton.interactable = true;
        shirtButton.gameObject.SetActive(true);
        ResetButtonAppearance(shirtButton);

        // ���� ��ư ��Ȱ��ȭ
        for (int i = activeCount; i < puzzleButtons.Length; i++)
        {
            puzzleButtons[i].gameObject.SetActive(false);
        }

        Debug.Log($"Ŀ���� ��ư ������ ����: {string.Join(", ", correctSequence)}");
    }

    // �⺻ �������� ��ư ���� (A, B, C, D, E)
    private void SetupButtonsWithDefaultSequence()
    {
        correctSequence.Clear();
        buttonIdMap.Clear();

        // �⺻ ���ĺ� ������ ���� (A-E)
        for (int i = 0; i < puzzleButtons.Length - 1; i++)
        {
            char letter = (char)('A' + i);
            correctSequence.Add(letter.ToString());
        }

        // ��� ��ư�� ������ �Ҵ�
        for (int i = 0; i < puzzleButtons.Length - 1; i++)
        {
            Button button = puzzleButtons[i];
            string buttonId = correctSequence[i];

            buttonIdMap[button] = buttonId;

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = buttonId;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnButtonClick(button));
            button.interactable = true;
            button.gameObject.SetActive(true);
            ResetButtonAppearance(button);
        }

        // ���̼��� ��ư ����
        int shirtIndex = puzzleButtons.Length - 1;
        Button shirtButton = puzzleButtons[shirtIndex];
        buttonIdMap[shirtButton] = shirtButtonId;

        TMP_Text shirtText = shirtButton.GetComponentInChildren<TMP_Text>();
        if (shirtText != null)
        {
            shirtText.text = "���̼��� ����";
        }

        shirtButton.onClick.RemoveAllListeners();
        shirtButton.onClick.AddListener(() => OnButtonClick(shirtButton));
        shirtButton.interactable = true;
        shirtButton.gameObject.SetActive(true);
        ResetButtonAppearance(shirtButton);

        Debug.Log($"�⺻ ��ư ������ ����: {string.Join(", ", correctSequence)}");
    }

    // ��ư Ŭ�� ó�� - ��� ���� ó��
    private void OnButtonClick(Button button)
    {
        if (!isRunning || !buttonIdMap.ContainsKey(button)) return;

        string buttonId = buttonIdMap[button];
        Debug.Log($"��ư Ŭ��: {buttonId}, ���� �ε���: {currentIndex}");

        if (buttonId == shirtButtonId)
        {
            // ���� ��ư Ŭ�� - ���� ����
            PlayButtonSound(shirtClickSfx);
            Debug.Log("[ButtonClick] ���� ���� Ŭ�� - ��� ����");
            FailGame("shirt");
            return;
        }

        // ���� �ε����� ��ư ID ��
        if (currentIndex < correctSequence.Count && buttonId == correctSequence[currentIndex])
        {
            // ���� ���� ���
            PlayButtonSound(correctClickSfx);

            button.interactable = false;
            StartCoroutine(FadeOutButton(button));
            currentIndex++;

            if (currentIndex >= correctSequence.Count)
            {
                CompleteGame();
            }
        }
        else
        {
            // ���� ���� ���
            PlayButtonSound(wrongClickSfx);

            Debug.Log($"[ButtonClick] �߸��� ������ Ŭ�� - ��� ����");
            FailGame("wrong_sequence");
        }
    }
    private void PlayButtonSound(string sfxName)
    {
        if (audioManager != null && !string.IsNullOrEmpty(sfxName))
        {
            // ���ϳ뺧 ������� SFX ���
            audioManager.PlaySfx(sfxName).Forget();
        }
    }

    // ���� �Ϸ� �� ���� ���� �߰� ����
    private void CompleteGame()
    {
        // ���� ���� ��� (�ɼ�)
        PlayButtonSound("GameComplete");

        isRunning = false;
        Hide();
        onSuccess?.Invoke();
    }

    private IEnumerator FadeOutButton(Button button)
    {
        // ��ư �̹����� �ؽ�Ʈ ������Ʈ ��������
        Image buttonImage = button.GetComponent<Image>();
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

        // �ʱ� ���� ����
        Color originalImageColor = buttonImage.color;
        Color originalTextColor = buttonText != null ? buttonText.color : Color.white;

        // ���̵� �ƿ� ����
        float duration = 0.5f;
        float elapsed = 0f;

        // ���������� ���İ� ����
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);

            // �̹��� ���İ� ����
            Color imageColor = buttonImage.color;
            imageColor.a = Mathf.Lerp(originalImageColor.a, 0f, normalizedTime);
            buttonImage.color = imageColor;

            // �ؽ�Ʈ ���İ� ����
            if (buttonText != null)
            {
                Color textColor = buttonText.color;
                textColor.a = Mathf.Lerp(originalTextColor.a, 0f, normalizedTime);
                buttonText.color = textColor;
            }

            yield return null;
        }

        // ������ �����ϰ�
        if (buttonImage != null)
        {
            Color finalColor = buttonImage.color;
            finalColor.a = 0f;
            buttonImage.color = finalColor;
        }

        if (buttonText != null)
        {
            Color finalTextColor = buttonText.color;
            finalTextColor.a = 0f;
            buttonText.color = finalTextColor;
        }
    }
    [SerializeField] private Transform[] buttonSlots; // Inspector���� �̸� ������ ���� ��ġ��
    
    private void ScatterButtons()
    {
        // Ȱ��ȭ�� ��ư�� ã��
        List<Button> activeButtons = new List<Button>();
        foreach (var button in puzzleButtons)
        {
            if (button.gameObject.activeSelf)
            {
                activeButtons.Add(button);
            }
        }

        if (activeButtons.Count == 0) return;

        // ���� �ε��� ����Ʈ ���� �� ����
        List<int> slotIndices = new List<int>();
        for (int i = 0; i < Mathf.Min(buttonSlots.Length, activeButtons.Count); i++)
        {
            slotIndices.Add(i);
        }

        // ���� ���� ����
        for (int i = 0; i < slotIndices.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, slotIndices.Count);
            int temp = slotIndices[i];
            slotIndices[i] = slotIndices[randomIndex];
            slotIndices[randomIndex] = temp;
        }

        // ��ư���� ���õ� ���Կ� ��ġ
        for (int i = 0; i < activeButtons.Count; i++)
        {
            if (i < slotIndices.Count && slotIndices[i] < buttonSlots.Length)
            {
                RectTransform buttonRect = activeButtons[i].GetComponent<RectTransform>();
                Transform targetSlot = buttonSlots[slotIndices[i]];

                // ���� ��ġ�� �̵�
                buttonRect.position = targetSlot.position;

                Debug.Log($"��ư {i} -> ���� {slotIndices[i]}�� ��ġ");
            }
        }
    }

    public void StopGameImmediately()
    {
        Debug.Log("[StopGameImmediately] ���� ��� �ߴ�");

        isRunning = false;
        currentTime = 0f;

        if (filledBar != null)
            filledBar.fillAmount = 0f;

        Hide();
    }

    
    private void FailGame(string reason)
    {
        Debug.Log($"[FailGame] ���� ����: {reason}");
        isRunning = false;
        Hide();
        onFail?.Invoke(reason);
    }
    
    private void OnEnable()
    {
        Debug.Log("[UI] ButtonGameUI Ȱ��ȭ��!");
    }
}