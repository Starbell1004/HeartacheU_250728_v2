using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;
using Naninovel.UI;

[RequireComponent(typeof(CanvasGroup))]
public class HomeSaveLoadMenu : CustomUI
{
    [Header("UI ������Ʈ")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;

    [Header("��� ��ȯ ��۵�")]
    [SerializeField] private Toggle saveToggle;
    [SerializeField] private Toggle loadToggle;
    [SerializeField] private Toggle quickLoadToggle;

    [Header("�׸����")]
    [SerializeField] private HomeGameStateSlotsGrid saveGrid;
    [SerializeField] private HomeGameStateSlotsGrid loadGrid;
    [SerializeField] private HomeGameStateSlotsGrid quickLoadGrid;

    // ISaveLoadUI ����
    public virtual SaveLoadUIPresentationMode PresentationMode
    {
        get => presentationMode;
        set => SetPresentationMode(value);
    }

    // ���ϳ뺧 �޽�����
    [ManagedText("DefaultUI")]
    protected static string OverwriteSaveSlotMessage = "���� ���� �����͸� ����ðڽ��ϱ�?";
    [ManagedText("DefaultUI")]
    protected static string DeleteSaveSlotMessage = "���� �����͸� �����Ͻðڽ��ϱ�?";

    private SaveLoadUIPresentationMode presentationMode;
    private IStateManager stateManager;
    private IScriptPlayer scriptPlayer;
    private IScriptManager scripts;
    private IConfirmationUI confirmationUI;
    private ISaveSlotManager<GameStateMap> slotManager => stateManager?.GameSlotManager;

    public override async UniTask Initialize()
    {
        Debug.Log("[HomeSaveLoadMenu] Initialize ����");

        stateManager = Engine.GetServiceOrErr<IStateManager>();
        scripts = Engine.GetServiceOrErr<IScriptManager>();
        scriptPlayer = Engine.GetServiceOrErr<IScriptPlayer>();
        confirmationUI = Engine.GetServiceOrErr<IUIManager>().GetUI<IConfirmationUI>();

        if (confirmationUI is null)
            throw new Exception("Confirmation UI is missing.");

        // ���ϳ뺧 ���: �� �׸��庰�� �ٸ� �ڵ鷯 �Ҵ�
        await UniTask.WhenAll(
            saveGrid.Initialize(stateManager.Configuration.SaveSlotLimit,
                HandleSaveSlotClicked, HandleDeleteSaveSlotClicked, LoadSaveSlot),

            loadGrid.Initialize(stateManager.Configuration.SaveSlotLimit,
                HandleLoadSlotClicked, HandleDeleteLoadSlotClicked, LoadSaveSlot),

            quickLoadGrid.Initialize(stateManager.Configuration.QuickSaveSlotLimit,
                HandleQuickLoadSlotClicked, HandleDeleteQuickLoadSlotClicked, LoadQuickSaveSlot)
        );

        // ��ư �̺�Ʈ ����
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        // ��� �̺�Ʈ ����
        if (saveToggle != null)
            saveToggle.onValueChanged.AddListener((isOn) => { if (isOn) PresentationMode = SaveLoadUIPresentationMode.Save; });
        if (loadToggle != null)
            loadToggle.onValueChanged.AddListener((isOn) => { if (isOn) PresentationMode = SaveLoadUIPresentationMode.Load; });
        if (quickLoadToggle != null)
            quickLoadToggle.onValueChanged.AddListener((isOn) => { if (isOn) PresentationMode = SaveLoadUIPresentationMode.QuickLoad; });

        Debug.Log("[HomeSaveLoadMenu] Initialize �Ϸ�");
    }

    public enum HomeSaveLoadMode
    {
        Save, Load, QuickLoad
    }
    public virtual SaveLoadUIPresentationMode GetLastLoadMode()
    {
        // ������ Load ��� ��ȯ (�ʿ�� ���� ���� �߰�)
        return SaveLoadUIPresentationMode.Load;
    }

    protected override void Awake()
    {
        base.Awake();

        // �ʼ� ������Ʈ üũ
        if (loadGrid == null)
            loadGrid = GetComponentInChildren<HomeGameStateSlotsGrid>();
    }

    private void SetPresentationMode(SaveLoadUIPresentationMode value)
    {
        Debug.Log($"[HomeSaveLoadMenu] SetPresentationMode: {value}");

        presentationMode = value;

        // ���ϳ뺧 ���: �׸��� ǥ��/����
        if (saveGrid != null) saveGrid.gameObject.SetActive(value == SaveLoadUIPresentationMode.Save);
        if (loadGrid != null) loadGrid.gameObject.SetActive(value == SaveLoadUIPresentationMode.Load);
        if (quickLoadGrid != null) quickLoadGrid.gameObject.SetActive(value == SaveLoadUIPresentationMode.QuickLoad);

        // ��� ���� ������Ʈ (��ȯ ���� ����)
        UpdateToggleStates(value);
        UpdateTitleText();
    }

    private void UpdateToggleStates(SaveLoadUIPresentationMode mode)
    {
        // ���ϳ뺧 ���: ��� ǥ��/���� + ���� ����
        switch (mode)
        {
            case SaveLoadUIPresentationMode.Save:
                if (saveToggle != null)
                {
                    saveToggle.gameObject.SetActive(true);
                    saveToggle.SetIsOnWithoutNotify(true);
                }
                if (loadToggle != null) loadToggle.gameObject.SetActive(false);
                if (quickLoadToggle != null) quickLoadToggle.gameObject.SetActive(false);
                break;

            case SaveLoadUIPresentationMode.Load:
                if (saveToggle != null) saveToggle.gameObject.SetActive(false);
                if (loadToggle != null)
                {
                    loadToggle.gameObject.SetActive(true);
                    loadToggle.SetIsOnWithoutNotify(true);
                }
                if (quickLoadToggle != null) quickLoadToggle.gameObject.SetActive(true);
                break;

            case SaveLoadUIPresentationMode.QuickLoad:
                if (saveToggle != null) saveToggle.gameObject.SetActive(false);
                if (loadToggle != null) loadToggle.gameObject.SetActive(true);
                if (quickLoadToggle != null)
                {
                    quickLoadToggle.gameObject.SetActive(true);
                    quickLoadToggle.SetIsOnWithoutNotify(true);
                }
                break;
        }
    }

    private void UpdateTitleText()
    {
        if (titleText == null) return;

        string newText = presentationMode switch
        {
            SaveLoadUIPresentationMode.Save => "�����ϱ�",
            SaveLoadUIPresentationMode.Load => "�ҷ�����",
            SaveLoadUIPresentationMode.QuickLoad => "�� �ε�",
            _ => "����/�ҷ�����"
        };

        titleText.text = newText;
    }

    // === Save ���� �ڵ鷯�� ===
    private void HandleSaveSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
        HandleSaveSlotClicked(slotId, slotNumber);
    }

