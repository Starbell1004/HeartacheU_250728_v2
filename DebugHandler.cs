using UnityEngine;
using Naninovel;
using System.IO; // 파일/폴더 관리를 위해 필수

public class DebugKeyHandler : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Debug.Log("[디버그] F8 - 모든 저장 데이터 파일 삭제 실행");
            DeleteAllSaveData();
        }
    }

    /// <summary>
    /// 디스크에 저장된 모든 나니노벨 데이터를 물리적으로 삭제합니다.
    /// </summary>
    public static void DeleteAllSaveData()
    {
        try
        {
            // 1. 나니노벨 설정에서 정확한 저장 폴더 이름을 가져옵니다.
            var stateConfig = Engine.GetConfiguration<StateConfiguration>();

            // 2. 직접 확인한 올바른 이름 'SaveFolderName'을 사용합니다.
            var saveFolderName = stateConfig.SaveFolderName;

            // 3. 전체 저장 폴더 경로를 만듭니다.
            var saveFolderPath = Path.Combine(Application.persistentDataPath, saveFolderName);

            if (Directory.Exists(saveFolderPath))
            {
                // 4. 폴더 자체를 전부 삭제합니다.
                Directory.Delete(saveFolderPath, true);
                Debug.Log($"저장 폴더({saveFolderPath})를 성공적으로 삭제했습니다.");
            }
            else
            {
                Debug.Log("저장 폴더가 존재하지 않아 삭제할 파일이 없습니다.");
            }

            Debug.LogWarning("모든 저장 파일이 디스크에서 삭제되었습니다. 게임을 재시작하면 완전히 초기화된 상태로 시작됩니다.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"데이터 삭제 중 오류 발생: {e.Message}");
        }
    }
}