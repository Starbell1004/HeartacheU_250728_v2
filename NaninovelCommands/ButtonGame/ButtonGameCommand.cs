using Naninovel;
using Naninovel.Commands;
using System.Collections.Generic;
using UnityEngine;

[CommandAlias("���߰���")]
public class ButtonGameCommand : Command
{
    [ParameterAlias("��")]
    [RequiredParameter]
    public IntegerParameter duration;

    [ParameterAlias("����")]
    public StringParameter sequence;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        Debug.Log("ButtonGameCommand.Execute ����");

        if (!Engine.Initialized)
        {
            Engine.Warn("������ �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return UniTask.CompletedTask;
        }

        if (!duration.HasValue)
        {
            Engine.Warn("���� �ð��� �������� �ʾҽ��ϴ�!");
            return UniTask.CompletedTask;
        }

        Debug.Log($"ButtonGameCommand - ������ �ð�: {duration.Value}��");

        var manager = ButtonGameManagerSingleton.Instance;

        if (manager == null)
        {
            Debug.LogError("ButtonGameManager �ν��Ͻ��� ã�� �� �����ϴ�!");
            return UniTask.CompletedTask;
        }

        // ���� �Ķ���� ó��
        List<string> buttonSequence = null;
        if (sequence.HasValue)
        {
            // ��ǥ�� �и��� ���� ���ڿ��� ����Ʈ�� ��ȯ
            buttonSequence = new List<string>(sequence.Value.Split(','));
            Debug.Log($"������ ���� ����: {string.Join(", ", buttonSequence)}");
        }

        Debug.Log("ButtonGameManager.StartNewGame ȣ�� ��");

        // �� �ٽ�: �׻� ������ ���ο� �������� ����
        manager.StartNewGame(duration.Value, buttonSequence);

        Debug.Log("ButtonGameManager.StartNewGame ȣ�� �Ϸ�");

        return UniTask.CompletedTask;
    }
}