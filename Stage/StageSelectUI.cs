using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using System.Collections.Generic;
using System.Collections;
using System;

public class StageSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class StageSlot
    {
        [Header("�⺻ ���")]
        public Button button;                    // ��ü ��ư
        public Image slotImage;                  // ���� ��ü �̹���

        [Header("��������Ʈ")]
        public Sprite unlockedSprite;            // �رݵ� ���� (�÷� ��Ʈ + �ؽ�Ʈ)
        public Sprite lockedSprite;              // ��� ���� (ȸ�� + �ڹ���)

        [Header("����")]
        public string scriptName;                // ������ ��ũ��Ʈ (#9, #23 ��)
        public int dayNumber;                    // 1, 2, 3
    }

    [Header("Stage Slots")]
    [SerializeField] private List<StageSlot> stageSlots = new List<StageSlot>();

    [Header("UI Elements")]
    [SerializeField] private Button closeButton;              // X ��ư

    private ICustomVariableManager variableManager;
    private IScriptPlayer scriptPlayer;
    private IStateManager stateManager;
    private IUIManager uiManager;

    private void Awake()
    {
        // ���� �ʱ�ȭ ���� Ȯ��
        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += OnEngineInitialized;
        }
    }
    void Start()
    {
        // �������ڸ��� ������ ������
        var allRenderers = GetComponentsInChildren<CanvasRenderer>(true);
        foreach (var cr in allRenderers)
        {
            cr.SetAlpha(1f);
        }
    }
    private void OnEngineInitialized()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;
        InitializeServices();
    }

    private void InitializeServices()
    {
        variableManager = Engine.GetService<ICustomVariableManager>();
        scriptPlayer = Engine.GetService<IScriptPlayer>();
        stateManager = Engine.GetService<IStateManager>();
        uiManager = Engine.GetService<IUIManager>();

        SetupButtons();

        Debug.Log($"[StageSelectUI] Services initialized - variableManager: {variableManager != null}");
    }

    private void SetupButtons()
    {
        Debug.Log("[StageSelectUI] SetupButtons ����");

        // X ��ư (�ݱ�)
        if (closeButton != null)
        {
            Debug.Log("[StageSelectUI] closeButton ����");
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => {
                Hide();
                // HomeUI ǥ��
                var homePanel = transform.parent.Find("HomePanel");
                if (homePanel != null)
                {
                    homePanel.gameObject.SetActive(true);
                }
            });
        }
        else
        {
            Debug.LogWarning("[StageSelectUI] closeButton�� null�Դϴ�!");
        }

        // �������� ��ư��
        Debug.Log($"[StageSelectUI] stageSlots ����: {stageSlots?.Count ?? 0}");

        if (stageSlots != null)
        {
            foreach (var slot in stageSlots)
            {
                if (slot == null)
                {
                    Debug.LogError("[StageSelectUI] slot�� null�Դϴ�!");
                    continue;
                }
                if (slot.button == null)
                {
                    Debug.LogError("[StageSelectUI] slot.button�� null�Դϴ�!");
                    continue;
                }

                var capturedSlot = slot;
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(() => OnStageSelected(capturedSlot));
            }
        }
    }
    private void OnEnable()
    {
        // Ȱ��ȭ�� ������ ����
        StartCoroutine(ForceAlphaEveryFrame());
    }

    private IEnumerator ForceAlphaEveryFrame()
    {
        // 0.5�� ���� �� �����Ӹ��� ���� ����
        float timer = 0f;
        while (timer < 0.5f)
        {
            var allRenderers = GetComponentsInChildren<CanvasRenderer>(true);
            foreach (var cr in allRenderers)
            {
                if (cr.GetAlpha() != 1f)
                {
                    Debug.LogWarning($"Alpha {cr.GetAlpha()} �� 1�� ���� ����!");
                    cr.SetAlpha(1f);
                }
            }

            timer += Time.deltaTime;
            yield return null;  // ���� �����ӱ��� ���
        }

        Debug.Log("Alpha ���� ���� ����");
    }
    public void Show()
    {
        gameObject.SetActive(true);

        if (variableManager == null)
        {
            InitializeServices();
        }

        RefreshStageStates();

        // 0.1�� �Ŀ� ������ ������ ó��
        Invoke("FixAlpha", 0.1f);
    }

    private void FixAlpha()
    {
        var allRenderers = GetComponentsInChildren<CanvasRenderer>(true);
        foreach (var cr in allRenderers)
        {
            cr.SetAlpha(1f);
        }
    }
  

    
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ForceSlotOpacity()
    {
        foreach (var slot in stageSlots)
        {
            if (slot.slotImage != null)
            {
                slot.slotImage.color = Color.white;

                // CanvasRenderer�� ������! 
                var cr = slot.slotImage.GetComponent<CanvasRenderer>();
                if (cr != null) cr.SetAlpha(1f);
            }

            // ��ư�� ��� �̹����鵵
            if (slot.button != null)
            {
                var images = slot.button.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    img.color = Color.white;
                    var cr = img.GetComponent<CanvasRenderer>();
                    if (cr != null) cr.SetAlpha(1f);
                }
            }
        }
    }

    private void RefreshStageStates()
    {
        foreach (var slot in stageSlots)
        {
            bool isUnlocked = CheckStageUnlocked(slot.dayNumber);
            UpdateSlotVisual(slot, isUnlocked);
        }
    }


    // StageSelectUI.cs�� CheckStageUnlocked �޼��忡 ����� �߰�
    private bool CheckStageUnlocked(int day)
    {
        string unlockVariable = $"G_Day{day}_Unlocked";

        if (variableManager == null) return false;

        // bool�� ���� �õ�
        if (variableManager.TryGetVariableValue<bool>(unlockVariable, out var boolValue))
        {
            Debug.Log($"[StageSelectUI] {unlockVariable} = {boolValue} (bool)");
            return boolValue;
        }

        // string���� �õ�
        if (variableManager.TryGetVariableValue<string>(unlockVariable, out var stringValue))
        {
            Debug.Log($"[StageSelectUI] {unlockVariable} = '{stringValue}' (string)");
            // "true", "1", "True" ���� true�� ó��
            return stringValue.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   stringValue == "1";
        }

        Debug.Log($"[StageSelectUI] {unlockVariable} ������ ���ų� ���� �� ����");
        return false;
    }

    private void UpdateSlotVisual(StageSlot slot, bool isUnlocked)
    {
        slot.button.interactable = isUnlocked;

        if (slot.slotImage != null)
        {
            slot.slotImage.sprite = isUnlocked ? slot.unlockedSprite : slot.lockedSprite;
            slot.slotImage.color = Color.white;

            // CanvasRenderer ���� ����! 
            var canvasRenderer = slot.slotImage.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
            {
                canvasRenderer.SetAlpha(1f);
            }
        }
    }
    private void OnStageSelected(StageSlot slot)
    {
        Debug.Log($"[StageSelectUI] {slot.dayNumber}���� ���õ�");

        // �� �κ��� ����ǰ� �ֳ���?
        if (variableManager != null)
        {
            variableManager.SetVariableValue("SelectedDay", new CustomVariableValue(slot.dayNumber.ToString()));
            Debug.Log($"[StageSelectUI] SelectedDay = {slot.dayNumber}");
        }
        else
        {
            Debug.LogError("[StageSelectUI] variableManager�� NULL�Դϴ�!");
        }

        // HomeUI���� �ö󰡱�
        Transform homeUITransform = transform.parent?.parent;

        if (homeUITransform != null)
        {
            var homeButtonHandler = homeUITransform.GetComponent<HomeUIButtonHandler>();

            gameObject.SetActive(false);

            if (homeButtonHandler != null)
            {
                // ���� ���� �޼��� ȣ��
                homeButtonHandler.StartGameForStage();
            }
        }
    }

    private IEnumerator ResetGameState()
    {
        Debug.Log("[StageSelectUI] ���� ���� �ʱ�ȭ ����");

        // ������ �ʱ�ȭ
        var itemSystem = StateBasedItemSystem.Instance;
        if (itemSystem != null)
        {
            foreach (var itemData in itemSystem.GetAllItemData())
            {
                itemSystem.SetItemState(itemData.Key, StateBasedItemSystem.ItemState.NotOwned);
            }
        }

        // ItemDisplayUI �ʱ�ȭ
        if (uiManager != null)
        {
            var itemDisplayUI = uiManager.GetUI<ItemDisplayUI>();
            if (itemDisplayUI != null)
            {
                var restoreMethod = itemDisplayUI.GetType().GetMethod("RestoreItemIconsFromState");
                restoreMethod?.Invoke(itemDisplayUI, null);
            }
        }

        // �Ϲ� ���� �ʱ�ȭ
        ClearNonGlobalVariables();

        yield return new WaitForSeconds(0.1f);
        Debug.Log("[StageSelectUI] ���� ���� �ʱ�ȭ �Ϸ�");
    }

    private void ClearNonGlobalVariables()
    {
        // �������� ���� ���� ������ �ʱ�ȭ
        // G_�� �������� �ʴ� ������
        if (variableManager != null && variableManager.VariableExists("CurrentDay"))
        {
            variableManager.SetVariableValue("CurrentDay", new CustomVariableValue("0"));
        }
    }

    private void InitializeStageVariables(int day)
    {
        if (variableManager != null)
        {
            variableManager.SetVariableValue("CurrentDay", new CustomVariableValue(day.ToString()));
            Debug.Log($"[StageSelectUI] {day}���� �ʱ�ȭ");
        }
    }

    private void OnDestroy()
    {
        // ���� �̺�Ʈ ����
        Engine.OnInitializationFinished -= OnEngineInitialized;

        // ��� ��ư ������ ����
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();

        if (stageSlots != null)
        {
            foreach (var slot in stageSlots)
            {
                if (slot != null && slot.button != null)
                {
                    slot.button.onClick.RemoveAllListeners();
                }
            }
        }
    }
}