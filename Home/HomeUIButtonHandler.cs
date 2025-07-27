using UnityEngine;
using Naninovel;
using Naninovel.UI;
using System;
using System.Linq;

public class HomeUIButtonHandler : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private StageSelectUI stageSelectUI; // 스테이지 선택 UI
    [SerializeField] private HomeGalleryPanel galleryPanel;
    [SerializeField] private HomeSettingsPanel settingsPanel;
    [SerializeField] private GameObject quitConfirmPanel;

    [Header("New Game 설정")]
    [Tooltip("Services to exclude from state reset when starting a new game.")]
    [SerializeField] private string[] excludeFromReset = new string[0];

    // TitleNewGameButton에서 가져온 필드들
    private string startScriptPath;
    private IScriptPlayer player;
    private IStateManager state;
    private IScriptManager scripts;
    private IUIManager uiManager;
    private ICustomVariableManager variableManager;

    private async void Start()
    {
        Debug.Log("[HomeUIButtonHandler] Start 메서드 시작");
        // TitleNewGameButton 초기화 로직
        if (Engine.Initialized)
        {
            await InitializeNewGameLogic();
        }
        else
        {
            Engine.OnInitializationFinished += async () => await InitializeNewGameLogic();
        }

        // 기존 초기화도 유지
        if (!Engine.Initialized)
        {
            Engine.OnInitializationFinished += OnEngineInitialized;
        }
    }

    private async UniTask InitializeNewGameLogic()
    {
        Debug.Log("[HomeUIButtonHandler] InitializeNewGameLogic 시작");
        scripts = Engine.GetServiceOrErr<IScriptManager>();
        startScriptPath = await ResolveStartScriptPath(scripts);
        player = Engine.GetServiceOrErr<IScriptPlayer>();
        state = Engine.GetServiceOrErr<IStateManager>();
        uiManager = Engine.GetService<IUIManager>();
        variableManager = Engine.GetService<ICustomVariableManager>();

        Debug.Log($"[HomeUIButtonHandler] 초기화 완료 - uiManager: {uiManager != null}");
    }

    // === 게임 시작/이어하기 관련 메서드들 ===

    // HomeUIButtonHandler.cs 파일

    // HomeUIButtonHandler.cs 수정된 부분

    // HomeUIButtonHandler.cs에 추가할 부분들

    public async void StartGame()
    {
        try
        {
            Debug.Log("새 게임 시작 버튼 클릭");

            // 아이템 시스템 초기화 추가
            if (StateBasedItemSystem.Instance != null)
            {
                StateBasedItemSystem.Instance.ResetAllItems();
                Debug.Log("[HomeUIButtonHandler] 새 게임 - 모든 아이템 초기화");
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

            Debug.Log($"게임 시작 완료: {startScriptPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"게임 시작 실패: {ex.Message}");
        }
    }

    public async void StartGameForStage()
    {
        try
        {
            Debug.Log("[HomeUIButtonHandler] 스테이지 선택하여 게임 시작");

            // 아이템 시스템 초기화 추가
            if (StateBasedItemSystem.Instance != null)
            {
                StateBasedItemSystem.Instance.ResetAllItems();
                Debug.Log("[HomeUIButtonHandler] 스테이지 선택 - 모든 아이템 초기화");
            }

            string selectedDay;
            if (variableManager != null && variableManager.TryGetVariableValue("SelectedDay", out string dayValue))
            {
                selectedDay = dayValue;
            }
            else
            {
                Debug.LogError("[HomeUIButtonHandler] 'SelectedDay' 변수를 찾지 못했습니다.");
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

            Debug.Log($"[HomeUIButtonHandler] {selectedDay}일차 플레이를 시작합니다.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"스테이지 시작 중 예외 발생: {ex.Message}");
        }
    }
    // 이어하기 메서드 - StageSelectUI 표시
    public void ContinueGame()
    {
        Debug.Log("[HomeUIButtonHandler] 이어하기 클릭");

        if (stageSelectUI != null)
        {
            // HomePanel만 숨기기
            var homePanel = transform.Find("HomePanel");
            if (homePanel != null)
            {
                homePanel.gameObject.SetActive(false);
            }

            stageSelectUI.Show();
        }
        else
        {
            Debug.LogError("[HomeUIButtonHandler] StageSelectUI가 Inspector에 연결되지 않았습니다!");
        }
    }

    // === 기타 UI 메서드들 ===

    public void OpenGallery()
    {
        try
        {
            if (galleryPanel != null)
            {
                galleryPanel.Show();
                Debug.Log("갤러리 패널 표시");
            }
            else
            {
                Debug.LogWarning("HomeGalleryPanel이 연결되지 않았습니다!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"갤러리 패널 열기 중 오류: {ex.Message}");
        }
    }

    public void OpenSettings()
    {
        try
        {
            if (settingsPanel != null)
            {
                settingsPanel.Show();
                Debug.Log("설정 패널 표시");
            }
            else if (uiManager != null)
            {
                var settingsUI = uiManager.GetUI<GameSettingsPanel>();
                if (settingsUI != null)
                {
                    settingsUI.Show();
                    Debug.Log("설정 패널 표시 (UIManager)");
                }
                else
                {
                    Debug.LogWarning("GameSettingsPanel을 찾을 수 없습니다!");
                }
            }
            else
            {
                Debug.LogWarning("GameSettingsPanel이 연결되지 않았습니다!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"설정 패널 열기 중 오류: {ex.Message}");
        }
    }

    public void QuitGame()
    {
        try
        {
            if (quitConfirmPanel != null)
            {
                quitConfirmPanel.SetActive(true);
                Debug.Log("종료 확인 패널 표시");
            }
            else
            {
                ConfirmQuit();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"게임 종료 중 오류: {ex.Message}");
        }
    }

    public void ConfirmQuit()
    {
        try
        {
            Debug.Log("게임 종료");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"게임 종료 중 오류: {ex.Message}");
        }
    }

    public void CancelQuit()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(false);
            Debug.Log("게임 종료 취소");
        }
    }

    // === 내부 헬퍼 메서드들 ===

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
            Debug.Log("=== Home UI 숨김 시작 ===");

            // HomeUI 컴포넌트의 Hide() 호출
            var homeUIComponent = GetComponent<HomeUI>();
            if (homeUIComponent != null)
            {
                homeUIComponent.Hide();
                Debug.Log("HomeUI Hide() 메서드 호출 완료");
            }

            // HomeUI GameObject 자체를 비활성화
            gameObject.SetActive(false);
            Debug.Log("HomeUI GameObject 비활성화 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Home UI 숨김 실패: {ex.Message}");
        }
    }
    private void OnEngineInitialized()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;
        Debug.Log("Naninovel Engine 초기화 완료 - UI 버튼 사용 가능");
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;
    }
}