    private async void HandleSaveSlotClicked(string slotId, int slotNumber)
    {
        if (slotManager.SaveSlotExists(slotId) &&
            !await confirmationUI.Confirm(OverwriteSaveSlotMessage)) return;

        using (new InteractionBlocker())
        {
            var state = await stateManager.SaveGame(slotId);
            saveGrid?.BindSlot(slotNumber, state);
            loadGrid?.BindSlot(slotNumber, state); // Load �׸��嵵 ����ȭ
        }

        Debug.Log($"���� {slotNumber} ���� �Ϸ�");
    }

    private async void HandleDeleteSaveSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
        if (!slotManager.SaveSlotExists(slotId)) return;

        if (!await confirmationUI.Confirm(DeleteSaveSlotMessage)) return;

        slotManager.DeleteSaveSlot(slotId);
        saveGrid?.BindSlot(slotNumber, null);
        loadGrid?.BindSlot(slotNumber, null);

        Debug.Log($"Save ���� {slotNumber} ���� �Ϸ�");
    }

    // === Load ���� �ڵ鷯�� ===
    private void HandleLoadSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
        HandleLoadSlotClicked(slotId);
    }

    private async void HandleLoadSlotClicked(string slotId)
    {
        if (!slotManager.SaveSlotExists(slotId)) return;

        await PlayTitleLoad();

        using (await LoadingScreen.Show())
        {
            Hide();
            Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
            await stateManager.LoadGame(slotId);
        }

        Debug.Log($"���� �ε� �Ϸ�: {slotId}");
    }

    private async void HandleDeleteLoadSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
        if (!slotManager.SaveSlotExists(slotId)) return;

        if (!await confirmationUI.Confirm(DeleteSaveSlotMessage)) return;

        slotManager.DeleteSaveSlot(slotId);
        saveGrid?.BindSlot(slotNumber, null);
        loadGrid?.BindSlot(slotNumber, null);

        Debug.Log($"Load ���� {slotNumber} ���� �Ϸ�");
    }

    // === QuickLoad ���� �ڵ鷯�� ===
    private void HandleQuickLoadSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToQuickSaveSlotId(slotNumber);
        HandleLoadSlotClicked(slotId); // �ε� ������ ����
    }

    private async void HandleDeleteQuickLoadSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToQuickSaveSlotId(slotNumber);
        if (!slotManager.SaveSlotExists(slotId)) return;

        if (!await confirmationUI.Confirm(DeleteSaveSlotMessage)) return;

        slotManager.DeleteSaveSlot(slotId);
        quickLoadGrid?.BindSlot(slotNumber, null);

        Debug.Log($"���ε� ���� {slotNumber} ���� �Ϸ�");
    }

    // === ��ƿ��Ƽ �޼���� ===
    private async UniTask<GameStateMap> LoadSaveSlot(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
        var state = slotManager.SaveSlotExists(slotId) ? await slotManager.Load(slotId) : null;
        return state;
    }

    private async UniTask<GameStateMap> LoadQuickSaveSlot(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToQuickSaveSlotId(slotNumber);
        var state = slotManager.SaveSlotExists(slotId) ? await slotManager.Load(slotId) : null;
        return state;
    }

    private async UniTask PlayTitleLoad()
    {
        const string label = "OnLoad";

        var scriptPath = scripts.Configuration.TitleScript;
        if (string.IsNullOrEmpty(scriptPath)) return;

        var script = (Script)await scripts.ScriptLoader.LoadOrErr(scripts.Configuration.TitleScript);
        if (!script.LabelExists(label)) return;

        scriptPlayer.ResetService();
        await scriptPlayer.LoadAndPlayAtLabel(scriptPath, label);
        await UniTask.WaitWhile(() => scriptPlayer.Playing);
    }

    // === Show/Hide �������̵� ===
    public override void Show()
    {
        Debug.Log("=== HomeSaveLoadMenu Show() ȣ���! ===");
        gameObject.SetActive(true);
        base.Show();

        // stateManager�� null�̸� ��� ����
        if (stateManager == null)
        {
            Debug.Log("stateManager ��� �ʱ�ȭ ��...");
            stateManager = Engine.GetServiceOrErr<IStateManager>();
            scripts = Engine.GetServiceOrErr<IScriptManager>();
            scriptPlayer = Engine.GetServiceOrErr<IScriptPlayer>();
            confirmationUI = Engine.GetServiceOrErr<IUIManager>().GetUI<IConfirmationUI>();

            // �� Initialize ���� ȣ��!
            _ = ForceInitializeAsync();
        }

        // �׸������ �ʱ�ȭ �� �Ǿ������� ���� �ʱ�ȭ
        Debug.Log($"LoadGrid Slots Count: {loadGrid?.Slots?.Count ?? -1}");

        // Initialize �Ŀ� RefreshActiveGrid ȣ���ϵ��� ����
        if (loadGrid?.Slots?.Count > 0)
        {
            RefreshActiveGrid();
        }
    }

    private async UniTask ForceInitializeAsync()
    {
        Debug.Log("ForceInitializeAsync ����");
        await Initialize();
        Debug.Log("ForceInitializeAsync �Ϸ� - ���� RefreshActiveGrid ȣ��");
        RefreshActiveGrid();
    }

    public override void Hide()
    {
        Debug.Log("=== HomeSaveLoadMenu Hide() ȣ���! ===");
        gameObject.SetActive(false);
        base.Hide();
    }

    private void RefreshActiveGrid()
    {
        Debug.Log($"RefreshActiveGrid ȣ�� - ���� ���: {presentationMode}");

        // ���� Ȱ�� �׸��� ���ΰ�ħ
        switch (presentationMode)
        {
            case SaveLoadUIPresentationMode.Save:
                Debug.Log("SaveGrid RefreshAllSlots ȣ��");
                saveGrid?.RefreshAllSlots();
                break;
            case SaveLoadUIPresentationMode.Load:
                Debug.Log("LoadGrid RefreshAllSlots ȣ��");
                loadGrid?.RefreshAllSlots();
                break;
            case SaveLoadUIPresentationMode.QuickLoad:
                Debug.Log("QuickLoadGrid RefreshAllSlots ȣ��");
                quickLoadGrid?.RefreshAllSlots();
                break;
        }
    }
}