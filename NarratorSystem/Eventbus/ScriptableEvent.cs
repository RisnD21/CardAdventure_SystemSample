using UnityEngine;
using System;

namespace QuestDialogueSystem
{
    public abstract class ScriptableEventBase : ScriptableObject{};

    public abstract class ScriptableEvent<T> : ScriptableObject
    {
        protected event Action<T> Listeners;

        public void Raised(T value)
        {
            Listeners?.Invoke(value);
        }

        public void Register(Action<T> listener) 
        {
            Listeners += listener;
        }

        public void UnRegister(Action<T> listener)
        {
            Listeners -= listener;
        }
    }

    public abstract class ScriptableEvent : ScriptableObject
    {
        protected event Action Listeners;

        public void Raised()
        {
            Listeners?.Invoke();
        }

        public void Register(Action listener)
        {
            Listeners += listener;
        }

        public void UnRegister(Action listener)
        {
            Listeners -= listener;
        }
    }
}