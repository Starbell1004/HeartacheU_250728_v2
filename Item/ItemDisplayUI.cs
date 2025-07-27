using Naninovel.UI;
using Naninovel;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class ItemDisplayUI : CustomUI
{
    [Header("���� �ý���")]
    [SerializeField] private GameObject itemSlotPrefab;     // ���� �߰�: ���� ������
    [SerializeField] private GameObject itemIconPrefab;     // ����: ������ ������
    [SerializeField] private RectTransform iconContainer;
    [SerializeField] private int maxSlots = 6;              // �ִ� ���� ����
    [SerializeField] private float animationDuration = 0.3f;

    private List<ItemSlotUI> activeSlots = new();           // ���� ����
    private HorizontalLayoutGroup _horizontalLayoutGroup;
    private CanvasGroup _canvasGroup;

    // ������ ���� ���� ����
    private bool _isHiddenByPrinter = false;
    private bool _wasVisibleBeforePrinterHide = false;
    private ITextPrinterManager _textPrinterManager;
    private Coroutine _printerMonitorCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // ������Ʈ �ʱ�ȭ
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (iconContainer != null)
        {
            _horizontalLayoutGroup = iconContainer.GetComponent<HorizontalLayoutGroup>();
        }

        // �ʱ� ���� ����
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(WaitForInitialization());
    }

    private IEnumerator WaitForInitialization()
    {
        while (!Engine.Initialized)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        InitializeServices();
    }

    private void InitializeServices()
    {
        Debug.Log("[ItemDisplayUI] ���� �ʱ�ȭ ����");

        try
        {
            // ������ ������ ����
            _textPrinterManager = Engine.GetService<ITextPrinterManager>();
            if (_textPrinterManager != null)
            {
                Debug.Log("[ItemDisplayUI] TextPrinterManager ���� ����");
                _printerMonitorCoroutine = StartCoroutine(MonitorPrinterVisibility());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemDisplayUI] ���� �ʱ�ȭ ����: {ex.Message}");
        }
    }

    private IEnumerator MonitorPrinterVisibility()
    {
        bool lastPrinterVisibility = true;

        while (true)
        {
            float waitTime = 0.02f;

            try
            {
                if (_textPrinterManager != null)
                {
                    bool currentPrinterVisibility = false;

                    var uiManager = Engine.GetService<IUIManager>();
                    if (uiManager != null)
                    {
                        var printerUI = uiManager.GetUI("Dialogue");
                        if (printerUI != null)
                        {
                            currentPrinterVisibility = printerUI.Visible;
                        }
                    }

                    // ���� ��ȭ ����
                    if (lastPrinterVisibility != currentPrinterVisibility)
                    {
                        OnPrinterVisibilityChanged(currentPrinterVisibility);
                        lastPrinterVisibility = currentPrinterVisibility;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ItemDisplayUI] ������ ����͸� ����: {ex.Message}");
                waitTime = 1f;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnPrinterVisibilityChanged(bool isVisible)
    {
        if (!isVisible) // ������ ����
        {
            if (activeSlots.Count > 0 && gameObject.activeInHierarchy && !_isHiddenByPrinter)
            {
                _wasVisibleBeforePrinterHide = true;
                _isHiddenByPrinter = true;
                HideUI();
            }
        }
        else // ������ ǥ��
        {
            if (_isHiddenByPrinter && _wasVisibleBeforePrinterHide)
            {
                _isHiddenByPrinter = false;
                _wasVisibleBeforePrinterHide = false;

                if (activeSlots.Any(slot => !slot.IsEmpty))
                {
                    ShowUIWithIcons();
                }
            }
        }
    }

    public void AddItemIcon(ItemData itemData)
    {
        if (itemData == null || itemSlotPrefab == null || itemIconPrefab == null || iconContainer == null)
        {
            Debug.LogError($"[ItemDisplayUI] AddItemIcon ���� - �ʼ� ������Ʈ�� �����ϴ�");
            return;
        }

        // �ߺ� üũ
        if (activeSlots.Any(slot => !slot.IsEmpty && slot.GetItemId() == itemData.itemId))
        {
            Debug.LogWarning($"ItemDisplayUI: �̹� {itemData.itemId} �������� �����մϴ�.");
            return;
        }

        Debug.Log($"[ItemDisplayUI] ������ ������ ���� ����: {itemData.itemId}");

        // 1. �� ���� ã�� �Ǵ� �� ���� ����
        ItemSlotUI targetSlot = GetAvailableSlot();

        if (targetSlot == null)
        {
            // �� ���� ����
            targetSlot = CreateNewSlot();
        }

        if (targetSlot == null)
        {
            Debug.LogError("[ItemDisplayUI] ���� ���� ����");
            return;
        }

        // 2. ���Կ� ������ �߰�
        targetSlot.SetItemIcon(itemData, itemIconPrefab);

        // 3. UI ǥ��
        if (!_isHiddenByPrinter)
        {
            ShowUIWithIcons();
        }

        // 4. ���̾ƿ� ������Ʈ
        StartCoroutine(ForceLayoutUpdate());
    }

    private ItemSlotUI GetAvailableSlot()
    {
        return activeSlots.FirstOrDefault(slot => slot.IsEmpty);
    }

    private ItemSlotUI CreateNewSlot()
    {
        if (activeSlots.Count >= maxSlots)
        {
            Debug.LogWarning("[ItemDisplayUI] �ִ� ���� ���� �ʰ�");
            return null;
        }

        GameObject slotGO = Instantiate(itemSlotPrefab, iconContainer);
        slotGO.name = $"ItemSlot_{activeSlots.Count}";

        ItemSlotUI slotUI = slotGO.GetComponent<ItemSlotUI>();

        if (slotUI != null)
        {
            activeSlots.Add(slotUI);
            slotUI.SetSlotIndex(activeSlots.Count - 1); // FIFO ����
            Debug.Log($"[ItemDisplayUI] �� ���� ����: {activeSlots.Count - 1}");
        }
        else
        {
            Debug.LogError("[ItemDisplayUI] ItemSlotUI ������Ʈ�� ã�� �� �����ϴ�!");
            Destroy(slotGO);
        }

        return slotUI;
    }

    public void RemoveItemIcon(string itemId)
    {
        // �ش� �������� ���� ���� ã��
        var targetSlot = activeSlots.FirstOrDefault(slot =>
            !slot.IsEmpty && slot.GetItemId() == itemId);

        if (targetSlot != null)
        {
            Debug.Log($"[ItemDisplayUI] ������ ���� ����: {itemId}");

            targetSlot.RemoveItemIcon(() => {
                // ������ ����, �����ܸ� ����
                Debug.Log($"[ItemDisplayUI] ������ ���� �Ϸ�: {itemId}");
                CheckIfAllSlotsEmpty();
            });
        }
        else
        {
            Debug.LogWarning($"[ItemDisplayUI] ������ �������� ã�� ���߽��ϴ�: {itemId}");
        }
    }

    private void CheckIfAllSlotsEmpty()
    {
        bool allEmpty = activeSlots.All(slot => slot.IsEmpty);
        if (allEmpty)
        {
            HideUI();
        }
    }

    public void ClearAllIcons()
    {
        Debug.Log($"[ItemDisplayUI] ��� ���� ���� ���� - ���� {activeSlots.Count}��");

        // ��� ���԰� ������ ����
        foreach (var slot in activeSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }

        activeSlots.Clear();
        HideUI();
        Debug.Log("[ItemDisplayUI] ��� ���� ���� �Ϸ�");
    }

    private void ShowUIWithIcons()
    {
        bool hasItems = activeSlots.Any(slot => !slot.IsEmpty);

        if (hasItems && !_isHiddenByPrinter)
        {
            Show();

            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            StartCoroutine(ForceLayoutUpdate());
        }
    }

    private IEnumerator ForceLayoutUpdate()
    {
        yield return null;

        if (iconContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(iconContainer);
        }

        if (_horizontalLayoutGroup != null)
        {
            // HorizontalLayoutGroup ���� ���� ����
            _horizontalLayoutGroup.spacing = 10f; // ���� ����
            _horizontalLayoutGroup.childAlignment = TextAnchor.MiddleRight; // �����ʺ��� ä����
            _horizontalLayoutGroup.reverseArrangement = true; // ���� ������ (FIFO �����ϸ鼭 ������ ����)
            _horizontalLayoutGroup.childControlWidth = false;  // ���� �ʺ� ���� ����
            _horizontalLayoutGroup.childControlHeight = false; // ���� ���� ���� ����
            _horizontalLayoutGroup.childForceExpandWidth = false;
            _horizontalLayoutGroup.childForceExpandHeight = false;

            _horizontalLayoutGroup.enabled = false;
            _horizontalLayoutGroup.enabled = true;

            Debug.Log("[ItemDisplayUI] HorizontalLayoutGroup ���� - �����ʺ��� FIFO ������ ä��");
        }
    }

    private void HideUI()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
        Hide();
    }

    public string GetOldestItemId()
    {
        var oldestSlot = activeSlots.FirstOrDefault(slot => !slot.IsEmpty);
        return oldestSlot?.GetItemId();
    }

    public bool RemoveOldestItemIcon()
    {
        string oldestItemId = GetOldestItemId();
        if (!string.IsNullOrEmpty(oldestItemId))
        {
            RemoveItemIcon(oldestItemId);
            return true;
        }
        return false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_printerMonitorCoroutine != null)
        {
            StopCoroutine(_printerMonitorCoroutine);
        }
    }
}