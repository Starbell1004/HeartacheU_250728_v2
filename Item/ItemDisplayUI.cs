using Naninovel.UI;
using Naninovel;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class ItemDisplayUI : CustomUI
{
    [Header("슬롯 시스템")]
    [SerializeField] private GameObject itemSlotPrefab;     // 새로 추가: 슬롯 프리팹
    [SerializeField] private GameObject itemIconPrefab;     // 기존: 아이콘 프리팹
    [SerializeField] private RectTransform iconContainer;
    [SerializeField] private int maxSlots = 6;              // 최대 슬롯 개수
    [SerializeField] private float animationDuration = 0.3f;

    private List<ItemSlotUI> activeSlots = new();           // 슬롯 관리
    private HorizontalLayoutGroup _horizontalLayoutGroup;
    private CanvasGroup _canvasGroup;

    // 프린터 연동 관련 변수
    private bool _isHiddenByPrinter = false;
    private bool _wasVisibleBeforePrinterHide = false;
    private ITextPrinterManager _textPrinterManager;
    private Coroutine _printerMonitorCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // 컴포넌트 초기화
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (iconContainer != null)
        {
            _horizontalLayoutGroup = iconContainer.GetComponent<HorizontalLayoutGroup>();
        }

        // 초기 상태 설정
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
        Debug.Log("[ItemDisplayUI] 서비스 초기화 시작");

        try
        {
            // 프린터 관리자 연결
            _textPrinterManager = Engine.GetService<ITextPrinterManager>();
            if (_textPrinterManager != null)
            {
                Debug.Log("[ItemDisplayUI] TextPrinterManager 연결 성공");
                _printerMonitorCoroutine = StartCoroutine(MonitorPrinterVisibility());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemDisplayUI] 서비스 초기화 오류: {ex.Message}");
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

                    // 상태 변화 감지
                    if (lastPrinterVisibility != currentPrinterVisibility)
                    {
                        OnPrinterVisibilityChanged(currentPrinterVisibility);
                        lastPrinterVisibility = currentPrinterVisibility;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ItemDisplayUI] 프린터 모니터링 오류: {ex.Message}");
                waitTime = 1f;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnPrinterVisibilityChanged(bool isVisible)
    {
        if (!isVisible) // 프린터 숨김
        {
            if (activeSlots.Count > 0 && gameObject.activeInHierarchy && !_isHiddenByPrinter)
            {
                _wasVisibleBeforePrinterHide = true;
                _isHiddenByPrinter = true;
                HideUI();
            }
        }
        else // 프린터 표시
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
            Debug.LogError($"[ItemDisplayUI] AddItemIcon 실패 - 필수 컴포넌트가 없습니다");
            return;
        }

        // 중복 체크
        if (activeSlots.Any(slot => !slot.IsEmpty && slot.GetItemId() == itemData.itemId))
        {
            Debug.LogWarning($"ItemDisplayUI: 이미 {itemData.itemId} 아이콘이 존재합니다.");
            return;
        }

        Debug.Log($"[ItemDisplayUI] 아이템 아이콘 생성 시작: {itemData.itemId}");

        // 1. 빈 슬롯 찾기 또는 새 슬롯 생성
        ItemSlotUI targetSlot = GetAvailableSlot();

        if (targetSlot == null)
        {
            // 새 슬롯 생성
            targetSlot = CreateNewSlot();
        }

        if (targetSlot == null)
        {
            Debug.LogError("[ItemDisplayUI] 슬롯 생성 실패");
            return;
        }

        // 2. 슬롯에 아이콘 추가
        targetSlot.SetItemIcon(itemData, itemIconPrefab);

        // 3. UI 표시
        if (!_isHiddenByPrinter)
        {
            ShowUIWithIcons();
        }

        // 4. 레이아웃 업데이트
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
            Debug.LogWarning("[ItemDisplayUI] 최대 슬롯 개수 초과");
            return null;
        }

        GameObject slotGO = Instantiate(itemSlotPrefab, iconContainer);
        slotGO.name = $"ItemSlot_{activeSlots.Count}";

        ItemSlotUI slotUI = slotGO.GetComponent<ItemSlotUI>();

        if (slotUI != null)
        {
            activeSlots.Add(slotUI);
            slotUI.SetSlotIndex(activeSlots.Count - 1); // FIFO 순서
            Debug.Log($"[ItemDisplayUI] 새 슬롯 생성: {activeSlots.Count - 1}");
        }
        else
        {
            Debug.LogError("[ItemDisplayUI] ItemSlotUI 컴포넌트를 찾을 수 없습니다!");
            Destroy(slotGO);
        }

        return slotUI;
    }

    public void RemoveItemIcon(string itemId)
    {
        // 해당 아이템을 가진 슬롯 찾기
        var targetSlot = activeSlots.FirstOrDefault(slot =>
            !slot.IsEmpty && slot.GetItemId() == itemId);

        if (targetSlot != null)
        {
            Debug.Log($"[ItemDisplayUI] 아이콘 제거 시작: {itemId}");

            targetSlot.RemoveItemIcon(() => {
                // 슬롯은 유지, 아이콘만 제거
                Debug.Log($"[ItemDisplayUI] 아이콘 제거 완료: {itemId}");
                CheckIfAllSlotsEmpty();
            });
        }
        else
        {
            Debug.LogWarning($"[ItemDisplayUI] 제거할 아이템을 찾지 못했습니다: {itemId}");
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
        Debug.Log($"[ItemDisplayUI] 모든 슬롯 제거 시작 - 현재 {activeSlots.Count}개");

        // 모든 슬롯과 아이콘 제거
        foreach (var slot in activeSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }

        activeSlots.Clear();
        HideUI();
        Debug.Log("[ItemDisplayUI] 모든 슬롯 제거 완료");
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
            // HorizontalLayoutGroup 설정 강제 적용
            _horizontalLayoutGroup.spacing = 10f; // 슬롯 간격
            _horizontalLayoutGroup.childAlignment = TextAnchor.MiddleRight; // 오른쪽부터 채워짐
            _horizontalLayoutGroup.reverseArrangement = true; // 순서 뒤집기 (FIFO 유지하면서 오른쪽 정렬)
            _horizontalLayoutGroup.childControlWidth = false;  // 슬롯 너비 제어 안함
            _horizontalLayoutGroup.childControlHeight = false; // 슬롯 높이 제거 안함
            _horizontalLayoutGroup.childForceExpandWidth = false;
            _horizontalLayoutGroup.childForceExpandHeight = false;

            _horizontalLayoutGroup.enabled = false;
            _horizontalLayoutGroup.enabled = true;

            Debug.Log("[ItemDisplayUI] HorizontalLayoutGroup 설정 - 오른쪽부터 FIFO 순서로 채움");
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