using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[BindDrawerTo(typeof(BeginTabGroupAttribute))]
public class TabGroupDrawer : BaseGroupAttributeDrawer {
    public class Tab {
        public string Name { get; private set; }
        public List<BaseDisplayable> Displayables { get; private set; }

        public Tab(string name) {
            Name = name;
            Displayables = new List<BaseDisplayable>();
        }
    }

    private static readonly GUIStyle containerBoxStyle;
    private static readonly GUIStyle tabButtonStyle;

    static TabGroupDrawer() {
        tabButtonStyle = new GUIStyle() {
            border = new RectOffset(1, 1, 1, 1),
            alignment = TextAnchor.MiddleCenter,
        };
        tabButtonStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color32(0xD2, 0xD2, 0xD2, 255) : new Color32(0x14, 0x14, 0x14, 255);
        tabButtonStyle.normal.background = Resources.Load<Texture2D>("Dark/mid-button-normal-0");

        tabButtonStyle.onNormal.background = Resources.Load<Texture2D>("Dark/mid-button-onNormal-0");
        tabButtonStyle.onNormal.textColor = tabButtonStyle.normal.textColor;

        tabButtonStyle.hover.background = Resources.Load<Texture2D>("Dark/mid-button-hover-0");
        tabButtonStyle.hover.textColor = tabButtonStyle.normal.textColor;

        tabButtonStyle.active.background = Resources.Load<Texture2D>("Dark/mid-button-active-0");
        tabButtonStyle.active.textColor = tabButtonStyle.normal.textColor;

        containerBoxStyle = new GUIStyle() {
            border = new RectOffset(6, 6, 0, 6),
        };
        containerBoxStyle.normal.background = Resources.Load<Texture2D>("Dark/end-rounded-box-0");
    }

    public int ID { get; set; }

    private Dictionary<string, Tab> Tabs { get; set; }

    public TabGroupDrawer() {
        Tabs = new Dictionary<string, Tab>(4);
    }

    //public override void HandleGroupBegin(SerializedFieldContainer begin) {
    //    var inspector = OverseerInspector.CurrentInspector;
    //    var allFields = inspector.AllFields;

    //    string gn = begin.BeginGroup.Name;

    //    EnsureTabRegistered(gn);
    //    RegisterTabDisplayable(gn, new FieldDisplayer(begin));

    //    inspector.FieldPointer++;

    //    while (allFields[inspector.FieldPointer].EndGroupCount <= 0) {
    //        RegisterTabDisplayable(gn, new FieldDisplayer(allFields[inspector.FieldPointer]));

    //        inspector.FieldPointer++;
    //    }

    //    RegisterTabDisplayable(gn, new FieldDisplayer(allFields[inspector.FieldPointer]));
    //}

    void EnsureTabRegistered(string name) {
        if (!Tabs.ContainsKey(name)) {
            RegisterNewTab(name);
        }
    }

    private string currentTab = string.Empty;
    public void RegisterNewTab(string name) {
        if (Tabs.ContainsKey(name)) {
            Debug.LogWarning("Trying to register a new tab while a tab with name of \"" + name + "\" is already exist.");
            return;
        }

        Tabs.Add(name, new Tab(name));

        if (string.IsNullOrEmpty(currentTab)) {
            currentTab = name;
        }
    }

    public void RegisterTabDisplayable(string name, BaseDisplayable display) {
        if (Tabs.TryGetValue(name, out var tab)) {
            tab.Displayables.Add(display);
            return;
        }

        Debug.LogError("Trying to register a property to tab section named \"" + name + "\", but that tab doesn't exist.");
    }

    public override void DrawLayout() {
        Debug.LogWarning("Tab Group are being redesign...");

        //OverseerInspector.CurrentInspector.RequestConstantRepaint();

        //EditorGUILayout.BeginHorizontal();

        //foreach (var tabPair in Tabs) {
        //    GUIContent buttonGUIContent = new GUIContent(tabPair.Key);
        //    var buttonRect = GUILayoutUtility.GetRect(buttonGUIContent, tabButtonStyle, GUILayout.Height(21));

        //    if (DoTabButton(buttonRect, buttonGUIContent, currentTab == tabPair.Key)) {
        //        currentTab = tabPair.Key;
        //    }
        //}

        //EditorGUILayout.EndHorizontal();

        //var rect = EditorGUILayout.BeginVertical();
        //{
        //    var containerRect = new Rect(rect.x, rect.y, rect.width, rect.height + EditorGUIUtility.standardVerticalSpacing);
        //    GUI.Box(containerRect, "", containerBoxStyle);

        //    EditorGUILayout.Space(2); // I have to do this to remove the spacing between the tab buttons and the box container for some reason, value is based on preference (0 also works)

        //    var tab = Tabs[currentTab];
        //    foreach (var displayable in tab.Displayables) {
        //        displayable.DrawLayout();
        //    }

        //    EditorGUILayout.Space(3);
        //}
        //EditorGUILayout.EndVertical();

        //EditorGUILayout.Space(3);
    }

    bool DoTabButton(Rect rect, GUIContent content, bool on) {
        int id = GUIUtility.GetControlID(FocusType.Passive);

        var evt = Event.current;

        bool pressed = false;
        bool isHover = rect.Contains(evt.mousePosition);

        switch (evt.GetTypeForControl(id)) {
            case EventType.Repaint:
                tabButtonStyle.Draw(rect, content, isHover, pressed, on, false);
                break;

            case EventType.MouseDown:
                if (isHover && evt.button == 0 && !on) {
                    pressed = true;
                    evt.Use();
                }
                break;
        }

        return pressed;
    }

    public override bool ShouldCreateNew(OverseerBeginGroupAttribute attribute) {
        switch (attribute) {
            case BeginTabGroupAttribute begin:
                return begin.ID != ID;
        }

        return true;
    }
}