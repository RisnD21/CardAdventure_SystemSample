using UnityEngine;

namespace QuestDialogueSystem
{
    [CreateAssetMenu(menuName = "GameJam/OnTrigger/Remove Item")]
    public class RemoveItemOnTrigger : ScriptableObject
    {
        public ItemStackData stack;
        public void OnTrigger()
        {
            Locator.Inventory.TryRemove(stack.item, stack.count, out int _);
        }   
    }
}