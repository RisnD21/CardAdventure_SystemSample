using UnityEngine;
using TMPro;
using UnityEngine.UI;
using AudioSystem.SFX;
using Unity.VisualScripting;
using DG.Tweening;
using System.Collections;

namespace QuestDialogueSystem
{
    public class DialogRunner : MonoBehaviour, IDialogueRunner
    {
        [Header("UI Reference")]
        [SerializeField] GameObject dialogPanel;
        CanvasGroup dialogPanelCanvasGroup;
        [SerializeField] TextMeshProUGUI dialogText;
        [SerializeField] Image portraitImage;
        [SerializeField] Sprite defaultPortraitImage;
        [SerializeField] Image backgroundImage;
        [SerializeField] TextMeshProUGUI portraitDescription;

        [SerializeField] Transform optionsParent;
        [SerializeField] GameObject optionPrefab;

        [Header("Current State")]
        [SerializeField] ConversationScript currentScript;
        ConversationPiece currentPiece;

        [Header("Dialog Helper")]
        [SerializeField] DialogHelper dialogHelper;

        bool isDebugMode = true;

        void Awake()
        {
            if (dialogPanel == null)
                Debug.LogError("DialogueRunner: dialogPanel is not assigned.");

            if (dialogText == null)
                Debug.LogError("DialogueRunner: dialogText is not assigned.");

            if (optionsParent == null)
                Debug.LogError("DialogueRunner: optionsParent is not assigned.");

            if (optionPrefab == null)
                Debug.LogError("DialogueRunner: optionButtonPrefab is not assigned.");

            if (portraitImage == null)
                Debug.LogError("DialogueRunner: portraitImage not assigned.");

            dialogPanelCanvasGroup = dialogPanel.GetComponent<CanvasGroup>();
            // Optional: 預設關閉對話面板
            SetDialogPanel(false, 0);
            
        }

        void InitializeOptions()
        {
            foreach (Transform child in optionsParent)
            {
                Destroy(child.gameObject);
            }
        }

        public void StartConversation(ConversationScript script)
        {
            if(isDebugMode) Debug.Log("Starting Conversation");
            currentScript = script;
            ShowPiece(script.GetFirstPiece());
        }

        void ShowPiece(ConversationPiece piece)
        {
            if(dialogPanel.activeInHierarchy) SetDialogPanel(true, 0f);
            else SetDialogPanel(true, 0.3f);
            
            if(isDebugMode) Debug.Log("Showing Dialog");

            currentPiece = piece;
            UpdateDialog(piece.text);
            
            if (currentPiece.portrait != null) 
            {
                if(dialogHelper) dialogHelper.ShowDialogPanel(true);
                

                portraitImage.enabled = true;
                UpdatePortraitImage(currentPiece.portrait);
            } else 
            {
                if(dialogHelper) dialogHelper.ShowDialogPanel(false);
                portraitImage.enabled = false;
            }

            if (piece.portraitDescription != string.Empty)
            {
                UpdatePortraitDescription(piece.portraitDescription);
            } else UpdatePortraitDescription("");

            if(backgroundImage != null)
            {
                if (currentPiece.background != null) 
                {
                    backgroundImage.enabled = true;
                    UpdateBackgroundImage(currentPiece.background);
                } else backgroundImage.enabled = false;
            }

            
            if (piece.audio != null) PlayAudio(piece.audio);

            InitializeOptions();

            foreach (var option in piece.options)
            {
                if (option != null)
                {
                    var optionUnit = Instantiate(optionPrefab, optionsParent);
                    var optionText = optionUnit.GetComponentInChildren<TextMeshProUGUI>();
                    optionText.text = option.text;
                    var optionButton = optionUnit.GetComponent<Button>();

                    optionButton.onClick.AddListener(() => HandleOption(option));
                }
            }
        }

        void UpdatePortraitImage(Sprite image)
        {
            if(portraitImage != null) portraitImage.sprite = image;
        }

        void UpdateBackgroundImage(Sprite image)
        {
            backgroundImage.sprite = image;
        }

        void UpdatePortraitDescription(string text)
        {
            Debug.Log("Update PortraitDescription to " + text);
            if(portraitDescription != null) portraitDescription.text = text;
        }

        void UpdateDialog(string text)
        {
            if (SimplePrinter.Instance != null)
            {
                SimplePrinter.Instance.Show(text, 0, dialogText);
            } else dialogText.text = text;
        }

        void PlayAudio(AudioClip audio)
        {
            
        }

        void HandleOption(ConversationOption option)
        {
            if(isDebugMode) Debug.Log("An option is selected");

            if (!string.IsNullOrEmpty(option.targetID))
            {
                ConversationPiece nextPiece = currentScript.GetPieceByID(option.targetID);
                ShowPiece(nextPiece);
            } else
            {
                ResetDialog();
            }

            option.onSelected?.Invoke();
        }

        public void PlayNextPiece()
        {
            if (currentPiece.NextPiece == string.Empty) return;
            var nextPiece = currentScript.GetPieceByID (currentPiece.NextPiece);
            if (nextPiece != null) ShowPiece(nextPiece);
        }

        public void ResetDialog()
        {
            if(isDebugMode) Debug.Log("Dialog Reset");
            SetDialogPanel(false, 0.3f);
            ResetPanel(0.3f);
        }

        IEnumerator ResetPanel(float duration)
        {
            yield return new WaitForSeconds(duration);
            UpdateDialog("");
            UpdatePortraitImage(defaultPortraitImage);
            UpdatePortraitDescription("");
            InitializeOptions();
        }

        void SetDialogPanel(bool status, float duration)
        {
            dialogPanelCanvasGroup.DOKill(); // 先停止舊動畫，避免重疊

            if (status)
            {
                dialogPanel.SetActive(true);
                dialogPanelCanvasGroup.alpha = 0;
                dialogPanelCanvasGroup.DOFade(1, duration).SetEase(Ease.OutQuad);
            }
            else
            {
                dialogPanelCanvasGroup.DOFade(0, duration).SetEase(Ease.InQuad)
                    .OnComplete(() => dialogPanel.SetActive(false));
            }
        }
    }    
}

