using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using QuestDialogueSystem;

public class CraftingResultPanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;          
    [SerializeField] private GameObject blocker;       

    [Header("UI Components")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text itemCountText;
    [SerializeField] private TooltipHoverHandler itemDescription;
    [SerializeField] private TMP_Text newRecipeText;
    [SerializeField] private Button proceedButton;

    [Header("Animation")]
    [SerializeField] private Transform panelTransform;   // 動畫用的 panel 根節點
    [SerializeField] private Transform proceedTransform; // 動畫用的 Proceed 節點
    [SerializeField] private float panelPopDuration = 0.25f;
    [SerializeField] private float proceedDelay = 0.15f;
    [SerializeField] private float proceedPopDuration = 0.18f;
    [SerializeField] private float proceedPunchScale = 1.15f;

    private Vector3 _panelDefaultScale;
    private Vector3 _proceedDefaultScale;
    private Sequence _currentSequence;

    private void Awake()
    {
        if (root != null)
            root.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);

        if (proceedButton != null)
            proceedButton.onClick.AddListener(ClosePanel);

        if (panelTransform == null && root != null)
            panelTransform = root.transform;

        if (proceedTransform == null && proceedButton != null)
            proceedTransform = proceedButton.transform;

        if (panelTransform != null)
            _panelDefaultScale = panelTransform.localScale == Vector3.zero ? Vector3.one : panelTransform.localScale;

        if (proceedTransform != null)
            _proceedDefaultScale = proceedTransform.localScale == Vector3.zero ? Vector3.one : proceedTransform.localScale;
    }

    private void OnDisable()
    {
        KillSequence();
    }

    private void KillSequence()
    {
        if (_currentSequence != null && _currentSequence.IsActive())
        {
            _currentSequence.Kill();
            _currentSequence = null;
        }
    }

    public void Open(ItemStack itemStack, bool isNewRecipe)
    {
        KillSequence();

        if (root != null)
            root.SetActive(true);

        if (blocker != null)
            blocker.SetActive(true);

        if (titleText != null)
            titleText.text = "Item Crafted!";

        itemIconImage.sprite = itemStack.Item.itemIcon;
        itemCountText.text = itemStack.Count.ToString();
        itemDescription.SetTooltip(itemStack.Item.description);

        if (newRecipeText != null)
            newRecipeText.gameObject.SetActive(isNewRecipe);

        // === 動畫初始狀態 ===
        if (panelTransform != null)
            panelTransform.localScale = Vector3.zero;

        if (proceedTransform != null)
            proceedTransform.localScale = Vector3.zero;

        if (proceedButton != null)
            proceedButton.interactable = false;

        // === DOTween Sequence ===
        _currentSequence = DOTween.Sequence();

        // 1. Panel 由小變大彈出
        if (panelTransform != null)
        {
            _currentSequence.Append(
                panelTransform.DOScale(_panelDefaultScale, panelPopDuration)
                              .SetEase(Ease.OutBack)
            );
        }

        // 2. 等一點點時間再讓 Proceed Button 出現 + 小彈一下
        if (proceedTransform != null)
        {
            _currentSequence.AppendInterval(proceedDelay);

            _currentSequence.Append(
                proceedTransform.DOScale(_proceedDefaultScale * proceedPunchScale, proceedPopDuration)
                                .SetEase(Ease.OutBack)
            );

            // 回到正常大小（避免永遠比原本大）
            _currentSequence.Append(
                proceedTransform.DOScale(_proceedDefaultScale, 0.08f)
            );
        }

        // 3. 最後再開啟互動
        _currentSequence.OnComplete(() =>
        {
            if (proceedButton != null)
                proceedButton.interactable = true;
        });
    }

    public void ClosePanel()
    {
        KillSequence();

        if (root != null)
            root.SetActive(false);

        if (blocker != null)
            blocker.SetActive(false);
    }
}
