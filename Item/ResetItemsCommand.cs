using Naninovel;
using Naninovel.Commands;
using UnityEngine;

/// <summary>
/// ��� �������� �ʱ�ȭ�ϴ� ���ϳ뺧 ��ɾ�
/// ����: @ResetItems
/// ���� ���۽� �Ǵ� ���� ������ �������� ����ϰ� �ʱ�ȭ�� �� ���
/// </summary>
[CommandAlias("ResetItems")]
public class ResetItemsCommand : Command
{
    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("ResetItems: ���ϳ뺧 ������ �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[ResetItems] StateBasedItemSystem.Instance�� null�Դϴ�!");
            return UniTask.CompletedTask;
        }

        // ��� ������ �ʱ�ȭ
        StateBasedItemSystem.Instance.ResetAllItems();
        Debug.Log("[ResetItems]  ��� �������� �ʱ�ȭ�Ǿ����ϴ�");

        return UniTask.CompletedTask;
    }
}