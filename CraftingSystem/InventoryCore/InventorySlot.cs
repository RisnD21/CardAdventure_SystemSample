using System.Collections;
using UnityEngine;
using System;


namespace QuestDialogueSystem
{
    public class InventorySlot : ISlottable
    {
        public event Action<ISlottable> OnInteract;
        public ItemStack stack = ItemStack.Empty;
        public int slotMaxStack = 999; //(default)

        public void BeingInteract() => OnInteract?.Invoke(this);

        ItemStack Clone(ItemStack subject) => new (subject.Item, subject.Count);

        public bool IsEmpty => stack.IsEmpty;
            
        public int SpaceLeft(ItemData item)
        {
            if (IsEmpty) return Mathf.Min(item.maxStack, slotMaxStack);
            else if (!HasItem(item) || IsFull) return 0;
            return Mathf.Min(item.maxStack, slotMaxStack) - stack.Count;
        }

        public bool IsFull => stack.Count == Mathf.Min(stack.Max, slotMaxStack);

        public bool HasItem(ItemData item)
        {
            if (IsEmpty) return false;
            return stack.Item.Equals(item);
        }

        public bool CanStackWith(ItemData other) 
            => IsEmpty || (!IsFull && other.Equals(stack.Item));

        public bool TryAtomicAdd(ItemStack other, out int remainder)
        {
            var stackBackup = Clone(stack);

            if (TryAddStack(other, out remainder))
            {
                return true;
            }

            stack = stackBackup;
            return false;
        }

        bool TryAddStack(ItemStack other, out int remainder)
        {
            remainder = other.Count;

            if (!CanStackWith(other.Item)) return false;

            int toAdd = Mathf.Min(SpaceLeft(other.Item), remainder);

            stack = other.WithCount(stack.Count + toAdd);
            remainder -= toAdd;

            return true;
        }

        public bool TryAtomicRemove(ItemStack other, out int remainder)
        {
            var stackBackup = Clone(stack);
            remainder = other.Count;

            if (TryRemoveStack(other, out int newRemainder))
            {
                remainder = newRemainder;
                return true;
            }
            
            stack = stackBackup;
            return false;
        }

        bool TryRemoveStack(ItemStack other, out int remainder)
        {
            remainder = other.Count;

            if (!HasItem(other.Item)) return false;

            int toRemove = Mathf.Min(stack.Count, other.Count);
            
            remainder -= toRemove;

            stack = other.WithCount(stack.Count - toRemove);

            if(IsEmpty) stack = ItemStack.Empty;

            return true;
        }

        public override string ToString()
        {
            if(stack.Item == null) return "Empty Slot";
            return $"{stack.Item.itemName} {stack.Count}/{Mathf.Min(stack.Max, slotMaxStack)}";
        }

        public static explicit operator InventorySlot(SlottableSlot v)
        {
            throw new NotImplementedException();
        }
    }
}