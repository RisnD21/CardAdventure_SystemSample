using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QuestDialogueSystem;
using UnityEngine.Rendering;

public class CraftingPanelUI : MonoBehaviour
{
    public InventoryUI inventoryUI;
    public Attunement defaultAttunement = Attunement.None;
    [SerializeField] CraftingManager craftingManager;
    [SerializeField] SlottableSlot[] ingredientSlots;
    
    [SerializeField] Button craftingButton;

    [Header("Recipe Info UI")]
    [SerializeField] TextMeshProUGUI descriptionPanel;
    [SerializeField] TextMeshProUGUI namePanel;
    [SerializeField] TextMeshProUGUI amountPanel;
    [SerializeField] Image iconImage;
    [SerializeField] Sprite defaultIcon;

    public SliderSlot sliderSlot;

    void Start()
    {
        craftingButton.onClick.AddListener(craftingManager.Craft);
    }

    void OnEnable()
    {
        craftingManager.OnCraftingPanel = true;

        inventoryUI.InitializeSlots();
        BindWithIngredientSlot();
        inventoryUI.ShowPage(defaultAttunement);
    }

    void OnDisable() 
    {
        craftingManager.OnCraftingPanel = false;
    }

    public void ShowPage(Attunement attunement)
    {
        inventoryUI.ShowPage(attunement);
    }

    void BindWithIngredientSlot()
    {
        for(int i =0;i<ingredientSlots.Length; i++) craftingManager.AddSlot(ingredientSlots[i]);
    }

    //後續需要更新 crafting 按鈕, craftin anime 就從這邊叫
    public void UpdateRecipeInfo(ItemStack stack)
    {
        // 空堆疊 → 清空 UI
        if (stack.IsEmpty || stack.Item == null)
        {
            namePanel.text = string.Empty;
            descriptionPanel.text = string.Empty;
            amountPanel.text = string.Empty;

            iconImage.sprite = defaultIcon;
            return;
        }

        var item = stack.Item.Product;

        namePanel.text = item.itemName;
        descriptionPanel.text = item.description;
        amountPanel.text = $"{stack.Count}";

        iconImage.sprite = item.itemIcon;
    }

    public void UpdateCraftSlider(int maxCount)
    {
        if (maxCount <= 0) sliderSlot.Initialized("", 0, 0);
        else sliderSlot.Initialized("", 1, maxCount);
    }

    public void UpdateProductAmount(int amount)
    {
        amountPanel.text = $"{amount}";
    }

    public void UnlockCraftingButton(bool set)
    {
        craftingButton.interactable = set;
    }

    
}
