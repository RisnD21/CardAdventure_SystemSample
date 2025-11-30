using UnityEngine;

namespace QuestDialogueSystem
{
    [CreateAssetMenu(menuName = "GameJam/OnTrigger/Receive Item")]
    public class ReceiveItemOnTrigger : ScriptableObject, ITriggerable
    {
        public ItemStackData stack;
        public void OnTrigger()
        {
            Locator.Inventory.TryAdd(stack.item, stack.count, out int _);
        }
    }
}