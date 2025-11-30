using System;
using System.Collections.Generic;

namespace QuestDialogueSystem
{
    public interface IInventory
    {
        IReadOnlyList<InventorySlot> Slots{get;}

        void PrintAllSlots();

        void Initialize();
        int Count(string id);
        int Count(ItemData item);
        bool TryAdd(string id, int count, out int Remainder);
        bool TryAdd(ItemData item, int count, out int Remainder);
        bool TryAdd(ItemStack set, out int Remainder);

        bool TryRemove(string id, int count, out int Remainder);
        bool TryRemove(ItemData item, int count, out int Remainder);
        bool TryRemove(ItemStack set, out int Remainder);

        bool TryRemoveFromSlot(ItemStack stack, InventorySlot slot ,ref int remainder);

        event Action<ItemStack> OnItemAdd;
        event Action<ItemStack> OnItemRemove;
    }
}