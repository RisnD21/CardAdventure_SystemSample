using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using QuestDialogueSystem;

public class SelectionPanelView : MonoBehaviour
{
    private const int MaxGearSlots = 16;
    private const int MaxCardSlotsPerRow = 3;

    [Header("Components")]
    [SerializeField] GameObject SelectionPanel;
    [SerializeField] RectTransform GearSlotsParent;
    [SerializeField] GearSlotView GearSlotPrefab;
    [SerializeField] Image gearPortrait;
    [SerializeField] Image descImg;
    [SerializeField] RectTransform normalCardsParent;
    [SerializeField] RectTransform specialCardsParent;
    [SerializeField] CanvasGroup cg;
    [SerializeField] float fadeOutDuration = 0.15f;
    [SerializeField] float fadeInDuration = 0.15f;



    [Header("Overview Switching")]
    [SerializeField] float switchingDuration = 0.5f;
    [SerializeField] RectTransform selectionPanelTransform;
    [SerializeField] Toggle gearPanelToggler;
    [SerializeField] RectTransform specialCardPanel;
    [SerializeField] Toggle specialCardPanelToggler;


    [SerializeField] ArmouryManager ArmouryManager;

    [Header("Prefabs")]
    [SerializeField] CardUnitView CardUnitViewPrefab;

    GearSlotType currentSlotType;
    GearSO currentGearSO;

    // 預建 16 個 GearSlotView，之後只更新內容 / 顯示狀態
    readonly List<GearSlotView> gearSlotViews = new();
    CardUnitView[] normalCardViews = new CardUnitView[MaxCardSlotsPerRow];
    CardUnitView[] specialCardViews = new CardUnitView[MaxCardSlotsPerRow];

    bool isInDetailView;

    void Awake()
    {
        InitializeGearSlots();
        InitializeCardViews();
    }

    #region LifeCycle

    void OnEnable()
    {
        GearSlotView.OnSlotClicked += HandleSlotClicked;
        GearSlotView.OnHover += HandleSlotHover;
        Narrator.Instance.TryPlayForScene(SceneId.ArmourySelection);
    }

    void OnDisable()
    {
        GearSlotView.OnSlotClicked -= HandleSlotClicked;
        GearSlotView.OnHover -= HandleSlotHover;
        ResetPanelPosition();
        Narrator.Instance.TryPlayForScene(SceneId.ArmourySelection);
    }

    #endregion

    #region Initialize

    void InitializeGearSlots()
    {
        gearSlotViews.Clear();
        ClearChildren(GearSlotsParent);

        for (int i = 0; i < MaxGearSlots; i++)
        {
            var view = Instantiate(GearSlotPrefab, GearSlotsParent);
            view.ChangeBorderStyle(GearSlotStyle.Square);
            view.ChangeState(GearSlotState.Hide);
            gearSlotViews.Add(view);
        }
    }

    void InitializeCardViews()
    {
        ClearChildren(normalCardsParent);
        ClearChildren(specialCardsParent);

        for (int i = 0; i < MaxCardSlotsPerRow; i++)
        {
            var normal = Instantiate(CardUnitViewPrefab, normalCardsParent);
            normal.transform.SetSiblingIndex(i);
            normal.SetCard(null);
            normalCardViews[i] = normal;

            var special = Instantiate(CardUnitViewPrefab, specialCardsParent);
            special.transform.SetSiblingIndex(i);
            special.SetCard(null);
            specialCardViews[i] = special;
        }
    }


    #endregion

    #region Public API

    public void SetcurrentSlotType(GearSlotType type)
    {
        currentSlotType = type;
    }

    /// <summary>
    /// 對外呼叫：顯示指定 slotType + GearType 的可選裝備
    /// </summary>
    public void ShowGears(GearSlotType slotType, GearType type, GearSO preselect)
    {
        currentSlotType = slotType;

        var list = ArmouryManager.AvaiListByType(slotType, type);
        UpdateGearSlots(list, preselect);

        SelectionPanel.SetActive(true);
    }

    /// <summary>
    /// 對外呼叫：強制更新描述 / 圖片 / 卡片
    /// </summary>
    public void UpdateInfo(GearSO gear)
    {
        currentGearSO = gear;

        if (gear == null || gear.itemName == "") 
        {
            ClearGearInfo();
            return;
        }

        descImg.enabled = true;
        descImg.sprite = gear.Description;
        gearPortrait.enabled = true;
        gearPortrait.sprite = gear.Portrait;

        UpdateNormalCardViews(gear);
        UpdateSpecialCardViews(gear);
    }

    //at start, both toggler is in isOn state
    public void ToOverview(bool toOverview)
    {
        if (toOverview) SwitchToOverview();
        else SwitchToDetailView();
    }

