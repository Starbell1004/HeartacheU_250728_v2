using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HoverCursorManager : MonoBehaviour
{
    [Header("Ŀ�� ����")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Vector2 defaultHotSpot = Vector2.zero;
    [SerializeField] private Vector2 hoverHotSpot = Vector2.zero;
    [SerializeField] private Texture2D cursorTexture;
    private static HoverCursorManager instance;
    private bool isHoveringUI = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Read/Write Ȱ��ȭ �ʿ�
        if (cursorTexture.width > 24 || cursorTexture.height > 24)
        {
            Debug.LogError($"Ŀ�� ũ�Ⱑ �ʹ� ŭ: {cursorTexture.width}x{cursorTexture.height}");

            // ���� ��������
            Texture2D resized = ResizeTexture(cursorTexture, 24, 24);
            Cursor.SetCursor(resized, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
    }

    Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D newTexture = new Texture2D(width, height);
        newTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        newTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return newTexture;
    }

    void Update()
    {
        // UI ���� ���콺�� �ִ��� üũ
        bool overUI = IsPointerOverUIElement();

        if (overUI && !isHoveringUI)
        {
            isHoveringUI = true;
            SetHoverCursor();
        }
        else if (!overUI && isHoveringUI)
        {
            isHoveringUI = false;
            SetDefaultCursor();
        }
    }

    private bool IsPointerOverUIElement()
    {
        // EventSystem�� ���� UI ȣ�� üũ
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // ��ư�̳� Ŭ�� ������ UI ��� ���� �ִ��� Ȯ��
        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null ||
                result.gameObject.GetComponent<UnityEngine.UI.Toggle>() != null ||
                result.gameObject.GetComponent<UnityEngine.UI.Slider>() != null)
            {
                return true;
            }
        }
        return false;
    }

    public void SetDefaultCursor()
    {
        if (defaultCursor != null)
            Cursor.SetCursor(defaultCursor, defaultHotSpot, CursorMode.Auto);
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void SetHoverCursor()
    {
        if (hoverCursor != null)
            Cursor.SetCursor(hoverCursor, hoverHotSpot, CursorMode.Auto);
    }
}