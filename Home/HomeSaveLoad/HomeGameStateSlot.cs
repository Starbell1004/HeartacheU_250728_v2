using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Naninovel;

public class HomeGameStateSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI ������Ʈ")]
    [SerializeField] private Image screenshotImage;
    [SerializeField] private TextMeshProUGUI slotNumberText;
    [SerializeField] private TextMeshProUGUI gameTimeText;
    [SerializeField] private TextMeshProUGUI realTimeText;
    [SerializeField] private Button slotButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject emptyOverlay;

    [Header("����")]
    [SerializeField] private string dateFormat = "MM/dd HH:mm";
    [SerializeField] private Color emptySlotColor = Color.gray;

    // Naninovel ��Ÿ�� ������Ƽ��
    public virtual int SlotNumber { get; private set; }
    public virtual GameStateMap State { get; private set; }
    public virtual bool Empty => State == null;
    public virtual string SlotId => $"CustomSlot_{SlotNumber - 1}"; // 0-based index

    [ManagedText("DefaultUI")]
    protected static string EmptySlotLabel = "�����ϱ�";

    private Action<int> onSlotClicked;
    private Action<int> onDeleteClicked;
    private IStateManager stateManager;

    private void Awake()
    {
        // ������Ʈ �ڵ� ã��
        FindUIComponents();

        // �̺�Ʈ ����
        SetupEventListeners();

        // Naninovel ���� ��������
        if (Engine.Initialized)
        {
            stateManager = Engine.GetServiceOrErr<IStateManager>();
        }
        else
        {
            Engine.OnInitializationFinished += () => stateManager = Engine.GetServiceOrErr<IStateManager>();
        }
    }

    public virtual void Initialize(Action<int> onSlotClicked, Action<int> onDeleteClicked)
    {
        this.onSlotClicked = onSlotClicked;
        this.onDeleteClicked = onDeleteClicked;

        Debug.Log($"[HomeGameStateSlot] Initialize - ���� �̺�Ʈ �����");
    }

    public virtual void Bind(int slotNumber, GameStateMap state)
    {
        SlotNumber = slotNumber;
        State = state;

        Debug.Log($"[HomeGameStateSlot] Bind - ���� {slotNumber}, Empty: {Empty}");

        if (Empty)
            SetEmptyState();
        else
            SetNonEmptyState();
    }

    private void FindUIComponents()
    {
        // ������Ʈ�� �ڵ� ã��
        if (screenshotImage == null) screenshotImage = GetComponentInChildren<Image>();
        if (slotButton == null) slotButton = GetComponent<Button>();
        if (deleteButton == null) deleteButton = GetComponentInChildren<Button>();

        // �ؽ�Ʈ ������Ʈ�� ã��
        var texts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            if (text.name.ToLower().Contains("slot") && slotNumberText == null)
                slotNumberText = text;
            else if (text.name.ToLower().Contains("game") && gameTimeText == null)
                gameTimeText = text;
            else if (text.name.ToLower().Contains("real") && realTimeText == null)
                realTimeText = text;
        }

        // EmptyOverlay ã��
        if (emptyOverlay == null)
        {
            var overlayTransform = transform.Find("EmptyOverlay");
            if (overlayTransform != null)
                emptyOverlay = overlayTransform.gameObject;
        }
    }

    private void SetupEventListeners()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(HandleSlotClicked);

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(HandleDeleteClicked);
            deleteButton.gameObject.SetActive(false); // �⺻������ ����
        }
    }

    private void SetEmptyState()
    {
        Debug.Log($"[HomeGameStateSlot] SetEmptyState - ���� {SlotNumber}");

        // ���� ��ư ����
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);

        // �� ���� �������� ǥ��
        if (emptyOverlay != null)
            emptyOverlay.SetActive(true);

        // ���� ��ȣ ����
        SetSlotNumberText();

        // �� ���� �ؽ�Ʈ
        if (gameTimeText != null)
            gameTimeText.text = EmptySlotLabel;

        if (realTimeText != null)
            realTimeText.text = "";

        // �� ���� �̹���
        if (screenshotImage != null)
        {
            screenshotImage.sprite = null;
            screenshotImage.color = emptySlotColor;
        }
    }

    private void SetNonEmptyState()
    {
        Debug.Log($"[HomeGameStateSlot] SetNonEmptyState - ���� {SlotNumber}");

        Debug.Log($"[HomeGameStateSlot] SetNonEmptyState - ���� {SlotNumber}");

        // ���� ��ư �׻� ���� (���� ��� ��Ȱ��ȭ)
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);

        // �� ���� �������� ����
        if (emptyOverlay != null)
            emptyOverlay.SetActive(false);
        // ���� ��ȣ ����
        SetSlotNumberText();

        // ���� �ð� ǥ��
        SetGameTimeText();

        // ���� ��¥ ǥ��
        if (realTimeText != null && State != null)
            realTimeText.text = State.SaveDateTime.ToString(dateFormat);

        // ��ũ���� ǥ��
        if (screenshotImage != null && State?.Thumbnail != null)
        {
            Sprite sprite = Sprite.Create(
                State.Thumbnail,
                new Rect(0, 0, State.Thumbnail.width, State.Thumbnail.height),
                Vector2.one * 0.5f
            );
            screenshotImage.sprite = sprite;
            screenshotImage.color = Color.white;
        }
    }

    private void SetSlotNumberText()
    {
        if (slotNumberText != null)
            slotNumberText.text = $"Slot {SlotNumber:D2}";
    }

    private void SetGameTimeText()
    {
        if (gameTimeText == null) return;

        try
        {
            // Naninovel �������� ���� �ð� ��������
            var varManager = Engine.GetService<ICustomVariableManager>();
            if (varManager != null)
            {
                string dayStr = GetStringVariableValue(varManager, "day", "0");
                string hourStr = GetStringVariableValue(varManager, "hour", "0");
                string minStr = GetStringVariableValue(varManager, "min", "0");

                // ���ڿ��� ���ڷ� ��ȯ
                int.TryParse(dayStr, out int day);
                int.TryParse(hourStr, out int hour);
                int.TryParse(minStr, out int min);

                // ��� 0�̸� �⺻ �޽���
                if (day == 0 && hour == 0 && min == 0)
                {
                    gameTimeText.text = "����� ����";
                }
                else
                {
                    gameTimeText.text = $"2�� {day}�� {hour:D2}:{min:D2}";  // 2�� �� 2��
                }
            }
            else
            {
                gameTimeText.text = "����� ����";
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[HomeGameStateSlot] ���� �ð� ǥ�� ����: {ex.Message}");
            gameTimeText.text = "����� ����";
        }
    }

    private string GetStringVariableValue(ICustomVariableManager varManager, string variableName, string defaultValue)
    {
        try
        {
            var variable = varManager.GetVariableValue(variableName);
            if (variable != null)
                return variable.ToString();
        }
        catch { }
        return defaultValue;
    }

    // �̺�Ʈ �ڵ鷯��
    private void HandleSlotClicked()
    {
        Debug.Log($"[HomeGameStateSlot] ���� {SlotNumber} Ŭ����");
        onSlotClicked?.Invoke(SlotNumber);
    }

    private void HandleDeleteClicked()
    {
        Debug.Log($"[HomeGameStateSlot] ���� {SlotNumber} ���� ��ư Ŭ����");
        onDeleteClicked?.Invoke(SlotNumber);
    }

    // ���콺 ȣ�� �̺�Ʈ
    // ���콺 ȣ�� �̺�Ʈ
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ���� ��� ��Ȱ��ȭ
        // if (deleteButton != null && !Empty)
        //     deleteButton.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // if (deleteButton != null)
        //     deleteButton.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        if (slotButton != null)
            slotButton.onClick.RemoveListener(HandleSlotClicked);

        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(HandleDeleteClicked);
    }
}