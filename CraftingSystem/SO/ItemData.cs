using UnityEngine;

namespace QuestDialogueSystem
{
    [CreateAssetMenu(menuName = "GameJam/Item/Item")]
    public class ItemData : ScriptableObject
    {
        public string itemID;
        public string itemName;
        public Sprite itemIcon;
        [TextArea(3,5)]
        public string description;
        public int maxStack;
        public Attunement Attunement;
        public bool IsRecipe => Attunement == Attunement.None;
        public ItemData[] Ingredients;
        public ItemData Product;
        public override bool Equals(object obj)
        {            
            if (obj != null && obj is ItemData other)
            {
                return other.itemID == itemID;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            if (itemID == null) return 0;
            return itemID.GetHashCode();
        }

        public override string ToString()
        {
            return itemName;
        }
    }
}