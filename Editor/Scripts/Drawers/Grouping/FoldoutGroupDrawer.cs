using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[BindDrawerTo(typeof(BeginFoldoutGroupAttribute))]
public class FoldoutGroupDrawer : BaseGroupAttributeDrawer {
    private AnimBool foldoutAnim;

    private readonly static GUIStyle boxStyle;

    static FoldoutGroupDrawer() {
        boxStyle = new GUIStyle() {
            padding = new RectOffset(4, 4, 4, 4),
            border = new RectOffset(3, 3, 3, 3),
        };

        boxStyle.normal.background = Resources.Load<Texture2D>("Dark/group-foldout-box");
    }

    public FoldoutGroupDrawer() {
        foldoutAnim = new AnimBool(false);

        foldoutAnim.valueChanged.AddListener(() => {
            OverseerInspector.CurrentInspector.RequestConstantRepaint();
        });
    }

    public override void DrawLayout() {
        EditorGUI.indentLevel--;
        var rect = EditorGUILayout.BeginVertical();

        if (Event.current.type == EventType.Repaint) {
            boxStyle.Draw(EditorGUI.IndentedRect(rect), false, false, false, false);
        }

        using (new EditorGUI.IndentLevelScope(1)) {
            EditorGUILayout.Space(3);

            var old = EditorStyles.foldout.fontStyle;
            EditorStyles.foldout.fontStyle = (AssociatedAttribute as BeginFoldoutGroupAttribute).FontStyle;

            foldoutAnim.target = EditorGUILayout.Foldout(foldoutAnim.target, (AssociatedAttribute as BeginFoldoutGroupAttribute).Name);
            EditorStyles.foldout.fontStyle = old;

            EditorGUILayout.Space(3);

            EditorGUILayout.BeginFadeGroup(foldoutAnim.faded);
            if (foldoutAnim.faded > 0.001f) {
                DrawAllChildsLayout();
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.EndFadeGroup();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

        EditorGUI.indentLevel++;
    }
}
