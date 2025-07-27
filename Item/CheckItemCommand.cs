using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("CheckItemState")]
public class CheckItemStateCommand : Command
{
    [ParameterAlias("id")]
    [RequiredParameter]
    public StringParameter ItemId;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("CheckItemState: ���ϳ뺧 ������ �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[CheckItemState] StateBasedItemSystem.Instance�� null�Դϴ�!");
            return UniTask.CompletedTask;
        }

        var id = ItemId?.Value;
        if (string.IsNullOrWhiteSpace(id))
        {
            Engine.Warn("CheckItemState: ������ ID�� ��� �ֽ��ϴ�.");
            return UniTask.CompletedTask;
        }

        // ������ ���� ���� ���� �߰�
        var itemData = StateBasedItemSystem.Instance.GetItemData(id);
        if (itemData == null)
        {
            Debug.LogWarning($"[CheckItemState] �������� �ʴ� ������ ID: {id}");

            // �������� �ʴ� �������� ��� �⺻��(NotOwned)���� ó��
            var variableManager = Engine.GetService<ICustomVariableManager>();
            if (variableManager != null)
            {
                try
                {
                    variableManager.SetVariableValue("item_state", new CustomVariableValue("0"));
                    variableManager.SetVariableValue("item_owned", new CustomVariableValue("0"));
                    variableManager.SetVariableValue("item_used", new CustomVariableValue("0"));
                    variableManager.SetVariableValue("item_ever_owned", new CustomVariableValue("0"));
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[CheckItemState] ���� ���� ����: {ex.Message}");
                }
            }

            return UniTask.CompletedTask;
        }

        var state = StateBasedItemSystem.Instance.GetItemState(id);
        Debug.Log($"[CheckItemState] ������ '{id}' ����: {state}({(int)state})");

        // ����� ������ ����
        var variableManager2 = Engine.GetService<ICustomVariableManager>();
        if (variableManager2 != null)
        {
            try
            {
                variableManager2.SetVariableValue("item_state", new CustomVariableValue(((int)state).ToString()));
                variableManager2.SetVariableValue("item_owned", new CustomVariableValue(state == StateBasedItemSystem.ItemState.Owned ? "1" : "0"));
                variableManager2.SetVariableValue("item_used", new CustomVariableValue(state == StateBasedItemSystem.ItemState.Used ? "1" : "0"));
                variableManager2.SetVariableValue("item_ever_owned", new CustomVariableValue(state >= StateBasedItemSystem.ItemState.Owned ? "1" : "0"));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CheckItemState] ���� ���� ����: {ex.Message}");
            }
        }

        return UniTask.CompletedTask;
    }
}