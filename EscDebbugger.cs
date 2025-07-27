using UnityEngine;
using Naninovel;
using Naninovel.UI;

public class ESCDebugger : MonoBehaviour
{
    void Update()
    {
        // 1. Unity Input System���� ESC Ű ����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"[ESCDebugger] ESC Ű ������! Time: {Time.time}, TimeScale: {Time.timeScale}");

            // 2. Naninovel InputManager ���� Ȯ��
            var inputManager = Engine.GetService<IInputManager>();
            if (inputManager != null)
            {
                Debug.Log($"[ESCDebugger] InputManager ������");

                // 3. InputManager�� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
                Debug.Log($"[ESCDebugger] InputManager Ÿ��: {inputManager.GetType().Name}");
            }
            else
            {
                Debug.LogError("[ESCDebugger] InputManager�� ã�� �� ����!");
            }

            // 4. ���� Ȱ��ȭ�� UI Ȯ��
            var uiManager = Engine.GetService<IUIManager>();
            if (uiManager != null)
            {
                var managedUIs = new System.Collections.Generic.List<IManagedUI>();
                uiManager.GetManagedUIs(managedUIs);

                Debug.Log($"[ESCDebugger] �� �����Ǵ� UI ��: {managedUIs.Count}");
                foreach (var ui in managedUIs)
                {
                    if (ui.Visible)
                    {
                        Debug.Log($"[ESCDebugger] Ȱ�� UI: {ui.GetType().Name}");

                        // PauseUI���� Ȯ��
                        if (ui is PauseUI)
                        {
                            Debug.Log($"[ESCDebugger] PauseUI �߰�! Visible: {ui.Visible}");
                        }
                    }
                }

                // 5. CustomPauseUI ���� ã��
                var pauseUI = uiManager.GetUI<PauseUI>();
                if (pauseUI != null)
                {
                    Debug.Log($"[ESCDebugger] PauseUI ���� - Visible: {pauseUI.Visible}, Type: {pauseUI.GetType().Name}");
                }
                else
                {
                    Debug.Log("[ESCDebugger] PauseUI�� ã�� �� ����");
                }
            }

            // 6. �ٸ� ��ũ��Ʈ�� ESC�� ����ä���� Ȯ��
            Debug.Log($"[ESCDebugger] ���� ��Ŀ���� GameObject: {UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.name ?? "����"}");
        }
    }
}