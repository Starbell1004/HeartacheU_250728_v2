using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("UseItem")]
public class UseItemCommand : Command
{
    [ParameterAlias("id")]
    public StringParameter ItemId;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("UseItemCommand: ���ϳ뺧 ������ �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[UseItemCommand]  StateBasedItemSystem.Instance�� null�Դϴ�!");
            return UniTask.CompletedTask;
        }

        string usedItemId = null;
        bool success = false;

        if (!string.IsNullOrEmpty(ItemId?.Value))
        {
            // Ư�� ������ ���
            Debug.Log($"[UseItemCommand]  Ư�� ������ ���: {ItemId.Value}");
            success = StateBasedItemSystem.Instance.UseItem(ItemId.Value);
            if (success) usedItemId = ItemId.Value;
        }
        else
        {
            // ���� ������ ������ ��� (FIFO)
            Debug.Log("[UseItemCommand]  ���� ������ ������ ���");
            usedItemId = StateBasedItemSystem.Instance.UseOldestItem();
            success = !string.IsNullOrEmpty(usedItemId);
        }

        // ����� ������ ����
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                variableManager.SetVariableValue("last_used_item", new CustomVariableValue(usedItemId ?? ""));
                variableManager.SetVariableValue("item_use_success", new CustomVariableValue(success ? "1" : "0"));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UseItemCommand]  ���� ���� ����: {ex.Message}");
            }
        }

        if (success)
        {
            Debug.Log($"[UseItemCommand]  ������ ��� ����: {usedItemId}");
        }
        else
        {
            Debug.LogWarning("[UseItemCommand]  ����� �������� ���ų� �����߽��ϴ�.");
        }

        return UniTask.CompletedTask;
    }
}