using UnityEngine;
using System.Collections.Generic;
using System;

namespace QuestDialogueSystem
{
    public class Inventory : MonoBehaviour, IInventory
    {
        [SerializeField] List<string> itemsHolding;
        //for checking current items
        static InventoryModel model;

        bool hasInitialized;

        public static void SetModel(InventoryModel inventoryModel)
        {
            model = inventoryModel;
        }

        public void Initialize()
        {
            if(hasInitialized) return;

            model.OnItemAdd += UpdateInventoryPreview;
            model.OnItemRemove += UpdateInventoryPreview;
            
            model.OnItemAdd += stack => OnItemAdd?.Invoke(stack);
            model.OnItemRemove += stack => OnItemRemove?.Invoke(stack);

            
            hasInitialized = true;
        }

        void OnDisable() {
            model.OnItemAdd -= UpdateInventoryPreview;
            model.OnItemRemove -= UpdateInventoryPreview;

            model.OnItemAdd -= stack => OnItemAdd?.Invoke(stack);
            model.OnItemRemove -= stack => OnItemRemove?.Invoke(stack);
        }

        void UpdateInventoryPreview(ItemStack _) 
        {
            itemsHolding.Clear();

            foreach(var slot in Slots)
            {
                string stackInfo = slot.IsEmpty? "Empty" : slot.stack.ToString();
                itemsHolding.Add(stackInfo);
            }
        }

        public InventoryModel GetModel() => model;

        public IReadOnlyList<InventorySlot> Slots => model.Slots;

        public int Count(string id) => model.Count(id);
        public int Count(ItemData item) => model.Count(item);
        public bool HasItem(string id) => model.Count(id) > 0;
        public bool HasItem(ItemData item) => model.Count(item) > 0;

        public bool TryGetFirstSlotMatch(ItemData item, out InventorySlot slot)
        {
            foreach (var e in Slots)
            {
                if(!e.HasItem(item)) continue;
                if (e.stack.Item.Equals(item))
                {
                    slot = e;
                    return true;
                }
            }
            slot = null;
            return false;
        }

        public bool TryAdd(string id, int count, out int remainder)
            => model.TryAdd(id, count, out remainder);

        public bool TryAdd(ItemData item, int count, out int remainder)
            => model.TryAdd(item, count, out remainder);

        public bool TryAdd(ItemStack set, out int remainder)
            => model.TryAdd(set, out remainder);

        public bool TryRemove(string id, int count, out int remainder)
            => model.TryRemove(id, count, out remainder);

        public bool TryRemove(ItemData item, int count, out int remainder)
            => model.TryRemove(item, count, out remainder);

        public bool TryRemove(ItemStack set, out int remainder)
            => model.TryRemove(set, out remainder);

        public bool TryRemoveFromSlot(ItemStack stack, InventorySlot slot ,ref int remainder)
            => model.TryRemoveFromSlot(stack, slot ,ref remainder);
        public event Action<ItemStack> OnItemAdd;
        public event Action<ItemStack> OnItemRemove;
        public void PrintAllSlots() => model.PrintAllSlots();
    }
}