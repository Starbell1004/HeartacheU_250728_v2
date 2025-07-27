using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("hasitem")]
public class HasItemCommand : Command
{
    [ParameterAlias("id")]
    [RequiredParameter]
    public StringParameter ItemId;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("HasItemCommand: ���ϳ뺧 ������ �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[HasItemCommand] StateBasedItemSystem.Instance�� null�Դϴ�!");
            return UniTask.CompletedTask;
        }

        var id = ItemId?.Value;
        if (string.IsNullOrWhiteSpace(id))
        {
            Engine.Warn("HasItemCommand: ������ ID�� ��� �ֽ��ϴ�.");
            return UniTask.CompletedTask;
        }

        // ������ ���� ���� ���� �߰�
        var itemData = StateBasedItemSystem.Instance.GetItemData(id);
        if (itemData == null)
        {
            Debug.LogWarning($"[HasItemCommand] �������� �ʴ� ������ ID: {id}");

            // �������� �ʴ� �������� false�� ó��
            var variableManager = Engine.GetService<ICustomVariableManager>();
            if (variableManager != null)
            {
                try
                {
                    variableManager.SetVariableValue("hasitem_result", new CustomVariableValue("0"));
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[HasItemCommand] ���� ���� ����: {ex.Message}");
                }
            }

            return UniTask.CompletedTask;
        }

        bool hasItem = StateBasedItemSystem.Instance.HasItem(id);
        Debug.Log($"[HasItemCommand] ������ '{id}' ���� ����: {hasItem}");

        // ����� ������ ���� (���� ȣȯ�� ����)
        var variableManager2 = Engine.GetService<ICustomVariableManager>();
        if (variableManager2 != null)
        {
            try
            {
                variableManager2.SetVariableValue("hasitem_result", new CustomVariableValue(hasItem ? "1" : "0"));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[HasItemCommand] ���� ���� ����: {ex.Message}");
            }
        }

        return UniTask.CompletedTask;
    }
}