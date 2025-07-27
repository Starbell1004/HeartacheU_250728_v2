using Naninovel;
using UnityEngine;

public static class ButtonGameManagerSingleton
{
    private static ButtonGameManager _instance;

    public static ButtonGameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ButtonGameManager();

                if (Engine.Initialized)
                {
                    _instance.InitializeService().Forget();
                    Debug.Log("ButtonGameManager �ʱ�ȭ �Ϸ�");
                }
                else
                {
                    Debug.LogWarning("���� �ʱ�ȭ ��. ButtonGameManager �ʱ�ȭ ��� ��");
                }
            }

            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Debug.Log("ButtonGameManagerSingleton ��� �Ϸ�");
    }
}