using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

[RequireComponent(typeof(LayoutElement), typeof(CanvasGroup))]
public class ItemIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private float defaultIconSize = 50f;

    public string ItemId { get; private set; }
    private ItemData itemData;

    private RectTransform _rectTransform;
    private LayoutElement _layoutElement;
    private CanvasGroup _canvasGroup;
    private float _animationDuration;
    private float _originalWidth;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _layoutElement = GetComponent<LayoutElement>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (iconImage == null)
        {
            iconImage = GetComponentInChildren<Image>();
        }

        var mainImage = GetComponent<Image>();
        if (mainImage != null && mainImage != iconImage)
        {
            Debug.Log($"[ItemIconUI] ���� GameObject�� Image ������Ʈ ����: {gameObject.name}");
            DestroyImmediate(mainImage);
        }

        if (iconImage == null)
        {
            Debug.LogWarning($"[ItemIconUI] Image ������Ʈ�� ã�� �� ��� ���� �����մϴ�: {gameObject.name}");
            CreateImageComponent();
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        // �����: ������Ʈ ���� Ȯ��
        Debug.Log($"[ItemIconUI] Awake �Ϸ� - {gameObject.name}");
        Debug.Log($"  - CanvasGroup: {(_canvasGroup != null ? "OK" : "NULL")}");
        Debug.Log($"  - Image: {(iconImage != null ? "OK" : "NULL")}");
        Debug.Log($"  - RectTransform: {(_rectTransform != null ? "OK" : "NULL")}");
    }

    private void CreateImageComponent()
    {
        GameObject imageGO = new GameObject("Icon");
        imageGO.transform.SetParent(transform, false);
        iconImage = imageGO.AddComponent<Image>();

        var imageRect = iconImage.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.anchoredPosition = Vector2.zero;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        Debug.Log($"[ItemIconUI] Image ������Ʈ ���� �Ϸ�: {gameObject.name}");
    }

    public void Setup(ItemData data, float animDuration)
    {
        if (data == null)
        {
            Debug.LogError("[ItemIconUI] ItemData�� null�Դϴ�!");
            return;
        }

        ItemId = data.itemId;
        itemData = data;
        _animationDuration = animDuration;

        Debug.Log($"[ItemIconUI] Setup ���� - ItemId: {ItemId}");

        if (iconImage != null)
        {
            iconImage.sprite = data.iconSprite;
            iconImage.enabled = true;
            iconImage.color = Color.white;

            Debug.Log($"[ItemIconUI] ��������Ʈ ���� �Ϸ� - Sprite: {(data.iconSprite != null ? data.iconSprite.name : "null")}");
        }
        else
        {
            Debug.LogError("[ItemIconUI] iconImage�� null�Դϴ�!");
        }

        SetupSize(data);

        // �����: ���� ���� ���� Ȯ��
        Debug.Log($"[ItemIconUI] Setup �Ϸ� - ItemId: {ItemId}");
        Debug.Log($"  - ItemData: {(itemData != null ? "OK" : "NULL")}");
        Debug.Log($"  - ItemName: {(itemData != null ? itemData.itemName : "NULL")}");
        Debug.Log($"  - Description: {(itemData != null ? itemData.description : "NULL")}");
        Debug.Log($"  - ItemTooltip.Instance: {(ItemTooltip.Instance != null ? "OK" : "NULL")}");
    }

    private void SetupSize(ItemData data)
    {

        Debug.Log($"=== [ItemIconUI] SetupSize ����� ===");
        Debug.Log($"defaultIconSize �ʵ尪: {defaultIconSize}");
        Debug.Log($"���ӿ�����Ʈ �̸�: {gameObject.name}");
        Debug.Log($"��������Ʈ �̸�: {data.iconSprite?.name}");

        float targetSize = defaultIconSize;
        Debug.Log($"targetSize ������: {targetSize}");

       
        // LayoutElement ������ ��Ȱ��ȭ
        if (_layoutElement != null)
        {
            _layoutElement.enabled = false;
        }

        if (data.iconSprite != null)
        {
            Rect spriteRect = data.iconSprite.rect;
            float aspectRatio = spriteRect.width / spriteRect.height;
            float finalWidth = targetSize * aspectRatio;
            float finalHeight = targetSize;

            // RectTransform���θ� ũ�� ����
            if (_rectTransform != null)
            {
                _rectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);
            }

            _originalWidth = finalWidth;
            Debug.Log($"[ItemIconUI] �� ũ�� ���� - {finalWidth}x{finalHeight} (targetSize: {targetSize})");
        }
        else
        {
            if (_rectTransform != null)
            {
                _rectTransform.sizeDelta = new Vector2(targetSize, targetSize);
            }

            _originalWidth = targetSize;
        }
    }

    public void PlayAppearAnimation()
    {
        Debug.Log($"[ItemIconUI] ���� �ִϸ��̼� ���� - {ItemId}");

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one * 0.8f;
        }

        StartCoroutine(AnimateAppear());
    }

    private IEnumerator AnimateAppear()
    {
        float timer = 0f;
        Vector3 startScale = Vector3.one * 0.8f;
        Vector3 endScale = Vector3.one;

        while (timer < _animationDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / _animationDuration);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = progress;
            }

            if (_rectTransform != null)
            {
                _rectTransform.localScale = Vector3.Lerp(startScale, endScale, progress);
            }

            yield return null;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }

        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one;
        }

        Debug.Log($"[ItemIconUI] ���� �ִϸ��̼� �Ϸ� - {ItemId}");
    }

    public void PlayDisappearAnimation(Action onComplete)
    {
        Debug.Log($"[ItemIconUI] ����� �ִϸ��̼� ���� - {ItemId}");
        StartCoroutine(AnimateDisappear(onComplete));
    }

    private IEnumerator AnimateDisappear(Action onComplete)
    {
        float startWidth = _layoutElement != null && _layoutElement.preferredWidth > 0
            ? _layoutElement.preferredWidth
            : _originalWidth;

        float timer = 0f;

        while (timer < _animationDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / _animationDuration);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            }

            if (_layoutElement != null)
            {
                _layoutElement.preferredWidth = Mathf.Lerp(startWidth, 0f, progress);
            }

            yield return null;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        if (_layoutElement != null)
        {
            _layoutElement.preferredWidth = 0f;
        }

        Debug.Log($"[ItemIconUI] ����� �ִϸ��̼� �Ϸ� - {ItemId}");

        onComplete?.Invoke();
        Destroy(gameObject);
    }

    // ���콺 ȣ�� �̺�Ʈ ó�� - ����� �߰�
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[ItemIconUI] ���콺 ���� - {ItemId}");
        Debug.Log($"  - ItemData: {(itemData != null ? "OK" : "NULL")}");
        Debug.Log($"  - ItemTooltip.Instance: {(ItemTooltip.Instance != null ? "OK" : "NULL")}");
        Debug.Log($"  - Transform Position: {transform.position}");
        Debug.Log($"  - Mouse Position: {Input.mousePosition}");
        Debug.Log(" ���콺 �̺�Ʈ ������! "); // �� �� �߰�

        if (itemData != null)
        {
            Debug.Log($"  - itemName: {itemData.itemName}");
            Debug.Log($"  - description: {itemData.description}");
        }

        if (itemData == null)
        {
            Debug.LogError("[ItemIconUI] itemData�� null�Դϴ�! ������ ǥ���� �� �����ϴ�.");
            return;
        }

        if (ItemTooltip.Instance == null)
        {
            Debug.LogError("[ItemIconUI] ItemTooltip.Instance�� null�Դϴ�! ������ ǥ���� �� �����ϴ�.");
            return;
        }

        Debug.Log($"[ItemIconUI] ���� ǥ�� �õ� - ������: {itemData.itemName}, ����: {itemData.description}");

        ItemTooltip.Instance.ShowTooltip(
            itemData.itemName,
            itemData.description,
            transform.position
        );

        Debug.Log("[ItemIconUI] ���� ǥ�� ��� �Ϸ�");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[ItemIconUI] ���콺 ��� - {ItemId}");

        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
            Debug.Log("[ItemIconUI] ���� ���� ��� �Ϸ�");
        }
        else
        {
            Debug.LogWarning("[ItemIconUI] ItemTooltip.Instance�� null�̾ ������ ���� �� �����ϴ�.");
        }
    }

    [ContextMenu("Debug Icon State")]
    public void DebugIconState()
    {
        Debug.Log($"[ItemIconUI] {ItemId} ����:");
        Debug.Log($"  - GameObject Ȱ��ȭ: {gameObject.activeInHierarchy}");
        Debug.Log($"  - CanvasGroup ����: {(_canvasGroup?.alpha ?? 1f)}");
        Debug.Log($"  - Image Ȱ��ȭ: {(iconImage?.enabled ?? false)}");
        Debug.Log($"  - Image ��������Ʈ: {(iconImage?.sprite?.name ?? "null")}");
        Debug.Log($"  - ���� ������: {transform.localScale}");
        Debug.Log($"  - ��ġ: {transform.position}");
        Debug.Log($"  - RectTransform ũ��: {(_rectTransform?.sizeDelta ?? Vector2.zero)}");
        Debug.Log($"  - LayoutElement ��ȣ ũ��: {(_layoutElement?.preferredWidth ?? 0)}, {(_layoutElement?.preferredHeight ?? 0)}");
        Debug.Log($"  - ItemData: {(itemData != null ? $"ID:{itemData.itemId}, Name:{itemData.itemName}" : "NULL")}");
        Debug.Log($"  - ItemTooltip.Instance ����: {(ItemTooltip.Instance != null ? "������" : "NULL")}");
    }

    [ContextMenu("Test Tooltip")]
    public void TestTooltip()
    {
        Debug.Log("[ItemIconUI] ���� ���� �׽�Ʈ ����");
        OnPointerEnter(null);
    }
}