using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuestDialogueSystem
{
    /// <summary>
    /// 輕量 Rule DB：
    /// - 從 Resources/Rules 載入所有 NarrativeRule
    /// - 提供查詢與標記 API
    /// </summary>
    public static class RuleDB
    {
        // load rules from：Resources/Rules
        private const string RES_PATH = "Rules";

        private static bool _loaded;
        private static readonly List<NarrativeRule> _all = new List<NarrativeRule>();
        private static readonly Dictionary<string, NarrativeRule> _byId = new Dictionary<string, NarrativeRule>();

        static RuleDB()
        {
            LoadIfNeeded();
        }

        static void LoadIfNeeded()
        {
            if (_loaded) return;

            var defs = Resources.LoadAll<NarrativeRule>(RES_PATH);

            _all.Clear();
            _byId.Clear();

            foreach (var def in defs)
            {
                // 建 runtime 拷貝，避免動到資產
                var inst = ScriptableObject.Instantiate(def);
                inst.name = def.name + " (Runtime)";
        #if UNITY_EDITOR
                inst.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.DontSave;
        #else
                inst.hideFlags = HideFlags.DontSave;
        #endif
                _all.Add(inst);

                if (!string.IsNullOrEmpty(inst.RuleID))
                    _byId[inst.RuleID] = inst;
                else
                    Debug.LogWarning($"[RuleDB] Rule {def.name} 沒有 RuleID，查表可能失敗。");
            }

            _loaded = true;
            Debug.Log($"[RuleDB] Loaded {_all.Count} runtime rules from Resources/{RES_PATH}");
        }


        public static NarrativeRule GetById(string ruleId)
        {
            LoadIfNeeded();
            return string.IsNullOrEmpty(ruleId) ? null :
                (_byId.TryGetValue(ruleId, out var r) ? r : null);
        }

        /// <summary>
        /// 依條件抓候選，並依 Priority 排序（高→低），過濾掉 PlayOnce 且已播過的。
        /// NeedPlay 若為 true 可視為加權或直接優先（此處採「先回傳 NeedPlay 的、再回傳一般的」）。
        /// priority 越大者越優先
        /// </summary>
        public static NarrativeRule GetNext(SceneId sceneID)
        {
            LoadIfNeeded();

            var candidates = _all.Where(r =>
                r.SceneId == sceneID &&
                (!r.PlayOnce || !r.HasPlayed)
            );

            // 先取 NeedPlay，再按 Priority
            var needPlayPick = candidates.Where(r => r.NeedPlay)
                                         .OrderByDescending(r => r.Priority)
                                         .FirstOrDefault();
            if (needPlayPick != null) return needPlayPick;

            return candidates.OrderByDescending(r => r.Priority)
                             .FirstOrDefault();
        }

        /// <summary>把某 Rule 標成 NeedPlay（或取消）。</summary>
        public static void SetNeedPlay(string ruleId, bool need)
        {
            var r = GetById(ruleId);
            if (r == null) return;
            r.NeedPlay = need;
        }

        /// <summary>播放後標記。</summary>
        public static void MarkPlayed(NarrativeRule rule)
        {
            if (rule == null) return;
            rule.HasPlayed = true;
            rule.NeedPlay = false;
        }
        public static void MarkPlayed(string id) => MarkPlayed(GetById(id));
    }
}
