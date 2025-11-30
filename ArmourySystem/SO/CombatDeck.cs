using UnityEngine;

//存擋用
[CreateAssetMenu(fileName = "New Combat Deck", menuName = "Scriptable Objects/Card/Combat Deck")]
public class CombatDeck : ScriptableObject 
{
    public CombatCardSO[] Pool;
}