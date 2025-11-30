using QuestDialogueSystem;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using MsgSystem;

public class CraftingManager : SlottableManager
{
    [SerializeField] CraftingPanelUI craftingPanelUI;
    [SerializeField] CraftingResultPanelUI craftingResultPanelUI;
    [SerializeField] Inventory inventory;
    [SerializeField] MsgArranger msgArranger;

    Dictionary<string, InventorySlot> knownRecipes = new();
    Dictionary<string, ItemData> allRecipeDatas = new();
    ItemData currentRecipe;

    public bool OnCraftingPanel;

    #region Unity Lifecycle

    void OnEnable() 
    {
        craftingPanelUI.sliderSlot.OnValueChanged += UpdateCraftAmount;
    }

    void OnDisable() 
    {
        craftingPanelUI.sliderSlot.OnValueChanged -= UpdateCraftAmount;
    }
    #endregion

    #region Public API

    public void Initialize()
    {
        BindSlottable();
        BuildRecipes();
        ClearRecipe();
    }

    public override void OnSlottableInteract(ISlottable slottable)
    {
        Debug.Log("[CraftingManager] OnSlottableInteract: " + slottable);
        if (!OnCraftingPanel) return;

        var slotItem = ((InventorySlot)slottable).stack.Item;
        if (slotItem.IsRecipe)
        {
            ClearRecipe(true);
            currentRecipe = slotItem;

            int maxCraftCount = CalculateMaxCraftCount(currentRecipe.Ingredients);
            craftingPanelUI.UnlockCraftingButton(maxCraftCount > 0);
            craftingPanelUI.UpdateCraftSlider(maxCraftCount);
            craftingPanelUI.UpdateRecipeInfo(((InventorySlot)slottable).stack);

            if (maxCraftCount == 0) 
            {
                ClearIngredientSlots();
                GatherIngredients(currentRecipe.Ingredients, 1);
            }
            return;
        }
        
        // is ingredient, update ingredientSlot, remove it from inventory
        TryGetFromSlot((InventorySlot)slottable, 1, out var slotToTransfer);
        if (!TryPlace(slotToTransfer)) 
        {
            using (msgArranger.MuteInventoryMsg())
            {
                inventory.TryAdd(slotToTransfer.stack, out int _);
            }
        }
            

        if (AreAllSlotsFilled()) 
        {
            if(TryLookupRecipe(out var recipeSlot)) OnSlottableInteract(recipeSlot);
            craftingPanelUI.UnlockCraftingButton(true);
        }else craftingPanelUI.UnlockCraftingButton(false);
    }

    public override void OnSlotInteract(SlottableSlot slot)
    {
        Debug.Log("[CraftingManager] OnSlotInteract: " + slot);
        CurrentSlot = slot;
        if (slot == null || slot.Current == null) return;

        ClearRecipe(clearSlots: false);

        
        using (msgArranger.MuteInventoryMsg())
        {
            inventory.TryAdd(((InventorySlot)slot.Current).stack, out _);
        }
        
        TryPlace(null);
        craftingPanelUI.UnlockCraftingButton(false);
    }

    public bool CanCraft()
    {
        if(!AreAllSlotsFilled()) return false;
        return true;
    }

    #endregion

    #region Binding / UI

    void BindSlottable()
    {
        foreach (var slot in inventory.Slots) 
            BindSlottable(slot);
    }

    void UpdateCraftAmount(int amount)
    {
        if (currentRecipe == null) return;
        ClearIngredientSlots();
        GatherIngredients(currentRecipe.Ingredients, amount);
        craftingPanelUI.UpdateProductAmount(amount);
    }

    void ClearRecipe(bool clearSlots = false)
    {
        currentRecipe = null;

        craftingPanelUI.UpdateCraftSlider(0);
        craftingPanelUI.UpdateRecipeInfo(ItemStack.Empty); 

        if (clearSlots) 
        {
            ClearIngredientSlots();
            craftingPanelUI.UnlockCraftingButton(false);
        }
    }

    void ClearIngredientSlots(bool returnToInv = true)
    {
        foreach (var slot in Slots)
        {
            if (slot == null || slot.Current == null)
                continue;

            if (returnToInv && slot.Current is InventorySlot inv && inv.stack.Count > 0)
            {
                using (msgArranger.MuteInventoryMsg())
                {
                    inventory.TryAdd(inv.stack, out _);
                }
            }

            slot.Set(null);
        }
    }

    public void Craft()
    {
        if (!CanCraft()) return;

        if(currentRecipe == null) //if its in free mode
        {
            var key = BuildKeyFromSlots();
            //if no recipe was found, the crafting attempt is failed
            if(!allRecipeDatas.TryGetValue(key, out ItemData recipe)) return;

            inventory.TryAdd(recipe,1,out int _);
            inventory.TryAdd(recipe.Product, 1, out int _);

            inventory.TryGetFirstSlotMatch(recipe, out var invSlot);
            knownRecipes[key] = invSlot;

            ItemStack recipeStack = new(recipe.Product, 1);
            craftingResultPanelUI.Open(recipeStack, true);

            ClearIngredientSlots(false);
            ClearRecipe(false);
            return;
        } //else in recipe batch mode

        int repeat = craftingPanelUI.sliderSlot.GetValue();
        inventory.TryAdd(currentRecipe.Product, repeat, out _);

        ItemStack productStack = new(currentRecipe.Product, repeat);
        craftingResultPanelUI.Open(productStack, false);

        ClearIngredientSlots(false);
        ClearRecipe(false);
    }

