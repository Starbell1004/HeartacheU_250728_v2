using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemSlotUI : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] private Image slotBackground;      // ��ȫ�� ���� ���

    private ItemIconUI currentIcon;
    private int slotIndex;

    public bool IsEmpty => currentIcon == null;

    void Awake()
    {
        // ���� ��� ���� (��ȫ��)
        if (slotBackground == null)
        {
            slotBackground = GetComponent<Image>();
        }

        // Layout Element�� ������ ���� (HorizontalLayoutGroup�� �浹 ����)
        var layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            Debug.LogWarning("[ItemSlotUI] Layout Element ���� - HorizontalLayoutGroup�� �浹 ����");
            DestroyImmediate(layoutElement);
        }

        // RectTransform ũ�� ���� ����
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(93f, 93f); // ��������Ʈ �԰ݿ� ����
        }
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
        gameObject.name = $"ItemSlot_{index}";
    }

    public void SetItemIcon(ItemData itemData, GameObject iconPrefab)
    {
        if (!IsEmpty)
        {
            Debug.LogWarning($"[ItemSlotUI] ���� {slotIndex}�� �̹� ��� ���Դϴ�!");
            return;
        }

        // �������� ���� �ڽ����� ����
        GameObject iconGO = Instantiate(iconPrefab, transform);
        currentIcon = iconGO.GetComponent<ItemIconUI>();

        if (currentIcon != null)
        {
            // �������� ���� ũ�⿡ �°� ����
            currentIcon.Setup(itemData, 0.3f);
            FitIconToSlot();

            Debug.Log($"[ItemSlotUI] ���� {slotIndex}�� {itemData.itemId} ������ �߰�");
        }
    }

    private void FitIconToSlot()
    {
        if (currentIcon == null) return;

        var iconRect = currentIcon.GetComponent<RectTransform>();
        var iconImage = currentIcon.GetComponent<Image>();

        if (iconRect != null && iconImage != null && iconImage.sprite != null)
        {
            // ���� ũ�� (93x93)
            float slotSize = 93f;
            float padding = 5f; // �е�
            float availableSize = slotSize - (padding * 2);

            // ��������Ʈ ���� ���� ���
            Sprite sprite = iconImage.sprite;
            float spriteWidth = sprite.rect.width;
            float spriteHeight = sprite.rect.height;
            float aspectRatio = spriteWidth / spriteHeight;

            // ������ �����ϸ鼭 ���Կ� �´� ũ�� ���
            float finalWidth, finalHeight;

            if (aspectRatio > 1f) // ���ΰ� �� �� ���
            {
                finalWidth = availableSize;
                finalHeight = availableSize / aspectRatio;
            }
            else // ���ΰ� �� ��ų� ���簢���� ���
            {
                finalHeight = availableSize;
                finalWidth = availableSize * aspectRatio;
            }

            // RectTransform ���� - �߾� ����
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(finalWidth, finalHeight);
            iconRect.localScale = Vector3.one;

            Debug.Log($"[ItemSlotUI] ������ ũ�� ����: {finalWidth}x{finalHeight} (����: {aspectRatio:F2})");
        }
    }

    public void RemoveItemIcon(Action onComplete)
    {
        if (IsEmpty)
        {
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"[ItemSlotUI] ���� {slotIndex}���� ������ ����");

        // ������ �ִϸ��̼ǰ� �Բ� ����
        currentIcon.PlayDisappearAnimation(() => {
            currentIcon = null;
            onComplete?.Invoke();
        });
    }

    public string GetItemId()
    {
        return IsEmpty ? null : currentIcon.ItemId;
    }
}