    public void SwitchToDetailView()
    {
        if(isInDetailView) return;

        float selectionPanelshiftX = -935;
        float cardPanelshiftX = 533;

        selectionPanelTransform.DOAnchorPos(
            selectionPanelTransform.anchoredPosition + new Vector2(selectionPanelshiftX, 0),
            switchingDuration
        ).SetEase(Ease.OutCubic);

        specialCardPanel.DOAnchorPos(
            specialCardPanel.anchoredPosition + new Vector2(cardPanelshiftX, 0),
            switchingDuration
        ).SetEase(Ease.OutCubic);

        gearPanelToggler.isOn = false;
        specialCardPanelToggler.isOn = false;

        isInDetailView = true;
    }


    public void SwitchToOverview()
    {
        if (!isInDetailView) return;

        float shiftX = 935f;
        float cardPanelshiftX = -533;

        selectionPanelTransform.DOAnchorPos(
            selectionPanelTransform.anchoredPosition + new Vector2(shiftX, 0),
            switchingDuration
        ).SetEase(Ease.OutCubic);

        specialCardPanel.DOAnchorPos(
            specialCardPanel.anchoredPosition + new Vector2(cardPanelshiftX, 0),
            switchingDuration
        ).SetEase(Ease.OutCubic);

        gearPanelToggler.isOn = true;
        specialCardPanelToggler.isOn = true;

        isInDetailView = false;
    }

    void ResetPanelPosition()
    {
        gearPanelToggler.isOn = true;
        specialCardPanelToggler.isOn = true;

        selectionPanelTransform.DOKill();
        selectionPanelTransform.anchoredPosition = new (385, 0);

        specialCardPanel.DOKill();
        specialCardPanel.anchoredPosition = new Vector2(115, -308.23f);
    }

    public void FadeOut()
    {
        if (cg == null) return;
        cg.DOFade(0f, fadeOutDuration).SetEase(Ease.OutQuad)
        .OnComplete(()=>gameObject.SetActive(false));        
    }

    public void FadeIn()
    {
        if (cg == null) return;
        cg.alpha = 0;
        gameObject.SetActive(true);
        cg.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
    }

    #endregion

    #region Slots 更新邏輯
    void HandleSlotClicked(GearSlotType slotType, GearSO gearSO)
    {
        UpdateInfo(gearSO);
        SwitchToDetailView();
    }

    void HandleSlotHover(GearSO gearSO)
    {
        if(currentGearSO == gearSO || isInDetailView) return;
        UpdateInfo(gearSO);
    }


    /// <summary>
    /// 依據 list 更新 16 格 GearSlot（不足的用 Hide/空）
    /// </summary>
    void UpdateGearSlots(List<GearSO> list, GearSO preselect)
    {
        if (gearSlotViews.Count == 0) InitializeGearSlots();

        int count = list.Count;
        for (int i = 0; i < gearSlotViews.Count; i++)
        {
            var view = gearSlotViews[i];

            if (i < count)
            {
                var gear = list[i];

                view.SetSlotType(currentSlotType);
                view.SetSlot(gear);
                view.ChangeState(GearSlotState.Equipped); // 或其他你定義的可選狀態

                // 預選邏輯
                if (gear == preselect ||
                    (preselect == null && gear.itemName == "Unequipped"))
                {
                    view.OnClick();
                }
            }
            else
            {
                view.SetSlot(null);
                view.ChangeState(GearSlotState.Hide);
            }

            view.transform.SetSiblingIndex(i);
        }
    }

    void ClearGearInfo()
    {
        descImg.enabled = false;
        gearPortrait.enabled = false;

        UpdateNormalCardViews(null);
        UpdateSpecialCardViews(null);
    }

    #endregion

    #region CardView 更新邏輯

    void UpdateNormalCardViews(GearSO gearSO)
    {
        var entries = (gearSO != null) ? gearSO.normalCards : null;
        UpdateCardViewsInternal(normalCardViews, entries);
    }

    void UpdateSpecialCardViews(GearSO gearSO)
    {
        var entries = (gearSO != null) ? gearSO.specialCards : null;
        UpdateCardViewsInternal(specialCardViews, entries);
    }

    /// <summary>
    /// 共用：更新某一排卡片（最多 3 格，不足的填 SetCard(null)）
    /// </summary>
    void UpdateCardViewsInternal(CardUnitView[] views, List<CardEntry> entries)
    {
        int count = (entries != null) ? entries.Count : 0;

        for (int i = 0; i < MaxCardSlotsPerRow; i++)
        {
            
            if (i < count)
            {
                views[i].SetCard(entries[i]);
                Debug.Log($"[SelectionPanelView] Updating CardFace: " + entries[i].Card.cardId);
            }
            else
            {
                // 固定 3 格，沒有卡就顯示空
                views[i].SetCard(null);
            }
        }
    }


    #endregion

    #region Utils

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                GameObject.DestroyImmediate(child.gameObject);
        }
    }
    #endregion
}
