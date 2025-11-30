using UnityEngine;

namespace QuestDialogueSystem
{
    public class ChangeSceneEventListener : ScriptableEventListener<SceneId>
    {
        protected void OnEventRaised(SceneId scene)
        {
            base.OnEventRaised(scene);
            Debug.Log("[ChangeSceneEvtListener] ChangingSceneTo " + scene);
            Narrator.Instance.TryPlayForScene(scene);
        }
    }
}