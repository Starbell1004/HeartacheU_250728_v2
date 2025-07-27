using UnityEngine;
using Naninovel;
using Naninovel.UI;
using System;
using System.Collections.Generic;

public class ButtonGameManager : IEngineService
{
    public static ButtonGameManager Instance { get; private set; }

    private ButtonGameUI gameUI;
    private string failReason = "none";
    private bool puzzleCompleted = false;
    private bool isGameActive = false; // �� ������ ���� �߰�

    public ButtonGameManager()
    {
        Instance = this;
    }

    public UniTask InitializeService()
    {
        if (Engine.Initialized)
            InitUI();
        else
            Engine.OnInitializationFinished += InitUI;

        return UniTask.CompletedTask;
    }

    private void InitUI()
    {
        var uiManager = Engine.GetService<UIManager>();
        uiManager.OnUICreated += OnUICreated;
        gameUI = uiManager.GetUI<ButtonGameUI>();
        gameUI?.Hide();
    }

    private void OnUICreated(CustomUI ui)
    {
        if (ui is ButtonGameUI buttonUI)
        {
            gameUI = buttonUI;
            gameUI.Hide();
        }
    }

    /// <summary>
    /// ������ ���ο� ���� ���� (�׻� ó������)
    /// �ؼ��� 15_2_0 �󺧷� ���ƿ� ������ ȣ���
    /// </summary>
    public void StartNewGame(float duration, List<string> sequence = null)
    {
        if (gameUI == null)
        {
            Debug.LogError("ButtonGameUI��ã�� �� �����ϴ�.");
            return;
        }

        // �� �ٽ�: ���� ���� ���� ��� ���� �ʱ�ȭ
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                // ���� ���� ��� ������ ��� �ʱ�ȭ
                variableManager.SetVariableValue("button_game_finished", new CustomVariableValue("false"));
                variableManager.SetVariableValue("success", new CustomVariableValue("false"));
                variableManager.SetVariableValue("failType", new CustomVariableValue("none"));

                Debug.Log("[ButtonGameManager] ���� ���� �� ���� �ʱ�ȭ �Ϸ�");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ButtonGameManager] ���� �ʱ�ȭ ����: {ex.Message}");
            }
        }

        // ���� ���� ���� ���� �ʱ�ȭ
        gameUI.StopGameImmediately();

        puzzleCompleted = false;
        failReason = "none";
        isGameActive = true;

        Debug.Log($"[ButtonGameManager] �� ���� ���� - duration: {duration}, sequence: {(sequence != null ? string.Join(",", sequence) : "�⺻")}");

        // ������ ���ο� �������� ����
        gameUI.StartNewGame(duration, sequence, OnSuccess, OnFail);
    }

    private void OnSuccess()
    {
        isGameActive = false;
        puzzleCompleted = true; // �� ���� �÷��׵� ����

        Debug.Log("[ButtonGameManager] ���� ����!");

        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                variableManager.SetVariableValue("success", new CustomVariableValue("true"));
                variableManager.SetVariableValue("failType", new CustomVariableValue("success"));

                // �� ���� �Ϸ� �÷��� ���� - ���ϳ뺧�� ��� �������� ����
                variableManager.SetVariableValue("button_game_finished", new CustomVariableValue("true"));

                Debug.Log("[ButtonGameManager] ���� ���� ���� �Ϸ�");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ButtonGameManager] ���� ���� ����: {ex.Message}");
            }
        }
    }

    private void OnFail(string reason)
    {
        isGameActive = false;
        failReason = reason; // �� ���� ������ ����

        Debug.Log($"[ButtonGameManager] ���� ���� - ����: {reason}");

        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                variableManager.SetVariableValue("success", new CustomVariableValue("false"));
                variableManager.SetVariableValue("failType", new CustomVariableValue(reason));

                // �� ���� �Ϸ� �÷��� ���� - ���ϳ뺧�� ��� �������� ����
                variableManager.SetVariableValue("button_game_finished", new CustomVariableValue("true"));

                Debug.Log($"[ButtonGameManager] ���� ���� ���� �Ϸ� - failType: {reason}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ButtonGameManager] ���� ���� ����: {ex.Message}");
            }
        }
    }

    // ���� ���� ���� Ȯ��
    public bool IsGameActive() => isGameActive; // �� �޼��� �߰�
    public bool IsPuzzleCompleted() => puzzleCompleted;
    public string GetFailReason() => failReason;

    public void ResetService()
    {
        isGameActive = false; // �� ���½� ��Ȱ��ȭ
        puzzleCompleted = false;
        failReason = "none";
    }

    public void DestroyService()
    {
        var uiManager = Engine.GetService<UIManager>();
        if (uiManager != null)
            uiManager.OnUICreated -= OnUICreated;

        if (Instance == this)
            Instance = null;
    }
}