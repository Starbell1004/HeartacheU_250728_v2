using UnityEngine;
using Naninovel;
using Naninovel.UI;
using System;
using System.Linq;

public class HomeUIButtonHandler : MonoBehaviour
{
    [Header("UI ������Ʈ")]
    [SerializeField] private StageSelectUI stageSelectUI; // �������� ���� UI
    [SerializeField] private HomeGalleryPanel galleryPanel;
    [SerializeField] private HomeSettingsPanel settingsPanel;
    [SerializeField] private GameObject quitConfirmPanel;

    [Header("New Game ����")]
    [Tooltip("Services to exclude from state reset when starting a new game.")]
    [SerializeField] private string[] excludeFromReset = new string[0];

    // TitleNewGameButton���� ������ �ʵ��
    private string startScriptPath;
    private IScriptPlayer player;
    private IStateManager state;
    private IScriptManager scripts;
    private IUIManager uiManager;
    private ICustomVariableManager variableManager;

    private async void Start()
    {
        Debug.Log("[HomeUIButtonHandler] Start �޼��� ����");
        // TitleNewGameButton �ʱ�ȭ ����
        if (Engine.Initialized)
        {
            await InitializeNewGameLogic();
        }
        else
        {
            Engine.OnInitializationFinished += async () => await InitializeNewGameLogic();
        }

        // ���� �ʱ�ȭ�� ����
        if (!Engine.Initialized)
        {
            Engine.OnInitializationFinished += OnEngineInitialized;
        }
    }

    private async UniTask InitializeNewGameLogic()
    {
        Debug.Log("[HomeUIButtonHandler] InitializeNewGameLogic ����");
        scripts = Engine.GetServiceOrErr<IScriptManager>();
        startScriptPath = await ResolveStartScriptPath(scripts);
        player = Engine.GetServiceOrErr<IScriptPlayer>();
        state = Engine.GetServiceOrErr<IStateManager>();
        uiManager = Engine.GetService<IUIManager>();
        variableManager = Engine.GetService<ICustomVariableManager>();

        Debug.Log($"[HomeUIButtonHandler] �ʱ�ȭ �Ϸ� - uiManager: {uiManager != null}");
    }

    // === ���� ����/�̾��ϱ� ���� �޼���� ===

    // HomeUIButtonHandler.cs ����

    // HomeUIButtonHandler.cs ������ �κ�

    // HomeUIButtonHandler.cs�� �߰��� �κе�

    public async void StartGame()
    {
        try
        {
            Debug.Log("�� ���� ���� ��ư Ŭ��");

            // ������ �ý��� �ʱ�ȭ �߰�
            if (StateBasedItemSystem.Instance != null)
            {
                StateBasedItemSystem.Instance.ResetAllItems();
                Debug.Log("[HomeUIButtonHandler] �� ���� - ��� ������ �ʱ�ȭ");
            }

            if (variableManager != null)
            {
                variableManager.SetVariableValue("G_GameStarted", new CustomVariableValue("true"));
                variableManager.SetVariableValue("GameMode", new CustomVariableValue("NewGame"));
                variableManager.SetVariableValue("SelectedDay", new CustomVariableValue("0"));
            }

            await PlayTitleNewGame();
            HideHomeUI();

            using (await LoadingScreen.Show())
            {
                await state.ResetState(excludeFromReset);

                if (variableManager != null)
                {
                    variableManager.SetVariableValue("GameMode", new CustomVariableValue("NewGame"));
                    variableManager.SetVariableValue("SelectedDay", new CustomVariableValue("0"));
                }

                await player.LoadAndPlay(startScriptPath);
            }

            Debug.Log($"���� ���� �Ϸ�: {startScriptPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� ���� ����: {ex.Message}");
        }
    }

