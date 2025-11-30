using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GearSlotView : MonoBehaviour, IPointerEnterHandler
{
    GearSlotType gearSlotType = GearSlotType.None;

    [Header("Ref")]
    [SerializeField] Button button;
    [SerializeField] HoverScaleUI hoverScaleUI;

    [Header("Settings")]
    [SerializeField] Color defaultColor = new(1,1,1,0.5f);
    [SerializeField] Sprite defaultIconSprite;
    [SerializeField] float defaultIconScale = 1.4f;
    [SerializeField] Color equippedColor = new(1,1,1,1);
    [SerializeField] float equippedIconScale = 1.4f;
    [SerializeField] Color lockedColor = new(1,1,1,0.5f);
    [SerializeField] Sprite lockedIconSprite;
    [SerializeField] float lockedIconScale = 1f;
    [SerializeField] Color hideColor = new(1,1,1,0.25f);

    [SerializeField] Sprite squareBorderSprite;
    [SerializeField] Sprite diamondBorderSprite;

    [Header("Bindings")]
    [SerializeField] Image gearIconImg;
    [SerializeField] Image borderImg;

    [Header("States")]
    public GearSO CurrentGear { get; private set; }
    public GearSlotState CurrentState { get; private set; }
    public GearSlotStyle CurrentStyle { get; private set; }

    bool ignoreClick;
    void Awake() => button.onClick.AddListener(OnClick);

    public static event Action<GearSlotType, GearSO> OnSlotClicked;
    public static event Action<GearSO> OnHover;

    public void SetSlot(GearSO gear)
    {
        if (CurrentState == GearSlotState.Locked) return;
        if (gear == null || gear.itemName == "") 
        {
            ChangeState(GearSlotState.Default);
            return;
        }
        ChangeState(GearSlotState.Equipped);

        CurrentGear = gear;
        gearIconImg.sprite = gear.icon;
    }

    public void SetSlotType(GearSlotType type) => gearSlotType = type;
    public GearSO GetCurrentGear() => CurrentGear;

    public void OnClick()
    {
        if (ignoreClick) return;
        OnSlotClicked?.Invoke(gearSlotType, CurrentGear);
    }

    public void ChangeState(GearSlotState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GearSlotState.Default:
                borderImg.color = defaultColor;

                gearIconImg.sprite = defaultIconSprite;
                gearIconImg.transform.localScale = Vector3.one * defaultIconScale;

                hoverScaleUI.IgnoreEvent = false;
                ignoreClick = false;
                break;

            case GearSlotState.Equipped:
                borderImg.color = equippedColor;

                gearIconImg.transform.localScale = Vector3.one * equippedIconScale;

                hoverScaleUI.IgnoreEvent = false;
                ignoreClick = false;
                break;

            case GearSlotState.Locked:
                borderImg.color = lockedColor;

                gearIconImg.sprite = lockedIconSprite;
                gearIconImg.transform.localScale = Vector3.one * lockedIconScale;

                hoverScaleUI.IgnoreEvent = true;
                ignoreClick = true;
                break;

            case GearSlotState.Hide:
                borderImg.color = hideColor;

                gearIconImg.sprite = defaultIconSprite;
                gearIconImg.transform.localScale = Vector3.one * defaultIconScale;

                hoverScaleUI.IgnoreEvent = true;
                ignoreClick = true;
                break;
        }
    }

    public void ChangeBorderStyle(GearSlotStyle style)
    {
        CurrentStyle = style;
        switch (style)
        {
            case GearSlotStyle.Diamond:
                if (diamondBorderSprite != null)
                    borderImg.sprite = diamondBorderSprite;
                break;

            case GearSlotStyle.Square:
                if (squareBorderSprite != null)
                    borderImg.sprite = squareBorderSprite;
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) 
    {
        OnHover?.Invoke(CurrentGear);
    }
}
