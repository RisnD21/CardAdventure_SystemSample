using UnityEngine;

namespace QuestDialogueSystem
{
    [CreateAssetMenu(fileName = "NewChangeSceneEvent", menuName = "Narration/Event")]
    public class ChangeSceneEvent : ScriptableEvent<SceneId>
    {
        [SerializeField] SceneId scene;
        public void Raised()
        {
            Raised(scene);
        }
    }
}