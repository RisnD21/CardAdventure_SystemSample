using System;
using System.Collections.Generic;
using UnityEngine;

public class ArmouryManager : MonoBehaviour
{
    //用於 inspector 中檢查
    [Header("擁有的裝備")]
    [SerializeField] List<GearSO> currentGears = new();
    [SerializeField] List<GearSO> ownGears = new();
    public Dictionary<GearSlotType, GearSO> current = new();
    public int CurrentProgression = 1;

    [Header("Reference")]
    public CardUnitView CardUnitViewPrefab;

    [SerializeField] UIFadeEffects ArmouryViewFader;

    [Header("Testing")]
    [SerializeField] List<GearSO> GearsToAdd = new();
    bool isDebugMode = false;


    void Awake()
    {
        foreach (GearSlotType s in Enum.GetValues(typeof(GearSlotType)))
        {
            if (!current.ContainsKey(s)) 
            {
                current[s] = null;
                if(isDebugMode) Debug.Log($"[ArmouryManager] 初始化槽位 {s} 為 null");
            }
        }
    }

    void OnEnable()
    {   
        if(isDebugMode) Debug.Log("[ArmouryManager] OnEnable, 訂閱 OnSlotClicked");
        GearSlotView.OnSlotClicked += SetCurrent;
    }

    void OnDisable()
    {
        if(isDebugMode) Debug.Log("[ArmouryManager] OnDisable, 取消訂閱 OnSlotClicked");
        GearSlotView.OnSlotClicked -= SetCurrent;
    }


    public void AddGear(GearSO gear)
    {
        if (gear && !ownGears.Contains(gear)) 
        {
            ownGears.Add(gear);
            if(isDebugMode) Debug.Log($"[ArmouryManager] 已新增裝備: {gear.name}");
        }
        else
        {
            if(isDebugMode) Debug.Log($"[ArmouryManager] 嘗試新增裝備失敗: {(gear ? gear.name : "null")} (可能已存在或為 null)");
        }
    }

    public void RemoveGear(GearSO gear)
    {
        if (!gear) 
        {
            if(isDebugMode) Debug.Log("[ArmouryManager] RemoveGear 被呼叫，但傳入 gear 為 null");
            return;
        }

        if (ownGears.Remove(gear))
        {
            if(isDebugMode) Debug.Log($"[ArmouryManager] 移除裝備: {gear.name}");
        }

        // 如果移除的是當前裝備，清空
        foreach (var kv in new List<GearSlotType>(current.Keys))
        {
            if (current[kv] == gear) 
            {
                SetCurrent(kv, null);
                if(isDebugMode) Debug.Log($"[ArmouryManager] 移除當前裝備 {gear.name}，清空槽位 {kv}");
            }
        }
    }

    public int CountGear(GearType type)
    {
        int c = 0;
        foreach (var g in ownGears) if (g && g.gearType == type) c++;
        if(isDebugMode) Debug.Log($"[ArmouryManager] CountGear 類型 {type} = {c}");
        return c;
    }

    public void SetCurrent(GearSlotType slot, GearSO gear)
    {
        current[slot] = gear;
        if(isDebugMode) Debug.Log($"[ArmouryManager] 設定當前裝備: 槽位 {slot} = {(gear ? gear.name : "null")}");

    }

    public List<GearSO> AvaiListByType(GearSlotType gearSlot, GearType type)
    {
        foreach(var gear in currentGears) if(gear != null) Debug.Log(gear.itemName);

        var list = new List<GearSO>();
        foreach (var g in ownGears) if (g != null && g.gearType == type) 
        {
            bool isEquipped = false;
            if(g.itemName == current[gearSlot]?.itemName || g.itemName == "Unequipped")
            {
                list.Add(g);
                continue;
            } else foreach (var gear in currentGears) 
            {
                if (g.itemName == gear.itemName)
                {
                    isEquipped = true;
                    break;
                }
            }

            if (!isEquipped) list.Add(g);
        }

        Debug.Log($"[ArmouryManager] ListByType 類型 {type}，共找到 {list.Count} 件裝備");
        return list;
    }

    //assign this to button in scene, on Pressing Armoury Button
    public void ToggleArmouryView(bool on)
    {
        if(isDebugMode) Debug.Log($"[ArmouryManager] ToggleArmouryView: {(on ? "開啟" : "關閉")} ArmouryView");

        // fetch data ref from session data
        current = ResourceManager.Instance.CurrentGears;
        ownGears = ResourceManager.Instance.OwnGears;

        foreach (var gear in GearsToAdd) AddGear(gear);

        if (on) ArmouryViewFader.FadeInFast();
        else ArmouryViewFader.FadeOutFast();
    }

    //assign this to button in scene, on pressing Confirm Button
    public void UpdateCurrentGearsList()
    {
        currentGears.Clear();
        foreach (var entry in current)
        {
            currentGears.Add(entry.Value);
            if(isDebugMode) Debug.Log($"[ArmouryManager] UpdateCurrentGearsList: {entry.Key} = {(entry.Value ? entry.Value.name : "null")}");
        }
    }

    //assign this to button in scene, on pressing Confirm Button
    public void UpdateCombatDeck()
    {
        List<CombatCardSO> combatDeck = new();
        foreach (var gear in currentGears)
        {
            if(gear == null || gear.normalCards.Count == 0) continue;
            foreach (var entry in gear.normalCards)
            {
                for (int i = 0; i < entry.Count; i++ ) combatDeck.Add(entry.Card);
            }
        }
        if(isDebugMode) Debug.Log($"[Armoury Manager] CombaCard Set:");
        foreach(var card in combatDeck)
        {   
            if(isDebugMode) Debug.Log($"[Armoury Manager] {card.title}");
        }

        ResourceManager.Instance.combatDeck = combatDeck;
    }
}