using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HoverCursorManager : MonoBehaviour
{
    [Header("커서 설정")]
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
        // Read/Write 활성화 필요
        if (cursorTexture.width > 24 || cursorTexture.height > 24)
        {
            Debug.LogError($"커서 크기가 너무 큼: {cursorTexture.width}x{cursorTexture.height}");

            // 강제 리사이즈
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
        // UI 위에 마우스가 있는지 체크
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
        // EventSystem을 통해 UI 호버 체크
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 버튼이나 클릭 가능한 UI 요소 위에 있는지 확인
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