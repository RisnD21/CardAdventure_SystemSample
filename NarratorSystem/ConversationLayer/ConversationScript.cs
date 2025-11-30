using System.Collections.Generic;
using UnityEngine;

namespace QuestDialogueSystem
{
    [CreateAssetMenu(menuName = "GameJam/Conversation Script")]
    public class ConversationScript : ScriptableObject
    {
        public string ScriptId;
        public List<ConversationPiece> pieces = new();
        Dictionary<string, ConversationPiece> index = new();

        void OnEnable()
        {
            InitializeDictionary();
        }

        void InitializeDictionary()
        {
            index.Clear();

            foreach (var piece in pieces)
            {
                if(piece != null && !string.IsNullOrEmpty(piece.id))
                {
                    index[piece.id] = piece;
                }
            }
        }

        public bool ContainID(string id) => index.ContainsKey(id);

        public ConversationPiece GetPieceByID(string id)
        {
            if (ContainID(id))
            {
                return index[id];
            } else {
                Debug.LogWarning("Conversation Piece ID not fuond, creating empty Conversation Piece");
                return new ConversationPiece();
            }
        }

        public void RemovePieceByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            if (index.TryGetValue(id, out var piece))
            {
                pieces.Remove(piece);
                index.Remove(id);
            }
        }

        public void AddPiece(string id, ConversationPiece piece)
        {
            pieces.Add(piece);
            index[id] = piece;
        }

        public ConversationPiece GetFirstPiece()
        {
            if (pieces.Count > 0) return pieces[0];
            else
            {
                Debug.LogWarning("[Script] pieces is empty");
                return new ConversationPiece();
            }
        }
    }
}

