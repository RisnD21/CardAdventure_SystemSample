using UnityEngine;
using System.Collections.Generic;

namespace QuestDialogueSystem
{
    public static class ConversationScriptDB
    {
        private const string RES_PATH = "ConversationScripts";
        private static bool _loaded;
        private static readonly List<ConversationScript> _all = new ();
        private static readonly Dictionary<string, ConversationScript> _byId = new();

        static ConversationScriptDB()
        {
            LoadIfNeeded();
        }

        static void LoadIfNeeded()
        {
            if (_loaded) return;
            var rules = Resources.LoadAll<ConversationScript>(RES_PATH);
            _all.AddRange(rules);

            _byId.Clear();
            foreach (var r in _all)
            {
                if (string.IsNullOrEmpty(r.ScriptId))
                {
                    Debug.LogWarning($"[ScriptDB] Rule {r.name} 沒有 ScriptId，查表可能失敗。");
                    continue;
                }
                _byId[r.ScriptId] = r;
            }

            _loaded = true;
            Debug.Log($"[ScriptDB] Loaded {_all.Count} scripts from Resources/{RES_PATH}");
        }

        public static ConversationScript GetById(string ScriptId)
        {
            LoadIfNeeded();
            return string.IsNullOrEmpty(ScriptId) ? null :
                (_byId.TryGetValue(ScriptId, out var r) ? r : null);
        }
    }
}