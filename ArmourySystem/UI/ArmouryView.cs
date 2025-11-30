using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArmouryView : MonoBehaviour
{
    [Header("Card Grid")]
    [SerializeField] RectTransform CardParent;
    CardUnitView cardUnitViewPrefab;
    [SerializeField] List<CardUnitView> CardUnitViews = new(); //so we can check in inspector

    [Header("Icons")]
    [SerializeField] GearSlotView Weapon;
    [SerializeField] GearSlotView Offhand;
    [SerializeField] GearSlotView FirstSealedRelic;
    [SerializeField] GearSlotView SecondSealedRelic;
    [SerializeField] GearSlotView ThirdSealedRelic;

    [Header("Components")]
    [SerializeField] TextMeshProUGUI overviewText;
    
    [Header("Reference")]
    [SerializeField] SelectionPanelView selectionPanel;
    [SerializeField] ArmouryManager ArmouryManager;

    Dictionary<GearSlotType, GearSO> current = new();

    void Start()
    {
        Weapon.SetSlotType(GearSlotType.Weapon);
        Offhand.SetSlotType(GearSlotType.Offhand);
        FirstSealedRelic.SetSlotType(GearSlotType.FirstRelic);
        SecondSealedRelic.SetSlotType(GearSlotType.SecondRelic);
        ThirdSealedRelic.SetSlotType(GearSlotType.ThirdRelic);
    }

    void OnEnable()
    {
        GearSlotView.OnSlotClicked += ToggleSelectionPanel;
        cardUnitViewPrefab = ArmouryManager.CardUnitViewPrefab;
        current = ArmouryManager.current;
        UpdateGearSlot();
        UpdateCards(); // 初次刷新
        RefreshIcons();
    }

    void UpdateGearSlot()
    {
        UnlockGearSlot(ArmouryManager.CurrentProgression);
        UpdateView();
    }

    void OnDisable()
    {
        GearSlotView.OnSlotClicked -= ToggleSelectionPanel;
    }

    public void ToggleSelectionPanel(GearSlotType slot, GearSO gearSO)
    {
        if (!selectionPanel || selectionPanel.isActiveAndEnabled) return;

        GearType geartype;
        if(slot == GearSlotType.Weapon || slot == GearSlotType.Offhand) geartype = GearType.Weapon;
        else geartype = GearType.SealedRelic;

        selectionPanel.FadeIn();
        selectionPanel.SetcurrentSlotType(slot);
        selectionPanel.ShowGears(slot, geartype, gearSO);
    }

    public void UpdateCards()
    {
        // 清理舊卡片
        foreach (var oldCard in CardUnitViews)
        {
            if (oldCard != null)
            {
                Destroy(oldCard.gameObject);
            }
        }
        CardUnitViews.Clear();
        int cardCount = 0;

        var currentGears = ArmouryManager.current;
        var collected = new List<CardEntry>();

        foreach (GearSlotType s in System.Enum.GetValues(typeof(GearSlotType)))
        {
            if(!currentGears.TryGetValue(s, out var gear) || gear == null) 
                continue;

            collected.AddRange(currentGears[s].normalCards);
        }

        const int TOTAL_SLOTS = 12;
        int collectedCount = collected.Count;

        for (int i = 0; i < TOTAL_SLOTS; i++)
        {
            var cardView = Instantiate(cardUnitViewPrefab, CardParent);

            if (i < collectedCount)
            {
                var entry = collected[i];
                cardView.SetCard(entry);
                cardCount += entry.Count;
            }
            else cardView.SetCard(null);

            CardUnitViews.Add(cardView);
        }


        string overviewMsg = $"Ability: None"; 
        UpdateOverview(overviewMsg);
    }

    void UpdateOverview(string msg)
    {
        overviewText.text = msg;
    }

    public void RefreshIcons()
    {
        current.TryGetValue(GearSlotType.Weapon, out var weapon);
        Weapon.SetSlot(weapon);

        current.TryGetValue(GearSlotType.Offhand, out var offhand);
        Offhand.SetSlot(offhand);
        current.TryGetValue(GearSlotType.FirstRelic, out var firstRelic);
        FirstSealedRelic.SetSlot(firstRelic);
        current.TryGetValue(GearSlotType.SecondRelic, out var secondRelic);
        SecondSealedRelic.SetSlot(secondRelic);
        current.TryGetValue(GearSlotType.ThirdRelic, out var thirdRelic);
        ThirdSealedRelic.SetSlot(thirdRelic);
    }

    //this is call by select button in selection panel
    public void UpdateView()
    {
        RefreshIcons();
        UpdateCards();
    }

    public void UnlockGearSlot(int count)
    {
        if (count > -1) Weapon.ChangeState(GearSlotState.Default);
        else Weapon.ChangeState(GearSlotState.Locked);
        if (count > 0) Offhand.ChangeState(GearSlotState.Default);
        else Offhand.ChangeState(GearSlotState.Locked);
        if (count > 1) FirstSealedRelic.ChangeState(GearSlotState.Default);
        else FirstSealedRelic.ChangeState(GearSlotState.Locked);
        if (count > 2) SecondSealedRelic.ChangeState(GearSlotState.Default);
        else SecondSealedRelic.ChangeState(GearSlotState.Locked);
        if (count > 3) ThirdSealedRelic.ChangeState(GearSlotState.Default);
        else ThirdSealedRelic.ChangeState(GearSlotState.Locked);
    }
}
