using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Naninovel;

public class HomeGameStateSlotsGrid : MonoBehaviour
{
    [Header("�׸��� ����")]
    [SerializeField] private Transform container;
    [SerializeField] private GameObject slotPrefab;

    public List<HomeGameStateSlot> Slots { get; private set; } = new List<HomeGameStateSlot>();

    private Action<int> onSlotClicked;
    private Action<int> onDeleteClicked;
    private Func<int, UniTask<GameStateMap>> loadStateAt;

    public async UniTask Initialize(int itemsCount, Action<int> onSlotClicked,
        Action<int> onDeleteClicked, Func<int, UniTask<GameStateMap>> loadStateAt)
    {
        Debug.Log($"[HomeGameStateSlotsGrid] Initialize - itemsCount: {itemsCount}");

        this.onSlotClicked = onSlotClicked;
        this.onDeleteClicked = onDeleteClicked;
        this.loadStateAt = loadStateAt;

        await InitializeSlots(itemsCount);
    }

    public virtual void BindSlot(int slotNumber, GameStateMap state)
    {
        var slot = Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
        slot?.Bind(slotNumber, state);

        Debug.Log($"[HomeGameStateSlotsGrid] BindSlot - ���� {slotNumber} ���ε�");
    }

    private async UniTask InitializeSlots(int itemsCount)
    {
        Debug.Log($"[HomeGameStateSlotsGrid] InitializeSlots ���� - itemsCount: {itemsCount}");

        // ���� ���Ե� ����
        ClearSlots();
        Debug.Log("[HomeGameStateSlotsGrid] ClearSlots �Ϸ�");

        // Container Ȯ��
        if (container == null)
        {
            container = transform.Find("Container") ?? transform;
            Debug.Log($"[HomeGameStateSlotsGrid] Container �ڵ� ����: {container.name}");
        }
        else
        {
            Debug.Log($"[HomeGameStateSlotsGrid] Container �̹� ������: {container.name}");
        }

        if (slotPrefab == null)
        {
            Debug.LogError("[HomeGameStateSlotsGrid] SlotPrefab�� �������� �ʾҽ��ϴ�!");
            return;
        }
        else
        {
            Debug.Log($"[HomeGameStateSlotsGrid] SlotPrefab Ȯ�ε�: {slotPrefab.name}");
        }

        Debug.Log("[HomeGameStateSlotsGrid] ���� ���� ����");

        // ���� ���� �� �ʱ�ȭ
        for (int i = 0; i < itemsCount; i++)
        {
            Debug.Log($"[HomeGameStateSlotsGrid] ���� {i} ���� ��...");
            await CreateAndBindSlot(i);
            Debug.Log($"[HomeGameStateSlotsGrid] ���� {i} ���� �Ϸ�");
        }

        Debug.Log($"[HomeGameStateSlotsGrid] {Slots.Count}�� ���� �ʱ�ȭ �Ϸ�");
    }

    private async UniTask CreateAndBindSlot(int itemIndex)
    {
        // ���� ������ ����
        GameObject slotObj = Instantiate(slotPrefab, container);
        slotObj.name = $"Slot_{itemIndex + 1:D2}";

        // HomeGameStateSlot ������Ʈ Ȯ��/�߰�
        HomeGameStateSlot slot = slotObj.GetComponent<HomeGameStateSlot>();
        if (slot == null)
        {
            slot = slotObj.AddComponent<HomeGameStateSlot>();
            Debug.LogWarning($"[HomeGameStateSlotsGrid] ���� {itemIndex}�� HomeGameStateSlot ������Ʈ �߰���");
        }

        // ���� �ʱ�ȭ
        InitializeSlot(slot);
        Slots.Add(slot);

        // ���� ������ ���ε�
        await BindSlotAsync(slot, itemIndex);
    }

    private void InitializeSlot(HomeGameStateSlot slot)
    {
        slot.Initialize(onSlotClicked, onDeleteClicked);
    }

    private async UniTask BindSlotAsync(HomeGameStateSlot slot, int itemIndex)
    {
        var slotNumber = itemIndex + 1;
        var state = await loadStateAt(slotNumber);

        if (slot != null) // ������ �ı����� �ʾҴ��� Ȯ��
        {
            slot.Bind(slotNumber, state);
        }
    }

    private void ClearSlots()
    {
        foreach (var slot in Slots)
        {
            if (slot != null && slot.gameObject != null)
                DestroyImmediate(slot.gameObject);
        }

        Slots.Clear();

        // Container�� ���� �ڽĵ鵵 ����
        if (container != null)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.GetChild(i).gameObject);
            }
        }
    }

    // ���� �޼���� (�ܺο��� ȣ�� ����)
    public void RefreshAllSlots()
    {
        Debug.Log("[HomeGameStateSlotsGrid] ��� ���� ���ΰ�ħ");

        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i] != null)
            {
                RefreshSlot(i + 1);
            }
        }
    }

    public async void RefreshSlot(int slotNumber)
    {
        var state = await loadStateAt(slotNumber);
        BindSlot(slotNumber, state);
    }
}