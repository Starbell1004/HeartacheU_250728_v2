using Naninovel;
using Naninovel.Commands;
using UnityEngine;

/// <summary>
/// ������Ƽ ������ ���� ���� Ȯ��
/// ����: @SafetyCheck
/// ���: has_safety_items (1=����, 0=����)
/// </summary>
[CommandAlias("SafetyCheck")]
public class SafetyCheckCommand : Command
{
    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("SafetyCheck: ���ϳ뺧 ������ �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[SafetyCheck] StateBasedItemSystem.Instance�� null�Դϴ�!");
            return UniTask.CompletedTask;
        }

        var ownedItems = StateBasedItemSystem.Instance.GetOwnedItems();
        bool hasSafetyItems = ownedItems.Count > 0;

        Debug.Log($"[SafetyCheck] ������Ƽ ������ ����: {hasSafetyItems} (����: {ownedItems.Count})");

        // ����� ������ ����
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                variableManager.SetVariableValue("has_safety_items", new CustomVariableValue(hasSafetyItems ? "1" : "0"));
                variableManager.SetVariableValue("safety_item_count", new CustomVariableValue(ownedItems.Count.ToString()));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SafetyCheck] ���� ���� ����: {ex.Message}");
            }
        }

        return UniTask.CompletedTask;
    }
}

/// <summary>
/// ���� ������ ������ �ڵ� ��� (������ ����)
/// ����: @UseOldest
/// ���: used_item (���� ������ ID)
/// </summary>
[CommandAlias("UseOldest")]
public class UseOldestCommand : Command
{
    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("UseOldest: ���ϳ뺧 ������ �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[UseOldest] StateBasedItemSystem.Instance�� null�Դϴ�!");
            return UniTask.CompletedTask;
        }

        string usedItem = StateBasedItemSystem.Instance.UseOldestItem();
        bool success = !string.IsNullOrEmpty(usedItem);

        Debug.Log($"[UseOldest] ���� ������ ������ ���: {(success ? usedItem : "����")}");

        // ����� ������ ���� (�����ϰ�)
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                // �⺻������ �� ���ڿ��� �ʱ�ȭ
                string safeUsedItem = usedItem ?? "";
                string safeSceneName = "";

                // �������� ���������� ���� ��쿡�� �� �̸� ���� (Scene ���̻� ����)
                if (success && !string.IsNullOrEmpty(usedItem))
                {
                    safeSceneName = usedItem; // "Scene" ���̻縦 ������ ����
                }

                variableManager.SetVariableValue("used_item", new CustomVariableValue(safeUsedItem));
                variableManager.SetVariableValue("used_item_scene", new CustomVariableValue(safeSceneName));
                variableManager.SetVariableValue("use_oldest_success", new CustomVariableValue(success ? "1" : "0"));

                Debug.Log($"[UseOldest] ���� ���� �Ϸ� - used_item: '{safeUsedItem}', used_item_scene: '{safeSceneName}'");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UseOldest] ���� ���� ����: {ex.Message}");

                // ���� �߻��� ������ �⺻������ ����
                try
                {
                    variableManager.SetVariableValue("used_item", new CustomVariableValue(""));
                    variableManager.SetVariableValue("used_item_scene", new CustomVariableValue(""));
                    variableManager.SetVariableValue("use_oldest_success", new CustomVariableValue("0"));
                }
                catch (System.Exception fallbackEx)
                {
                    Debug.LogError($"[UseOldest] �⺻�� ������ ����: {fallbackEx.Message}");
                }
            }
        }

        return UniTask.CompletedTask;
    }
}