using DG.Tweening;
using QuestDialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngredientSlot : SlottableSlot, IPointerClickHandler
{
    [SerializeField] Image iconImg;
    [SerializeField] Sprite defaultSprite;
    [SerializeField] TextMeshProUGUI countPanel;
    public int Requirement { get; private set; }

    Color originalColor;
    Tween highlightTween;

    public InventorySlot InventorySlot  { get; private set; }

    void Awake()
    {
        originalColor = iconImg.color;
        Reset();
    }

    public override bool CanAccept(ISlottable item)
    {
        if (item == null) return true;
        if (Current != null) return false;
        if (item is not InventorySlot inventorySlot) return false;
        return true;
    }

    public override void Set(ISlottable item)
    {
        base.Set(item);
        if (item == null) 
        {
            Reset();
            return;
        }

        InventorySlot = (InventorySlot) item;
        var stack = InventorySlot.stack;
        
        UpdateCount(stack.Count);
        UpdateIcon(stack.Item.itemIcon);
        if (stack.IsEmpty) SetHighlight(true);
        else SetHighlight(false);
    }

    void UpdateIcon(Sprite icon)
    {
        iconImg.sprite = icon;
    }

    void UpdateCount(int count)
    {
        countPanel.text = count.ToString();
    }

    public void Reset()
    {
        UpdateIcon(defaultSprite);
        countPanel.text = "";
        SetHighlight(false);
    }

    public void SetHighlight(bool set)
    {
        // 停掉原有的 tween，避免殘留
        highlightTween?.Kill();
        Debug.Log("Changing text color");
        if (set)
        {
            Debug.Log("Changing Color to red");
            // 文字變紅色
            countPanel.color = Color.red;

            // Pop 動畫：放大 → 回到 1
            countPanel.transform.DOScale(1f, 0f); // 先確保初始值
            highlightTween = countPanel.transform
                .DOScale(2f, 0.15f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    countPanel.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
                });
        }
        else
        {
            // 回到正常黑色
            countPanel.color = Color.black;

            // 確保 scale 也回到 1
            countPanel.transform.DOScale(1f, 0.1f);
        }
    }


    public void OnPointerClick(PointerEventData _)
    {
        if(Current == null || InventorySlot == null) return;
        TriggerInteract();
    }
}