    public async void StartGameForStage()
    {
        try
        {
            Debug.Log("[HomeUIButtonHandler] �������� �����Ͽ� ���� ����");

            // ������ �ý��� �ʱ�ȭ �߰�
            if (StateBasedItemSystem.Instance != null)
            {
                StateBasedItemSystem.Instance.ResetAllItems();
                Debug.Log("[HomeUIButtonHandler] �������� ���� - ��� ������ �ʱ�ȭ");
            }

            string selectedDay;
            if (variableManager != null && variableManager.TryGetVariableValue("SelectedDay", out string dayValue))
            {
                selectedDay = dayValue;
            }
            else
            {
                Debug.LogError("[HomeUIButtonHandler] 'SelectedDay' ������ ã�� ���߽��ϴ�.");
                return;
            }

            if (variableManager != null)
            {
                variableManager.SetVariableValue("GameMode", new CustomVariableValue("StageSelect"));
            }

            await PlayTitleNewGame();
            HideHomeUI();

            using (await LoadingScreen.Show())
            {
                await state.ResetState(excludeFromReset);

                if (variableManager != null)
                {
                    variableManager.SetVariableValue("GameMode", new CustomVariableValue("StageSelect"));
                    variableManager.SetVariableValue("SelectedDay", new CustomVariableValue(selectedDay));
                }

                await player.LoadAndPlay(startScriptPath);
            }

            Debug.Log($"[HomeUIButtonHandler] {selectedDay}���� �÷��̸� �����մϴ�.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"�������� ���� �� ���� �߻�: {ex.Message}");
        }
    }
    // �̾��ϱ� �޼��� - StageSelectUI ǥ��
    public void ContinueGame()
    {
        Debug.Log("[HomeUIButtonHandler] �̾��ϱ� Ŭ��");

        if (stageSelectUI != null)
        {
            // HomePanel�� �����
            var homePanel = transform.Find("HomePanel");
            if (homePanel != null)
            {
                homePanel.gameObject.SetActive(false);
            }

            stageSelectUI.Show();
        }
        else
        {
            Debug.LogError("[HomeUIButtonHandler] StageSelectUI�� Inspector�� ������� �ʾҽ��ϴ�!");
        }
    }

    // === ��Ÿ UI �޼���� ===

    public void OpenGallery()
    {
        try
        {
            if (galleryPanel != null)
            {
                galleryPanel.Show();
                Debug.Log("������ �г� ǥ��");
            }
            else
            {
                Debug.LogWarning("HomeGalleryPanel�� ������� �ʾҽ��ϴ�!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"������ �г� ���� �� ����: {ex.Message}");
        }
    }

    public void OpenSettings()
    {
        try
        {
            if (settingsPanel != null)
            {
                settingsPanel.Show();
                Debug.Log("���� �г� ǥ��");
            }
            else if (uiManager != null)
            {
                var settingsUI = uiManager.GetUI<GameSettingsPanel>();
                if (settingsUI != null)
                {
                    settingsUI.Show();
                    Debug.Log("���� �г� ǥ�� (UIManager)");
                }
                else
                {
                    Debug.LogWarning("GameSettingsPanel�� ã�� �� �����ϴ�!");
                }
            }
            else
            {
                Debug.LogWarning("GameSettingsPanel�� ������� �ʾҽ��ϴ�!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� �г� ���� �� ����: {ex.Message}");
        }
    }

    public void QuitGame()
    {
        try
        {
            if (quitConfirmPanel != null)
            {
                quitConfirmPanel.SetActive(true);
                Debug.Log("���� Ȯ�� �г� ǥ��");
            }
            else
            {
                ConfirmQuit();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� ���� �� ����: {ex.Message}");
        }
    }

    public void ConfirmQuit()
    {
        try
        {
            Debug.Log("���� ����");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� ���� �� ����: {ex.Message}");
        }
    }

    public void CancelQuit()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(false);
            Debug.Log("���� ���� ���");
        }
    }

    // === ���� ���� �޼���� ===

    private async UniTask PlayTitleNewGame()
    {
        const string label = "OnNewGame";

        var scriptPath = scripts.Configuration.TitleScript;
        if (string.IsNullOrEmpty(scriptPath)) return;
        var script = (Script)await scripts.ScriptLoader.LoadOrErr(scripts.Configuration.TitleScript);
        if (!script.LabelExists(label)) return;

        player.ResetService();
        await player.LoadAndPlayAtLabel(scriptPath, label);
        await UniTask.WaitWhile(() => player.Playing);
    }

    private async UniTask<string> ResolveStartScriptPath(IScriptManager scripts)
    {
        if (!string.IsNullOrEmpty(scripts.Configuration.StartGameScript))
            return scripts.Configuration.StartGameScript;
        if (!Application.isEditor)
            Engine.Warn("Please specify 'Start Game Script' in the scripts configuration. " +
                        "When not specified, Naninovel will pick first available script, " +
                        "which may differ between the editor and build environments.");
        return (await scripts.ScriptLoader.Locate()).OrderBy(p => p).FirstOrDefault();
    }

    private void HideHomeUI()
    {
        try
        {
            Debug.Log("=== Home UI ���� ���� ===");

            // HomeUI ������Ʈ�� Hide() ȣ��
            var homeUIComponent = GetComponent<HomeUI>();
            if (homeUIComponent != null)
            {
                homeUIComponent.Hide();
                Debug.Log("HomeUI Hide() �޼��� ȣ�� �Ϸ�");
            }

            // HomeUI GameObject ��ü�� ��Ȱ��ȭ
            gameObject.SetActive(false);
            Debug.Log("HomeUI GameObject ��Ȱ��ȭ �Ϸ�");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Home UI ���� ����: {ex.Message}");
        }
    }
    private void OnEngineInitialized()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;
        Debug.Log("Naninovel Engine �ʱ�ȭ �Ϸ� - UI ��ư ��� ����");
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;
    }
}