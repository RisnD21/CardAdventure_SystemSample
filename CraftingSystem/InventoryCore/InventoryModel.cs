using System;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

namespace QuestDialogueSystem
{
    public class InventoryModel : IInventory
    {
        [SerializeField] int inventorySize = 30;
        List<InventorySlot> slots = new();
        public IReadOnlyList<InventorySlot> Slots => slots;

        public event Action<ItemStack> OnItemAdd;
        public event Action<ItemStack> OnItemRemove;

        public InventoryModel(int size)
        {
            for(int i = 0; i < size; i++)
            {
                slots.Add(new InventorySlot());
            }
        }

        public void Initialize(){}

        public void PrintAllSlots()
        {
            foreach (var slot in Slots)
            {
                Debug.Log(slot);
            }
        }

        public List<ItemStack> RetriveAllStacks()
        {
            List<ItemStack> stacks = slots.Where(x => !x.IsEmpty).Select(x => x.stack).ToList();
            
            return stacks;
        }

        public void ClearInventory()
        {
            slots.Clear();
        }

        public int Count(ItemData item)
            => Slots.Where(s => s.HasItem(item)).Sum(s => s.stack.Count);

        public int Count(string id)
        {
            if(ItemDatabase.TryGetItemData(id, out ItemData item))
            {
                return Count(item);
            } else
            {
                return 0;
            }
        }

        public int SpaceLeftFor(ItemData item)
            => Slots.Where(s => s.CanStackWith(item)).Sum(s => s.SpaceLeft(item));


        public bool TryAdd(ItemStack set, out int remainder)
        {
            remainder = set.Count;

            if(SpaceLeftFor(set.Item) < set.Count)
            {
                string msg = SpaceLeftFor(set.Item) == 0 ?
                    "Bag is full" : $"Bag can only contain {SpaceLeftFor(set.Item)} {set.Item.itemName}, please clean up more space";

                Locator.NotificationUI.PrintInventoryMsg(msg);
                return false;
            }
            return TryForceAdd(set, out remainder);
        }
        

        public bool TryForceAdd(ItemStack set, out int remainder)
        {
            remainder = set.Count;
            int remainderBef = remainder;

            List<InventorySlot> nonEmptySlots 
                = Slots.Where(s => !s.IsEmpty && s.CanStackWith(set.Item)).ToList();
            
            foreach (var slot in nonEmptySlots)
            {
                TryAddToSlot(new ItemStack(set.Item, remainder), slot, ref remainder);
                if (remainder == 0) break;
            }
                
            if (remainder > 0)
            {
                List<InventorySlot> emptySlots
                    = Slots.Where(s => s.IsEmpty).ToList();
                
                foreach (var slot in emptySlots)
                {
                    TryAddToSlot(new ItemStack(set.Item, remainder), slot, ref remainder);
                    if (remainder == 0) break;
                }
            }

                
            if (remainder > 0)
            {
                string msg = $"Bag is full, {set.Item.itemName} discarded: {remainder}.";
                Locator.NotificationUI.PrintTitleMsg(msg);
            }

            OnItemAdd?.Invoke(new ItemStack(set.Item, remainderBef - remainder));
            return remainder < set.Count;
        }

        bool TryAddToSlot(ItemStack stack, InventorySlot slot ,ref int remainder)
        {
            if (slot == null) return false;

            if(!slot.TryAtomicAdd(stack, out int newRemainder)) return false;
            
            if (newRemainder >= 0)
            {
                remainder = newRemainder;
                return true;
            } 
            else
            {
                Debug.LogWarning("[Inventory] Remainder is negative");
                return false;
            }
        }
            
        public bool TryAdd(string id, int count, out int remainder)
        {
            remainder = count;

            if(!ItemDatabase.TryGetItemData(id, out ItemData item))
            {
                Debug.LogWarning("[Inventory] Add fail, can't recognize item " + id);
                return false;
            }
            
            bool result = TryAdd(new ItemStack(item, count), out remainder);
            return result;
        }

        public bool TryAdd(ItemData item, int count, out int remainder)
        {
            bool result = TryAdd(new ItemStack(item, count), out remainder);
            return result;
        }

        public bool TryForceRemove(ItemStack set, out int remainder)
        {
            //remove set even if insufficient
            remainder = set.Count;
            int remainderBef = remainder;
            if(set.Count <= 0) return true;
            if (Count(set.Item) <= 0) return false;

            List<InventorySlot> matchSlots 
                = Slots.Where(s => s.HasItem(set.Item)).ToList();

            foreach (var slot in matchSlots)
            {
                TryRemoveFromSlot(new ItemStack(set.Item, remainder), slot, ref remainder);
                if (remainder == 0) break;
            }
                

            return true;
        }

        public bool TryRemove(ItemStack set, out int remainder)
        {
            //remove set only when sufficient
            if (Count(set.Item) < set.Count)
            {
                remainder = set.Count;

                string msg = $"尚未達成任務需求";
                Locator.NotificationUI.PrintInventoryMsg(msg);

                return false;
            }

            TryForceRemove(set, out int newRemainder);
            remainder = newRemainder;
            return true;
        }

        public bool TryRemoveFromSlot(ItemStack stack, InventorySlot slot ,ref int remainder)
        {
            
            if (slot == null || slot.stack.IsEmpty) return false;
    
            if(!slot.TryAtomicRemove(stack, out int newRemainder)) return false;

            OnItemRemove?.Invoke(new ItemStack(stack.Item, remainder - newRemainder));
            
            if (newRemainder >= 0)
            {
                remainder = newRemainder;
                return true;
            }
            else
            {
                Debug.LogWarning("[Inventory] Remainder is negative");
                return false;
            }
        }

        public bool TryRemove(string id, int count, out int remainder)
        {
            bool hasRemoved;

            hasRemoved = TryRemove(new ItemStack(id, count), out var newRemainder);
            remainder = newRemainder;
            return hasRemoved;
        }
        public bool TryRemove(ItemData item, int count, out int remainder)
        {
            bool hasRemoved;
            hasRemoved = TryRemove(new ItemStack(item, count), out var newRemainder);
            remainder = newRemainder;
            return hasRemoved;
        }    
    }
}