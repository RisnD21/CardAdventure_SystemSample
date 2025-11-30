using System;
using System.Collections.Generic;
using QuestDialogueSystem;
using UnityEngine;

public static class RecipeKeyUtil
{
    /// <summary>
    /// 根據一組 ItemData，產生「順序無關」的配方 key。
    /// 規則：
    /// - 取出每個 item 的 itemID
    /// - 過濾掉 null 或空字串
    /// - 排序 (StringComparer.Ordinal)
    /// - 用 '+' 串起來，例如 "IT001+IT003+IT010"
    /// </summary>
    public static string BuildKeyFromItems(ItemData[] items)
    {
        if (items == null || items.Length == 0)
            return string.Empty;

        List<string> ids = new(items.Length);

        foreach (var item in items)
        {
            if (item == null)
                continue;

            if (string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogWarning($"[RecipeKeyUtil] Item {item.name} has empty itemID.");
                continue;
            }

            ids.Add(item.itemID);
        }

        if (ids.Count == 0)
            return string.Empty;

        // 關鍵：排序 → 同一組材料不管順序怎麼放，key 都一樣
        ids.Sort(StringComparer.Ordinal);

        return string.Join("+", ids);
    }
}
