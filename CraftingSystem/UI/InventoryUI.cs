using UnityEngine;
using QuestDialogueSystem;
using System.Collections.Generic;
using System;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] Transform slotsParent;
    SlotUI[] slots;

    Dictionary<Attunement, List<InventorySlot>> itemFilters = new();

    [SerializeField] Inventory inventory;
    Attunement currentPage;

    void OnEnable()
    {
        inventory.OnItemAdd += OnSlotModified;
        inventory.OnItemRemove += OnSlotModified;
    }

    void OnDisable()
    {
        inventory.OnItemAdd -= OnSlotModified;
        inventory.OnItemRemove -= OnSlotModified;
    }

    void OnSlotModified(ItemStack itemStack)
    {
        var attunement = itemStack.Item.Attunement;
        RefreshPage(attunement);
    }

    public void InitializeSlots()
    {
        slots = slotsParent.GetComponentsInChildren<SlotUI>(true);
        foreach(var slot in slots) slot.InitializeSlot();
        RearrangeSlots(slots);
        
        itemFilters.Clear();
        UpdateItemFilters();
    }

    void RearrangeSlots(SlotUI[] slots)
    {
        Array.Sort(slots, (a, b) => 
            a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
    }

    void UpdateItemFilter(Attunement attunement)
    {
        if(!itemFilters.TryGetValue(attunement, out var attFilter)) 
        {
            attFilter = new List<InventorySlot>();
            itemFilters[attunement] = attFilter;
        } else attFilter.Clear();
        
        foreach(var slot in inventory.Slots)
        {
            if (slot.IsEmpty || slot.stack.Item.Attunement != attunement) continue;
            attFilter.Add(slot);
        }        
    }

    void UpdateItemFilters()
    {
        itemFilters.Clear();
        foreach(var slot in inventory.Slots)
        {
            if (slot.IsEmpty) continue;
            if(!itemFilters.TryGetValue(slot.stack.Item.Attunement, out var attFilter))
            {
                attFilter = new();
                itemFilters[slot.stack.Item.Attunement] = attFilter;
            }

            attFilter.Add(slot);
        }
    }

    public void ShowPage(Attunement attunement)
    {
        currentPage = attunement;
        RefreshPage(attunement);
    }

    void RefreshPage(Attunement attunement)
    {
        if (currentPage != attunement) return;

        UpdateItemFilter(attunement);
        var invSlots = RearrangeInvSlotOrder(itemFilters[attunement]);
        Debug.Log("[InventoryUI] invSlots count = " + invSlots.Count);
        UpdateSlots(invSlots);
    }

    void UpdateSlots(List<InventorySlot> invSlots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if(i < invSlots.Count) slots[i].SetSlot(invSlots[i]);
            else slots[i].ClearSlot();
        }
    }

    List<InventorySlot> RearrangeInvSlotOrder(List<InventorySlot> invSlots)
    {
        //implement later
        return invSlots;
    }
}