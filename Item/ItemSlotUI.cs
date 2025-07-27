using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemSlotUI : MonoBehaviour
{
    [Header("슬롯 구성")]
    [SerializeField] private Image slotBackground;      // 분홍색 슬롯 배경

    private ItemIconUI currentIcon;
    private int slotIndex;

    public bool IsEmpty => currentIcon == null;

    void Awake()
    {
        // 슬롯 배경 설정 (분홍색)
        if (slotBackground == null)
        {
            slotBackground = GetComponent<Image>();
        }

        // Layout Element가 있으면 제거 (HorizontalLayoutGroup과 충돌 방지)
        var layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            Debug.LogWarning("[ItemSlotUI] Layout Element 제거 - HorizontalLayoutGroup과 충돌 방지");
            DestroyImmediate(layoutElement);
        }

        // RectTransform 크기 직접 설정
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(93f, 93f); // 스프라이트 규격에 맞춤
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
            Debug.LogWarning($"[ItemSlotUI] 슬롯 {slotIndex}이 이미 사용 중입니다!");
            return;
        }

        // 아이콘을 직접 자식으로 생성
        GameObject iconGO = Instantiate(iconPrefab, transform);
        currentIcon = iconGO.GetComponent<ItemIconUI>();

        if (currentIcon != null)
        {
            // 아이콘을 슬롯 크기에 맞게 조정
            currentIcon.Setup(itemData, 0.3f);
            FitIconToSlot();

            Debug.Log($"[ItemSlotUI] 슬롯 {slotIndex}에 {itemData.itemId} 아이콘 추가");
        }
    }

    private void FitIconToSlot()
    {
        if (currentIcon == null) return;

        var iconRect = currentIcon.GetComponent<RectTransform>();
        var iconImage = currentIcon.GetComponent<Image>();

        if (iconRect != null && iconImage != null && iconImage.sprite != null)
        {
            // 슬롯 크기 (93x93)
            float slotSize = 93f;
            float padding = 5f; // 패딩
            float availableSize = slotSize - (padding * 2);

            // 스프라이트 원본 비율 계산
            Sprite sprite = iconImage.sprite;
            float spriteWidth = sprite.rect.width;
            float spriteHeight = sprite.rect.height;
            float aspectRatio = spriteWidth / spriteHeight;

            // 비율을 유지하면서 슬롯에 맞는 크기 계산
            float finalWidth, finalHeight;

            if (aspectRatio > 1f) // 가로가 더 긴 경우
            {
                finalWidth = availableSize;
                finalHeight = availableSize / aspectRatio;
            }
            else // 세로가 더 길거나 정사각형인 경우
            {
                finalHeight = availableSize;
                finalWidth = availableSize * aspectRatio;
            }

            // RectTransform 설정 - 중앙 정렬
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(finalWidth, finalHeight);
            iconRect.localScale = Vector3.one;

            Debug.Log($"[ItemSlotUI] 아이콘 크기 조정: {finalWidth}x{finalHeight} (비율: {aspectRatio:F2})");
        }
    }

    public void RemoveItemIcon(Action onComplete)
    {
        if (IsEmpty)
        {
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"[ItemSlotUI] 슬롯 {slotIndex}에서 아이콘 제거");

        // 아이콘 애니메이션과 함께 제거
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