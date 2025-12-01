# Crafting System

**Combine ingredients → Auto-detect recipes → Produce crafted items**

---

## 1. What Problem This Solves

Many inventory-driven games need a crafting pipeline where:

* Players place items into ingredient slots
* Recipes are detected automatically (order-independent)
* Crafted products are added back to the inventory
* UI remains thin and free of game logic
* Recipe items (ItemSO) can define batch-crafting behavior

This system provides a modular solution that:

* Converts **InventorySlot inputs** into crafted results
* Supports **free crafting** and **recipe-item-based crafting**
* Uses **data-driven ItemSO** for ingredients and products
* Keeps Inventory, UI, and Crafting logic decoupled
* Provides a **slot-based interaction model**

---

## 2. System Architecture Overview

```
CraftingManager
 ├─ Handles slottable interactions
 ├─ Looks up recipes (free-mode or recipe-mode)
 ├─ Gathers ingredients from Inventory
 └─ Crafts products and returns unused items
        ↓
Inventory
 ├─ TryAdd / TryRemove / TryRemoveFromSlot
 └─ Counts items for craft amount calculation
        ↓
ItemDatabase
 └─ Index of ItemSO (including possible recipes)
        ↓
CraftingPanelUI
 ├─ Displays recipe info
 ├─ Updates craft slider
 └─ Enables/Disables craft button
        ↓
SlotUI / IngredientSlot
 └─ Forwards pointer interactions to CraftingManager
```

**Core idea:**
**Slot interactions → Recipe resolution → Inventory-based crafting → UI updates**

---

## 3. Flow: Player Actions → Recipe Detection → Crafting Result

### **1) Player interacts with a slot**

Clicking behaves differently depending on the item:

* **Recipe ItemSO** → Enters recipe mode
* **Normal ingredients** → Fills ingredient slots
* **Empty slot** → Clears recipe and returns ingredient to inventory

### **2) CraftingManager evaluates the slot state**

Possible paths:

* **Recipe Mode:**
  Identify the recipe item → auto-calculate max craft amount → auto-gather ingredients.

* **Free Craft Mode:**
  When all ingredient slots are filled:
  Build a normalized key → match against known recipes → craft if found.

### **3) CraftingManager gathers ingredients**

Ingredients are collected using:

```
inventory.TryGetFirstSlotMatch()
TryGetFromSlot()
TryPlace()
```

Greyed-out hint items appear if an ingredient is missing.

### **4) CraftingManager executes Craft()**

Two modes:

* **Batch Recipe Crafting**
  Repeats crafting based on slider amount.

* **Free Crafting**
  Crafts a single product and registers the discovered recipe.

All modifications go through `Inventory.TryAdd` / `TryRemove`.

### **5) UI displays the results**

`CraftingResultPanelUI` shows the crafted items.
Slots and recipe state are cleared afterward.

---

## 4. Folder Structure

```plaintext
CraftingSystem/
 ├─ Core/
 │   ├─ CraftingManager.cs              # Main crafting logic
 │   └─ RecipeKeyUtil.cs                # Order-independent recipe key builder
 │
 ├─ InventoryCore/
 │   ├─ Inventory.cs                    # Public API for inventory operations
 │   ├─ InventoryModel.cs               # Raw data: InventorySlot[]
 │   ├─ InventorySlot.cs                # Holds ItemStack
 │   ├─ ItemStack.cs                    # Item + Count container
 │   ├─ ItemStackData.cs
 │   └─ IInventory.cs
 │
 ├─ SO/
 │   ├─ ItemData.cs                     # Defines recipes: Ingredients + Product
 │   └─ ItemDatabase.cs                 # Index of item data by attributes
 │
 ├─ UI/
 │   ├─ CraftingPanelUI.cs              # Main crafting UI (slider + info)
 │   ├─ CraftingResultPanelUI.cs        # Result popup
 │   ├─ CraftingTabController.cs
 │   ├─ IngredientSlot.cs               # Ingredient slottable slot
 │   ├─ SlotUI.cs                       # Viewer for inventory slots
 │   └─ InventoryUI.cs                  # Displays inventory pages
 │
 └─ Util/
     └─ RecieveItemOnStart.cs
```

(Structure from uploaded files: )

---

## 5. Key Components

### **CraftingManager.cs**

The main controller that handles:

* Slottable interactions
* Recipe lookup (free-mode & recipe-mode)
* Auto-gathering ingredients
* Validating craftable count
* Executing the craft
* Communicating with UI

Example recipe-mode crafting:

```csharp
int repeat = craftingPanelUI.sliderSlot.GetValue();
inventory.TryAdd(currentRecipe.Product, repeat, out _);

ItemStack productStack = new(currentRecipe.Product, repeat);
craftingResultPanelUI.Open(productStack, false);

ClearIngredientSlots(false);
ClearRecipe(false);
```

### **RecipeKeyUtil.cs**

Builds order-independent recipe keys:

```csharp
ids.Sort(StringComparer.Ordinal);
return string.Join("+", ids);
```

Used for both:

* Free crafting recipe discovery
* Recipe item registration

### **Inventory.cs + InventorySlot.cs**

Inventory provides all safe mutation APIs:

```
TryAdd
TryRemove
TryRemoveFromSlot
Count
TryGetFirstSlotMatch
```

UI and crafting code never modify slots directly.

---

### **CraftingPanelUI.cs**

Handles UI responsibilities only:

* Shows recipe name/desc/icon
* Updates slider based on max craft count
* Binds ingredient slots to CraftingManager
* Enables/disables craft button

Logic-free by design.

---

### **SlotUI / IngredientSlot**

Thin viewer components:

* Display assigned `InventorySlot`
* OnPointerClick → forwarded to CraftingManager
* No crafting logic inside

---

## 6. ScriptableObjects

### **ItemData**

Defines:

* `itemID`
* `Ingredients[]` (ItemData references)
* `Product` (ItemData)
* `itemName`, `description`, `itemIcon`
* `IsRecipe`

### **ItemDatabase**

Provides search access:

```csharp
ItemDatabase.TryGetItemsByAttunement(...)
```

Used for loading all possible recipe datas.

---

## 7. Future Extensions

### **Recipe Unlock Progression**

* Skill tree unlocks
* Discoverable recipes
