using UnityEngine;

namespace QuestDialogueSystem
{
    [CreateAssetMenu(menuName = "Narration/Rule", fileName = "Rule_")]
    public class NarrativeRule : ScriptableObject
    {
        [Header("ID")]
        public string RuleID;
        public ConversationScript Script;

        [Header("Condition")]
        public SceneId SceneId;    // 場景

        [Header("Rule")]
        public int Priority = 0;   // 數值越大，越優先
        public bool HasPlayed = false; // 是否已播過
        public bool NeedPlay = false;  // 是否被標記「下次條件達成就需播放」
        public bool PlayOnce = true;   // 只播放一次

        // 之後如需更進階的條件（旗標、數值、任務階段），可在此擴充
    }
}