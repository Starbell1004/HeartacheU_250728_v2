using UnityEngine;
using Naninovel;
using System.Threading.Tasks;

public static class StartupVariableFix
{
    // �� �ڵ�� Unity�� ������ ������ �� �ڵ����� �� �� �� �����մϴ�.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static async void ForceVariableReset()
    {
        // ���ϳ뺧 ������ ������ �غ�� ������ ��ٸ��ϴ�.
        await UniTask.WaitUntil(() => Engine.Initialized);

        // ��ü���� ���� �����ڸ� �����ɴϴ�.
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager == null) return;

        // ������ �����̵�, ����� ������ ����ϴ�.
        variableManager.SetVariableValue("G_Day1_Unlocked", new CustomVariableValue(false));

        // �ֿܼ� ��� �޽����� ����ؼ� �� �ڵ尡 ����Ǿ����� �˸��ϴ�.
        Debug.LogWarning("!!! [StartupVariableFix] ���� ����: G_Day1_Unlocked ������ 'false'�� �������ϴ�.");
    }
}