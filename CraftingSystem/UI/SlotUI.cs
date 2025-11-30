using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QuestDialogueSystem;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System;

// [RequireComponent(typeof(TooltipHoverHandler))]
public class SlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public InventorySlot Slot{get; private set;}
    [SerializeField] Image iconImg;
    [SerializeField] Image highlightImg;
    [SerializeField] Image borderImg;
    [SerializeField] TextMeshProUGUI count;
    public bool IsEmpty => Slot == null || Slot.IsEmpty;

    public void InitializeSlot()
    {
        Slot = new();
        ClearSlot();
    }

    public void SetSlot(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty) 
        {
            ClearSlot();
            return;
        }

        Slot = slot;

        iconImg.sprite = Slot.stack.Item.itemIcon;
        iconImg.enabled = true;
        borderImg.enabled = true;

        count.text = Slot.stack.Count.ToString();
        GetComponent<TooltipHoverHandler>().SetTooltip(slot.stack.Item.description);

        gameObject.SetActive(true);
    }

    public void FakeCount(int fakeCount)
    {
        count.text = fakeCount.ToString();
    }

    public void ReCount()
    {
        if (Slot == null || Slot.IsEmpty)
        {
            count.text = "0";
            return;
        }

        count.text = Slot.stack.Count.ToString();
    }   

    public void ClearSlot()
    {
        iconImg.sprite = null;
        iconImg.enabled = false;
        borderImg.enabled = false;
        count.text = "";

        gameObject.SetActive(false);
    }


    void OnEnable()
    {
        Highlight(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Slot == null || Slot.IsEmpty) return;
        Debug.Log("[SlotUI] Clicked on slot: " + Slot.stack);
        Slot.BeingInteract();
    
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Highlight(false);
    }

    public void Highlight(bool set)
    {
        highlightImg.enabled = set;
    }

    public override string ToString()
    {
        if(Slot == null || Slot.IsEmpty) return "Empty Slot";
        return $"{Slot.stack.Item.itemName} {Slot.stack.Count}/{Mathf.Min(Slot.stack.Max, Slot.slotMaxStack)}";
    }
}
