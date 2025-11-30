using UnityEngine;
using UnityEngine.Events;

namespace QuestDialogueSystem
{
    public abstract class ScriptableEventListener<T> : MonoBehaviour
    {
        [SerializeField] protected ScriptableEvent<T> channel;
        [SerializeField] protected UnityEvent<T> onEventRaised;

        void OnEnable()
        {
            if(channel != null) channel.Register(OnEventRaised);
        }

        void OnDisable()
        {
            if(channel != null) channel.UnRegister(OnEventRaised);
        }

        protected void OnEventRaised(T value)
        {
            onEventRaised?.Invoke(value);
        }
    }

    public abstract class ScriptableEventListener : MonoBehaviour
    {
        [SerializeField]protected ScriptableEvent channel;
        [SerializeField]protected UnityEvent onEventRaised;

        void OnEnable()
        {
            if(channel != null) channel.Register(OnEventRaised);
        }

        void OnDisable()
        {
            if(channel != null) channel.UnRegister(OnEventRaised);
        }

        protected void OnEventRaised()
        {
            onEventRaised?.Invoke();
        }
    }
}