/*using Naninovel;
using UnityEngine;
using System.Collections;

/// <summary>
/// ���� �ε� �� ������ UI�� �����ϴ� �ڵ鷯
/// </summary>
[InitializeAtRuntime]
public class ItemSaveLoadHandler : IEngineService
{
    private IStateManager stateManager;
    private ICustomVariableManager variableManager;
    private IUIManager uiManager;

    public UniTask InitializeService()
    {
        stateManager = Engine.GetService<IStateManager>();
        variableManager = Engine.GetService<ICustomVariableManager>();
        uiManager = Engine.GetService<IUIManager>();

        if (stateManager != null)
        {
            stateManager.OnGameLoadFinished += OnGameLoadFinished;
            Debug.Log("[ItemSaveLoadHandler] �ʱ�ȭ �Ϸ�");
        }

        return UniTask.CompletedTask;
    }

    private void OnGameLoadFinished(GameSaveLoadArgs args)
    {
        Debug.Log($"[ItemSaveLoadHandler] ���� �ε� �Ϸ� - ����: {args.SlotId}");

        // �ڷ�ƾ���� ���� ����
        CoroutineRunner.StartCoroutine(RestoreItemUIAfterLoad());
    }

    private IEnumerator RestoreItemUIAfterLoad()
    {
        // UI�� ������ �ε�� ������ ����� ���
        yield return new WaitForSeconds(2f);

        Debug.Log("[ItemSaveLoadHandler] ������ UI ���� ����");

        // ItemDisplayUI ã��
        var itemDisplayUI = uiManager?.GetUI<ItemDisplayUI>();
        if (itemDisplayUI == null)
        {
            itemDisplayUI = GameObject.FindObjectOfType<ItemDisplayUI>();
        }

        if (itemDisplayUI == null)
        {
            Debug.LogError("[ItemSaveLoadHandler] ItemDisplayUI�� ã�� �� �����ϴ�!");
            yield break;
        }

        // StateBasedItemSystem�� �غ�� ������ ���
        int waitCount = 0;
        while (StateBasedItemSystem.Instance == null && waitCount < 10)
        {
            yield return new WaitForSeconds(0.5f);
            waitCount++;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[ItemSaveLoadHandler] StateBasedItemSystem.Instance�� null�Դϴ�!");
            yield break;
        }

        // ��� ������ Ȯ��
        var itemDataMap = StateBasedItemSystem.Instance.GetAllItemData();
        int restoredCount = 0;

        foreach (var kvp in itemDataMap)
        {
            string itemId = kvp.Key;
            string variableName = $"item_{itemId}";

            try
            {
                // ���� Ȯ��
                if (variableManager.VariableExists(variableName))
                {
                    var value = variableManager.GetVariableValue(variableName);

                    // ���� "1"�̸� ���� ��
                    if (value != null && value.ToString() == "1")
                    {
                        var itemData = kvp.Value;
                        if (itemData != null)
                        {
                            // UI�� ������ �߰�
                            itemDisplayUI.AddItemIcon(itemData);
                            restoredCount++;
                            Debug.Log($"[ItemSaveLoadHandler] ������ UI ����: {itemId}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ItemSaveLoadHandler] ������ ���� ���� {itemId}: {ex.Message}");
            }
        }

        if (restoredCount > 0)
        {
            // UI ǥ��
            itemDisplayUI.Show();
            Debug.Log($"[ItemSaveLoadHandler] UI ���� �Ϸ� - {restoredCount}�� ������");
        }
        else
        {
            Debug.Log("[ItemSaveLoadHandler] ������ �������� �����ϴ�");
        }
    }

    public void ResetService()
    {
        // ���� �� ó��
    }

    public void DestroyService()
    {
        if (stateManager != null)
        {
            stateManager.OnGameLoadFinished -= OnGameLoadFinished;
        }
    }
}
*/