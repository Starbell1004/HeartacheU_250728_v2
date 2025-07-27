using UnityEngine;
using Naninovel;
using System.Threading.Tasks;

public static class StartupVariableFix
{
    // 이 코드는 Unity가 게임을 시작할 때 자동으로 단 한 번 실행합니다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static async void ForceVariableReset()
    {
        // 나니노벨 엔진이 완전히 준비될 때까지 기다립니다.
        await UniTask.WaitUntil(() => Engine.Initialized);

        // 지체없이 변수 관리자를 가져옵니다.
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager == null) return;

        // 원인이 무엇이든, 결과를 강제로 덮어씁니다.
        variableManager.SetVariableValue("G_Day1_Unlocked", new CustomVariableValue(false));

        // 콘솔에 경고 메시지를 출력해서 이 코드가 실행되었음을 알립니다.
        Debug.LogWarning("!!! [StartupVariableFix] 강제 실행: G_Day1_Unlocked 변수를 'false'로 덮어썼습니다.");
    }
}