    public void CloseCraftingPanel()
    {
        ClearIngredientSlots();
        ClearRecipe();
        craftingPanelUI.gameObject.SetActive(false);
    }

    public void OpenCraftingPanel()
    {
        craftingPanelUI.gameObject.SetActive(true);
    }

    #endregion

    #region Crafting / Inventory Helpers

    bool AreAllSlotsFilled()
    {
        foreach (var slot in Slots)
        {
            if (slot?.Current is not InventorySlot inv)
                return false;

            if (inv.stack.Item == null || inv.stack.Count <= 0)
                return false;
        }
        return true;
    }

    string BuildKeyFromSlots()
    {
        ItemData[] items = new ItemData[Slots.Count];

        for (int i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];

            if (slot?.Current is not InventorySlot inv || inv.stack.Item == null || inv.stack.Count <= 0)
            {
                items[i] = null;
                continue;
            }

            items[i] = inv.stack.Item;
        }

        return RecipeKeyUtil.BuildKeyFromItems(items);
    }

    bool TryLookupRecipe(out InventorySlot recipe)
    {
        string key = BuildKeyFromSlots();
        if(!knownRecipes.TryGetValue(key, out recipe)) return false;

        return true;
    }

    void BuildRecipes()
    {
        knownRecipes = new Dictionary<string, InventorySlot>();

        foreach (var slot in inventory.Slots)
        {
            if (slot == null || slot.stack.Item == null) continue;
            var item = slot.stack.Item;

            if (!item.IsRecipe || item.Ingredients == null || item.Ingredients.Length == 0)
                continue;

            string key = RecipeKeyUtil.BuildKeyFromItems(item.Ingredients);
            if (string.IsNullOrEmpty(key))
                continue;

            if (knownRecipes.ContainsKey(key))
            {
                Debug.LogWarning($"[CraftingManager] Duplicate recipe key: {key} for {item.name}");
                continue;
            }

            knownRecipes.Add(key, slot);
        }

        ItemDatabase.TryGetItemsByAttunement(Attunement.None, out var recipes);
        foreach (var recipe in recipes)
        {
            string key = RecipeKeyUtil.BuildKeyFromItems(recipe.Ingredients);
            allRecipeDatas[key] = recipe;
        }
    }


    void GatherIngredients(ItemData[] items, int amount)
    {
        foreach (var item in items)
        {
            if (inventory.TryGetFirstSlotMatch(item, out var slot))
            {
                // 是的我知道，由於 inventory count, getfromSlot 在物品計數上不同步，可能出錯
                // 但先不修它，我們應該不會碰上這個問題
                TryGetFromSlot(slot, amount, out var slotToTransfer);
                TryPlace(slotToTransfer);
            }
            else PlaceHint(item);
        }
    }

    void PlaceHint(ItemData item) // place greyout item
    {
        var invSlot = new InventorySlot();
        invSlot.stack = new(item, 0);
        TryPlace(invSlot);
    }

    bool TryGetFromSlot(InventorySlot slot, int count, out InventorySlot toTransfer)
    {
        toTransfer = new InventorySlot();

        if (slot.stack.Count < count) 
        {
            toTransfer.stack = new(slot.stack.Item, 0);
            return false;
        }

        toTransfer.stack = new(slot.stack.Item, count);

        int discard = 0;
        using (msgArranger.MuteInventoryMsg())
        {
            inventory.TryRemoveFromSlot(toTransfer.stack, slot, ref discard);
        }
        
        return true;
    }

    int CalculateMaxCraftCount(ItemData[] ingredients)
    {
        if (ingredients == null || ingredients.Length == 0) return 0;

        Dictionary<ItemData, int> Consumed = new();
        foreach (var ingredient in ingredients) 
        {
            if(Consumed.TryGetValue(ingredient, out int count)) Consumed[ingredient] = count +1;
            else Consumed[ingredient] = 1;
        }

        int maxCraftCount = int.MaxValue;

        foreach (var entry in Consumed)
        {
            int needPerCraft = entry.Value;
            int totalAvailable = CountTotalAvailable(entry.Key);
            int craftsForThisItem = totalAvailable / needPerCraft;

            if (craftsForThisItem == 0) return 0;
            if (craftsForThisItem < maxCraftCount) maxCraftCount = craftsForThisItem;     
        }

        return maxCraftCount == int.MaxValue ? 0 : maxCraftCount;
    }

    int CountTotalAvailable(ItemData ingredient)
    {
        int fromInventory = inventory.Count(ingredient);
        int fromSlots = 0;

        foreach (var slot in Slots)
        {
            if (slot.Current is not InventorySlot inv) 
                continue;

            if (inv.stack.Item.Equals(ingredient)) 
                fromSlots += inv.stack.Count;
        }

        return fromInventory + fromSlots;
    }

    #endregion
}
