using UnityEngine;

namespace QuestDialogueSystem
{
    [CreateAssetMenu(menuName = "GameJam/OnTrigger/Print Msg")]
    public class PrintMsgOnTrigger : ScriptableObject
    {
        [TextArea(1,7)]
        [SerializeField] string msg;
        public void OnTrigger()
        {
            Locator.NotificationUI.PrintInventoryMsg(msg);
        }   
    }
}