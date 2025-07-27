using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using Naninovel.UI;
using System.Collections;

public class HomeUI : CustomUI
{
    [Header("Main Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Other UI References")]
    [SerializeField] private GameObject confirmNewGamePanel;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    private ICustomVariableManager variableManager;
    private HomeUIButtonHandler buttonHandler;

    protected override void Awake()
    {
        base.Awake();

        variableManager = Engine.GetService<ICustomVariableManager>();
        buttonHandler = GetComponent<HomeUIButtonHandler>();

        SetupButtons();
    }

    private void SetupButtons()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => {
                if (buttonHandler != null)
                    buttonHandler.ContinueGame();
                else
                    Debug.LogError("[HomeUI] HomeUIButtonHandler를 찾을 수 없습니다!");
            });
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => {
                if (buttonHandler != null)
                    buttonHandler.OpenSettings();
            });
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => {
                if (buttonHandler != null)
                    buttonHandler.QuitGame();
            });
        }

        // 확인 패널 버튼 설정
        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.RemoveAllListeners();
            confirmYesButton.onClick.AddListener(() => {
                confirmNewGamePanel.SetActive(false);
                if (buttonHandler != null)
                    buttonHandler.StartGame();
            });
        }

        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.RemoveAllListeners();
            confirmNoButton.onClick.AddListener(() => {
                confirmNewGamePanel.SetActive(false);
            });
        }
    }

   
    
   

    private void OnNewGameClicked()
    {
        bool hasProgress = false;
        if (variableManager != null && variableManager.VariableExists("G_GameStarted"))
        {
            var value = variableManager.GetVariableValue("G_GameStarted");
            hasProgress = value.ToString() == "true" || value.ToString() == "1";
        }

        if (hasProgress && confirmNewGamePanel != null)
        {
            confirmNewGamePanel.SetActive(true);
        }
        else
        {
            if (buttonHandler != null)
                buttonHandler.StartGame();
        }
    }

    protected override void OnDestroy()
    {
        if (newGameButton != null)
            newGameButton.onClick.RemoveAllListeners();
        if (continueButton != null)
            continueButton.onClick.RemoveAllListeners();
        if (settingsButton != null)
            settingsButton.onClick.RemoveAllListeners();
        if (exitButton != null)
            exitButton.onClick.RemoveAllListeners();
        if (confirmYesButton != null)
            confirmYesButton.onClick.RemoveAllListeners();
        if (confirmNoButton != null)
            confirmNoButton.onClick.RemoveAllListeners();

        base.OnDestroy();
    }
}