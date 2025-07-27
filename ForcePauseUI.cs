using UnityEngine;
using Naninovel;
using Naninovel.UI;

public class ForcePauseUI : MonoBehaviour
{
    private IUIManager uiManager;
    private IPauseUI pauseUI;
    private bool initialized = false;

    void Start()
    {
        StartCoroutine(DelayedInit());
    }

    System.Collections.IEnumerator DelayedInit()
    {
        // ������ ������ �ʱ�ȭ�� ������ ���
        yield return new WaitForSeconds(1f);

        uiManager = Engine.GetService<IUIManager>();
        if (uiManager == null)
        {
            Debug.LogError("[ForcePauseUI] UIManager�� ã�� �� ����!");
            yield break;
        }

        pauseUI = uiManager.GetUI<IPauseUI>();
        if (pauseUI == null)
        {
            Debug.LogError("[ForcePauseUI] PauseUI�� ã�� �� ����!");
        }
        else
        {
            initialized = true;
        }
    }

    void Update()
    {
        if (!initialized || pauseUI == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseUI.Visible)
                pauseUI.Hide();
            else
                pauseUI.Show();
        }
    }
}