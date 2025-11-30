
using System.Collections.Generic;
using QuestDialogueSystem;
using UnityEngine;    

[RequireComponent(typeof(IInventory))]
public class ReceiveItemOnStart : MonoBehaviour
{
    public List<ItemStackData> stacks;

    IInventory inventory;

    void Awake()
    {
        inventory = GetComponent<IInventory>();
    }
    public void Initialize()
    {
        foreach (var stack in stacks)
        {
            inventory.TryAdd(new ItemStack(stack.item, stack.count), out _);
        }
    }
}