using UnityEngine;
using UnityEngine.UI;

public class CraftingTabController : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Attunement attunement;
        public Toggle toggle;
    }

    public Tab[] tabs;
    public CraftingPanelUI craftingPanelUI;

    private void Awake()
    {
        foreach (var t in tabs)
        {
            Attunement a = t.attunement;   // local capture 防閉包問題
            t.toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                    OnToggle(a);
            });
        }
    }

    private void OnToggle(Attunement attunement)
    {
        craftingPanelUI.ShowPage(attunement);
    }
}
