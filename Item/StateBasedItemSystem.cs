using Naninovel;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[InitializeAtRuntime]
public class StateBasedItemSystem : IEngineService
{
    public static StateBasedItemSystem Instance { get; private set; }

    public enum ItemState
    {
        NotOwned = 0,
        Owned = 1,
        Used = 2
    }

    private ItemDisplayUI _itemDisplayUI;
    private Dictionary<string, ItemData> _itemDataMap = new();
    private Dictionary<string, ItemState> _itemStates = new(); // 메모리에만 저장
    private List<string> _itemOrder = new(); // FIFO 순서 관리용

    public StateBasedItemSystem()
    {
        Instance = this;
        Debug.Log("[ItemSystem] 생성자 호출");
    }

    public UniTask InitializeService()
    {
        Debug.Log("[ItemSystem] InitializeService 시작");

        // ItemData 로드
        _itemDataMap = Resources.LoadAll<ItemData>("ItemData")
            .ToDictionary(data => data.itemId, data => data);
        Debug.Log($"[ItemSystem] 로드된 ItemData 개수: {_itemDataMap.Count}");
        foreach (var kvp in _itemDataMap)
        {
            Debug.Log($"  - ItemData: '{kvp.Key}' = {kvp.Value.itemName}");
        }
        Debug.Log($"[ItemSystem] ItemData 로드 완료: {_itemDataMap.Count}개");

        // UI 찾기
        FindItemDisplayUI();

        return UniTask.CompletedTask;
    }

    private void FindItemDisplayUI()
    {
        var uiManager = Engine.GetService<IUIManager>();
        _itemDisplayUI = uiManager?.GetUI<ItemDisplayUI>();

        if (_itemDisplayUI == null)
        {
            _itemDisplayUI = GameObject.FindObjectOfType<ItemDisplayUI>();
        }

        if (_itemDisplayUI == null)
        {
            Debug.LogWarning("[ItemSystem] ItemDisplayUI를 찾을 수 없음");
        }
        else
        {
            Debug.Log("[ItemSystem] ItemDisplayUI 찾기 성공");
        }
    }

    // 일차 시작시 모든 아이템 초기화
    public void ResetAllItems()
    {
        Debug.Log("[ItemSystem] 모든 아이템 초기화");

        _itemStates.Clear();
        _itemOrder.Clear(); // 순서도 초기화

        // UI가 없으면 찾아보기
        if (_itemDisplayUI == null)
        {
            FindItemDisplayUI();
        }

        // UI가 있으면 초기화
        if (_itemDisplayUI != null)
        {
            _itemDisplayUI.ClearAllIcons();
        }
        else
        {
            Debug.LogWarning("[ItemSystem] ItemDisplayUI를 찾을 수 없어서 UI 초기화를 건너뜁니다.");
        }
    }

    public ItemState GetItemState(string itemId)
    {
        return _itemStates.TryGetValue(itemId, out ItemState state) ? state : ItemState.NotOwned;
    }

    public void SetItemState(string itemId, ItemState state)
    {
        if (!ValidateItem(itemId)) return;

        var previousState = GetItemState(itemId);
        _itemStates[itemId] = state;

        // FIFO 순서 관리
        if (state == ItemState.Owned && previousState != ItemState.Owned)
        {
            // 새로 소유하게 된 경우 순서 추가
            if (!_itemOrder.Contains(itemId))
            {
                _itemOrder.Add(itemId);
            }
        }
        else if (state != ItemState.Owned && previousState == ItemState.Owned)
        {
            // 더 이상 소유하지 않는 경우 순서에서 제거
            _itemOrder.Remove(itemId);
        }

        Debug.Log($"[ItemSystem] 아이템 상태 변경: {itemId} -> {state}");
    }

    public bool AcquireItem(string itemId)
    {
        if (!ValidateItem(itemId)) return false;

        var currentState = GetItemState(itemId);
        if (currentState == ItemState.Owned)
        {
            Debug.LogWarning($"[ItemSystem] 이미 소유한 아이템: {itemId}");
            return false;
        }

        SetItemState(itemId, ItemState.Owned);

        // UI 업데이트
        if (_itemDisplayUI != null && _itemDataMap.TryGetValue(itemId, out ItemData itemData))
        {
            _itemDisplayUI.AddItemIcon(itemData);
            Debug.Log($"[ItemSystem] 아이템 획득 완료: {itemId}");
        }

        return true;
    }

    public bool UseItem(string itemId)
    {
        if (!ValidateItem(itemId)) return false;

        var currentState = GetItemState(itemId);
        if (currentState != ItemState.Owned) return false;

        SetItemState(itemId, ItemState.Used);
        _itemDisplayUI?.RemoveItemIcon(itemId);

        Debug.Log($"[ItemSystem] 아이템 사용: {itemId}");
        return true;
    }

    public string UseOldestItem()
    {
        var ownedItems = GetOwnedItems();
        return ownedItems.Count > 0 ? UseItem(ownedItems[0]) ? ownedItems[0] : null : null;
    }

    private bool ValidateItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogError("[ItemSystem] 아이템 ID가 비어있습니다");
            return false;
        }

        if (!_itemDataMap.ContainsKey(itemId))
        {
            Debug.LogError($"[ItemSystem] 존재하지 않는 아이템 ID: {itemId}");
            return false;
        }

        return true;
    }

    public List<string> GetItemsByState(ItemState state)
    {
        if (state == ItemState.Owned)
        {
            // Owned 상태는 FIFO 순서 보장
            return _itemOrder.Where(id => _itemStates.TryGetValue(id, out ItemState s) && s == ItemState.Owned).ToList();
        }
        else
        {
            // 다른 상태들은 순서 상관없음
            return _itemStates.Where(kvp => kvp.Value == state).Select(kvp => kvp.Key).ToList();
        }
    }

    public List<string> GetOwnedItems() => GetItemsByState(ItemState.Owned);
    public List<string> GetUsedItems() => GetItemsByState(ItemState.Used);

    public bool HasItem(string itemId) => GetItemState(itemId) == ItemState.Owned;
    public bool HasUsedItem(string itemId) => GetItemState(itemId) == ItemState.Used;
    public bool HasEverOwnedItem(string itemId) => GetItemState(itemId) >= ItemState.Owned;

    public Dictionary<string, ItemData> GetAllItemData() => _itemDataMap;
    public ItemData GetItemData(string itemId) => _itemDataMap.TryGetValue(itemId, out ItemData data) ? data : null;

    public void ResetService()
    {
        ResetAllItems();
    }

    public void DestroyService()
    {
        Instance = null;
    }
}