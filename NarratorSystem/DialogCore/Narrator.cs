using UnityEngine;

namespace QuestDialogueSystem
{
    public class Narrator : MonoBehaviour
    {
        public static Narrator Instance { get; private set; }

        [SerializeField] DialogRunner dialogRunner;

        bool isStoryMode;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 觸發靜態載入（不需要回傳），啟動就先確保 DB 已載好
            _ = RuleDB.GetById("");
            _ = ConversationScriptDB.GetById("");
        }

        public void EnterStoryMode()
        {
            isStoryMode = true;
        }

        public bool TryPlayForScene(SceneId sceneId)
        {
            if (!isStoryMode) return false;
            
            Debug.Log("[Narrator] Searching Scripts for sceneID: " + sceneId);
            var rule = RuleDB.GetNext(sceneId);
            if (rule == null) return false;

            Debug.Log("[Narrator] Starting conversation: " + rule.Script.ScriptId);
            StartConversation(rule.Script.ScriptId);

            RuleDB.MarkPlayed(rule);
            return true;
        }

        void StartConversation(string ScriptId)
        {
            if (string.IsNullOrEmpty(ScriptId))
            {
                Debug.LogWarning("[Narrator] ScriptId 為空。");
                return;
            }

            var script = ConversationScriptDB.GetById(ScriptId);
            if(script == null) Debug.LogError("[Narrotor] script is null");

            dialogRunner.StartConversation(script);
        }
    }
}
