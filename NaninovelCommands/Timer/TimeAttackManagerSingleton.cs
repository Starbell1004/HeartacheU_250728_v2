using Naninovel;
using UnityEngine;

public static class TimeAttackManagerSingleton
{
    private static TimeAttackManager _instance;

    public static TimeAttackManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TimeAttackManager();
                // ������ �ʱ�ȭ�� �Ŀ��� ���� �ʱ�ȭ �õ�
                if (Engine.Initialized)
                {
                    _instance.InitializeService().Forget();
                    Debug.Log("TimeAttackManager �ʱ�ȭ �Ϸ�");
                }
                else
                {
                    Debug.LogWarning("������ ���� �ʱ�ȭ���� �ʾ� TimeAttackManager �ʱ�ȭ�� �����˴ϴ�.");
                }
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Unity ���� �� �ʱ�ȭ
        Debug.Log("TimeAttackManagerSingleton�� ��ϵǾ����ϴ�.");
    }
}