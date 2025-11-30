using System;
using UnityEngine.Events;

namespace QuestDialogueSystem
{
    [Serializable]
    public class ConversationOption
    {
        public string text;
        public string targetID;
        public UnityEvent onSelected; 
    }
}

