using UnityEngine;
using System.Collections.Generic;

namespace QuestDialogueSystem
{
    public static class ItemDatabase
    {
        static Dictionary<string, ItemData> itemMap = new();

        // ★ 改成用 Attunement
        static Dictionary<Attunement, List<ItemData>> attunementMap = new();

        public static List<string> itemList = new();

        static bool isDebugMode = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitOnStart()
        {
            Initialize();
        }

        public static void Initialize()
        {
            ResetDictionary();
            LoadAll();
        }

        static void LoadAll()
        {
            var items = Resources.LoadAll<ItemData>("Items");
            foreach (var item in items)
            {
                if (!itemMap.ContainsKey(item.itemID))
                {
                    itemList.Add(item.itemID);
                    itemMap[item.itemID] = item;

                    if (isDebugMode)
                        Debug.Log($"[ItemDatabase] register item: {item.itemID} {item.itemName}");

                    // --- ★ 用 Attunement 作為 Key ---
                    if (!attunementMap.TryGetValue(item.Attunement, out var list))
                    {
                        list = new();
                        attunementMap[item.Attunement] = list;
                    }
                    list.Add(item);
                }
                else
                {
                    Debug.LogWarning("[ItemDatabase] Duplicate Item ID detected " + item.itemID);
                }
            }
        }

        static void ResetDictionary()
        {
            itemMap.Clear();
            attunementMap.Clear();
        }

        public static bool TryGetItemData(string id, out ItemData itemData)
        {
            if (itemMap.ContainsKey(id))
            {
                itemData = itemMap[id];
                return true;
            }
            else
            {
                itemData = null;
                Debug.LogWarning("[ItemDatabase] ID Invalid");
                return false;
            }
        }

        // --- ★ 新的查詢方法 ---
        public static bool TryGetItemsByAttunement(Attunement attune, out List<ItemData> itemDatas)
        {
            if (attunementMap.ContainsKey(attune))
            {
                itemDatas = attunementMap[attune];
                return true;
            }
            else
            {
                itemDatas = null;
                Debug.LogWarning("[ItemDatabase] Attunement Invalid");
                return false;
            }
        }
    }
}
