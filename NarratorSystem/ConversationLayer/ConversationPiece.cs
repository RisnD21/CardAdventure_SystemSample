using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuestDialogueSystem
{
    [Serializable]
    public class ConversationPiece
    {
        public string id;
        public Sprite portrait;
        public string portraitDescription;
        public Sprite background;
        public string text;
        public AudioClip audio;
        public string NextPiece;
        public List<ConversationOption> options;

        public override bool Equals(object obj)
        {
            if (obj is ConversationPiece other)
            {
                return id == other.id;
            }
            else return false;
        }
        public override int GetHashCode() => id.GetHashCode();
    }
}

