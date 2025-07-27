using UnityEngine;
using Naninovel;
using System.IO; // ����/���� ������ ���� �ʼ�

public class DebugKeyHandler : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Debug.Log("[�����] F8 - ��� ���� ������ ���� ���� ����");
            DeleteAllSaveData();
        }
    }

    /// <summary>
    /// ��ũ�� ����� ��� ���ϳ뺧 �����͸� ���������� �����մϴ�.
    /// </summary>
    public static void DeleteAllSaveData()
    {
        try
        {
            // 1. ���ϳ뺧 �������� ��Ȯ�� ���� ���� �̸��� �����ɴϴ�.
            var stateConfig = Engine.GetConfiguration<StateConfiguration>();

            // 2. ���� Ȯ���� �ùٸ� �̸� 'SaveFolderName'�� ����մϴ�.
            var saveFolderName = stateConfig.SaveFolderName;

            // 3. ��ü ���� ���� ��θ� ����ϴ�.
            var saveFolderPath = Path.Combine(Application.persistentDataPath, saveFolderName);

            if (Directory.Exists(saveFolderPath))
            {
                // 4. ���� ��ü�� ���� �����մϴ�.
                Directory.Delete(saveFolderPath, true);
                Debug.Log($"���� ����({saveFolderPath})�� ���������� �����߽��ϴ�.");
            }
            else
            {
                Debug.Log("���� ������ �������� �ʾ� ������ ������ �����ϴ�.");
            }

            Debug.LogWarning("��� ���� ������ ��ũ���� �����Ǿ����ϴ�. ������ ������ϸ� ������ �ʱ�ȭ�� ���·� ���۵˴ϴ�.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"������ ���� �� ���� �߻�: {e.Message}");
        }
    }
}