using Naninovel;
using Naninovel.Commands;
using UnityEngine;

/// <summary>
/// 모든 아이템을 초기화하는 나니노벨 명령어
/// 사용법: @ResetItems
/// 일차 시작시 또는 게임 오버시 아이템을 깔끔하게 초기화할 때 사용
/// </summary>
[CommandAlias("ResetItems")]
public class ResetItemsCommand : Command
{
    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("ResetItems: 나니노벨 엔진이 초기화되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[ResetItems] StateBasedItemSystem.Instance가 null입니다!");
            return UniTask.CompletedTask;
        }

        // 모든 아이템 초기화
        StateBasedItemSystem.Instance.ResetAllItems();
        Debug.Log("[ResetItems]  모든 아이템이 초기화되었습니다");

        return UniTask.CompletedTask;
    }
}