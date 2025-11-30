using UnityEngine;

public abstract class CardSO : ScriptableObject 
{
    public string cardId;          // 唯一ID，存檔用
    public string title;
    public string description;
    public int cost;              
    public Sprite artwork;         // 一張圖搞定
}