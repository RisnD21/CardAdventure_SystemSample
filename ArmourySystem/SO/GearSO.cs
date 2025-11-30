using System.Collections.Generic;
using UnityEngine;

public abstract class GearSO : ScriptableObject
{
    [Header("基本資料")]
    public string itemName;
    public GearType gearType;
    public Sprite icon;
    public Sprite Portrait;
    public Sprite Description;

    [Header("定義卡牌")]
    public List<CardEntry> normalCards = new();
    public List<CardEntry> specialCards = new();
}