using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("AddItem")]  // ���� �̸� ������ ȣȯ�� ����
public class AddItemCommand : Command
{
    [ParameterAlias("id")]
    [RequiredParameter]
    public StringParameter ItemId;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("AddItemCommand: ���ϳ뺧 ������ �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[AddItemCommand]  StateBasedItemSystem.Instance�� null�Դϴ�!");
            return UniTask.CompletedTask;
        }

        var id = ItemId?.Value;
        if (string.IsNullOrWhiteSpace(id))
        {
            Engine.Warn("AddItemCommand: ������ ID�� ��� �ֽ��ϴ�.");
            return UniTask.CompletedTask;
        }

        Debug.Log($"[AddItemCommand]  ������ '{id}' ȹ�� �õ�");
        bool success = StateBasedItemSystem.Instance.AcquireItem(id);

        // ����� ������ ����
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                variableManager.SetVariableValue("last_acquired_item", new CustomVariableValue(success ? id : ""));
                variableManager.SetVariableValue("acquire_success", new CustomVariableValue(success ? "1" : "0"));

                // ���� ȣȯ���� ���� ������ ����
                variableManager.SetVariableValue("item_add_success", new CustomVariableValue(success ? "1" : "0"));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AddItemCommand]  ���� ���� ����: {ex.Message}");
            }
        }

        Debug.Log($"[AddItemCommand] {(success ? "V" : "X")} ������ '{id}' ȹ�� ���: {(success ? "����" : "����")}");
        return UniTask.CompletedTask;
    }
}