using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PanelState
{
    public Coroutine typeRoutine;
    public TypingRequest Request;
    public bool IsPlaying;
    public float SpeedMult = 1;
    public float CloseAftPrint = 0;
}

public class TypingRequest
{
    public string msg;
    public int cps;

    public TypingRequest(string msg, int cps)
    {
        this.msg = msg;
        this.cps = cps;
    }
}

public class SimplePrinter : MonoBehaviour
{
    public static SimplePrinter Instance { get; private set; }
    [SerializeField] int defaultCPS = 30;
    [SerializeField] float speedUpMult = 2;
    [SerializeField] TMP_Text defaultPanel;
    [SerializeField] TextMeshProUGUI[] stageDialogues;
    Dictionary<TMP_Text, PanelState> panelStates = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (stageDialogues == null || stageDialogues.Length == 0) return;
        foreach (var dialog in stageDialogues)
        {
            dialog.text = "";
            dialog.transform.parent.gameObject.SetActive(false);
        }
    }

    public void ShowStageMsg(string message, int index, int charsPerSecond = -1, float CloseAftPrint = -1)
    {
        stageDialogues[index].transform.parent.gameObject.SetActive(true);
        Show(message, charsPerSecond, stageDialogues[index], CloseAftPrint);
    }


    /// <summary>
    /// Shows text on a panel.
    /// </summary>
    /// <param name="msg">Text to display.</param>
    /// <param name="cps">Typing speed: -1 = instant, 0 = default, others = custom.</param>
    /// <param name="panel">Target panel (null = default).</param>
    /// <param name="CloseAftPrint">Seconds before auto-close; -1 = won't close.</param>
    public void Show (string msg, int cps = 0, TMP_Text panel = null, float CloseAftPrint = -1)
    {
        if (cps == 0) cps = defaultCPS;
        if (panel == null) panel = defaultPanel;

        TypingRequest request = new (msg, cps);
        if (!panelStates.TryGetValue(panel, out PanelState state))
        {
            state = new PanelState();
            panelStates.Add(panel, state);
        }

        if (state.IsPlaying && state.typeRoutine != null)
            StopCoroutine(state.typeRoutine);

        state.Request = request;
        state.IsPlaying = true;
        state.typeRoutine = StartCoroutine(TypeRoutine(panel, state));
        state.CloseAftPrint = CloseAftPrint;
    }

    /// <summary>
    /// Append text to panel's existing text.
    /// </summary>
    /// <param name="msg">Text to display.</param>
    /// <param name="cps">Typing speed: -1 = instant, 0 = default, others = custom.</param>
    /// <param name="panel">Target panel (null = default).</param>
    /// <param name="CloseAftPrint">Seconds before auto-close; -1 = won't close.</param>
    public void Append (string msg, int cps = 0, TMP_Text panel = null, float CloseAftPrint = -1)
    {
        if (cps == 0) cps = defaultCPS;
        if (panel == null) panel = defaultPanel;

        if (!panelStates.TryGetValue(panel, out PanelState state))
        {
            state = new PanelState();
            panelStates.Add(panel, state);
        }

        //查詢目標 panel 之當前 state 並複製文本，終止他並計算 textInfo.characterCount 
        if(state.IsPlaying && state.typeRoutine != null)
        {
            StopCoroutine(state.typeRoutine);
            panel.text = state.Request.msg;
        }
        panel.ForceMeshUpdate();
        int baseVisible = panel.maxVisibleCharacters;

        string baseFull  = panel.text ?? string.Empty;

        state.Request = new TypingRequest(baseFull + msg, cps);
        state.IsPlaying = true;
        state.typeRoutine = StartCoroutine(TypeRoutine(panel, state, baseVisible));
        state.CloseAftPrint = CloseAftPrint;
    }

    public void SpeedUp(TMP_Text panel)
    {
        if (panelStates.TryGetValue(panel, out PanelState state)) state.SpeedMult *= speedUpMult;
    } 

    public bool PanelIsPlaying(TMP_Text panel)
    {
        if (!panelStates.TryGetValue(panel, out PanelState state)) return false;
        return state.IsPlaying;
    }

    IEnumerator TypeRoutine(TMP_Text panel, PanelState state, int StartVisibleChar = 0)
    {
        string msg = state.Request.msg;
        int cps = state.Request.cps;

        panel.text = msg;
        panel.ForceMeshUpdate();
        panel.maxVisibleCharacters = Mathf.Clamp(StartVisibleChar, 0, panel.textInfo.characterCount);

        int shown = StartVisibleChar;
        if (cps <= 0) 
        {
            panel.maxVisibleCharacters = panel.textInfo.characterCount;
            yield return null;
        } else while (shown < panel.textInfo.characterCount)
        {
            shown++;
            panel.maxVisibleCharacters = shown;
            yield return new WaitForSeconds(1f / (cps * state.SpeedMult));
        }

        state.IsPlaying = false;
        state.typeRoutine = null;
        state.SpeedMult = 1;
        Debug.Log("Reseting SpeedMult");

        if (state.CloseAftPrint > -1)
        {
            yield return new WaitForSeconds(state.CloseAftPrint);
            if (!state.IsPlaying) 
            {
                panel.text = "";
                panel.transform.parent.gameObject.SetActive(false);
                panelStates.Remove(panel);
            }
        }
    }
}